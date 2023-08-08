using GuiByReflection.ViewModels;
using System;

namespace GuiByReflection.Views.DesignInstances;

internal class DesignMethodVM : MethodVM
{
    public static void DesignMethod(
        string stringParam,
        bool boolParam,
        DesignEnumUserEntryVM.DesignEnum enumParam,
        int? nullableIntParam
    )
    {
    }

    public DesignMethodVM()
        : base(null, typeof(DesignMethodVM).GetMethod(nameof(DesignMethod)) ?? throw new Exception("design method not found"))
    {
    }
}
