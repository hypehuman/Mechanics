using GuiByReflection.ViewModels.UserEntryVMs;

namespace GuiByReflection.ViewModels;

public class DefaultUserEntryVMSelector : IUserEntryVMSelector
{
    public static DefaultUserEntryVMSelector Instance { get; } = new DefaultUserEntryVMSelector();

    public IUserEntryVM SelectUserEntryVM(Type parameterType, IUserEntryVMSelector userEntryVMSelector)
    {
        if (parameterType == typeof(bool))
        {
            return new BoolUserEntryVM();
        }

        if (parameterType.IsEnum)
        {
            return new EnumUserEntryVM(parameterType);
        }

        var underlying = Nullable.GetUnderlyingType(parameterType);
        if (underlying != null)
        {
            return new NullableUserEntryVM(underlying, this);
        }

        return new UserEntryVM();
    }
}
