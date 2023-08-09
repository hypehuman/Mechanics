namespace GuiByReflection.ViewModels;

public interface IParameterVM : ICanHaveHelp
{
    public Type ParameterType { get; }
    string Title { get; }

    /// <summary>
    /// The value that will be passed to the method call.
    /// </summary>
    object? ActualValue { get; }

    IUserEntryVM UserEntryVM { get; }

    bool HasMessage { get; }
    string? Message { get; }

    void SetActualValue(object? value, bool isOriginalAction);
}
