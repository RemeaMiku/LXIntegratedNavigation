using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Win32;
using Syncfusion.UI.Xaml.Charts;

namespace LXIntegratedNavigation.WPF.Views
{
    /// <summary>
    /// ChartPage.xaml 的交互逻辑
    /// </summary>
    public partial class ChartPage : UserControl
    {
        #region Public Constructors

        public ChartPage(ChartPageViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            DataContext = this;
        }

        #endregion Public Constructors

        #region Public Properties

        public ChartPageViewModel ViewModel { get; }

        #endregion Public Properties

        #region Private Methods

        private void SfChart_SelectionChanged(object sender, ChartSelectionChangedEventArgs e)
        {
            if (e.SelectedIndex == -1)
                return;
            var pose = ViewModel.Poses[e.SelectedIndex];
            WeakReferenceMessenger.Default.Send(pose, "SelectedPose");
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog
            {
                Filter = "Bitmap(*.bmp)|*.bmp|JPEG(*.jpg,*.jpeg)|*.jpg;*.jpeg|Gif (*.gif)|*.gif|PNG(*.png)|*.png|TIFF(*.tif,*.tiff)|*.tif|All files (*.*)|*.*"
            };
            if (sfd.ShowDialog() == true)
            {
                using Stream fs = sfd.OpenFile();
                Chart.Save(fs, new PngBitmapEncoder());
            }
        }

        #endregion Private Methods
    }
}
