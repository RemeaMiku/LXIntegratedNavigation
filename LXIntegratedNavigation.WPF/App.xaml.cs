using System;
using System.Windows.Media;
using LXIntegratedNavigation.WPF.Services;
using Syncfusion.SfSkinManager;
using Syncfusion.Themes.FluentLight.WPF;
using Wpf.Ui.Appearance;
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
            .AddSingleton<TrajectoryPageViewModel>()
            .AddSingleton<FileExportPageViewModel>()
            .AddSingleton<PropertyPageViewModel>()
            .AddSingleton<MainWindow>()
            .AddSingleton<StartPage>()
            .AddSingleton<LogPage>()
            .AddSingleton<TrajectoryPage>()
            .AddSingleton<FileExportPage>()
            .AddSingleton<PropertyPage>();
        return services.BuildServiceProvider();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mgo+DSMBaFt+QHJqXE1hXk5Hd0BLVGpAblJ3T2ZQdVt5ZDU7a15RRnVfRFxnS3xXdEViXnZWcw==;Mgo+DSMBPh8sVXJ1S0R+VVpFdEBBXHxAd1p/VWJYdVt5flBPcDwsT3RfQF5jT39XdEdnW39feHVTRQ==;ORg4AjUWIQA/Gnt2VFhiQlhPd11dXmJWd1p/THNYflR1fV9DaUwxOX1dQl9gSXhSckdhXX1ecXxWRWM=;MjQyMjI2NUAzMjMxMmUzMDJlMzBSWmdsdlg0Y0NrQXBhcVVLVFRjczV5UHExdTRSS1o4TU9sL1doVXo0VXc0PQ==;MjQyMjI2NkAzMjMxMmUzMDJlMzBOVTlUOElPSWRzc3VxeHZiaGcvN3p1eEVKTUFhQ0p0bWpSaDBqemY4NFRJPQ==;NRAiBiAaIQQuGjN/V0d+Xk9NfV5AQmBIYVp/TGpJfl96cVxMZVVBJAtUQF1hSn5Vd0FhW3tccHZVRWhf;MjQyMjI2OEAzMjMxMmUzMDJlMzBlbDJiMUNZU1pvalJ5ODExdW80cExlK2lZNzhEL0VHTlY3SWlXU0x3NWhFPQ==;MjQyMjI2OUAzMjMxMmUzMDJlMzBEVDl1Ri9wNU9MQlNyQW1SRTFoeEFnUERzL2RlTEE4cFJFN0pTUG1ESWQwPQ==;Mgo+DSMBMAY9C3t2VFhiQlhPd11dXmJWd1p/THNYflR1fV9DaUwxOX1dQl9gSXhSckdhXX1ecnVTTmM=;MjQyMjI3MUAzMjMxMmUzMDJlMzBpMzd4NTNtTUR6K1Y4VTRTUTg3MnpuQW45cnQzWkFqMFdvVUdUY3BzZjUwPQ==;MjQyMjI3MkAzMjMxMmUzMDJlMzBlVEVNK2w5cWdtRHBKa3hxYmVKY3BjaURwVGJxWjV5NTUyaysrTUxyNThRPQ==;MjQyMjI3M0AzMjMxMmUzMDJlMzBlbDJiMUNZU1pvalJ5ODExdW80cExlK2lZNzhEL0VHTlY3SWlXU0x3NWhFPQ==");
        var themeSettings = new FluentLightThemeSettings
        {
            PrimaryBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#39c5bb")),
            FontFamily = new FontFamily("Microsoft Yahei UI Light")
        };
        SfSkinManager.RegisterThemeSettings("FluentLight", themeSettings);

        Accent.Apply((Color)ColorConverter.ConvertFromString("#39c5bb"));

        var mainWindow = Services.GetRequiredService<MainWindow>();

        mainWindow.Show();
    }
}
