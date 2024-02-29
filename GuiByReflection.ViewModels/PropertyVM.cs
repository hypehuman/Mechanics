using GuiByReflection.Models;
using System.ComponentModel;
using System.Reflection;

namespace GuiByReflection.ViewModels;

public interface IPropertyVM : IHasGuiName, IHasGuiHelp
{
    object? GetValue();
}

public class PropertyVM : IPropertyVM
{
    /// <summary>
    /// Never needs to be raised, as <see cref="ActualGuiName"/> <see cref="ActualGuiHelp"/> never change.
    /// Interface is implemented to prevent WPF from trying to track changes on these properties,
    /// which could impact performance and cause memory leaks.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged { add { } remove { } }

    private readonly object? _object;
    private readonly PropertyInfo _propertyInfo;

    public string ActualGuiName { get; }
    public string? ActualGuiHelp { get; }

    /// <param name="obj">The object on which to call the property; null if the property is static.</param>
    public PropertyVM(object? obj, PropertyInfo propertyInfo)
    {
        _object = obj;
        _propertyInfo = propertyInfo;
        ActualGuiName = _propertyInfo.GetCustomAttribute<GuiNameAttribute>(false).GetActualGuiName(_propertyInfo.Name);
        ActualGuiHelp = _propertyInfo.GetCustomAttribute<GuiHelpAttribute>(false).GetActualGuiHelp();
    }

    public object? GetValue() => _propertyInfo.GetValue(_object);
}
