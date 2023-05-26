using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LXIntegratedNavigation.Shared.Interfaces;
using LXIntegratedNavigation.Shared.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LXIntegratedNavigation.Shared.Services;

public class AscFileService
{

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

    public static string GetPathAtDesktop(string fileName)
    => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);

    public static IEnumerable<ImuData> ReadImuDatas(string filePath)
    {
        const double accScaleFactor = 0.05 / 32768;
        const double gyroScaleFactor = 0.1 / 3600 / 256;
        const double samplingRate = 100;
        var func = (string line) =>
        {
            var data = line.Trim().Split('*')[0].Split(';');
            if (data is null || data.Length == 0)
                return null;
            var header = data[0].Split(',');
            var record = data[1].Split(',');
            if (header[0] == "%RAWIMUSA")
            {
                var week = ushort.Parse(record[0]);
                var sow = double.Parse(record[1]);
                var accX = -double.Parse(record[4]) * accScaleFactor * samplingRate;
                var accY = double.Parse(record[5]) * accScaleFactor * samplingRate;
                var accZ = -double.Parse(record[3]) * accScaleFactor * samplingRate;
                var gyroX = -double.Parse(record[7]) * gyroScaleFactor * samplingRate;
                var gyroY = double.Parse(record[8]) * gyroScaleFactor * samplingRate;
                var gyroZ = -double.Parse(record[6]) * gyroScaleFactor * samplingRate;
                return new ImuData(new(week, sow), new(new double[] { accX, accY, accZ }), new(new double[] { gyroX, gyroY, gyroZ }));
            }
            return null;
        };
        return FileStreamReadLine(filePath, func);
    }

    public static void WritePoses(string filePath, IEnumerable<NavigationPose> poses)
    {
        var func = (NavigationPose pose) => $"{pose.TimeStamp.Week},{pose.TimeStamp.Sow:F2},{pose.Latitude.Degrees:F8},{pose.Longitude.Degrees:F8},{pose.Altitude:F4},{pose.NorthVelocity:F4},{pose.EastVellocity:F4},{pose.GroundVelocity:F4},{pose.Yaw.Degrees:F8},{pose.Pitch.Degrees:F8},{pose.Roll.Degrees:F8}";
        FileStreamWriteLine(filePath, poses, func, "Week, Sow(s), Lat(deg), Lon(deg), Hgt(m), NorthVel(m / s), EastVel(m / s), DownVel(m / s), Yaw(deg), Pitch(deg), Roll(deg)");
    }

    public static IEnumerable<NavigationPose> ReadPosFile(string filePath)
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
            var lat = double.Parse(values[2]);
            var lon = double.Parse(values[3]);
            var hgt = double.Parse(values[4]);
            var ve = double.Parse(values[5]);
            var vn = double.Parse(values[6]);
            var vu = double.Parse(values[7]);
            var yaw = Map(FromDegrees(double.Parse(values[11])), AngleRange.NegativeStraightToStraight);
            var pitch = FromDegrees(double.Parse(values[12]));
            var roll = FromDegrees(double.Parse(values[13]));
            yield return new NavigationPose(new(week, sow), new(lat, lon, hgt), new(new[] { vn, ve, -vu }), new(yaw, pitch, roll));
        }
    }
}
