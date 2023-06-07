using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LXIntegratedNavigation.Shared.Essentials.Navigation;
using LXIntegratedNavigation.Shared.Helpers;
using LXIntegratedNavigation.Shared.Models;
using LXIntegratedNavigation.WPF.Models;
using NaviSharp;

namespace LXIntegratedNavigation.WPF.Services;

public class DataService
{

    public IEnumerable<ImuData>? ImuDatas { get; private set; }
    public IEnumerable<GnssData>? GnssDatas { get; private set; }

    public async Task<bool> InitializeImuDatasAsync(string imuFilePath, TimeSpan interval)
    {
        try
        {
            ImuDatas = await Task.Run(() => ReadImuDatas(imuFilePath, interval).DistinctBy(d => d.TimeStamp));
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> InitializeGnssDatasAsync(string gnssFilePath)
    {
        try
        {
            GnssDatas = await Task.Run(() => ReadGnssDatas(gnssFilePath));
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public NavigationData GetNavigationData(GpsTime initTime, GeodeticCoord initLocation, Vector initVelocity, Orientation initOrientation, LooseCombinationOptions? options = null)
    {
        if (ImuDatas is null || GnssDatas is null)
            throw new InvalidOperationException();
        options ??= new LooseCombinationOptions
                (
                    StdInitR_n: 0.009,
                    StdInitR_e: 0.008,
                    StdInitR_d: 0.022,
                    StdInitV_n: 0,
                    StdInitV_e: 0,
                    StdInitV_d: 0,
                    StdInitPhi_n: FromDegrees(0),
                    StdInitPhi_e: FromDegrees(0),
                    StdInitPhi_d: FromDegrees(0),
                    GnssLeverArm: new(0.235, 0.1, 0.89),
                    Arw: 0.2 * RadiansPerDegree / 60,
                    Vrw: 0.4 / 60,
                    StdAccBias: 400E-5,
                    StdAccScale: 1000E-6,
                    StdGyroBias: 24 * RadiansPerDegree / 3600,
                    StdGyroScale: 1000E-6,
                    RelevantTimeAccBias: 3600,
                    RelevantTimeAccScale: 3600,
                    RelevantTimeGyroBias: 3600,
                    RelevantTimeGyroScale: 3600
                );
        return new(initTime, new(initTime, initLocation, initVelocity, initOrientation), options, ImuDatas, GnssDatas);
    }

}
