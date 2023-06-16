using System;
using System.Collections.Generic;
using LXIntegratedNavigation.Shared.Essentials.Navigation;
using LXIntegratedNavigation.Shared.Models;
using NaviSharp;

namespace LXIntegratedNavigation.WPF.Models;

public class NavigationData
{
    #region Public Constructors

    public NavigationData(GpsTime initTime, NaviPose initPose, LooseCombinationOptions options, IEnumerable<ImuData> imuDatas, IEnumerable<GnssData> gnssDatas)
    {
        InitTime = initTime;
        InitPose = initPose;
        Options = options;
        ImuDatas = new(imuDatas);
        GnssDatas = new(gnssDatas);
    }

    #endregion Public Constructors

    #region Public Properties

    public GpsTime InitTime { get; init; }
    public NaviPose InitPose { get; init; }

    public LooseCombinationOptions Options { get; init; }

    public List<ImuData> ImuDatas { get; init; }
    public List<GnssData> GnssDatas { get; init; }

    public List<NaviPose>? NaviPoses { get; set; }

    public IEnumerable<(double X, double Y)> GetTrajectoryData()
    {
        if (NaviPoses is null)
            throw new InvalidOperationException();
        var initPose = NaviPoses[0];
        for (var i = 1; i < NaviPoses.Count; i++)
        {
            var x = (NaviPoses[i].L - initPose.L) * 6371000;
            var y = (NaviPoses[i].B - initPose.B) * 6371000;
            yield return new(x, y);
        }
    }


    #endregion Public Properties
}
