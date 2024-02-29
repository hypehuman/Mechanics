using System;
using System.ComponentModel;

namespace MechanicsUI;

public interface IValidationTextBoxViewModel : INotifyPropertyChanged
{
    public bool HasError { get; }
    public string? TooltipText { get; }
    public string TextboxText { get; set; }
}

public interface IValidationTextBoxViewModel<T> : IValidationTextBoxViewModel
{
    public T CurrentValue { get; set; }
}

public class ValidationTextBoxViewModel<T> : IValidationTextBoxViewModel<T>
{
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(PropertyChangedEventArgs e) => PropertyChanged?.Invoke(this, e);

    public delegate bool TryParseDelegate(string s, out T parsed, out string message);

    private readonly ValidationTextBoxViewModel<T>.TryParseDelegate _tryParse;
    private readonly Func<T, string> _valueToString;
    private bool _hasError;
    private string? _tooltipText;
    private T _currentValue;
    private string _textboxText;

    public ValidationTextBoxViewModel(TryParseDelegate tryParse, Func<T, string>? valueToString = null, T initialValue = default)
    {
        _currentValue = initialValue;
        _tryParse = tryParse;
        _valueToString = valueToString ?? new Func<T, string>(v => v?.ToString() ?? string.Empty);
        _textboxText = _valueToString(initialValue);
    }

    private static readonly PropertyChangedEventArgs sHasErrorChangedArgs = new(nameof(HasError));
    public bool HasError
    {
        get => _hasError;
        private set
        {
            if (_hasError == value) return;
            _hasError = value;
            OnPropertyChanged(sHasErrorChangedArgs);
        }
    }

    private static readonly PropertyChangedEventArgs sTooltipTextChangedArgs = new(nameof(TooltipText));
    public string? TooltipText
    {
        get => _tooltipText;
        private set
        {
            if (_tooltipText == value) return;
            _tooltipText = value;
            OnPropertyChanged(sTooltipTextChangedArgs);
        }
    }

    private static readonly PropertyChangedEventArgs sCurrentValueChangedArgs = new(nameof(CurrentValue));
    public T CurrentValue
    {
        get => _currentValue;
        set => SetCurrentValue(value);
    }

    private static readonly PropertyChangedEventArgs sTextboxTextChangedArgs = new(nameof(TextboxText));
    public string TextboxText
    {
        get => _textboxText;
        set => SetTextboxText(value);
    }

    private void SetCurrentValue(T input, bool requiresUserEntryUpdate = true)
    {
        // Don't return here even if the values are equal;
        // We still want to update the Textbox Text, in case the user
        // has made an invalid edit.
        var newEqualsOld = object.Equals(input, _currentValue);

        _currentValue = input;
        // The UI doesn't care about the current value,
        // but other ViewModels might want to know when it changes.

        if (!newEqualsOld)
            OnPropertyChanged(sCurrentValueChangedArgs);

        if (!requiresUserEntryUpdate)
            return;

        SetTextboxText(_valueToString(input), requiresValidation: false);
        HasError = false;
        TooltipText = null;
    }

    private void SetTextboxText(string input, bool requiresValidation = true)
    {
        if (input == _textboxText)
            return;

        _textboxText = input;
        OnPropertyChanged(sTextboxTextChangedArgs);

        if (!requiresValidation)
            return;

        if (_tryParse(input, out var parsed, out var message))
        {
            SetCurrentValue(parsed, requiresUserEntryUpdate: false);
            HasError = false;
            TooltipText = null;
        }
        else
        {
            HasError = true;
            TooltipText =
                (
                    string.IsNullOrEmpty(message) ?
                    string.Empty :
                    message + Environment.NewLine + Environment.NewLine
                ) + $"Current value remains '{_valueToString(CurrentValue)}'";
        }
    }
}

public class DesignValidationTextBoxViewModel : IValidationTextBoxViewModel
{
    public event PropertyChangedEventHandler? PropertyChanged { add { } remove { } }

    public bool HasError => true;
    public string? TooltipText => "Design tooltip text";
    public string TextboxText { get; set; } = "Design textbox text";
}
