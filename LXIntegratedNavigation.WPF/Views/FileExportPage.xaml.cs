using System.Windows.Controls;

namespace LXIntegratedNavigation.WPF.Views
{
    /// <summary>
    /// FileExportPage.xaml 的交互逻辑
    /// </summary>
    public partial class FileExportPage : UserControl
    {
        public static FileExportPage Instance => Current.Services.GetRequiredService<FileExportPage>();

        public FileExportPageViewModel ViewModel { get; init; }

        public FileExportPage(FileExportPageViewModel viewModel)
        {
            InitializeComponent();
            DataContext = this;
            ViewModel = viewModel;
        }
    }
}
