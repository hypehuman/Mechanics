using GuiByReflection.Models;
using System.ComponentModel;
using System.Reflection;

namespace GuiByReflection.ViewModels;

public interface ITypeVM : IHasGuiName, IHasGuiHelp
{
    public Type Model { get; }
}

public class TypeVM : ITypeVM
{
    /// <summary>
    /// Never needs to be raised, as bound properties never change.
    /// Interface is implemented to prevent WPF from trying to track changes on these properties,
    /// which could impact performance and cause memory leaks.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged { add { } remove { } }

    public Type Model { get; }
    public string ActualGuiName { get; }
    public string? ActualGuiHelp { get; }

    public TypeVM(Type model)
    {
        Model = model;

        ActualGuiName = Model.GetCustomAttribute<GuiNameAttribute>(false).GetActualGuiName(Model.Name);
        ActualGuiHelp = Model.GetCustomAttribute<GuiHelpAttribute>(false).GetActualGuiHelp();
    }
}
