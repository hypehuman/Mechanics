using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GuiByReflection.Views;

partial class ParameterView
{
    public ParameterView()
    {
        InitializeComponent();
    }
}

public class ParameterView_HasMessageToBorderBrushConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value switch
        {
            true => Brushes.Orange,
            false => null,
            _ => Brushes.Gray, // Binding error
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class ParameterView_TooltipConverter : IMultiValueConverter
{
    public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values != null && values.Length >=2 && values[0] is bool hasMessage)
        {
            return hasMessage ? values[1] : null;
        }

        return "Binding error";
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
