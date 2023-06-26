using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LXIntegratedNavigation.Shared.Essentials.Navigation;
using LXIntegratedNavigation.Shared.Models;
using LXIntegratedNavigation.WPF.Models;
using LXIntegratedNavigation.WPF.Services;
using Microsoft.Win32;
using NaviSharp;
using Wpf.Ui.Mvvm.Contracts;
using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;

namespace LXIntegratedNavigation.WPF.ViewModels;

#pragma warning disable 8603

public partial class StartPageViewModel : ObservableValidator, IProgress<int>
{
    readonly NavigationService _navigationService;
    readonly DataService _dataService;
    readonly ISnackbarService _snackbarService;
    readonly LogService _logService;
    public StartPageViewModel(ISnackbarService snackbarService, LogService logService, DataService dataService, NavigationService navigationService)
    {
        _snackbarService = snackbarService;
        _logService = logService;
        _dataService = dataService;
        _navigationService = navigationService;
    }

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "不能为空")]
    [CustomValidation(typeof(StartPageViewModel), nameof(ValidateFilePath))]
    string _imuFilePath = "D:\\RemeaMiku study\\course in progress\\2023大三实习\\友谊广场0511\\ProcessedData\\wide_Rover\\20230511_wide_imu.ASC";
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "不能为空")]
    [CustomValidation(typeof(StartPageViewModel), nameof(ValidateFilePath))]
    string _gnssFilePath = "D:\\onedrive\\文档\\Tencent Files\\1597638582\\FileRecv\\wide.pos";
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Range(0.0001, 1, ErrorMessage = "必须在0.0001到1范围内")]
    double _imuInterval = 0.01;
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [CustomValidation(typeof(StartPageViewModel), nameof(ValidateGpsTime))]
    string _initTimeText = string.Empty;
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "不能为空")]
    [CustomValidation(typeof(StartPageViewModel), nameof(ValidateLocation))]
    string _initLocationText = "30.5278108948,114.3557126173,22.321";
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(AllowEmptyStrings = true)]
    [CustomValidation(typeof(StartPageViewModel), nameof(ValidateVector3d))]
    string _initVelocityText = string.Empty;
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [CustomValidation(typeof(StartPageViewModel), nameof(ValidateOrientation))]
    string _initOrientationText = string.Empty;
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Range(0, double.MaxValue, ErrorMessage = "必须>=0")]
    double _staticDuration = 300;
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Range(0, double.MaxValue, ErrorMessage = "必须>=0")]
    double _arw = 0.2 * RadiansPerDegree / 60;
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Range(0, double.MaxValue, ErrorMessage = "必须>=0")]
    double _vrw = 0.4 / 60;
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Range(0, double.MaxValue, ErrorMessage = "必须>=0")]
    double _stdAccBias = 400E-5;
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Range(0, double.MaxValue, ErrorMessage = "必须>=0")]
    double _stdGyroBias = 24 * RadiansPerDegree / 3600;
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Range(0, double.MaxValue, ErrorMessage = "必须>=0")]
    double _stdAccScale = 1000E-6;
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Range(0, double.MaxValue, ErrorMessage = "必须>=0")]
    double _stdGyroScale = 1000E-6;
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Range(0, double.MaxValue, ErrorMessage = "必须>=0")]
    double _cotAccBias = 3600;
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Range(0, double.MaxValue, ErrorMessage = "必须>=0")]
    double _cotGyroBias = 3600;
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Range(0, double.MaxValue, ErrorMessage = "必须>=0")]
    double _cotAccScale = 3600;
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Range(0, double.MaxValue, ErrorMessage = "必须>=0")]
    double _cotGyroScale = 3600;
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "不能为空")]
    [CustomValidation(typeof(StartPageViewModel), nameof(ValidateVector3d))]
    string _stdInitRText = "0.1,0.1,0.1";
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "不能为空")]
    [CustomValidation(typeof(StartPageViewModel), nameof(ValidateVector3d))]
    string _stdInitVText = "0,0,0";
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "不能为空")]
    [CustomValidation(typeof(StartPageViewModel), nameof(ValidateVector3d))]
    string _stdInitPhiText = "0.05,0.05,0.05";
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(AllowEmptyStrings = false, ErrorMessage = "不能为空")]
    [CustomValidation(typeof(StartPageViewModel), nameof(ValidateVector3d))]
    string _gnssLeverArmText = "-0.1000,0.2350,-0.8510";

    public static ValidationResult ValidateFilePath(string filePath)
    {
        try
        {
            var isvalid = new FileInfo(filePath).Exists;
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


    public static ValidationResult ValidateGpsTime(string str)
    {
        if (string.IsNullOrEmpty(str) || GpsTime.TryParse(str, null, out _))
            return ValidationResult.Success;
        return new ValidationResult("格式有误");
    }
    public static ValidationResult ValidateLocation(string str)
    {
        if (GeodeticCoord.TryParse(str, null, out _))
            return ValidationResult.Success;
        return new ValidationResult("格式有误");
    }


    public static ValidationResult ValidateVector3d(string str)
    {
        if (string.IsNullOrEmpty(str))
            return ValidationResult.Success;
        if (Vector.TryParse(str, null, out var vel) && vel.IsSizeOf(3))
        {
            return ValidationResult.Success;
        }
        return new ValidationResult("格式有误");
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
            Filter = "PosMind Pos文件(*.pos)|*.pos",
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
        _logService.Send(LogType.Info, "开始核验表单");
        ValidateAllProperties();
        if (HasErrors)
        {
            _snackbarService.Show("错误", "表单填写存在错误", SymbolRegular.Dismiss24, ControlAppearance.Danger);
            _logService.Send(LogType.Error, "表单填写存在错误，解算已取消");
            return;
        }
        _logService.Send(LogType.Info, $"开始读取IMU文件: {ImuFilePath}");
        if (!await _dataService.InitializeImuDatasAsync(ImuFilePath, TimeSpan.FromSeconds(ImuInterval)))
        {
            _snackbarService.Show("错误", "IMU文件读取出错", SymbolRegular.Dismiss24, ControlAppearance.Danger);
            _logService.Send(LogType.Error, $"读取 {ImuFilePath} 时出错，解算已取消");
            return;
        }
        _logService.Send(LogType.Info, $"开始读取GNSS结果文件: {GnssFilePath}");
        if (!await _dataService.InitializeGnssDatasAsync(GnssFilePath))
        {
            _snackbarService.Show("错误", "GNSS文件读取出错", SymbolRegular.Dismiss24, ControlAppearance.Danger);
            _logService.Send(LogType.Error, $"读取 {GnssFilePath} 时出错，解算已取消");
            return;
        }
        if (_dataService.ImuDatas is null || _dataService.GnssDatas is null)
        {
            _snackbarService.Show("错误", "未知原因", SymbolRegular.Dismiss24, ControlAppearance.Danger);
            _logService.Send(LogType.Error, "发生了一个未知错误，解算已取消");
            return;
        }
        _snackbarService.Show("提示", "解算开始", SymbolRegular.Info28, ControlAppearance.Info);
        _logService.Send(LogType.Info, "开始解算");
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
        var imuErrorModel = new ImuErrorModel(Arw, Vrw, StdAccBias, StdAccScale, StdGyroBias, StdGyroScale, CotAccBias, CotAccScale, CotGyroBias, CotGyroScale);
        var option = new LooseCombinationOptions(Vector.Parse(GnssLeverArmText), Vector.Parse(StdInitRText).Data, Vector.Parse(StdInitVText).Data, Vector.Parse(StdInitPhiText).Data, imuErrorModel);
        var naviData = _dataService.GetNavigationData(initTime, initLocation, initVelocity, initOrientation, option);
        try
        {
            naviData = await _navigationService.LooseCombinationAsync(naviData, this);
        }
        catch (Exception e)
        {
            _snackbarService.Show("错误", $"解算出错：{e.Message}", SymbolRegular.Dismiss24, ControlAppearance.Danger);
            return;
        }
        _snackbarService.Show("成功", "解算完成", SymbolRegular.CheckmarkCircle48, ControlAppearance.Success);
        _logService.Send(LogType.Info, "解算完毕");
        WeakReferenceMessenger.Default.Send(naviData, "NavigationResult");
    }
    [ObservableProperty]
    int _progress = 0;

    public void Report(int value)
        => Progress = value;
}