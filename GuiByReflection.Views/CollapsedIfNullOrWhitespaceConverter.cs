﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GuiByReflection.Views;

public class CollapsedIfNullOrWhitespaceConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return
            value == null || value is string str && string.IsNullOrWhiteSpace(str) ?
            Visibility.Collapsed :
            (object)Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
