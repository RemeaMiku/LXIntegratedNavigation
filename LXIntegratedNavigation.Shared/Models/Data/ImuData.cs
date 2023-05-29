namespace LXIntegratedNavigation.Shared.Models;
/// <summary>
/// IMU数据结构，存储的是瞬时加速度和角速度
/// </summary>
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
    public bool IsVirtual { get; set; }
    public ImuData(GpsTime gpsTime, Vector accelerometer, Vector gyroscope, bool isVirtual = false)
    {
        TimeStamp = gpsTime;
        Accelerometer = accelerometer;
        Gyroscope = gyroscope;
        Accelerometer.IsColumn = false;
        Gyroscope.IsColumn = false;
        IsVirtual = isVirtual;
    }
}