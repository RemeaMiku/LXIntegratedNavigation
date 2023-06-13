using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using LXIntegratedNavigation.WPF.Models;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using Brush = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;

namespace LXIntegratedNavigation.WPF.ViewModels;

public partial class LogViewModel : ObservableObject
{
    [ObservableProperty]
    SymbolRegular _symbol;
    [ObservableProperty]
    string _time;
    [ObservableProperty]
    string _message;
    [ObservableProperty]
    Brush _foreground;
    static readonly Dictionary<LogType, Brush> _brushes = new()
    {
        {LogType.Info, new SolidColorBrush(Colors.DeepSkyBlue) },
        {LogType.Warning, new SolidColorBrush(Colors.Orange) },
        {LogType.Error, new SolidColorBrush(Colors.Red) }
    };
    static readonly Dictionary<LogType, SymbolRegular> _symbols = new()
    {
        {LogType.Info, SymbolRegular.Info24 },
        {LogType.Warning,SymbolRegular.Warning24 },
        {LogType.Error, SymbolRegular.DismissCircle24 }
    };
    public LogViewModel(Log log)
    {
        _symbol = _symbols[log.Type];
        _foreground = _brushes[log.Type];
        _time = log.TimeStamp.ToString();
        _message = log.Message;
    }
}
