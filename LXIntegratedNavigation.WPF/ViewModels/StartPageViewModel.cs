using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LXIntegratedNavigation.Shared.Models;
using LXIntegratedNavigation.WPF.Services;
using Microsoft.Win32;
using NaviSharp;
using Wpf.Ui.Controls;

namespace LXIntegratedNavigation.WPF.ViewModels;

public partial class StartPageViewModel : ObservableObject
{
    private readonly DataService _dataService;
    public StartPageViewModel(DataService dataService)
    {
        _dataService = dataService;
    }
    [ObservableProperty]
    string _imuFilePath = string.Empty;
    [ObservableProperty]
    string _imuIntervalText = string.Empty;
    [ObservableProperty]
    string _initTimeText = string.Empty;
    [ObservableProperty]
    string _initLocationText = string.Empty;
    [ObservableProperty]
    string _initVelocityText = string.Empty;
    [ObservableProperty]
    string _initOrientationText = string.Empty;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFromTextBox))]
    bool _isFromStaticAlignment = true;
    [ObservableProperty]
    int _selectedIndex = 0;


    public bool IsFromTextBox => !IsFromStaticAlignment;

    [RelayCommand]
    void OpenImuFile()
    {
        var dialog = new OpenFileDialog()
        {
            Title = "打开IMU文件",
            Filter = "Novatel ASC文件(*.ASC)|*.ASC",
            CheckFileExists = true,
            CheckPathExists = true,
            ReadOnlyChecked = true,
        };
        if (dialog.ShowDialog() == true)
        {
            ImuFilePath = dialog.FileName;
        }
    }
    [RelayCommand]
    async void StartAsync()
    {
        var interval = double.Parse(ImuIntervalText);

        await _dataService.InitializeImuDatas(ImuFilePath, TimeSpan.FromSeconds(interval));
        _dataService.InitTime = ParseInitTime();
        _dataService.InitLocation = ParseInitLocation();
        _dataService.InitVelocity = ParseInitVeclocity();
        _dataService.InitOrientation = ParseInitOrientation();
    }

    GpsTime ParseInitTime()
    {
        if (string.IsNullOrEmpty(InitTimeText))
            return _dataService.ImuDatas.First().TimeStamp;
        switch (SelectedIndex)
        {
            case 0:
                var values = InitTimeText.Split(',');
                var week = ushort.Parse(values[0]);
                var sow = double.Parse(values[1]);
                return new(week, sow);
            case 1:
                return GpsTime.FromUtc(UtcTime.ParseExact(InitTimeText, "yyyy/MM/dd HH:mm:ss.ffffff", null));
            default:
                throw new NotImplementedException();
        }
    }

    GeodeticCoord ParseInitLocation()
    {
        var values = InitLocationText.Split(",");
        var lat = double.Parse(values[0]);
        var lon = double.Parse(values[1]);
        var h = double.Parse(values[2]);
        return new(Angle.FromDegrees(lat), Angle.FromDegrees(lon), h);
    }
    Vector ParseInitVeclocity()
    {
        var values = InitVelocityText.Split(",");
        var v_n = double.Parse(values[0]);
        var v_e = double.Parse(values[1]);
        var v_d = double.Parse(values[2]);
        return new(v_n, v_e, v_d);
    }
    Orientation ParseInitOrientation()
    {
        var values = InitOrientationText.Split(",");
        var yaw = Angle.FromDegrees(double.Parse(values[0]));
        var pitch = Angle.FromDegrees(double.Parse(values[1]));
        var roll = Angle.FromDegrees(double.Parse(values[2]));
        return new(new EulerAngles(yaw, pitch, roll));
    }
}
