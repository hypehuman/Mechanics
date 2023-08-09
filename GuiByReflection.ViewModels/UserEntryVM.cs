using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GuiByReflection.ViewModels;

public class UserEntryVM : IUserEntryVM
{
    private object? _userEntry;

    public object? UserEntry
    {
        get => _userEntry;
        set => SetUserEntry(value, updateActualValue: true);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler<UserEntryChangedEventArgs>? UserEntryChanged;

    public void SetUserEntry(object? userEntry, bool updateActualValue)
    {
        // You might be tempted to return if _auserEntry == userEntry.
        // But for nullable types, we need to continue even if the value is changing from null to null.
        // This ensures that we get properly set up via the initial call from the constructor.

        _userEntry = userEntry;
        OnPropertyChanged(new(nameof(UserEntry)));
        UserEntryChanged?.Invoke(this, new(userEntry, updateActualValue));
    }

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
