using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace LXIntegratedNavigation.WPF.ViewModels;

public partial class PropertyPageViewModel : ObservableObject
{
    [ObservableProperty]
    public NaviPoseViewModel? _pose;

    [ObservableProperty]
    int _selectedIndex = 0;

    public ObservableCollection<string> Items { get; } = new()
    {
        "纬度B(°)",
        "经度L(°)",
        "大地高H(m)",
        "北向速度VN(m/s)",
        "东向速度VE(m/s)",
        "垂向速度VD(m/s)",
        "航向Yaw(°)",
        "俯仰Pitch(°)",
        "横滚Roll(°)",
        "ECEF-X(m)",
        "ECEF-Y(m)",
        "ECEF-Z(m)",
        "垂向速度VU(m/s)",
        "东向距离RE(m)",
        "北向距离RN(m)",
        "垂向距离RU(m)"
    };
    [RelayCommand]
    void CreateChart()
    {
        WeakReferenceMessenger.Default.Send(Items[SelectedIndex], "ChartTitle");
    }

    public PropertyPageViewModel()
    {
        WeakReferenceMessenger.Default.Register<NaviPoseViewModel, string>(this, "SelectedPose", (recipient, message) =>
        {
            Pose = message;
        });
    }
}
