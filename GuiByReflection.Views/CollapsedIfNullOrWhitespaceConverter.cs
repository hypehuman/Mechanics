using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GuiByReflection.Views;

/// <summary>
/// Visible if its string representation has any non-whitespace characters.
/// Collapsed if null, or if its string representation is null or whitespace.
/// </summary>
public class CollapsedIfNullOrWhitespaceConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return string.IsNullOrWhiteSpace(value?.ToString()) ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
