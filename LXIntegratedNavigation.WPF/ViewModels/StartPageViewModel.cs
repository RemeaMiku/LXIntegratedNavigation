using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Wpf.Ui.Controls;

namespace LXIntegratedNavigation.WPF.ViewModels;

internal partial class StartPageViewModel : ObservableObject
{
    private readonly StartPage _page;
    public StartPageViewModel(StartPage startPage)
    {
        _page = startPage;
    }
    [ObservableProperty]
    string _imuFilePath = string.Empty;
    [RelayCommand]
    void OpenImuFile()
    {
        var dialog = new OpenFileDialog()
        {
            Title = "打开IMU文件",
            Filter = "NovatelASC文件(*.ASC)|*.ASC",
            CheckFileExists = true,
            CheckPathExists = true,
            ReadOnlyChecked = true,
        };
        if (dialog.ShowDialog() == true)
        {
            ImuFilePath = dialog.FileName;
        }
    }

}
