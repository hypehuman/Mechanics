using GuiByReflection.ViewModels;
using System;
using System.ComponentModel;

namespace GuiByReflection.Views.DesignInstances;

public class DesignEnumParameterVM : IParameterVM
{
    public Type ParameterType => typeof(DesignEnum);
    public string ActualGuiName => "Design GUI Name";
    public string? ActualGuiHelp => "Design GUI Help";

    public object? ActualValue { get; private set; } = default(DesignEnum);

    public object? UserEntry { get; set; } = default(DesignEnum);

    public bool HasMessage => true;
    public string? Message => "Design Message";

    public event PropertyChangedEventHandler? PropertyChanged;

    public void SetActualValue(object? value, bool updateUserEnteredValue)
    {
        throw new NotImplementedException();
    }
}
