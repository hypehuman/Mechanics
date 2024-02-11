using MechanicsCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace MechanicsUI;

public class SimulationVM : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public Simulation Model { get; }
    public RenderOrNotVM AboveRenderVM { get; }
    public RenderOrNotVM FrontRenderVM { get; }
    public RenderOrNotVM RightRenderVM { get; }
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
        AboveRenderVM = new(this, Perspective.Orthogonal_FromAbove) { ShouldRender = true };
        FrontRenderVM = new(this, Perspective.Orthogonal_FromFront);
        RightRenderVM = new(this, Perspective.Orthogonal_FromRight);
        StepsPerLeapVM.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(StepsPerLeapVM.CurrentValue))
                PropertyChanged?.Invoke(this, new(nameof(LeapTimeText)));
        };
    }

    public IEnumerable<RenderOrNotVM> RenderVMs
    {
        get
        {
            yield return AboveRenderVM;
            yield return FrontRenderVM;
            yield return RightRenderVM;
        }
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

        foreach (var rvm in RenderVMs)
        {
            rvm.NullableRenderVM?.RefreshSim();
        }
    }
}

public class SimulationVM_RenderGridSizeConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        double max = 0;
        foreach (var item in values)
            if (item is double value)
                max = Math.Max(max, value);
        return new GridLength(max, GridUnitType.Star);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class SimulationVM_SpacerGridSizeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return new GridLength(true.Equals(value) ? 5 : 0);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class DefaultSimulationVM : SimulationVM
{
    public DefaultSimulationVM()
        : base(new(ScenarioGallery.Default()))
    {
    }
}
