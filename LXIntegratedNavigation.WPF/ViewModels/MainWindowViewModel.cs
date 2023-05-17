using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LXIntegratedNavigation.WPF.Views;

namespace LXWindowsPresentationFoundation;

internal partial class MainWindowViewModel : ObservableObject
{
    public MainWindowViewModel(Window window)
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
    [ObservableProperty]
    public Page _currentPage = HomePage.Instance;
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
}
