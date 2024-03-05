using System.ComponentModel;

namespace MechanicsUI;

public class RenderOrNotVM : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(PropertyChangedEventArgs e) => PropertyChanged?.Invoke(this, e);

    public SimulationVM SimulationVM { get; }
    public Perspective Perspective { get; }
    private RenderVM? _nullableRenderVM;

    public RenderOrNotVM(SimulationVM simulationVM, Perspective perspective)
    {
        SimulationVM = simulationVM;
        Perspective = perspective;
    }

    private static readonly PropertyChangedEventArgs sNullableRenderVMChangedArgs = new(nameof(NullableRenderVM));
    public RenderVM? NullableRenderVM
    {
        get => _nullableRenderVM;
        set
        {
            if (_nullableRenderVM == value) return;

            var old = _nullableRenderVM;
            if (old != null)
            {
                old.Unhook();
            }

            _nullableRenderVM = value;

            OnPropertyChanged(sNullableRenderVMChangedArgs);
            OnPropertyChanged(sShouldRenderChangedArgs);
            OnPropertyChanged(sGridWidthChangedArgs);
            OnPropertyChanged(sGridHeightChangedArgs);
        }
    }

    private static readonly PropertyChangedEventArgs sShouldRenderChangedArgs = new(nameof(ShouldRender));
    public bool ShouldRender
    {
        get => NullableRenderVM != null;
        set
        {
            if (ShouldRender == value) return;
            NullableRenderVM = value ? new(SimulationVM, Perspective) : null;
        }
    }

    private static readonly PropertyChangedEventArgs sGridWidthChangedArgs = new(nameof(GridWidth));
    public double GridWidth => NullableRenderVM?.GridWidth ?? default;

    private static readonly PropertyChangedEventArgs sGridHeightChangedArgs = new(nameof(GridHeight));
    public double GridHeight => NullableRenderVM?.GridHeight ?? default;
}

public class DesignRenderOrNotVM : RenderOrNotVM
{
    public DesignRenderOrNotVM()
        : base(new DefaultSimulationVM(), Perspective.Orthogonal_FromAbove)
    {
        ShouldRender = true;
    }
}
