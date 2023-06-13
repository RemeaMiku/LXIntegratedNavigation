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
using LXIntegratedNavigation.WPF.Services;

namespace LXIntegratedNavigation.WPF.ViewModels;

public partial class TrajectoryPageViewModel : ObservableObject
{
    [ObservableProperty]
    ObservableCollection<NaviPoseViewModel> _poses = new();
}
