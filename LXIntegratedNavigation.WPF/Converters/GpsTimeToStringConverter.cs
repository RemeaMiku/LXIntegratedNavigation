using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using NaviSharp;

namespace LXIntegratedNavigation.WPF.Converters;

public class GpsTimeToStringConverter : BaseConverter<GpsTimeToStringConverter>
{

    public override object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is null)
            return string.Empty;
        return ((GpsTime)value).ToString();

    }


    public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var str = value.ToString();
        if (string.IsNullOrEmpty(str))
        {
            return new();
        }
        return GpsTime.Parse(str);
    }
}
