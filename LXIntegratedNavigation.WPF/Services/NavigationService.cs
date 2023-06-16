using System;
using System.Linq;
using System.Threading.Tasks;
using LXIntegratedNavigation.Shared.Essentials.Navigation;
using LXIntegratedNavigation.Shared.Essentials.NormalGravityModel;
using LXIntegratedNavigation.WPF.Models;

namespace LXIntegratedNavigation.WPF.Services;

public class NavigationService
{
    #region Public Properties

    public InertialNavigation Ins { get; } = new(new Grs80NormalGravityModel());

    #endregion Public Properties

    #region Public Methods

    public async Task<NavigationData> LooseCombinationAsync(NavigationData data, IProgress<int>? progress = null)
    {
        // Run the calculations on a background thread and await the result
        data.NaviPoses = await Task.Run(() =>
        {
            var lc = new LooseCombination(Ins, data.Options);
            return lc.Solve(data.InitPose, data.ImuDatas, data.GnssDatas, data.InitTime, progress).ToList();
        });

        // Write the poses on the UI thread
        //WritePoses(GetPathAtDesktop($"InsResult_{DateTime.Now:yyMMddHHmmss}.csv"), data.NaviPoses);

        // Return the data
        return data;
    }

    #endregion Public Methods


}
