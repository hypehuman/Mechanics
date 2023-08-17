namespace GuiByReflection.ViewModels;

public interface IParameterVM : IHasGuiHelp
{
    public Type ParameterType { get; }
    string ActualGuiName { get; }

    /// <summary>
    /// The value that will be passed to the method call.
    /// </summary>
    object? ActualValue { get; }

    /// <summary>
    /// Some user-entered value, set via binding.
    /// Might be the same type as <see cref="ActualValue"/>,
    /// or might be, e.g., a string that gets parsed to a number.
    /// </summary>
    object? UserEntry { get; set; }

    bool HasMessage { get; }
    string? Message { get; }

    void SetActualValue(object? value, bool updateUserEnteredValue);
}
