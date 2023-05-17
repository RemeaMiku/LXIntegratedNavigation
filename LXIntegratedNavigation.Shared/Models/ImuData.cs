using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NaviSharp;

namespace LXIntegratedNavigation.Shared.Models;

public record class ImuData
{
    public GpsTime TimeStamp { get; }
    public Vector Accelerometer { get; }
    public Vector Gyroscope { get; }
    public double AccX => Accelerometer[0];
    public double AccY => Accelerometer[1];
    public double AccZ => Accelerometer[2];
    public double GyroX => Gyroscope[0];
    public double GyroY => Gyroscope[1];
    public double GyroZ => Gyroscope[2];
    public ImuData(GpsTime gpsTime, Vector accelerometer, Vector gyroscope)
    {
        TimeStamp = gpsTime;
        Accelerometer = accelerometer;
        Gyroscope = gyroscope;
        Accelerometer.IsColumn = false;
        Gyroscope.IsColumn = false;
    }
}