using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LXIntegratedNavigation.Shared.Models;

public record class NavigationPose
{
    public GpsTime TimeStamp { get; set; }
    public EulerAngle EulerAngle { get; set; }
    public GeodeticCoord Location { get; set; }
    public Vector Velocity { get; set; }
    public Angle Yaw => EulerAngle.Yaw;
    public Angle Pitch => EulerAngle.Pitch;
    public Angle Roll => EulerAngle.Roll;

    public double NorthVelocity => Velocity[0];
    public double GroundVelocity => Velocity[2];

    public double EastVellocity => Velocity[1];
    public Angle Latitude => Location.Latitude;
    public Angle Longitude => Location.Longitude;
    public double Altitude => Location.Altitude;
    public double B => Location.B;
    public double L => Location.L;

    public NavigationPose(GpsTime timeStamp, GeodeticCoord location, Vector velocity, EulerAngle eulerAngle)
    {
        TimeStamp = timeStamp;
        Location = location;
        Velocity = velocity;
        EulerAngle = eulerAngle;
    }
}
