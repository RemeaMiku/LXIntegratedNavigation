using System.Windows.Controls;

namespace LXIntegratedNavigation.WPF.Views
{
    /// <summary>
    /// StartPage.xaml 的交互逻辑
    /// </summary>
    public partial class StartPage : UserControl
    {
        #region Public Constructors
        public static StartPage Instance => Current.Services.GetRequiredService<StartPage>();
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
