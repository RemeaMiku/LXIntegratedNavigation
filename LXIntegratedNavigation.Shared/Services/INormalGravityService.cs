namespace LXIntegratedNavigation.Shared.Services;

public interface INormalGravityService
{
    public EarthEllipsoid Ellipsoid { get; }
    public double NormalGravityAt(Angle latitude, double altitude);

    public Vector NormalGravityAsVectorAt(Angle latitude, double altitude)
        => new(0, 0, NormalGravityAt(latitude, altitude));

}
