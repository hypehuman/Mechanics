using GuiByReflection.Models;
using System.ComponentModel;

namespace GuiByReflection.ViewModels;

public interface IHasGuiHelp : INotifyPropertyChanged
{
    string? ActualGuiHelp { get; }
}

public static class HasGuiHelpExtensions
{
    public static string? GetActualGuiHelp(this GuiHelpAttribute? attr)
    {
        return attr?.Value;
    }

    public static string? GetActualGuiHelp(this IEnumerable<GuiHelpAttribute?> attributes)
    {
        foreach (var attr in attributes)
        {
            var explicitGuiHelp = attr?.Value;
            if (!string.IsNullOrWhiteSpace(explicitGuiHelp))
            {
                return explicitGuiHelp;
            }
        }

        return null;
    }
}
