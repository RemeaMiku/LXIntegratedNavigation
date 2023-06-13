using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Win32;
using Syncfusion.UI.Xaml.Charts;

namespace LXIntegratedNavigation.WPF.Views
{
    /// <summary>
    /// TrajectoryPage.xaml 的交互逻辑
    /// </summary>
    public partial class TrajectoryPage : UserControl
    {
        public static TrajectoryPage Instance => Current.Services.GetRequiredService<TrajectoryPage>();
        public TrajectoryPageViewModel ViewModel { get; init; }
        public TrajectoryPage(TrajectoryPageViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            DataContext = this;
        }

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



        private void DisplayButton_Click(object sender, RoutedEventArgs e)
        {

            //if (ViewModel.Poses.Count == 0)
            //    return;
            //ViewModel.DisplayPose.Add(ViewModel.Poses[0]);
            //await Task.Run(() =>
            //{
            //    for (int i = 0; i < ViewModel.Poses.Count - 1; i++)
            //    {
            //        var timeSpan = ViewModel.Poses[i + 1].TimeSpan - ViewModel.DisplayPose[0].TimeSpan;
            //        Thread.Sleep(timeSpan);
            //        ViewModel.DisplayPose[0] = ViewModel.Poses[i + 1];
            //    }
            //});
        }
    }
}
