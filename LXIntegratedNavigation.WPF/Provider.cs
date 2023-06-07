using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LXIntegratedNavigation.WPF.Services;
using Wpf.Ui.Controls;
using Wpf.Ui.Mvvm.Services;

namespace LXIntegratedNavigation.WPF;

public static class Provider
{
    public static DataService DataService { get; } = new();

    public static NavigationService NavigationService { get; } = new();

    public static StartPageViewModel StartPageViewModel { get; } = new(DataService, NavigationService);
    public static StartPage StartPage { get; } = new();

    public static DialogService DialogService { get; } = new();

    static Provider()
    {
        DialogService.SetDialogControl(new Dialog());
    }
}
