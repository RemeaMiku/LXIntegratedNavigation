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

    #region Public Properties

    public IEnumerable<ImuData>? ImuDatas { get; private set; }
    public IEnumerable<GnssData>? GnssDatas { get; private set; }

    #endregion Public Properties

    #region Public Methods

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
                    stdInitR_n: 0.009,
                    stdInitR_e: 0.008,
                    stdInitR_d: 0.022,
                    stdInitV_n: 0,
                    stdInitV_e: 0,
                    stdInitV_d: 0,
                    stdInitPhi_n: FromDegrees(0),
                    stdInitPhi_e: FromDegrees(0),
                    stdInitPhi_d: FromDegrees(0),
                    gnssLeverArm: new(0.235, 0.1, 0.89),
                    imuErrorModel: new(
                        arw: 0.2 * RadiansPerDegree / 60,
                        vrw: 0.4 / 60,
                        stdAccBias: 400E-5,
                        stdAccScale: 1000E-6,
                        stdGyroBias: 24 * RadiansPerDegree / 3600,
                        stdGyroScale: 1000E-6,
                        cotAccBias: 3600,
                        cotAccScale: 3600,
                        cotGyroBias: 3600,
                        cotGyroScale: 3600)
                );
        return new(initTime, new(initTime, initLocation, initVelocity, initOrientation), options, ImuDatas, GnssDatas);
    }

    #endregion Public Methods

}
