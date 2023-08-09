using GuiByReflection.ViewModels;
using GuiByReflection.ViewModels.UserEntryVMs;

namespace GuiByReflection.Views.DesignInstances
{
    public class DesignNullableIntEntryVM : DesignUserEntryVM, INullableUserEntryVM
    {
        public IUserEntryVM HasValueVM { get; } = new BoolUserEntryVM();
        public IUserEntryVM UnderlyingValueVM { get; } = new DesignUserEntryVM();
    }
}
