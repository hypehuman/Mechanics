namespace GuiByReflection.ViewModels;

/// <summary>
/// I considered using <see cref="System.Windows.Data.IValueConverter"/> or
/// <see cref="System.Windows.Controls.ValidationRule"/> instead of this interface,
/// but those didn't do exactly what I wanted,
/// and also I didn't want to have to add WPF as a dependency.
/// </summary>
public interface IUserEntryHandler
{
    /// <summary>
    /// Returns the <see cref="IParameterVM.ActualValue"/> to use, based on <paramref name="userEntry"/>.
    /// <paramref name="userEntry"/> might be a string that you have to parse.
    /// You can return <paramref name="currentActualValue"/> if you want to leave the current value unchanged,
    /// Or you can return <see cref="ParameterVM.GetDefaultValue(Type)"/> if you prefer.
    /// TODO: Allow different levels of message: an "error" could block the method call, or a "warning" could just be displayed.
    /// </summary>
    object? UserEntryToValue(object? userEntry, Type type, object? currentActualValue, out string? message);
}
