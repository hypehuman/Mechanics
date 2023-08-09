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

    public IUserEntryVM UserEntryVM { get; } = new DesignUserEntryVM();
    public object? ActualValue { get; private set; } = "DesignActualValue";

    public bool HasMessage => true;
    public string? Message => "DesignMessage";

    public event PropertyChangedEventHandler? PropertyChanged;

    public void SetActualValue(object? value, bool isOriginalAction)
    {
        throw new NotImplementedException();
    }
}
