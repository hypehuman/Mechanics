using GuiByReflection.Models;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace GuiByReflection.ViewModels;

public class ParameterVM : IParameterVM
{
    private readonly ParameterInfo _parameterInfo;
    private readonly IUserEntryHandler _userEntryHandler;
    private object? _actualValue;
    private bool _hasMessage;
    private string? _message;

    public event PropertyChangedEventHandler? PropertyChanged;
    public Type ParameterType { get; }
    public string Title { get; }
    public string? Help { get; }

    public ParameterVM(ParameterInfo parameterInfo, IUserEntryHandler? userEntryHandler = null, IUserEntryVMSelector? userEntryVMSelector = null)
    {
        _parameterInfo = parameterInfo;
        _userEntryHandler = userEntryHandler ?? DefaultUserEntryHandler.Instance;
        ParameterType = _parameterInfo.ParameterType;

        var explicitLabel = _parameterInfo.GetCustomAttribute<GuiTitleAttribute>(false)?.Value;
        Title = !string.IsNullOrWhiteSpace(explicitLabel) ? explicitLabel : _parameterInfo.Name ?? "[unnamed parameter]";

        Help = _parameterInfo.GetCustomAttribute<GuiHelpAttribute>(false)?.Value;

        UserEntryVM = (userEntryVMSelector ?? DefaultUserEntryVMSelector.Instance).SelectUserEntryVM(ParameterType, userEntryVMSelector);
        UserEntryVM.UserEntryChanged += UserEntryVM_UserEntryChanged;

        SetActualValue(GetDefaultValue(ParameterType), isOriginalAction: true);
    }

    private void UserEntryVM_UserEntryChanged(object? sender, UserEntryChangedEventArgs e)
    {
        if (e.IsOriginalAction)
        {
            var value = _userEntryHandler.UserEntryToValue(e.UserEntry, ParameterType, ActualValue, out var message);
            SetActualValue(value, isOriginalAction: false);
            Message = message;
        }
    }

    public bool HasHelp => !string.IsNullOrWhiteSpace(Help);

    public IUserEntryVM UserEntryVM { get; }

    public object? ActualValue => _actualValue;

    /// <summary>
    // TODO: Check whether the value can be assigned to the type.
    // .NET internally uses System.RuntimeType.CheckValue to do that when calling the method.
    /// </summary>
    public void SetActualValue(object? actualValue, bool isOriginalAction)
    {
        // You might be tempted to return if _actualValue == actualValue.
        // But for nullable types, we need to continue even if the value is changing from null to null.
        // This ensures that we get properly set up via the initial call from the constructor.

        _actualValue = actualValue;
        OnPropertyChanged(nameof(ActualValue));

        if (isOriginalAction)
        {
            UserEntryVM.SetUserEntry(actualValue, isOriginalAction: false);
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

    public static object? GetDefaultValue(Type type)
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

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
