using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace LXIntegratedNavigation.WPF.Converters;

public abstract class BaseConverter<T> : IValueConverter where T : class, new()
{
    public static T Instance { get; } = new();
    public abstract object Convert(object value, Type targetType, object parameter, CultureInfo culture);
    public abstract object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture);
}
