using System.Windows.Controls;

namespace LXIntegratedNavigation.WPF.Views
{
    /// <summary>
    /// PropertyPage.xaml 的交互逻辑
    /// </summary>
    public partial class PropertyPage : UserControl
    {
        public static PropertyPage Instance => Current.Services.GetRequiredService<PropertyPage>();

        public PropertyPageViewModel ViewModel { get; init; }
        public PropertyPage(PropertyPageViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            DataContext = this;
        }
    }
}
