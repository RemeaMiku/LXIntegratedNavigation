using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LXIntegratedNavigation.Shared.Essentials.NormalGravityModel;

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
