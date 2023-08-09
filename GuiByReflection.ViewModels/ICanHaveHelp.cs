using System.ComponentModel;

namespace GuiByReflection.ViewModels;

public interface ICanHaveHelp : INotifyPropertyChanged
{
    bool HasHelp { get; }
    string? Help { get; }
}
