using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace MechanicsUI;

public class HasErrorToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return false.Equals(value) ? Brushes.Transparent : Brushes.Red;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
