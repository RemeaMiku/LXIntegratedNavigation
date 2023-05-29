namespace LXIntegratedNavigation.Shared.Helpers;

public class BuildHelper
{
    public static Vector BuildOmega_ie_n(Angle latitude)
    => EarthRotationSpeed * new Vector(Cos(latitude), 0, -Sin(latitude));

    public static Vector BuildOmega_en_n(Angle latitude, double altitude, double velNorth, double velEast, EarthEllipsoid ellipsoid)
    {
        var rm = ellipsoid.M(latitude);
        var rn = ellipsoid.N(latitude);
        return new(velEast / (rn + altitude), -velNorth / (rm + altitude), -velEast * Tan(latitude) / (rn + altitude));
    }
}
