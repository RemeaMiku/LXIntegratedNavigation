using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LXIntegratedNavigation.Shared.Helpers;
using LXIntegratedNavigation.Shared.Models;
using NaviSharp;

namespace LXIntegratedNavigation.WPF.Services;

public class DataService
{
    private List<ImuData>? _imuDatas;
    public double ImuInterval { get; set; }
    public GpsTime InitTime { get; set; }
    public GeodeticCoord InitLocation { get; set; }
    public Vector? InitVelocity { get; set; }

    public Orientation? InitOrientation { get; set; }
    public List<ImuData> ImuDatas => _imuDatas ?? throw new InvalidOperationException();
    public Task InitializeImuDatas(string imuFilePath, TimeSpan interval)
    {
        _imuDatas = new(FileHelper.ReadImuDatas(imuFilePath, interval));
        return Task.CompletedTask;
    }
}
