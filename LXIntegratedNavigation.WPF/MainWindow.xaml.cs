using LXIntegratedNavigation.WPF.Services;
using Syncfusion.SfSkinManager;
using Wpf.Ui.Mvvm.Contracts;

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
