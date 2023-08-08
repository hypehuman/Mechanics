namespace GuiByReflection.ViewModels.UserEntryVMs;

/// <summary>
/// Adds no extra properties, just provides a type for the view selector to switch on.
/// </summary>
public interface IBoolUserEntryVM : IUserEntryVM
{
}

public class BoolUserEntryVM : UserEntryVM, IBoolUserEntryVM
{
}
