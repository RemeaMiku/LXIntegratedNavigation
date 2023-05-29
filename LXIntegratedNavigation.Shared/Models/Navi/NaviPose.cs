namespace LXIntegratedNavigation.Shared.Models.Navi;

public record class NaviPose
{
    public GpsTime TimeStamp { get; set; }
    public Orientation Orientation { get; set; }
    public EulerAngles EulerAngles => Orientation.EulerAngles;
    public GeodeticCoord Location { get; set; }
    public Vector Velocity { get; set; }
    public Angle Yaw => EulerAngles.Yaw;
    public Angle Pitch => EulerAngles.Pitch;
    public Angle Roll => EulerAngles.Roll;

    public double NorthVelocity => Velocity[0];
    public double GroundVelocity => Velocity[2];

    public double EastVellocity => Velocity[1];
    public Angle Latitude => Location.Latitude;
    public Angle Longitude => Location.Longitude;
    public double Altitude => Location.Altitude;
    public double B => Location.B;
    public double L => Location.L;

    public NaviPose(GpsTime timeStamp, GeodeticCoord location, Vector velocity, Orientation orientation)
    {
        TimeStamp = timeStamp;
        Location = location;
        Velocity = velocity;
        Orientation = orientation;
    }
}
