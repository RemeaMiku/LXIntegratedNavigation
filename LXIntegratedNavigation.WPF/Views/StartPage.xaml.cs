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

namespace LXIntegratedNavigation.WPF.Views
{
    /// <summary>
    /// StartPage.xaml 的交互逻辑
    /// </summary>
    public partial class StartPage : UserControl
    {
        #region Public Constructors

        public StartPageViewModel ViewModel { get; }

        public StartPage()
        {
            InitializeComponent();
            DataContext = Provider.StartPageViewModel;
            ViewModel = Provider.StartPageViewModel;
        }

        #endregion Public Constructors

        private void OrientationSwitch_Checked(object sender, RoutedEventArgs e)
        {
            ViewModel.InitOrientationText = string.Empty;
        }
    }
}
