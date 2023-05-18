using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;

namespace LXIntegratedNavigation.WPF.ViewModels;

internal partial class WindowViewModel : ObservableObject
{
    public WindowViewModel(Window window)
    {
        _window = window;
        _window.StateChanged += (sender, e) =>
        {
            OnPropertyChanged(nameof(ResizeBorderThickness));
            OnPropertyChanged(nameof(OuterMarginSize));
            OnPropertyChanged(nameof(OuterMarginSizeThickness));
            OnPropertyChanged(nameof(WindowRadius));
            OnPropertyChanged(nameof(WindowCornerRadius));
        };
        _title = window.Title;
        var resizer = new WindowResizer(_window);
    }
    public int ResizeBorder { get; set; } = 0;
    public int OuterMarginSize
    {
        get => _window.WindowState == WindowState.Maximized ? 0 : _outerMarginSize;
        set => _outerMarginSize = value;
    }
    public int WindowRadius
    {
        get => _window.WindowState == WindowState.Maximized ? 0 : _windowRadius;
        set => _windowRadius = value;
    }
    public int CaptionHeight { get; set; } = 39;
    public double WindowMinWidth { get; set; } = 400;
    public double WindowMaxHeight { get; set; } = 400;
    public GridLength CaptionHeightGridLength => new(CaptionHeight + ResizeBorder);
    public Thickness ResizeBorderThickness => new(ResizeBorder + OuterMarginSize);
    public Thickness OuterMarginSizeThickness => new(OuterMarginSize);
    public CornerRadius WindowCornerRadius => new(WindowRadius);
    public Thickness InnerContentPadding => new(ResizeBorder);
    [RelayCommand]
    void Minimize() => _window.WindowState = WindowState.Minimized;
    [RelayCommand]
    void Maximize()
    {
        if (_window.WindowState == WindowState.Maximized)
            _window.WindowState = WindowState.Normal;
        else
            _window.WindowState = WindowState.Maximized;
    }
    [RelayCommand]
    void Close() => _window.Close();
    [RelayCommand]
    void Menu() => SystemCommands.ShowSystemMenu(_window, GetMouseScreenPosition(_window));

    private readonly Window _window;
    private int _outerMarginSize = 10;
    private int _windowRadius = 5;
    public static Point GetMouseScreenPosition(Window window)
    {
        var result = Mouse.GetPosition(window);
        result.X += window.Left;
        result.Y += window.Top;
        return result;
    }
    [ObservableProperty]
    private string _title;
}
