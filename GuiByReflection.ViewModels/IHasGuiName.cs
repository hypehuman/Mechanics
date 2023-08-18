using GuiByReflection.Models;
using System.ComponentModel;

namespace GuiByReflection.ViewModels;

public interface IHasGuiName : INotifyPropertyChanged
{
    string ActualGuiName { get; }
}

public static class HasGuiNameExtensions
{
    public static string GetActualGuiName(this GuiNameAttribute? attr, string? codeName)
    {
        var explicitGuiName = attr?.Value;
        return !string.IsNullOrWhiteSpace(explicitGuiName) ? explicitGuiName : codeName ?? "[unnamed]";
    }

    public static string GetActualGuiName(this IEnumerable<GuiNameAttribute?> attributes, string? codeName)
    {
        foreach (var attr in attributes)
        {
            var explicitGuiName = attr?.Value;
            if (!string.IsNullOrWhiteSpace(explicitGuiName))
            {
                return explicitGuiName;
            }
        }

        return codeName ?? "[unnamed]";
    }
}
