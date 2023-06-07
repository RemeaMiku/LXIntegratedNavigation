using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Xps.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LXIntegratedNavigation.Shared.Essentials.Navigation;
using LXIntegratedNavigation.Shared.Models;
using LXIntegratedNavigation.WPF.Converters;
using LXIntegratedNavigation.WPF.Models;
using LXIntegratedNavigation.WPF.Services;
using Microsoft.Win32;
using NaviSharp;
using Wpf.Ui.Controls;
using Wpf.Ui.Controls.Interfaces;
using Wpf.Ui.Mvvm.Contracts;
using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;

namespace LXIntegratedNavigation.WPF.ViewModels;

#pragma warning disable 8603

public partial class StartPageViewModel : ObservableValidator, IProgress<int>
{
    readonly NavigationService _navigationService;
    readonly DataService _dataService;
    readonly ISnackbarService _snackbarService;
    public StartPageViewModel(ISnackbarService snackbarService, DataService dataService, NavigationService navigationService)
    {
        _snackbarService = snackbarService;
        _dataService = dataService;
        _navigationService = navigationService;
    }
#if DEBUG
    string _imuFilePath = "D:\\RemeaMiku study\\course in progress\\2023大三实习\\友谊广场0511\\ProcessedData\\wide_Rover\\20230511_wide_imu.ASC";
    string _gnssFilePath = "D:\\onedrive\\文档\\Tencent Files\\1597638582\\FileRecv\\wide.pos";
    double _imuInterval = 0.01;
    string _initTimeText = string.Empty;
    string _initLocationText = "30.5278108404, 114.3557126448, 22.312";
    string _initVelocityText = string.Empty;
    string _initOrientationText = string.Empty;
    double _staticDuration = 300;
#endif
#if RELEASE
    string _imuFilePath = string.Empty;
    string _gnssFilePath = string.Empty;
    double _imuInterval = 0.01;
    string _initTimeText = string.Empty;
    string _initLocationText = string.Empty;
    string _initVelocityText = string.Empty;
    string _initOrientationText = string.Empty;
    double _staticDuration = 0;
#endif
    [CustomValidation(typeof(StartPageViewModel), nameof(ValidateFilePath))]
    public string ImuFilePath
    {
        get => _imuFilePath;
        set => SetProperty(ref _imuFilePath, value, true);
    }
    [CustomValidation(typeof(StartPageViewModel), nameof(ValidateFilePath))]
    public string GnssFilePath
    {
        get => _gnssFilePath;
        set => SetProperty(ref _gnssFilePath, value, true);
    }

    [Range(0.0001, 1, ErrorMessage = "必须在0.0001到1范围内")]
    public double ImuInterval
    {
        get => _imuInterval;
        set => SetProperty(ref _imuInterval, value, true);
    }

    public static ValidationResult ValidateFilePath(string imuFilePath)
    {
        if (string.IsNullOrEmpty(imuFilePath))
            return new ValidationResult("不能为空");
        try
        {
            var isvalid = new FileInfo(imuFilePath).Exists;
            if (isvalid)
            {
                return ValidationResult.Success;
            }
            return new ValidationResult("路径非法");
        }
        catch (Exception)
        {
            return new ValidationResult("路径非法");
        }
    }

    [CustomValidation(typeof(StartPageViewModel), nameof(ValidateGpsTime))]
    public string InitTimeText
    {
        get => _initTimeText;
        set => SetProperty(ref _initTimeText, value, true);
    }
    public static ValidationResult ValidateGpsTime(string str)
    {
        if (string.IsNullOrEmpty(str) || GpsTime.TryParse(str, null, out _))
            return ValidationResult.Success;
        return new ValidationResult("格式有误");
    }
    public static ValidationResult ValidateLocation(string str)
    {
        if (string.IsNullOrEmpty(str))
            return new ValidationResult("不能为空");
        if (GeodeticCoord.TryParse(str, null, out _))
            return ValidationResult.Success;
        return new ValidationResult("格式有误");
    }

    [CustomValidation(typeof(StartPageViewModel), nameof(ValidateLocation))]
    public string InitLocationText
    {
        get => _initLocationText;
        set => SetProperty(ref _initLocationText, value, true);
    }

