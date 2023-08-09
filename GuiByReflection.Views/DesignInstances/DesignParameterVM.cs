using GuiByReflection.ViewModels;
using System;
using System.ComponentModel;

namespace GuiByReflection.Views.DesignInstances;

public class DesignParameterVM : IParameterVM
{
    public Type ParameterType => typeof(int);
    public string Title => "DesignTitle";
    public bool HasHelp => true;
    public string? Help => "DesignHelp";

    public object? ActualValue { get; private set; } = "DesignActualValue";

    public object? UserEntry { get; set; } = "DesignUserEntry";

    public bool HasMessage => true;
    public string? Message => "DesignMessage";

    public event PropertyChangedEventHandler? PropertyChanged;

    public void SetActualValue(object? value, bool updateUserEnteredValue)
    {
        throw new NotImplementedException();
    }
}
