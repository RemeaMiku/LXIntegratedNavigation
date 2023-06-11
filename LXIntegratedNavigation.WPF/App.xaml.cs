using System;
using System.Windows;
using System.Windows.Media;
using LXIntegratedNavigation.WPF.Services;
using Microsoft.Extensions.DependencyInjection;
using Syncfusion.SfSkinManager;
using Syncfusion.Themes.FluentLight.WPF;
using Syncfusion.Themes.Windows11Light.WPF;
using Wpf.Ui.Mvvm.Contracts;
using Wpf.Ui.Mvvm.Services;

namespace LXIntegratedNavigation.WPF;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public sealed partial class App : Application
{
    public App()
    {
        Services = ConfigureServices();
        InitializeComponent();
    }

    /// <summary>
    /// Gets the current <see cref="App"/> instance in use
    /// </summary>
    public new static App Current => (App)Application.Current;

    /// <summary>
    /// Gets the <see cref="IServiceProvider"/> instance to resolve application services.
    /// </summary>
    public IServiceProvider Services { get; }

    /// <summary>
    /// Configures the services for the application.
    /// </summary>
    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection()
            .AddSingleton<ISnackbarService, SnackbarService>()
            .AddSingleton<DataService>()
            .AddSingleton<LogService>()
            .AddSingleton<NavigationService>()
            .AddSingleton<StartPageViewModel>()
            .AddSingleton<LogPageViewModel>()
            .AddSingleton<MainWindow>()
            .AddSingleton<StartPage>()
            .AddSingleton<LogPage>();
        return services.BuildServiceProvider();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        var mainWindow = Services.GetService<MainWindow>();
        mainWindow?.Show();
    }
}
