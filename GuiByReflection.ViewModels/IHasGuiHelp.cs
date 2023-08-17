using System.ComponentModel;

namespace GuiByReflection.ViewModels;

public interface IHasGuiHelp : INotifyPropertyChanged
{
    bool HasHelp { get; }
    string? ActualGuiHelp { get; }
}
