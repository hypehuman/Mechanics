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
        if (_userEntry == userEntry)
            return;
        _userEntry = userEntry;
        OnPropertyChanged(new(nameof(UserEntry)));
        UserEntryChanged?.Invoke(this, new(userEntry, updateActualValue));
    }

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
