using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MechanicsUI;

public class RenderOrNotVM : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public SimulationVM SimulationVM { get; }
    public Perspective Perspective { get; }

    public RenderVM? NullableRenderVM
    {
        get;
        set
        {
            var old = field;
            if (old != null)
            {
                old.PropertyChanged -= NullableRenderVM_OnPropertyChanged;
                old.Unhook();
            }

            field = value;

            OnPropertyChanged();
            OnPropertyChanged(nameof(ShouldRender));
            OnPropertyChanged(nameof(GridWidth));
            OnPropertyChanged(nameof(GridHeight));

            if (value != null)
                value.PropertyChanged += NullableRenderVM_OnPropertyChanged;
        }
    }

    public RenderOrNotVM(SimulationVM simulationVM, Perspective perspective)
    {
        SimulationVM = simulationVM;
        Perspective = perspective;
    }

    public bool ShouldRender
    {
        get => NullableRenderVM != null;
        set
        {
            NullableRenderVM = value ? new(SimulationVM, Perspective) : null;
        }
    }

    public double GridWidth => NullableRenderVM?.GridWidth ?? default;
    public double GridHeight => NullableRenderVM?.GridHeight ?? default;

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    private void NullableRenderVM_OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(GridWidth) || e.PropertyName == nameof(GridHeight))
        {
            OnPropertyChanged(e.PropertyName);
        }
    }
}
