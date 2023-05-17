using System;
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

    public static IEnumerable<ImuData> ReadImuDatas(string filePath)
    {
        const double accScaleFactor = 0.05 / 32768;
        const double gyroScaleFactor = 0.1 / 3600 / 256;
        const double samplingRate = 100;
        var func = new Func<string, ImuData?>((line) =>
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
                return new(new(week, sow), new(new double[] { accX, accY, accZ }), new(new double[] { gyroX, gyroY, gyroZ }));
            }
            return null;
        });
        return FileStreamReadLine(filePath, func);
    }
}
