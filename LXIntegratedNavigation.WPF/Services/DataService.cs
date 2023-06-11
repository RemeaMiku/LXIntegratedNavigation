using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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

    public List<NavigationData>? NavigationDatas { get; private set; }

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

    public NavigationData GetNavigationData(GpsTime initTime, GeodeticCoord initLocation, Vector initVelocity, Orientation initOrientation, LooseCombinationOptions options)
    {
        if (ImuDatas is null || GnssDatas is null)
            throw new InvalidOperationException();
        var data = new NavigationData(initTime, new(initTime, initLocation, initVelocity, initOrientation), options, ImuDatas, GnssDatas);
        NavigationDatas ??= new();
        NavigationDatas.Add(data);
        return data;
    }

    #endregion Public Methods

}
