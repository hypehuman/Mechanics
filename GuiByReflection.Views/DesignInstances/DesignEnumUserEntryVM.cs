using GuiByReflection.ViewModels.UserEntryVMs;
using System;

namespace GuiByReflection.Views.DesignInstances
{
    public class DesignEnumUserEntryVM : DesignUserEntryVM, IEnumUserEntryVM
    {
        public enum DesignEnum { DesignEnum1, DesignEnum2 };

        public DesignEnumUserEntryVM()
        {
            UserEntry = Options.GetValue(0);
        }

        public Array Options => new[] { DesignEnum.DesignEnum1, DesignEnum.DesignEnum2 };
    }
}
