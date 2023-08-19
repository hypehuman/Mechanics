using MechanicsCore;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace MechanicsUI;

public class SimulationVM : INotifyPropertyChanged
{
    public Simulation Model { get; }
    public string Title => GetTitleOrConfig(", ");
    public string Config => GetTitleOrConfig(Environment.NewLine);
    public IValidationTextBoxViewModel<int> StepsPerLeapVM { get; } = new StepsPerLeapTextBoxViewModel();

    public BodyVM[] BodyVMs { get; }
    public string StateSummary => string.Join(Environment.NewLine, Model.GetStateSummaryLines());
    public double CanvasTranslateX { get; private set; }
    public double CanvasTranslateY { get; private set; }
    public double CanvasScaleX { get; private set; } = 1;
    public double CanvasScaleY { get; private set; } = -1;
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
            // Minimum glow radius is a fraction of the entire frame's width or height, whichever is larger.
            var frameWidth = Math.Abs(Model.DisplayBound1.X - Model.DisplayBound0.X);
            var frameHeight = Math.Abs(Model.DisplayBound1.Y - Model.DisplayBound0.Y);
            var minGlowRadius = _minGlowRadiusFractionOfFrame * Math.Max(frameWidth, frameHeight);
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
        BodyVMs = Model.Bodies.Select(b => new BodyVM(b, this)).ToArray();
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
    private static readonly PropertyChangedEventArgs CanvasScaleXChangedArgs = new(nameof(CanvasScaleX));
    private static readonly PropertyChangedEventArgs CanvasScaleYChangedArgs = new(nameof(CanvasScaleY));
    private static readonly PropertyChangedEventArgs MinGlowRadiusFractionOfFrameChangedArgs = new(nameof(MinGlowRadiusFractionOfFrame));
    private static readonly PropertyChangedEventArgs MinGlowRadiusChangedArgs = new(nameof(MinGlowRadius));
    private static readonly PropertyChangedEventArgs IsAutoLeapingChangedArgs = new(nameof(IsAutoLeaping));

    private void RefreshSim()
    {
        PropertyChanged?.Invoke(this, StateSummaryChangedArgs);

        foreach (var bodyVM in BodyVMs)
        {
            bodyVM.Refresh();
        }
    }

    private void RefreshBounds()
    {
        Sort(Model.DisplayBound0.X, Model.DisplayBound1.X, out var xMin, out var xMax);
        Sort(Model.DisplayBound0.Y, Model.DisplayBound1.Y, out var yMin, out var yMax);
        var systemWidth = xMax - xMin;
        var systemHeight = yMax - yMin;
        var xScale = _availableSizePix.Width / systemWidth;
        var yScale = _availableSizePix.Height / systemHeight;
        xScale = Math.Min(xScale, yScale);
        yScale = -xScale;
        CanvasScaleX = xScale;
        CanvasScaleY = yScale;
        CanvasTranslateX = -(xMin + xMax) / 2 * xScale;
        CanvasTranslateY = -(yMin + yMax) / 2 * yScale;
        PropertyChanged?.Invoke(this, CanvasTranslateXChangedArgs);
        PropertyChanged?.Invoke(this, CanvasTranslateYChangedArgs);
        PropertyChanged?.Invoke(this, CanvasScaleXChangedArgs);
        PropertyChanged?.Invoke(this, CanvasScaleYChangedArgs);
        PropertyChanged?.Invoke(this, MinGlowRadiusChangedArgs);
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
