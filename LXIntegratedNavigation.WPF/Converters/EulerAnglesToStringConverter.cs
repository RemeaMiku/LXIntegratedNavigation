using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NaviSharp;

namespace LXIntegratedNavigation.WPF.Converters;

public class EulerAnglesToStringConverter : BaseConverter<EulerAnglesToStringConverter>
{
    #region Public Methods

    public override object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is null)
            return string.Empty;
        return ((EulerAngles)value).ToString();
    }

    public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var str = value.ToString();
        if (string.IsNullOrEmpty(str))
        {
            return new();
        }
        return EulerAngles.Parse(str);
    }

    #endregion Public Methods
}
