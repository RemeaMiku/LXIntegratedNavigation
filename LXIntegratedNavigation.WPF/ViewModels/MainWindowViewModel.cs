using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LXIntegratedNavigation.WPF.Models;
using LXIntegratedNavigation.WPF.Services;
using Syncfusion.Windows.Tools.Controls;
using Wpf.Ui.Mvvm.Contracts;

namespace LXIntegratedNavigation.WPF.ViewModels;

public partial class MainWindowViewModel : WindowViewModel
{
    #region Public Constructors
    [ObservableProperty]
    ObservableCollection<DockItem> _dockItems = new()
    {
       HeaderItemPairs["开始"],
       HeaderItemPairs["输出"],
       HeaderItemPairs["属性"]
    };

    ObservableCollection<NaviPoseViewModel>? _poses;

    public static Dictionary<string, DockItem> HeaderItemPairs { get; } = new()
    {
        {
            "开始",
            new()
            {
                Header = "开始",
                State = DockState.Document,
                CanMaximize = true,
                Content = StartPage.Instance
            }
        },
        {
            "输出",
            new()
            {
                Header = "输出",
                CanClose = false,
                CanMaximize = false,
                CanMinimize = false,
                DesiredHeightInDockedMode = 200,
                DesiredWidthInDockedMode = 200,
                SideInDockedMode = DockSide.Bottom,
                Content = LogPage.Instance
            }
        },
        {
            "属性",
            new()
            {
                Header = "属性",
                CanClose = false,
                DesiredHeightInDockedMode = 200,
                DesiredWidthInDockedMode = 200,
                SideInDockedMode = DockSide.Right,
                Content=PropertyPage.Instance
            }
        },
        {
            "导出",
            new()
            {
                Header = "导出",
                State = DockState.Document,
                Content = FileExportPage.Instance
            }
        },
        {
            "轨迹",
            new()
            {
                Header = "轨迹",
                State = DockState.Document,
                Content = TrajectoryPage.Instance
            }
        }
    };
    [RelayCommand]
    void NavigateTo(string header)
    {
        var item = HeaderItemPairs[header];
        if (DockItems.Contains(item))
        {
            item.State = DockState.Document;
            return;
        }
        DockItems.Add(item);
    }

    readonly ISnackbarService _snackbarService;
    readonly LogService _logService;

    public MainWindowViewModel(ISnackbarService snackbarService, LogService logService, Window window) : base(window)
    {
        _snackbarService = snackbarService;
        _logService = logService;
        WeakReferenceMessenger.Default.Register<NavigationData, string>(this, "NavigationResult", (recipient, message) =>
        {
            if (message.NaviPoses is null)
                return;
            _poses = NaviPoseViewModel.FromNaviPoses(message.NaviPoses);
            TrajectoryPage.Instance.ViewModel.Poses = _poses;
            FileExportPage.Instance.ViewModel.Poses = _poses;
        });
        WeakReferenceMessenger.Default.Register<string, string>(this, "ChartTitle", (recipient, message) =>
        {
            if (_poses is null)
            {
                _snackbarService.Show("错误", "尚无数据可绘制", SymbolRegular.Dismiss24, ControlAppearance.Danger);
                _logService.Send(LogType.Error, "尚无数据可绘制");
                return;
            }
            var viewModel = new ChartPageViewModel(_poses, message);
            DockItems.Add(new()
            {
                Header = message,
                State = DockState.Document,
                Content = new ChartPage(viewModel)
            });
        });
    }

    #endregion Public Constructors


}
