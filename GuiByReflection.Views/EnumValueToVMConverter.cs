using GuiByReflection.ViewModels;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;

namespace GuiByReflection.Views;

public class EnumValueToVMConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Enum enumValue)
        {
            return new EnumValueVM(enumValue);
        }

        return new FallbackEnumValueVM(value);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    private class FallbackEnumValueVM : IEnumValueVM
    {
        /// <summary>
        /// Never needs to be raised, as bound properties never change.
        /// Interface is implemented to prevent WPF from trying to track changes on these properties,
        /// which could impact performance and cause memory leaks.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged { add { } remove { } }

        public object? ActualValue { get; }

        public FallbackEnumValueVM(object? actualValue)
        {
            ActualValue = actualValue;
        }

        public Enum Value => throw new InvalidOperationException(ActualGuiName);

        public string ActualGuiName
        {
            get
            {
                var actualType = ActualValue?.GetType();
                return
                    actualType == null
                    ? "[null]"
                    : $"[non-Enum value {actualType} '{ActualValue}']"
                    ;
            }
        }

        public string? ActualGuiHelp => null;
    }
}
