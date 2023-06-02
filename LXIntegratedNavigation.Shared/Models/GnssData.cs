namespace LXIntegratedNavigation.Shared.Models;

public record class GnssData(GpsTime TimeStamp, GeodeticCoord Location, double StdR_n, double StdR_e, double StdR_d, Vector Velocity, double StdV_n, double StdV_e, double StdV_d);