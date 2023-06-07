using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LXIntegratedNavigation.Shared.Essentials.Navigation;
using LXIntegratedNavigation.Shared.Essentials.NormalGravityModel;
using LXIntegratedNavigation.Shared.Models;
using LXIntegratedNavigation.WPF.Models;
using NaviSharp;

namespace LXIntegratedNavigation.WPF.Services;

public class NavigationService
{
    public InertialNavigation Ins { get; } = new(new Grs80NormalGravityModel());

    public async Task<NavigationData> LooseCombinationAsync(NavigationData data)
    {
        // Run the calculations on a background thread and await the result
        data.NaviPoses = await Task.Run(() =>
        {
            var lc = new LooseCombination(Ins, data.Options);
            return lc.Solve(data.InitPose, data.ImuDatas, data.GnssDatas, data.InitTime).ToList();
        });
#if DEBUG
        // Write the poses on the UI thread
        WritePoses(GetPathAtDesktop($"InsResult_{DateTime.Now:yyMMddHHmmss}.csv"), data.NaviPoses);
#endif
        // Return the data
        return data;
    }

}
