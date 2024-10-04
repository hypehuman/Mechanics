using GuiByReflection.Models;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace GuiByReflection.ViewModels;

public class ParameterVM : IParameterVM
{
    private readonly ParameterInfo _parameterInfo;
    private readonly IUserEntryHandler _userEntryHandler;
    private object? _userEntry;
    private object? _actualValue;
    private bool _hasMessage;
    private string? _message;

    public event PropertyChangedEventHandler? PropertyChanged;
    public Type ParameterType { get; }
    public string ActualGuiName { get; }
    public string? ActualGuiHelp { get; }

    public ParameterVM(ParameterInfo parameterInfo, IUserEntryHandler? userEntryHandler = null)
    {
        _parameterInfo = parameterInfo;
        _userEntryHandler = userEntryHandler ?? DefaultUserEntryHandler.Instance;
        ParameterType = _parameterInfo.ParameterType;

        ActualGuiName = _parameterInfo.GetCustomAttribute<GuiNameAttribute>(false).GetActualGuiName(_parameterInfo.Name);
        ActualGuiHelp = _parameterInfo.GetCustomAttribute<GuiHelpAttribute>(false).GetActualGuiHelp();

        SetActualValue(GetDefaultParameterValue(), updateUserEnteredValue: true);
    }

    public object? UserEntry
    {
        get => _userEntry;
        set => SetUserEntry(value, updateActualValue: true);
    }

    private void SetUserEntry(object? userEntry, bool updateActualValue)
    {
        if (_userEntry == userEntry)
            return;
        _userEntry = userEntry;
        OnPropertyChanged(nameof(UserEntry));

        if (updateActualValue)
        {
            var value = _userEntryHandler.UserEntryToValue(userEntry, ParameterType, ActualValue, out var message);
            SetActualValue(value, updateUserEnteredValue: false);
            Message = message;
        }
    }

    public object? ActualValue => _actualValue;

    /// <summary>
    // TODO: Check whether the value can be assigned to the type.
    // .NET internally uses System.RuntimeType.CheckValue to do that when calling the method.
    /// </summary>
    public void SetActualValue(object? actualValue, bool updateUserEnteredValue)
    {
        if (_actualValue == actualValue)
            return;
        _actualValue = actualValue;
        OnPropertyChanged(nameof(ActualValue));

        if (updateUserEnteredValue)
        {
            SetUserEntry(actualValue, updateActualValue: false);
            Message = string.Empty;
        }
    }

    public bool HasMessage
    {
        get => _hasMessage;
        private set
        {
            if (_hasMessage == value)
                return;
            _hasMessage = value;
            OnPropertyChanged();
        }
    }

    public string? Message
    {
        get => _message;
        private set
        {
            if (_message == value)
                return;
            _message = value;
            OnPropertyChanged();
            HasMessage = !string.IsNullOrWhiteSpace(value);
        }
    }

    public object? GetDefaultParameterValue()
    {
        if (_parameterInfo.HasDefaultValue)
        {
            var defaultValue = _parameterInfo.DefaultValue;
            if (ParameterType.IsValueType && defaultValue == null)
            {
                // Seems to happen for custom structs.
                // I don't know why this happens; is it a bug in .NET?
            }
            else
            {
                return defaultValue;
            }
        }

        return GetDefaultValueOfType(ParameterType);
    }

    public static object? GetDefaultValueOfType(Type type)
    {
        if (type.IsValueType)
        {
            return typeof(ParameterVM)
                .GetMethod(nameof(GetDefaultValueOfTypeGeneric))
                .MakeGenericMethod(type)
                .Invoke(null, null);
        }
        return null;
    }

    public static T GetDefaultValueOfTypeGeneric<T>()
    {
        return default;
    }

    /// <summary>
    /// Determines whether <paramref name="value"/> can be assigned to a variable of the specified <paramref name="targetType"/>.
    /// </summary>
    public static bool CanAssignTypeFromValue(Type targetType, object? value)
    {
        return
            value == null ?
                // If the value is null, return whether the type can accept null values.
                !targetType.IsValueType || Nullable.GetUnderlyingType(targetType) != null :
                // If the value is not null, use IsAssignableFrom
                targetType.IsAssignableFrom(value.GetType());
    }

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
