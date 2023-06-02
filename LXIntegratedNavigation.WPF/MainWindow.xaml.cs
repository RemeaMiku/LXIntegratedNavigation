using System.Windows;
using System.Windows.Media;
using Wpf.Ui.Appearance;

namespace LXIntegratedNavigation.WPF;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    #region Public Constructors

    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel(this);
        Accent.Apply((Color)ColorConverter.ConvertFromString("#aa39c5bb"));
    }

    #endregion Public Constructors
}
