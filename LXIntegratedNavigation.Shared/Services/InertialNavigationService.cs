using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LXIntegratedNavigation.Shared.Interfaces;
using LXIntegratedNavigation.Shared.Models;

namespace LXIntegratedNavigation.Shared.Services;

public class InertialNavigationService
{
    private readonly IGravityService _gravityService;

    public InertialNavigationService(IGravityService gravityService)
    {
        _gravityService = gravityService;
    }

    private static Vector BuildOmega_ie_n(double latRads)
    => new(new double[] { EarthRotationSpeed * Cos(latRads), 0, -EarthRotationSpeed * Sin(latRads) });

    public EulerAngle StaticAlignment(Angle initLatitude, double initHeight, IEnumerable<ImuData> imuDatas)
    {
        var gravity = _gravityService.CalculateNormalGravity(initLatitude, initHeight);
        var gn = new Vector(new double[] { 0, 0, gravity });
        var omega_ie_n = BuildOmega_ie_n(initLatitude.Radians);
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

}
