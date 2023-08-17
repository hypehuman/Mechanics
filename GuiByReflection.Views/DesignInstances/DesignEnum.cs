using GuiByReflection.Models;

namespace GuiByReflection.Views.DesignInstances;

public enum DesignEnum
{
    [GuiName($"GUI Name of {nameof(DesignEnumValueA)}")]
    [GuiHelp($"GUI Help for {nameof(DesignEnumValueA)}")]
    DesignEnumValueA,

    DesignEnumValueB,
};

