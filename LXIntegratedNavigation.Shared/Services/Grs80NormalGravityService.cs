namespace LXIntegratedNavigation.Shared.Services;

public class Grs80NormalGravityService : INormalGravityService
{
    public EarthEllipsoid Ellipsoid => Grs80;

    public double NormalGravityAt(Angle latitude, double altitude)
    {
        var sin = Sin(latitude);
        var sin2 = sin * sin;
        var g0 = 9.7803267715 * (1 + sin2 * (0.0052790414 + 0.0000232718 * sin2));
        return g0 + altitude * (-3.087691089E-6 + 4.397731E-9 * sin2 + 0.721E-12 * altitude);
    }
}
