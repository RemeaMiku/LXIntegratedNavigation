using LXIntegratedNavigation.Shared.Models;

namespace LXIntegratedNavigation.Shared.Helpers;

public class FileHelper
{
    #region Public Methods

    public static string GetPathAtDesktop(string fileName)
    => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);

    public static IEnumerable<ImuData> ReadImuDatas(string filePath, TimeSpan interval)
    {
        const double accScaleFactor = 0.05 / 32768;
        const double gyroScaleFactor = 0.1 / 3600 / 256;
        var intervalSeconds = interval.TotalSeconds;
        ImuData? preImu = null;
        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        using var reader = new StreamReader(stream);
        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            if (line is null || string.IsNullOrWhiteSpace(line))
                continue;
            var data = line.Trim().Split('*')[0].Split(';');
            if (data is null || data.Length == 0)
                continue;
            var header = data[0].Split(',');
            var record = data[1].Split(',');
            if (header[0] == "%RAWIMUSA")
            {
                var week = ushort.Parse(record[0]);
                var sow = double.Parse(record[1]);
                var timeStamp = new GpsTime(week, sow);
                var accX = -double.Parse(record[4]);
                var accY = double.Parse(record[5]);
                var accZ = -double.Parse(record[3]);
                var gyroX = -double.Parse(record[7]);
                var gyroY = double.Parse(record[8]);
                var gyroZ = -double.Parse(record[6]);
                var acc = new Vector(accX, accY, accZ) * accScaleFactor / intervalSeconds;
                var gyro = new Vector(gyroX, gyroY, gyroZ) * gyroScaleFactor / intervalSeconds;
                var imudata = new ImuData(timeStamp, intervalSeconds, acc, gyro);
                if (preImu is not null && timeStamp - preImu.TimeStamp >= 1.1 * interval)
                {
                    (var former, var latter) = SplitImuData(preImu.TimeStamp, timeStamp, preImu.TimeStamp + interval, imudata);
                    yield return former;
                    yield return latter;
                    preImu = latter;
                    continue;
                }
                yield return imudata;
                preImu = imudata;
            }
        }
    }

    public static void WritePoses(string filePath, IEnumerable<NaviPose> poses)
    {
        var func = (NaviPose pose) => $"{pose.TimeStamp.Week},{pose.TimeStamp.Sow:F2},{pose.Latitude.Degrees:F8},{pose.Longitude.Degrees:F8},{pose.H:F4},{pose.NorthVelocity:F4},{pose.EastVellocity:F4},{pose.DownVelocity:F4},{pose.Yaw.Degrees:F8},{pose.Pitch.Degrees:F8},{pose.Roll.Degrees:F8}";
        FileStreamWriteLine(filePath, poses, func, "Week, Sow(s), Lat(deg), Lon(deg), Hgt(m), NorthVel(m / s), EastVel(m / s), DownVel(m / s), Yaw(deg), Pitch(deg), Roll(deg)");
    }

    public static IEnumerable<NaviPose> ReadPosFile(string filePath)
    {
        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        using var reader = new StreamReader(stream);
        reader.ReadLine();
        reader.ReadLine();
        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            if (line is null || string.IsNullOrWhiteSpace(line))
                continue;
            var values = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var week = ushort.Parse(values[0]);
            var sow = double.Parse(values[1]);
            var lat = FromDegrees(double.Parse(values[2]));
            var lon = FromDegrees(double.Parse(values[3]));
            var hgt = double.Parse(values[4]);
            var ve = double.Parse(values[5]);
            var vn = double.Parse(values[6]);
            var vu = double.Parse(values[7]);
            var yaw = Map(FromDegrees(double.Parse(values[11])), AngleRange.NegativeStraightToStraight);
            var pitch = FromDegrees(double.Parse(values[12]));
            var roll = FromDegrees(double.Parse(values[13]));
            yield return new NaviPose(new(week, sow), new(lat, lon, hgt), new(vn, ve, -vu), new(new EulerAngles(yaw, pitch, roll)));
        }
    }

    public static IEnumerable<GnssData> ReadGnssDatas(string filePath)
    {
        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        using var reader = new StreamReader(stream);
        reader.ReadLine();
        reader.ReadLine();
        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            if (line is null || string.IsNullOrWhiteSpace(line))
                continue;
            var values = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var week = ushort.Parse(values[0]);
            var sow = double.Parse(values[1]);
            var lat = FromDegrees(double.Parse(values[2]));
            var lon = FromDegrees(double.Parse(values[3]));
            var hgt = double.Parse(values[4]);
            var stdre = double.Parse(values[5]);
            var stdrn = double.Parse(values[6]);
            var stdru = double.Parse(values[7]);
            var ve = double.Parse(values[8]);
            var vn = double.Parse(values[9]);
            var vu = double.Parse(values[10]);
            var stdve = double.Parse(values[11]);
            var stdvn = double.Parse(values[12]);
            var stdvu = double.Parse(values[13]);
            yield return new GnssData(new(week, sow), new(lat, lon, hgt), stdrn, stdre, stdru, new(vn, ve, -vu), stdvn, stdve, stdvu);
        }
    }

    #endregion Public Methods

    #region Private Methods

    private static IEnumerable<T> FileStreamReadLine<T>(string filePath, Func<string, T?> func)
    {
        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        using var reader = new StreamReader(stream);
        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            if (line is null || string.IsNullOrWhiteSpace(line))
                continue;
            var result = func(line);
            if (result is null)
                continue;
            yield return result;
        }
    }

    private static void FileStreamWriteLine<T>(string filePath, IEnumerable<T> values, Func<T, string> func, string? title)
    {
        using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        using var writer = new StreamWriter(stream);
        if (title is not null)
            writer.WriteLine(title);
        foreach (var value in values)
            writer.WriteLine(func(value));
    }

    #endregion Private Methods

    //public static IEnumerable<T> ReadCsvFile<T>(string filePath, IDictionary<string, string> titlePropertyPairs, char separator = ',', int skipLines = 0)
    //{

    //}
}
