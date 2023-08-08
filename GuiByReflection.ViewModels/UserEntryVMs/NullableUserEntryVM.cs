namespace GuiByReflection.ViewModels.UserEntryVMs;

public interface INullableUserEntryVM : IUserEntryVM
{
    public bool UserEntryHasValue { get; set; }
    public object? UnderlyingUserEntry { get; set; }
    public IUserEntryVM UnderlyingVM { get; }
}

public class NullableUserEntryVM : UserEntryVM, INullableUserEntryVM
{
    private readonly Type _underlyingType;
    public IUserEntryVM UnderlyingVM { get; }

    public bool UserEntryHasValue
    {
        get => UnderlyingUserEntry != null;
        set => UserEntry = value ? ParameterVM.GetDefaultValue(_underlyingType) : null;
    }

    public object? UnderlyingUserEntry
    {
        get
        {
            var nullableEntry = UserEntry;
            return
                nullableEntry?.GetType().IsAssignableTo(_underlyingType) == true ?
                nullableEntry :
                ParameterVM.GetDefaultValue(_underlyingType);
        }
        set
        {
            UserEntry = value;
        }
    }

    public NullableUserEntryVM(Type underlying, IUserEntryVMSelector userEntryVMSelector)
    {
        _underlyingType = underlying;
        userEntryVMSelector ??= DefaultUserEntryVMSelector.Instance;
        UnderlyingVM = userEntryVMSelector.SelectUserEntryVM(_underlyingType, userEntryVMSelector);

        UserEntryChanged += (sender, args) =>
        {
            OnPropertyChanged(nameof(UserEntryHasValue));
            OnPropertyChanged(nameof(UnderlyingUserEntry));
        };
    }
}