    [CustomValidation(typeof(StartPageViewModel), nameof(ValidateVelocity))]
    public string InitVelocityText
    {
        get => _initVelocityText;
        set => SetProperty(ref _initVelocityText, value, true);
    }

    public static ValidationResult ValidateVelocity(string str)
    {
        if (string.IsNullOrEmpty(str))
            return ValidationResult.Success;
        if (Vector.TryParse(str, null, out var vel) && vel.IsSizeOf(3))
        {
            return ValidationResult.Success;
        }
        return new ValidationResult("格式有误");
    }
    [CustomValidation(typeof(StartPageViewModel), nameof(ValidateOrientation))]
    public string InitOrientationText
    {
        get => _initOrientationText;
        set => SetProperty(ref _initOrientationText, value, true);
    }

    public static ValidationResult ValidateOrientation(string str)
    {
        if (string.IsNullOrEmpty(str))
            return ValidationResult.Success;
        if (EulerAngles.TryParse(str, null, out _))
        {
            return ValidationResult.Success;
        }
        return new ValidationResult("格式有误");
    }

    [Range(0, double.MaxValue, ErrorMessage = $"必须>=0")]
    public double StaticDuration
    {
        get => _staticDuration;
        set => SetProperty(ref _staticDuration, value, true);
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFromTextBox))]
    bool _isFromStaticAlignment = true;
    [ObservableProperty]
    double? _staticSeconds;
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
    void OpenGnssFile()
    {
        var dialog = new OpenFileDialog()
        {
            Title = "打开GNSS文件",
            CheckFileExists = true,
            CheckPathExists = true,
            ReadOnlyChecked = true,
        };
        if (dialog.ShowDialog() == true)
        {
            GnssFilePath = dialog.FileName;
        }
    }

    [RelayCommand]
    async Task StartAsync()
    {
        ValidateAllProperties();
        if (HasErrors)
        {
            _snackbarService.Show("错误", "填写存在错误", SymbolRegular.ErrorCircle24, ControlAppearance.Danger);
            return;
        }
        if (!await _dataService.InitializeImuDatasAsync(ImuFilePath, TimeSpan.FromSeconds(ImuInterval)))
        {
            _snackbarService.Show("错误", "IMU文件读取出错", SymbolRegular.ErrorCircle24, ControlAppearance.Danger);
            return;
        }
        if (!await _dataService.InitializeGnssDatasAsync(GnssFilePath))
        {
            _snackbarService.Show("错误", "GNSS文件读取出错", SymbolRegular.ErrorCircle24, ControlAppearance.Danger);
            return;
        }
        if (_dataService.ImuDatas is null || _dataService.GnssDatas is null)
        {
            MessageBox.Show("程序内部发生了一个错误", "错误", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
            return;
        }
        _snackbarService.Show("提示", "计算开始", SymbolRegular.Info28, ControlAppearance.Info);
        var initTime = string.IsNullOrEmpty(InitTimeText) ? _dataService.ImuDatas.First().TimeStamp : GpsTime.Parse(InitTimeText);
        var initLocation = GeodeticCoord.Parse(InitLocationText);
        var initVelocity = string.IsNullOrEmpty(InitVelocityText) ? new(3) : Vector.Parse(InitVelocityText);
        var initOrientation = new Orientation(default(EulerAngles));
        if (IsFromStaticAlignment)
        {
            var span = TimeSpan.FromSeconds(StaticDuration);
            var staticImuDatas = _dataService.ImuDatas.Where(d => d.TimeStamp >= initTime && d.TimeStamp < initTime + span).ToList();
            initOrientation = _navigationService.Ins.StaticAlignment(initLocation, staticImuDatas);
            initTime += span;
        }
        var naviData = _dataService.GetNavigationData(initTime, initLocation, initVelocity, initOrientation);
        naviData = await _navigationService.LooseCombinationAsync(naviData, this);
        _snackbarService.Show("成功", "计算完成", SymbolRegular.CheckmarkCircle48, ControlAppearance.Success);
    }
    [ObservableProperty]
    double _progress = 0;
    public void Report(int value)
    {
        Progress = value;
    }
}