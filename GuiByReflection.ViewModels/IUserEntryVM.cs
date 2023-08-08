using System.ComponentModel;

namespace GuiByReflection.ViewModels;

public interface IUserEntryVM : INotifyPropertyChanged
{
    /// <summary>
    /// Some user-entered value, set via binding.
    /// Might be the same type as <see cref="ActualValue"/>,
    /// or might be, e.g., a string that gets parsed to a number.
    /// </summary>
    object? UserEntry { get; set; }

    event EventHandler<UserEntryChangedEventArgs>? UserEntryChanged;

    void SetUserEntry(object? userEntry, bool updateActualValue);
}
