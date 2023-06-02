using LXIntegratedNavigation.Shared.Essentials.NormalGravityModel;
using LXIntegratedNavigation.Shared.Models;

namespace LXIntegratedNavigation.Shared.Essentials.Navigation;

public class InertialNavigation
{
    #region Public Constructors

    public InertialNavigation(INormalGravityModel gravityService)
    {
        GravityModel = gravityService;
    }

    #endregion Public Constructors

    #region Public Properties

    public INormalGravityModel GravityModel { get; init; }

    #endregion Public Properties

    #region Public Methods

    public Orientation StaticAlignment(Angle initLatitude, double initAltitude, IEnumerable<ImuData> imuDatas)
    {
        var gn = GravityModel.NormalGravityAsVectorAt(initLatitude, initAltitude);
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
        var gb = -new Vector(meanAccX, meanAccY, meanAccZ);
        var omega_ie_b = new Vector(meanGyroX, meanGyroY, meanGyroZ);
        var w_g = gb.Unitization();
        var w_omega = gb.OuterProduct(omega_ie_b).Unitization();
        var w_gOmega = gb.OuterProduct(omega_ie_b).OuterProduct(gb).Unitization();
        var V = Matrix.FromVectorsAsColumns(v_g, v_omega, v_gOmega);
        var W = Matrix.FromVectorsAsRows(w_g, w_omega, w_gOmega);
        var rotationMatrix = V * W;
        return new(rotationMatrix);
    }

    public Orientation StaticAlignment(GeodeticCoord initCoord, IEnumerable<ImuData> imuDatas)
        => StaticAlignment(initCoord.Latitude, initCoord.Altitude, imuDatas);

    public NaviPose Mechanizations(NaviPose prePose, ImuData preImu, ImuData curImu, double? intervalSeconds = null)
    {
        var dt = intervalSeconds ?? curImu.IntervalSeconds;
        if (dt <= 0)
            throw new ArgumentException($"The timestamp of {nameof(curImu)}({curImu.TimeStamp}) should be after the {nameof(preImu)}({preImu.TimeStamp}).");
        var dv_cur = curImu.Accelerometer * dt;
        var dtheta_cur = curImu.Gyroscope * dt;
        var dv_pre = preImu.Accelerometer * dt;
        var dtheta_pre = preImu.Gyroscope * dt;
        var deltav_fk_b = dv_cur + 0.5 * dtheta_cur.OuterProduct(dv_cur) + (dtheta_pre.OuterProduct(dv_cur) + dv_pre.OuterProduct(dtheta_cur)) / 12;
        var omega_ie_n = BuildOmega_ie_n(prePose.Latitude);
        var omega_en_n = BuildOmega_en_n(prePose.Latitude, prePose.H, prePose.NorthVelocity, prePose.EastVellocity, GravityModel.Ellipsoid);
        var zeta = (omega_ie_n + omega_en_n) * dt;
        var preRotationMatrix = prePose.Orientation.Matrix;
        var deltav_fk_n = (Matrix.Identity(3) - 0.5 * Matrix.FromAxialVector(zeta)) * preRotationMatrix * deltav_fk_b;
        var preGn = GravityModel.NormalGravityAsVectorAt(prePose.Latitude, prePose.H);
        var deltav_gcork_n = (preGn - (2 * omega_ie_n + omega_en_n).OuterProduct(prePose.Velocity)) * dt;
        var velocity = prePose.Velocity + deltav_fk_n + deltav_gcork_n;
        var meanVel = 0.5 * (velocity + prePose.Velocity);
        var height = prePose.H - meanVel[2] * dt;
        var meanHgt = 0.5 * (height + prePose.H);
        var latitude = prePose.B + meanVel[0] * dt / (GravityModel.Ellipsoid.M(prePose.Latitude) + meanHgt);
        var meanLat = 0.5 * (latitude + prePose.B);
        var longitude = prePose.L + meanVel[1] * dt / ((GravityModel.Ellipsoid.N(meanLat) + meanHgt) * Cos(meanLat));
        var phi_k = dtheta_cur + dtheta_pre.OuterProduct(dtheta_cur) / 12;
        var q_bk = phi_k.ToQuaternion();
        var q_nk1 = (-zeta).ToQuaternion();
        var preQuaternion = prePose.Orientation.Quaternion;
        var curQuaternion = q_nk1 * preQuaternion * q_bk;
        return new(curImu.TimeStamp, new(latitude, longitude, height), velocity, new(curQuaternion));
    }

    public IEnumerable<NaviPose> Solve(NaviPose initPose, IEnumerable<ImuData> imuDatas, double? intervalSeconds = null)
    {
        var prePose = initPose;
        var preImu = imuDatas.First();
        yield return initPose;
        imuDatas = imuDatas.Skip(1);
        foreach (var curImu in imuDatas)
        {
            var curPose = Mechanizations(prePose, preImu, curImu, intervalSeconds);
            yield return curPose;
            prePose = curPose;
            preImu = curImu;
        }
    }

    #endregion Public Methods
}
