﻿using System;
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