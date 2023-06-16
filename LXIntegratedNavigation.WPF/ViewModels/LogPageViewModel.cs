using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using LXIntegratedNavigation.WPF.Models;
using LXIntegratedNavigation.WPF.Services;

namespace LXIntegratedNavigation.WPF.ViewModels;

public partial class LogPageViewModel : ObservableObject, ILogReceiver
{
    [ObservableProperty]
    ObservableCollection<LogViewModel> _logs = new();

    readonly LogService _logService;

    public LogPageViewModel(LogService logService)
    {
        _logService = logService;
        logService.LogReceiver = this;
    }

    public void Receive(Log log)
    {
        Logs.Add(new(log));
    }
}
