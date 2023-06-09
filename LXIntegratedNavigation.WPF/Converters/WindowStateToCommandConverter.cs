using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace LXIntegratedNavigation.WPF.Converters;

public class WindowStateToCommandConverter : BaseConverter<WindowStateToCommandConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is WindowState state)
        {
            return state == WindowState.Maximized ? SystemCommands.RestoreWindowCommand : SystemCommands.MaximizeWindowCommand;
        }
        return Binding.DoNothing;
    }

    public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
