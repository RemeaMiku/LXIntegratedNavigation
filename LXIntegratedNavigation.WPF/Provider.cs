using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LXIntegratedNavigation.WPF.Services;

namespace LXIntegratedNavigation.WPF;

public static class Provider
{
    public static DataService DataService { get; }
    public static StartPageViewModel StartPageViewModel { get; }
    public static StartPage StartPage { get; }

    static Provider()
    {
        DataService = new DataService();
        StartPageViewModel = new StartPageViewModel(DataService);
        StartPage = new StartPage();
    }
}
