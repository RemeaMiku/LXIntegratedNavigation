using System.Linq;
using System.Windows;
using System.Windows.Media;
using LXIntegratedNavigation.WPF.Services;
using Syncfusion.SfSkinManager;
using Syncfusion.Themes.FluentLight.WPF;
using Syncfusion.Windows.Tools.Controls;
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

    readonly MainWindowViewModel _viewModel;

    public MainWindow(ISnackbarService snackbarService)
    {
        InitializeComponent();
        SfSkinManager.SetTheme(this, new FluentTheme() { ThemeName = "FluentLight", PressedEffectMode = PressedEffect.Glow, HoverEffectMode = HoverEffect.BackgroundAndBorder });
        snackbarService.SetSnackbarControl(Snackbar);
        _viewModel = new MainWindowViewModel(snackbarService, Current.Services.GetRequiredService<LogService>(), this);
        DataContext = _viewModel;
    }

    #endregion Public Constructors

}
