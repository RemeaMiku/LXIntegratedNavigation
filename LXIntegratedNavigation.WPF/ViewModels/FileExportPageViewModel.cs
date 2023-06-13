using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LXIntegratedNavigation.Shared.Models;
using LXIntegratedNavigation.WPF.Services;
using Microsoft.Win32;
using NaviSharp;
using Wpf.Ui.Mvvm.Contracts;

namespace LXIntegratedNavigation.WPF.ViewModels;

public partial class FileExportPageViewModel : ObservableObject
{
    [ObservableProperty]
    ObservableCollection<NaviPoseViewModel> _poses = new();

    readonly ISnackbarService _snackbarService;
    readonly LogService _logService;

    public FileExportPageViewModel(ISnackbarService snackbarService, LogService logService)
    {
        _snackbarService = snackbarService;
        _logService = logService;
    }

    [RelayCommand]
    void SaveResFile()
    {
        var dialog = new SaveFileDialog()
        {
            Title = "导出至",
            Filter = "逗号分隔符文件(*.csv)|*.csv",
            FileName = $"Res_{UtcTime.Now:yyyyMMddHHmmss}.csv",
            CheckPathExists = true,
        };
        if (dialog.ShowDialog() == true)
        {
            TargetPath = dialog.FileName;
        }
    }

    public ObservableCollection<string> SelectedItems { get; } = new()
    {
        "GPS周",
        "GPS秒",
        "纬度B(°)",
        "经度L(°)",
        "大地高H(m)",
        "北向速度VN(m/s)",
        "东向速度VE(m/s)",
        "垂向速度VD(m/s)",
        "航向Yaw(°)",
        "俯仰Pitch(°)",
        "横滚Roll(°)"
    };

    public ObservableCollection<string> UnselectedItems { get; } = new()
    {
        "UTC",
        "ECEF-X(m)",
        "ECEF-Y(m)",
        "ECEF-Z(m)",
        "垂向速度VU(m/s)",
        "东向距离RE(m)",
        "北向距离RN(m)",
        "垂向距离RU(m)"
    };

    [ObservableProperty]
    string _targetPath = string.Empty;

    readonly Type _type = typeof(NaviPoseViewModel);


    [RelayCommand]
    async void Export()
    {
        if (Poses.Count == 0)
        {
            _logService.Send(Models.LogType.Error, "尚无数据可导出");
            _snackbarService.Show("错误", "尚无数据可导出", SymbolRegular.DismissCircle24, ControlAppearance.Danger);
            return;
        }
        if (File.Exists(TargetPath))
        {
            _logService.Send(Models.LogType.Warning, $"{TargetPath} 已存在，将被覆盖");
            _snackbarService.Show("警告", "目标路径已存在，将被覆盖", SymbolRegular.Warning24, ControlAppearance.Caution);
        }
        try
        {
            await Task.Run(() =>
            {
                var builder = new StringBuilder();
                for (int i = 0; i < Poses.Count; i++)
                {
                    for (int j = 0; j < SelectedItems.Count; j++)
                    {
                        (var name, var format) = NaviPoseViewModel.ItemToPropertyNamePairs[SelectedItems[j]];
                        var value = _type.GetProperty(name)?.GetValue(Poses[i]);
                        if (value is float @float)
                            builder.Append(@float.ToString(format));
                        else if (value is double @double)
                            builder.Append(@double.ToString(format));
                        else if (value is UtcTime utc)
                            builder.Append(utc.ToString(format));
                        else
                            builder.Append(value);
                        if (j != SelectedItems.Count - 1)
                            builder.Append(',');
                    }
                    builder.AppendLine();
                }
                using var fileStream = new FileStream(TargetPath, FileMode.Create, FileAccess.Write);
                using var streamWriter = new StreamWriter(fileStream);
                streamWriter.WriteLine(string.Join(',', SelectedItems));
                streamWriter.WriteLine(builder.ToString());
            });
        }
        catch (Exception)
        {
            _logService.Send(Models.LogType.Error, $"未能导出到：{TargetPath}");
            _snackbarService.Show("错误", "文件导出失败", SymbolRegular.DismissCircle24, ControlAppearance.Danger);
            return;
        }
        _logService.Send(Models.LogType.Info, $"已成功导出到：{TargetPath}");
        _snackbarService.Show("成功", "文件导出完成", SymbolRegular.Checkmark24, ControlAppearance.Success);
    }
}



