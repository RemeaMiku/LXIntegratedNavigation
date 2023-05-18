using System.Windows.Controls;

namespace LXIntegratedNavigation.WPF.Views
{
    /// <summary>
    /// HomePage.xaml 的交互逻辑
    /// </summary>
    public partial class HomePage : Page
    {
        public HomePage()
        {
            InitializeComponent();
        }
        private static HomePage? _instance;
        public static HomePage Instance
        {
            get
            {
                _instance ??= new HomePage();
                return _instance;
            }
        }
    }
}
