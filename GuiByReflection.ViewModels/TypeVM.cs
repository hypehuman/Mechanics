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
    public Type Model { get; }
    public string ActualGuiName { get; }
    public string? ActualGuiHelp { get; }
    public event PropertyChangedEventHandler? PropertyChanged;

    public TypeVM(Type model)
    {
        Model = model;

        ActualGuiName = Model.GetCustomAttribute<GuiNameAttribute>(false).GetActualGuiName(Model.Name);
        ActualGuiHelp = Model.GetCustomAttribute<GuiHelpAttribute>(false).GetActualGuiHelp();
    }
}
