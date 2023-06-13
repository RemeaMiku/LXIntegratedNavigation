using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LXIntegratedNavigation.Shared.Models;
using LXIntegratedNavigation.WPF.Models;
using LXIntegratedNavigation.WPF.Services;

namespace LXIntegratedNavigation.WPF.ViewModels;

public partial class TrajectoryPageViewModel : ObservableObject
{
    [ObservableProperty]
    ObservableCollection<NaviPoseViewModel> _poses = new();

    [ObservableProperty]
    ObservableCollection<NaviPoseViewModel> _displayPose = new();
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    bool _isBusy = false;

    public double DisplayVelocity => DisplayPose.Count == 0 ? 0 : DisplayPose[0].V * 3.6;
    public double DisplayYaw => DisplayPose.Count == 0 ? 0 : DisplayPose[0].Yaw;

    public double DisplayRedPointer => -DisplayYaw;

    public double DisplayWhitePointer => DisplayRedPointer >= 0 ? DisplayRedPointer - 180 : DisplayRedPointer + 180;

    public bool IsNotBusy => !IsBusy;

    [RelayCommand]
    async void Display()
    {
        if (Poses.Count == 0)
            return;
        IsBusy = true;
        DisplayPose.Add(Poses[0]);
        await Task.Run(() =>
        {
            for (var i = 1; i < Poses.Count; i++)
            {
                var span = Poses[i].TimeSpan - DisplayPose[0].TimeSpan;
                if (span >= TimeSpan.FromSeconds(1))
                {
                    Thread.Sleep(TimeSpan.FromSeconds(0.1));
                    Current.Dispatcher.BeginInvoke(() =>
                    {
                        if (i < Poses.Count)
                        {
                            DisplayPose[0] = Poses[i];
                            OnPropertyChanged(nameof(DisplayVelocity));
                            OnPropertyChanged(nameof(DisplayRedPointer));
                            OnPropertyChanged(nameof(DisplayWhitePointer));
                        }
                    });
                }
            }
        });
        DisplayPose.Clear();
        IsBusy = false;
    }
}
