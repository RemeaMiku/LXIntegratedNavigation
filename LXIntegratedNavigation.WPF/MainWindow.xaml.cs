using System.Windows;
using System.Windows.Media;
using Syncfusion.SfSkinManager;
using Syncfusion.Themes.FluentLight.WPF;
using Syncfusion.Themes.Windows11Light.WPF;
using Wpf.Ui.Appearance;
using Wpf.Ui.Mvvm.Contracts;
using Wpf.Ui.Mvvm.Services;

namespace LXIntegratedNavigation.WPF;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    #region Public Constructors    

    public MainWindow(ISnackbarService snackbarService)
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel(this);
        var themeSettings = new FluentLightThemeSettings
        {
            PrimaryBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#39c5bb")),
            FontFamily = new FontFamily("Microsoft Yahei UI Light")
        };
        SfSkinManager.RegisterThemeSettings("FluentLight", themeSettings);
        SfSkinManager.SetTheme(this, new FluentTheme() { ThemeName = "FluentLight", PressedEffectMode = PressedEffect.Glow, HoverEffectMode = HoverEffect.BackgroundAndBorder });
        Accent.Apply((Color)ColorConverter.ConvertFromString("#aa39c5bb"));
        snackbarService.SetSnackbarControl(Snackbar);
    }

    #endregion Public Constructors
}
