namespace GuiByReflection.ViewModels.UserEntryVMs;

public interface INullableUserEntryVM : IUserEntryVM
{
    public IUserEntryVM HasValueVM { get; }
    public IUserEntryVM UnderlyingValueVM { get; }
}

public class NullableUserEntryVM : UserEntryVM, INullableUserEntryVM
{
    private readonly Type _underlyingType;
    public IUserEntryVM HasValueVM { get; }
    public IUserEntryVM UnderlyingValueVM { get; }

    public NullableUserEntryVM(Type underlying, IUserEntryVMSelector userEntryVMSelector)
    {
        _underlyingType = underlying;
        HasValueVM = userEntryVMSelector.SelectUserEntryVM(typeof(bool), userEntryVMSelector);
        userEntryVMSelector ??= DefaultUserEntryVMSelector.Instance;
        UnderlyingValueVM = userEntryVMSelector.SelectUserEntryVM(_underlyingType, userEntryVMSelector);

        UserEntryChanged += (sender, args) =>
        {
            // TODO: Keep them all in sync somehow
            //OnPropertyChanged(nameof(UserEntryHasValue));
            //OnPropertyChanged(nameof(UnderlyingUserEntry));
        };
    }
}
