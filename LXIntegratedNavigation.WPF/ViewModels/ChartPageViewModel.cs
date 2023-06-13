using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace LXIntegratedNavigation.WPF.ViewModels;

public partial class ChartPageViewModel : ObservableObject
{
    public ObservableCollection<NaviPoseViewModel> Poses { get; }
    [ObservableProperty]
    string _title = string.Empty;
    [ObservableProperty]
    string _xPath = "TimeSpan";
    [ObservableProperty]
    string _yPath;
    public ChartPageViewModel(ObservableCollection<NaviPoseViewModel> poses, string title)
    {
        Poses = poses;
        Title = title;
        _yPath = NaviPoseViewModel.ItemToPropertyNamePairs[title].Name;
    }
}
