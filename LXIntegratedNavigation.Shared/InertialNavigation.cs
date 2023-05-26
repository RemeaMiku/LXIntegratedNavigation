using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LXIntegratedNavigation.Shared.Interfaces;
using LXIntegratedNavigation.Shared.Models;

namespace LXIntegratedNavigation.Shared;

public class InertialNavigation
{
    private readonly INormalGravityService _gravityService;
    private readonly EarthEllipsoid _ellipsoid;

    public InertialNavigation(INormalGravityService gravityService, EarthEllipsoid ellipsoid)
    {
        _gravityService = gravityService;
        _ellipsoid = ellipsoid;
    }

    private Vector BuildGn(Angle latitude, double altitude) => new(new double[] { 0, 0, _gravityService.NormalGravityAt(latitude, altitude) });

    private static Vector BuildOmega_ie_n(Angle latitude)
    => new(new double[] { EarthRotationSpeed * Cos(latitude), 0, -EarthRotationSpeed * Sin(latitude) });

    private Vector BuildOmega_en_n(Angle latitude, double altitude, double velNorth, double velEast)
    {
        var rm = _ellipsoid.M(latitude);
        var rn = _ellipsoid.N(latitude);
        return new(new double[] { velEast / (rn + altitude), -velNorth / (rm + altitude), -velEast * Tan(latitude) / (rn + altitude) });
    }

    public EulerAngle StaticAlignment(Angle initLatitude, double initAltitude, IEnumerable<ImuData> imuDatas)
    {
        var gn = BuildGn(initLatitude, initAltitude);
        var omega_ie_n = BuildOmega_ie_n(initLatitude);
        var v_g = gn.Unitization();
        var v_omega = gn.OuterProduct(omega_ie_n).Unitization();
        var v_gOmega = gn.OuterProduct(omega_ie_n).OuterProduct(gn).Unitization();
        var meanAccX = imuDatas.Average(data => data.AccX);
        var meanAccY = imuDatas.Average(data => data.AccY);
        var meanAccZ = imuDatas.Average(data => data.AccZ);
        var meanGyroX = imuDatas.Average(data => data.GyroX);
        var meanGyroY = imuDatas.Average(data => data.GyroY);
        var meanGyroZ = imuDatas.Average(data => data.GyroZ);
        var gb = -new Vector(new double[] { meanAccX, meanAccY, meanAccZ });
        var omega_ie_b = new Vector(new double[] { meanGyroX, meanGyroY, meanGyroZ });
        var w_g = gb.Unitization();
        var w_omega = gb.OuterProduct(omega_ie_b).Unitization();
        var w_gOmega = gb.OuterProduct(omega_ie_b).OuterProduct(gb).Unitization();
        var V = Matrix.FromVectorsAsColumns(v_g, v_omega, v_gOmega);
        var W = Matrix.FromVectorsAsRows(w_g, w_omega, w_gOmega);
        var rotationMatrix = V * W;
        return rotationMatrix.ToEulerAngle();
    }

    public EulerAngle StaticAlignment(GeodeticCoord initCoord, IEnumerable<ImuData> imuDatas)
        => StaticAlignment(initCoord.Latitude, initCoord.Altitude, imuDatas);

    public NavigationPose Mechanizations(NavigationPose prePose, ImuData preImu, ImuData curImu, double? intervalSeconds = null)
    {
        var dt = intervalSeconds ?? (curImu.TimeStamp - preImu.TimeStamp).TotalSeconds;
        if (dt <= 0)
            throw new ArgumentException($"The timestamp of{nameof(curImu)} should be after the {nameof(preImu)}.");
        var dv_cur = curImu.Accelerometer * dt;
        var dtheta_cur = curImu.Gyroscope * dt;
        var dv_pre = preImu.Accelerometer * dt;
        var dtheta_pre = preImu.Gyroscope * dt;
        var deltav_fk_b = dv_cur + 0.5 * dtheta_cur.OuterProduct(dv_cur) + (dtheta_pre.OuterProduct(dv_cur) + dv_pre.OuterProduct(dtheta_cur)) / 12;
        var omega_ie_n = BuildOmega_ie_n(prePose.Latitude);
        var omega_en_n = BuildOmega_en_n(prePose.Latitude, prePose.Altitude, prePose.NorthVelocity, prePose.EastVellocity);
        var zeta = (omega_ie_n + omega_en_n) * dt;
        var preRotationMatrix = prePose.EulerAngle.ToRotationMatrix<double>();
        var deltav_fk_n = (Matrix.Identity(3) - 0.5 * Matrix.FromAxialVector(zeta)) * preRotationMatrix * deltav_fk_b;
        var preGn = BuildGn(prePose.Latitude, prePose.Altitude);
        var deltav_gcork_n = (preGn - (2 * omega_ie_n + omega_en_n).OuterProduct(prePose.Velocity)) * dt;
        var velocity = prePose.Velocity + deltav_fk_n + deltav_gcork_n;
        var meanVel = 0.5 * (velocity + prePose.Velocity);
        var height = prePose.Altitude - meanVel[2] * dt;
        var meanHgt = 0.5 * (height + prePose.Altitude);
        var latitude = prePose.B + meanVel[0] * dt / (_ellipsoid.M(prePose.Latitude) + meanHgt);
        var meanLat = 0.5 * (latitude + prePose.B);
        var longitude = prePose.L + meanVel[1] * dt / ((_ellipsoid.N(meanLat) + meanHgt) * Cos(meanLat));
        var phi_k = dtheta_cur + dtheta_pre.OuterProduct(dtheta_cur) / 12;
        var q_bk = phi_k.ToQuaternion();
        var q_nk1 = (-zeta).ToQuaternion();
        var preQuaternion = prePose.EulerAngle.ToQuaternion<double>();
        var curQuaternion = q_nk1 * preQuaternion * q_bk;
        var curEulerAngle = curQuaternion.ToRotationMatrix().ToEulerAngle();
        return new(curImu.TimeStamp, new(latitude, longitude, height), velocity, curEulerAngle);
    }

    public IEnumerable<NavigationPose> Solve(NavigationPose initNagationPose, IEnumerable<ImuData> imuDatas, double? intervalSeconds = null)
    {
        var prePose = initNagationPose;
        var preImu = imuDatas.First();
        yield return prePose;
        imuDatas = imuDatas.Skip(1);
        foreach (var curImu in imuDatas)
        {
            var curPose = Mechanizations(prePose, preImu, curImu, intervalSeconds);
            yield return curPose;
            prePose = curPose;
            preImu = curImu;
        }
    }
}
