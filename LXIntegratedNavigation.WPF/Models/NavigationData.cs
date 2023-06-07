using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LXIntegratedNavigation.Shared.Essentials.Navigation;
using LXIntegratedNavigation.Shared.Models;
using NaviSharp;

namespace LXIntegratedNavigation.WPF.Models;

public class NavigationData
{
    public GpsTime InitTime { get; init; }
    public NaviPose InitPose { get; init; }

    public LooseCombinationOptions Options { get; init; }

    public List<ImuData> ImuDatas { get; init; }
    public List<GnssData> GnssDatas { get; init; }

    public List<NaviPose>? NaviPoses { get; set; }

    public NavigationData(GpsTime initTime, NaviPose initPose, LooseCombinationOptions options, IEnumerable<ImuData> imuDatas, IEnumerable<GnssData> gnssDatas)
    {
        InitTime = initTime;
        InitPose = initPose;
        Options = options;
        ImuDatas = new(imuDatas);
        GnssDatas = new(gnssDatas);
    }
}
