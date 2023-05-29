namespace LXIntegratedNavigation.Shared.Models;

public record class GnssData(GeodeticCoord Coord, double StdB, double StdL, double StdH, Vector Velocity, double StdV_n, double StdV_e, double StdV_d);