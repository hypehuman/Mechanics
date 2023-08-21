using MechanicsCore;
using System;
using System.ComponentModel;
using System.Windows.Threading;

namespace MechanicsUI;

public class SimulationVM : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public Simulation Model { get; }
    public RenderVM RenderVM { get; }
    public string Title => GetTitleOrConfig(", ");
    public string Config => GetTitleOrConfig(Environment.NewLine);
    public IValidationTextBoxViewModel<int> StepsPerLeapVM { get; } = new StepsPerLeapTextBoxViewModel();

    private static readonly PropertyChangedEventArgs sStateSummaryChangedArgs = new(nameof(StateSummary));
    public string StateSummary => string.Join(Environment.NewLine, Model.GetStateSummaryLines());

    private static readonly PropertyChangedEventArgs sMinGlowRadiusFractionOfFrameChangedArgs = new(nameof(MinGlowRadiusFractionOfFrame));
    private double _minGlowRadiusFractionOfFrame = 0.002;
    public double MinGlowRadiusFractionOfFrame
    {
        get => _minGlowRadiusFractionOfFrame;
        set
        {
            _minGlowRadiusFractionOfFrame = value;
            PropertyChanged?.Invoke(this, sMinGlowRadiusFractionOfFrameChangedArgs);
            PropertyChanged?.Invoke(this, sMinGlowRadiusChangedArgs);
        }
    }

    private static readonly PropertyChangedEventArgs sMinGlowRadiusChangedArgs = new(nameof(MinGlowRadius));
    public double MinGlowRadius
    {
        get
        {
            // Minimum glow radius is a fraction of the longest straight path through the simulation bounds.
            var diagonalLength = (Model.DisplayBound1 - Model.DisplayBound0).Length;
            var minGlowRadius = _minGlowRadiusFractionOfFrame * diagonalLength;
            return minGlowRadius;
        }
    }

    public string GlowRatioTooltip =>
        "Increase this to improve the visibility of small bodies." + Environment.NewLine +
        "Set this to 0 to display actual sizes.";

    public string LeapTimeText =>
        "Leap time: " + Simulation.TimeToString(StepsPerLeapVM.CurrentValue * Model.PhysicsConfig.StepTime);

    private static readonly PropertyChangedEventArgs sIsAutoLeapingChangedArgs = new(nameof(IsAutoLeaping));
    private bool _isAutoLeaping;
    public bool IsAutoLeaping
    {
        get => _isAutoLeaping;
        set
        {
            _isAutoLeaping = value;
            PropertyChanged?.Invoke(this, sIsAutoLeapingChangedArgs);
            DoAutoLeap(Dispatcher.CurrentDispatcher);
        }
    }

    public event EventHandler? DoingAutoLeap;

    public SimulationVM(Simulation model)
    {
        Model = model;
        RenderVM = new RenderVM(this, Perspective.Orthogonal_FromAbove);
        StepsPerLeapVM.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(StepsPerLeapVM.CurrentValue))
                PropertyChanged?.Invoke(this, new(nameof(LeapTimeText)));
        };
    }

    private string GetTitleOrConfig(string separator)
    {
        return string.Join(separator, Model.GetConfigLines());
    }

    public void LeapAndRefresh()
    {
        if (!Model.TryLeap(StepsPerLeapVM.CurrentValue))
        {
            IsAutoLeaping = false;
        }

        RefreshSim();
    }

    private void DoAutoLeap(Dispatcher dispatcher)
    {
        if (!_isAutoLeaping)
        {
            return;
        }

        DoingAutoLeap?.Invoke(this, EventArgs.Empty);

        // Check again; the event subscriber may have turned auto-leap off.
        if (!_isAutoLeaping)
        {
            return;
        }

        LeapAndRefresh();
        dispatcher.InvokeAsync(() => DoAutoLeap(dispatcher), DispatcherPriority.Background);
    }

    private void RefreshSim()
    {
        PropertyChanged?.Invoke(this, sStateSummaryChangedArgs);

        RenderVM.RefreshSim();
    }
}

public class DefaultSimulationVM : SimulationVM
{
    public DefaultSimulationVM()
        : base(new(ScenarioGallery.Default()))
    {
    }
}
