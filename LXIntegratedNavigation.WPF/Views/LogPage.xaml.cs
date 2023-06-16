using System;
using System.Windows.Controls;

namespace LXIntegratedNavigation.WPF.Views;

/// <summary>
/// LogPage.xaml 的交互逻辑
/// </summary>
public partial class LogPage : UserControl
{
    public static LogPage Instance => Current.Services.GetService<LogPage>() ?? throw new NullReferenceException();

    public LogPageViewModel ViewModel { get; }

    public LogPage(LogPageViewModel viewModel)
    {
        InitializeComponent();
        DataContext = this;
        ViewModel = viewModel;
    }

}
