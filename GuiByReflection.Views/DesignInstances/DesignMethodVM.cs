using GuiByReflection.ViewModels;
using System;

namespace GuiByReflection.Views.DesignInstances;

internal class DesignMethodVM : MethodVM
{
    public static void DesignMethod(
        string stringParam,
        bool boolParam,
        DesignEnum enumParam,
        int? nullableIntParam,
        string optionalParam = "Design Default Parameter Value"
    )
    {
    }

    public DesignMethodVM()
        : base(null, typeof(DesignMethodVM).GetMethod(nameof(DesignMethod)) ?? throw new Exception("design method not found"))
    {
    }
}
