namespace GuiByReflection.ViewModels.UserEntryVMs;

public interface IEnumUserEntryVM : IUserEntryVM
{
    public Array Options { get; }
}

public class EnumUserEntryVM : UserEntryVM, IEnumUserEntryVM
{
    public Array Options { get; }

    public EnumUserEntryVM(Type parameterType)
    {
        Options = Enum.GetValues(parameterType);
    }
}
