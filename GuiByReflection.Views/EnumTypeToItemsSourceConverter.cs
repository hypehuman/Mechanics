using System;
using System.Globalization;
using System.Windows.Data;

namespace GuiByReflection.Views;

public class EnumTypeToItemsSourceConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Type type && type.IsEnum)
        {
            return Enum.GetValues(type);
        }

        return Array.Empty<object>();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
