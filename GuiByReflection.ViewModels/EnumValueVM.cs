using GuiByReflection.Models;
using System.ComponentModel;
using System.Reflection;

namespace GuiByReflection.ViewModels;

public interface IEnumValueVM : IHasGuiName, IHasGuiHelp
{
    Enum Value { get; }
}

public class EnumValueVM : IEnumValueVM
{
    /// <summary>
    /// Never needs to be raised, as bound properties never change.
    /// Interface is implemented to prevent WPF from trying to track changes on these properties,
    /// which could impact performance and cause memory leaks.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged { add { } remove { } }

    public Enum Value { get; }
    public string ActualGuiName { get; }
    public string? ActualGuiHelp { get; }

    public EnumValueVM(Enum value)
    {
        Value = value;
        var codeName = Value.ToString();
        var memberInfos = Value.GetType().GetMember(codeName);
        ActualGuiName =
            memberInfos.SelectMany(mi => mi.GetCustomAttributes<GuiNameAttribute>())
            .GetActualGuiName(codeName);
        ActualGuiHelp =
            memberInfos.SelectMany(mi => mi.GetCustomAttributes<GuiHelpAttribute>())
            .GetActualGuiHelp();
    }
}
