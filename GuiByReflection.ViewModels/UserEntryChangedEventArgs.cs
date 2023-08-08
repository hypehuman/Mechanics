namespace GuiByReflection.ViewModels;

public class UserEntryChangedEventArgs : EventArgs
{
    public object? UserEntry { get; }
    public bool UpdateActualValue { get; }

    public UserEntryChangedEventArgs(object? userEntry, bool updateActualValue)
    {
        UserEntry = userEntry;
        UpdateActualValue = updateActualValue;
    }
}
