using MathNet.Spatial.Euclidean;
using MechanicsCore;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace MechanicsUI;

public class SimulationVM : INotifyPropertyChanged
{
    public static Perspective Perspective => Perspective.Orthogonal_FromAbove;

    public Simulation Model { get; }
    public string Title => GetTitleOrConfig(", ");
    public string Config => GetTitleOrConfig(Environment.NewLine);
    public IValidationTextBoxViewModel<int> StepsPerLeapVM { get; } = new StepsPerLeapTextBoxViewModel();

    public ObservableCollection<BodyVM> BodyVMs { get; }
    public string StateSummary => string.Join(Environment.NewLine, Model.GetStateSummaryLines());
    public Vector3D PanelDisplayBound0 => Perspective.SimToPanel(Model.DisplayBound0);
    public Vector3D PanelDisplayBound1 => Perspective.SimToPanel(Model.DisplayBound1);
    public double CanvasTranslateX { get; private set; }
    public double CanvasTranslateY { get; private set; }
    public double CanvasScale { get; private set; } = 1;

    private double _minGlowRadiusFractionOfFrame = 0.002;
    public double MinGlowRadiusFractionOfFrame
    {
        get => _minGlowRadiusFractionOfFrame;
        set
        {
            _minGlowRadiusFractionOfFrame = value;
            PropertyChanged?.Invoke(this, MinGlowRadiusFractionOfFrameChangedArgs);
            PropertyChanged?.Invoke(this, MinGlowRadiusChangedArgs);
        }
    }

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

    private bool _isAutoLeaping;
    public bool IsAutoLeaping
    {
        get => _isAutoLeaping;
        set
        {
            _isAutoLeaping = value;
            PropertyChanged?.Invoke(this, IsAutoLeapingChangedArgs);
            DoAutoLeap(Dispatcher.CurrentDispatcher);
        }
    }

    private Size _availableSizePix;
    public Size AvailableSizePix
    {
        set
        {
            _availableSizePix = value;
            RefreshBounds();
        }
    }

    public event EventHandler? DoingAutoLeap;

    public SimulationVM(Simulation model)
    {
        Model = model;
        BodyVMs = new(Model.Bodies.Select(b => new BodyVM(b, this)));
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

    public event PropertyChangedEventHandler? PropertyChanged;
    private static readonly PropertyChangedEventArgs StateSummaryChangedArgs = new(nameof(StateSummary));
    private static readonly PropertyChangedEventArgs CanvasTranslateXChangedArgs = new(nameof(CanvasTranslateX));
    private static readonly PropertyChangedEventArgs CanvasTranslateYChangedArgs = new(nameof(CanvasTranslateY));
    private static readonly PropertyChangedEventArgs CanvasScaleChangedArgs = new(nameof(CanvasScale));
    private static readonly PropertyChangedEventArgs MinGlowRadiusFractionOfFrameChangedArgs = new(nameof(MinGlowRadiusFractionOfFrame));
    private static readonly PropertyChangedEventArgs MinGlowRadiusChangedArgs = new(nameof(MinGlowRadius));
    private static readonly PropertyChangedEventArgs IsAutoLeapingChangedArgs = new(nameof(IsAutoLeaping));

    private void RefreshSim()
    {
        PropertyChanged?.Invoke(this, StateSummaryChangedArgs);

        for (var i = BodyVMs.Count - 1; i >= 0; i--)
        {
            var bodyVM = BodyVMs[i];
            if (bodyVM.Model.Exists)
            {
                bodyVM.Refresh();
            }
            else
            {
                BodyVMs.RemoveAt(i);
            }
        }
    }

    private void RefreshBounds()
    {
        var panelBound0 = PanelDisplayBound0;
        var panelBound1 = PanelDisplayBound1;
        Sort(panelBound0.X, panelBound1.X, out var xMin, out var xMax);
        Sort(panelBound0.Y, panelBound1.Y, out var yMin, out var yMax);
        var systemWidth = xMax - xMin;
        var systemHeight = yMax - yMin;
        var xScale = _availableSizePix.Width / systemWidth;
        var yScale = _availableSizePix.Height / systemHeight;
        CanvasScale = Math.Min(xScale, yScale);
        CanvasTranslateX = -(xMin + xMax) / 2 * xScale;
        CanvasTranslateY = -(yMin + yMax) / 2 * yScale;
        PropertyChanged?.Invoke(this, CanvasTranslateXChangedArgs);
        PropertyChanged?.Invoke(this, CanvasTranslateYChangedArgs);
        PropertyChanged?.Invoke(this, CanvasScaleChangedArgs);
    }

    public static void Sort(double a, double b, out double min, out double max)
    {
        if (a > b)
        {
            min = b;
            max = a;
        }
        else
        {
            min = a;
            max = b;
        }
    }
}

public class DefaultSimulationVM : SimulationVM
{
    public DefaultSimulationVM()
        : base(new(ScenarioGallery.Default()))
    {
    }
}
