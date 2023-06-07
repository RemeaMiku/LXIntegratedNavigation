using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LXIntegratedNavigation.WPF.Converters;

public class UtcTimeToStringConverter : BaseConverter<UtcTimeToStringConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return ((UtcTime)value).ToString("yyyy.MM.dd HH:mm:ss.ffffff");
    }

    public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return UtcTime.Parse((string)value);
    }
}
