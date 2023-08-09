namespace GuiByReflection.ViewModels;

public class UserEntryChangedEventArgs : EventArgs
{
    public object? UserEntry { get; }
    public bool IsOriginalAction { get; }

    public UserEntryChangedEventArgs(object? userEntry, bool isOriginalAction)
    {
        UserEntry = userEntry;
        IsOriginalAction = isOriginalAction;
    }
}
