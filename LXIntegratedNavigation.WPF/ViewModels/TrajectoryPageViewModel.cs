using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using LXIntegratedNavigation.Shared.Models;
using LXIntegratedNavigation.WPF.Models;

namespace LXIntegratedNavigation.WPF.ViewModels;

public partial class TrajectoryPageViewModel : ObservableObject
{
    [ObservableProperty]
    ObservableCollection<NaviPoseViewModel> _poses;

    [ObservableProperty]
    double _interval = 1;

    public TrajectoryPageViewModel(List<NaviPose> poses)
    {
        _poses = new();
        for (int i = 0; i < poses.Count; i++)
            _poses.Add(new(poses[i], poses[0]));
    }


}
