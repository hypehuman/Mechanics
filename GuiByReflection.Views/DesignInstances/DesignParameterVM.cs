using GuiByReflection.ViewModels;
using System;
using System.ComponentModel;

namespace GuiByReflection.Views.DesignInstances;

public class DesignParameterVM : IParameterVM
{
    public event PropertyChangedEventHandler? PropertyChanged { add { } remove { } }

    public Type ParameterType => typeof(int);
    public string ActualGuiName => "Design GUI Name";
    public string? ActualGuiHelp => "Design GUI Help";

    public object? ActualValue { get; private set; } = "Design Actual Value";

    public object? UserEntry { get; set; } = "Design User Entry";

    public bool HasMessage => true;
    public string? Message => "Design Message";

    public void SetActualValue(object? value, bool updateUserEnteredValue)
    {
        throw new NotImplementedException();
    }
}
