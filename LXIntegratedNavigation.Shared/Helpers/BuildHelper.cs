using LXIntegratedNavigation.Shared.Models;

namespace LXIntegratedNavigation.Shared.Helpers;

public class BuildHelper
{
    #region Public Methods

    public static Vector BuildOmega_ie_n(Angle latitude)
    => EarthRotationSpeed * new Vector(Cos(latitude), 0, -Sin(latitude));

    public static Vector BuildOmega_en_n(Angle latitude, double altitude, double velNorth, double velEast, EarthEllipsoid ellipsoid)
    {
        var rm = ellipsoid.M(latitude);
        var rn = ellipsoid.N(latitude);
        return new(velEast / (rn + altitude), -velNorth / (rm + altitude), -velEast * Tan(latitude) / (rn + altitude));
    }

    public static (ImuData Former, ImuData Latter) SplitImuData(GpsTime startTime, GpsTime endTime, GpsTime splitTime, ImuData imuData)
    {
        var intervalSeconds = (splitTime - startTime).TotalSeconds;
        var former = new ImuData(splitTime, intervalSeconds, imuData.Accelerometer, imuData.Gyroscope);
        var latter = new ImuData(endTime, (endTime - splitTime).TotalSeconds, imuData.Accelerometer, imuData.Gyroscope);
        return (former, latter);
    }

    #endregion Public Methods
}
