using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

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
    public delegate bool TryParseDelegate(string s, out T parsed, out string message);

    private readonly ValidationTextBoxViewModel<T>.TryParseDelegate _tryParse;
    private readonly Func<T, string> _valueToString;
    private bool _hasError;
    private string? _tooltipText;
    private T _currentValue;
    private string _textboxText;
    public event PropertyChangedEventHandler? PropertyChanged;

    public ValidationTextBoxViewModel(TryParseDelegate tryParse, Func<T, string>? valueToString = null, T initialValue = default)
    {
        _currentValue = initialValue;
        _tryParse = tryParse;
        _valueToString = valueToString ?? new Func<T, string>(v => v?.ToString() ?? string.Empty);
        _textboxText = _valueToString(initialValue);
    }

    public bool HasError
    {
        get => _hasError;
        private set
        {
            if (value == _hasError)
                return;

            _hasError = value;
            OnPropertyChanged();
        }
    }

    public string? TooltipText
    {
        get => _tooltipText;
        private set
        {
            if (value == _tooltipText)
                return;

            _tooltipText = value;
            OnPropertyChanged();
        }
    }

    public T CurrentValue
    {
        get => _currentValue;
        set => SetCurrentValue(value);
    }

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
            OnPropertyChanged(nameof(CurrentValue));

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
        OnPropertyChanged(nameof(TextboxText));

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

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

public class DesignValidationTextBoxViewModel : IValidationTextBoxViewModel
{
    public bool HasError => true;
    public string? TooltipText => "Design tooltip text";
    public string TextboxText { get; set; } = "Design textbox text";
    public event PropertyChangedEventHandler? PropertyChanged;
}
