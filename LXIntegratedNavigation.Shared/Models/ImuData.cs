namespace LXIntegratedNavigation.Shared.Models;

public record class ImuData
{
    public GpsTime TimeStamp { get; set; }
    public double IntervalSeconds { get; set; }
    public Vector Accelerometer { get; set; }
    public Vector Gyroscope { get; set; }
    public Vector DeltaVelocity => Accelerometer * IntervalSeconds;
    public Vector DeltaAngular => Gyroscope * IntervalSeconds;
    public double AccX => Accelerometer[0];
    public double AccY => Accelerometer[1];
    public double AccZ => Accelerometer[2];
    public double GyroX => Gyroscope[0];
    public double GyroY => Gyroscope[1];
    public double GyroZ => Gyroscope[2];
    public bool IsVirtual { get; set; }
    public ImuData(GpsTime gpsTime, double intervalSeconds, Vector accelerometer, Vector gyroscope, bool isVirtual = false)
    {
        TimeStamp = gpsTime;
        IntervalSeconds = intervalSeconds;
        Accelerometer = accelerometer;
        Gyroscope = gyroscope;
        Accelerometer.IsColumn = false;
        Gyroscope.IsColumn = false;
        IsVirtual = isVirtual;
    }
}