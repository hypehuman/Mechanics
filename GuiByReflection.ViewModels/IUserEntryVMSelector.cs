namespace GuiByReflection.ViewModels;

public interface IUserEntryVMSelector
{
    IUserEntryVM SelectUserEntryVM(Type parameterType, IUserEntryVMSelector userEntryVMSelector);
}
