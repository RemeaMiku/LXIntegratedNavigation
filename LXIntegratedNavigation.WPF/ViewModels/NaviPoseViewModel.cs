using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using LXIntegratedNavigation.Shared.Models;

namespace LXIntegratedNavigation.WPF.ViewModels;

public class NaviPoseViewModel : ObservableObject
{
    public NaviPose Pose { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
    public NaviPoseViewModel(NaviPose pose, NaviPose initPose)
    {
        Pose = pose;
        X = (float)(pose.L - initPose.L) * 6371000;
        Y = (float)(pose.B - initPose.B) * 6371000;
    }
}
