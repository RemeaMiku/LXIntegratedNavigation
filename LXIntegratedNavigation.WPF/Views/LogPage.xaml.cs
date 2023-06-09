using System;
using System.Collections.Generic;
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

    private bool _autoScroll = true;

    private void ScrollViewer_ScrollChanged(Object sender, ScrollChangedEventArgs e)
    {
        // User scroll event : set or unset auto-scroll mode
        if (e.ExtentHeightChange == 0)
        {   // Content unchanged : user scroll event
            if (ScrollViewer.VerticalOffset == ScrollViewer.ScrollableHeight)
            {   // Scroll bar is in bottom
                // Set auto-scroll mode
                _autoScroll = true;
            }
            else
            {   // Scroll bar isn't in bottom
                // Unset auto-scroll mode
                _autoScroll = false;
            }
        }

        // Content scroll event : auto-scroll eventually
        if (_autoScroll && e.ExtentHeightChange != 0)
        {   // Content changed and auto-scroll mode set
            // Autoscroll
            ScrollViewer.ScrollToVerticalOffset(ScrollViewer.ExtentHeight);
        }
    }
}
