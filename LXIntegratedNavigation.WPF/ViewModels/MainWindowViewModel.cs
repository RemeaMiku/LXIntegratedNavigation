using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;

namespace LXIntegratedNavigation.WPF.ViewModels;

internal partial class MainWindowViewModel : WindowViewModel
{
    public MainWindowViewModel(Window window) : base(window) { }

    [ObservableProperty]
    public Page _currentPage = HomePage.Instance;
}
