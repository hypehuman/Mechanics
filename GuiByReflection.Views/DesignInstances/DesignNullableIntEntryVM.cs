using GuiByReflection.ViewModels;
using GuiByReflection.ViewModels.UserEntryVMs;

namespace GuiByReflection.Views.DesignInstances
{
    public class DesignNullableIntEntryVM : DesignUserEntryVM, INullableUserEntryVM
    {
        public IUserEntryVM UnderlyingVM { get; } = new DesignUserEntryVM();

        public bool UserEntryHasValue { get; set; } = true;

        public object? UnderlyingUserEntry { get; set; } = 1;
    }
}
