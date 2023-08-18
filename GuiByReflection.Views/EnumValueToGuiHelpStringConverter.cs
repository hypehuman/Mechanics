using GuiByReflection.ViewModels;
using System;
using System.Globalization;
using System.Windows.Data;

namespace GuiByReflection.Views;

public class EnumValueToGuiHelpStringConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Enum enumValue)
        {
            return new EnumValueVM(enumValue).ActualGuiHelp;
        }

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
