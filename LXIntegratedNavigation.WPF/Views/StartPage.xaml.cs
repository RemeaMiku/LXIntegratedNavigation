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
using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui.Controls;
using Wpf.Ui.Mvvm.Contracts;
using Wpf.Ui.Mvvm.Services;

namespace LXIntegratedNavigation.WPF.Views
{
    /// <summary>
    /// StartPage.xaml 的交互逻辑
    /// </summary>
    public partial class StartPage : UserControl
    {
        #region Public Constructors
        public static StartPage Instance => Current.Services.GetService<StartPage>() ?? throw new NullReferenceException();
        public StartPageViewModel ViewModel { get; }

        public StartPage(StartPageViewModel viewModel)
        {
            DataContext = viewModel;
            ViewModel = viewModel;
            InitializeComponent();
        }

        #endregion Public Constructors

        private void OrientationSwitch_Checked(object sender, RoutedEventArgs e)
        {
            ViewModel.InitOrientationText = string.Empty;
        }
    }
}
