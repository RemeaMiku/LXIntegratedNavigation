namespace LXIntegratedNavigation.Shared.Essentials.NormalGravityModel;

public interface INormalGravityModel
{
    #region Public Properties

    public EarthEllipsoid Ellipsoid { get; }

    #endregion Public Properties

    #region Public Methods

    public double NormalGravityAt(Angle latitude, double altitude);

    public Vector NormalGravityAsVectorAt(Angle latitude, double altitude)
        => new(0, 0, NormalGravityAt(latitude, altitude));

    #endregion Public Methods

}
