using System;
using System.Windows;
using System.Windows.Media;
using LXIntegratedNavigation.WPF.Services;
using Microsoft.Extensions.DependencyInjection;
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
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MjM1NDg4N0AzMjMxMmUzMDJlMzBIcEhSZFI3ZDg0SDVXaE1HalpYQWZocXl0TjBLWjVtcHNrckE5cjA5bzBRPQ==;Mgo+DSMBaFt+QHJqVk1mQ1BAaV1CX2BZf1J8QGpTf1pgFChNYlxTR3ZZQlphSntadkFgXHlb;Mgo+DSMBMAY9C3t2VFhiQlJPcEBDWXxLflF1VWJYdVt5flVCcDwsT3RfQF5jT35UdEZmUH9ac3NVQw==;Mgo+DSMBPh8sVXJ1S0R+X1pCaV5GQmFJfFBmRGJTeld6dFFWACFaRnZdQV1lSXlRdUBqWXtdeHBQ;MjM1NDg5MUAzMjMxMmUzMDJlMzBZcWdaUERVajhzcTdiRjJIWGFNclppdHdXdFFURG1oMWlqZ0YzYUVNUkt3PQ==;NRAiBiAaIQQuGjN/V0d+Xk9HfVldXGdWfFN0RnNYflR1fV9GZEwxOX1dQl9gSXhTcUdgXHZedHBUTmU=;ORg4AjUWIQA/Gnt2VFhiQlJPcEBDWXxLflF1VWJYdVt5flVCcDwsT3RfQF5jT35UdEZmUH9adHdXQw==;MjM1NDg5NEAzMjMxMmUzMDJlMzBidTlLaWQ3bVpkdEt6TTh6cmNKTFBqTUpsT2tnUlZpWmxZLzlyRmtTaFFJPQ==;MjM1NDg5NUAzMjMxMmUzMDJlMzBVQTczV1Z3YkF3ZExVZm9GbHFLV29xZUU2WXRSSkFsSWI3WC9MS2Y0VXZZPQ==;MjM1NDg5NkAzMjMxMmUzMDJlMzBQSncvUEZxZ0hnNzZxdGE1QXRoK25yYmxQODJUKzA1SWZsZnBrWndHQnI4PQ==;MjM1NDg5N0AzMjMxMmUzMDJlMzBEYUV4VlFKcWw4VDlWUUV5SURsdFZpWFNOMmozQzUvZzNnNVgwYTI5bW5VPQ==;MjM1NDg5OEAzMjMxMmUzMDJlMzBIcEhSZFI3ZDg0SDVXaE1HalpYQWZocXl0TjBLWjVtcHNrckE5cjA5bzBRPQ==");
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
