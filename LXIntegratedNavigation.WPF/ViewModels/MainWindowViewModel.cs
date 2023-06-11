using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using LXIntegratedNavigation.WPF.Models;
using Microsoft.Extensions.DependencyInjection;
using Syncfusion.Windows.Tools.Controls;

namespace LXIntegratedNavigation.WPF.ViewModels;

internal partial class MainWindowViewModel : WindowViewModel
{
    #region Public Constructors    
    [ObservableProperty]
    ObservableCollection<DockItem> _dockItems = new();

    public MainWindowViewModel(Window window) : base(window)
    {
        DockItems.Add(new()
        {
            Header = "属性",
            CanClose = false,
            DesiredHeightInDockedMode = 200,
            DesiredWidthInDockedMode = 200,
            SideInDockedMode = DockSide.Right
        });
        DockItems.Add(new()
        {
            Header = "输出",
            CanClose = false,
            CanMaximize = false,
            CanMinimize = false,
            DesiredHeightInDockedMode = 200,
            DesiredWidthInDockedMode = 200,
            SideInDockedMode = DockSide.Bottom,
            Content = LogPage.Instance
        });
        DockItems.Add(new()
        {
            CanClose = false,
            Header = "开始",
            State = DockState.Document,
            Content = StartPage.Instance
        });

        WeakReferenceMessenger.Default.Register<NavigationData, string>(this, "NavigationResult", (recipient, message) =>
        {
            if (message.NaviPoses is null)
                return;
            var viewModel = new TrajectoryPageViewModel(message.NaviPoses);
            var page = new TrajectoryPage(viewModel);
            DockItems.Add(new()
            {
                CanClose = false,
                Header = "轨迹",
                State = DockState.Document,
                Content = page
            });
        });
    }

    #endregion Public Constructors


}
