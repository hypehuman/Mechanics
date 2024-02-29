using GuiByReflection.Models;
using System.ComponentModel;
using System.Reflection;

namespace GuiByReflection.ViewModels;

public class ParameterVM : IParameterVM
{
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(PropertyChangedEventArgs e) => PropertyChanged?.Invoke(this, e);

    private readonly ParameterInfo _parameterInfo;
    private readonly IUserEntryHandler _userEntryHandler;
    private object? _userEntry;
    private object? _actualValue;
    private bool _hasMessage;
    private string? _message;

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

    private static readonly PropertyChangedEventArgs sUserEntryChangedArgs = new(nameof(UserEntry));
    public object? UserEntry
    {
        get => _userEntry;
        set => SetUserEntry(value, updateActualValue: true);
    }

    private void SetUserEntry(object? userEntry, bool updateActualValue)
    {
        if (_userEntry == userEntry) return;
        _userEntry = userEntry;
        OnPropertyChanged(sUserEntryChangedArgs);

        if (updateActualValue)
        {
            var value = _userEntryHandler.UserEntryToValue(userEntry, ParameterType, ActualValue, out var message);
            SetActualValue(value, updateUserEnteredValue: false);
            Message = message;
        }
    }

    private static readonly PropertyChangedEventArgs sActualValueChangedArgs = new(nameof(ActualValue));
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
        OnPropertyChanged(sActualValueChangedArgs);

        if (updateUserEnteredValue)
        {
            SetUserEntry(actualValue, updateActualValue: false);
            Message = string.Empty;
        }
    }

    private static readonly PropertyChangedEventArgs sHasMessageChangedArgs = new(nameof(HasMessage));
    public bool HasMessage
    {
        get => _hasMessage;
        private set
        {
            if (_hasMessage == value) return;
            _hasMessage = value;
            OnPropertyChanged(sHasMessageChangedArgs);
        }
    }

    private static readonly PropertyChangedEventArgs sMessageChangedArgs = new(nameof(Message));
    public string? Message
    {
        get => _message;
        private set
        {
            if (_message == value) return;
            _message = value;
            OnPropertyChanged(sMessageChangedArgs);
            HasMessage = !string.IsNullOrWhiteSpace(value);
        }
    }

    public object? GetDefaultParameterValue()
    {
        return _parameterInfo.HasDefaultValue ? _parameterInfo.RawDefaultValue : GetDefaultValueOfType(ParameterType);
    }

    public static object? GetDefaultValueOfType(Type type)
    {
        if (type.IsValueType)
        {
            return Activator.CreateInstance(type);
        }
        return null;
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
}
