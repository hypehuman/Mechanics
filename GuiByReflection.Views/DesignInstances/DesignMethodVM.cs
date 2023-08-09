using GuiByReflection.ViewModels;
using System;

namespace GuiByReflection.Views.DesignInstances;

internal class DesignMethodVM : MethodVM
{
    public enum DesignEnum { DesignEnum1, DesignEnum2 };

    public static void DesignMethod(
        string stringParam,
        bool boolParam,
        DesignEnum enumParam,
        int? nullableIntParam
    )
    {
    }

    public DesignMethodVM()
        : base(null, typeof(DesignMethodVM).GetMethod(nameof(DesignMethod)) ?? throw new Exception("design method not found"))
    {
    }
}
