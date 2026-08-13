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
    public IValidationTextBoxVM<int> StepsPerLeapVM { get; } = new StepsPerLeapTextBoxVM();

    private static readonly PropertyChangedEventArgs sStateSummaryChangedArgs = new(nameof(StateSummary));
    public string StateSummary => string.Join(Environment.NewLine, Model.GetStateSummaryLines());

    /// <summary>
    /// Explains <see cref="GlowFactor"/> and its relationship with <see cref="MinGlowRadius"/>.
    /// </summary>
    public static string GlowFactorTooltip =>
        "Increase this to improve the visibility of small bodies." + Environment.NewLine +
        "Set this to 0 to display actual sizes." + Environment.NewLine +
        "This value is the ratio of the minimum glow radius to the length of the diagonal of the scenario's bounding box.";

    private static readonly PropertyChangedEventArgs sGlowFactorChangedArgs = new(nameof(GlowFactor));
    /// <summary>
    /// See explanation in <see cref="GlowFactorTooltip"/>.
    /// </summary>
    public double GlowFactor
    {
        get;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, sGlowFactorChangedArgs);
            PropertyChanged?.Invoke(this, sGlowFactor_TextChangedArgs);
            PropertyChanged?.Invoke(this, sMinGlowRadiusChangedArgs);
        }
    } = 0.002;

    /// <summary>
    /// Minimum allowed value of <see cref="GlowFactor"/>.
    /// </summary>
    public static double GlowFactor_Min => 0;

    /// <summary>
    /// Maximum allowed value of <see cref="GlowFactor"/>.
    /// </summary>
    public static double GlowFactor_Max => 0.01;

    /// <summary>
    /// Smallest allowed slider increment of <see cref="GlowFactor"/>.
    /// </summary>
    public static double GlowFactor_Epsilon => 0.0001;

    /// <summary>
    /// String format for displaying <see cref="GlowFactor"/>.
    /// Should display enough precision to show <see cref="GlowFactor_Epsilon"/>.
    /// </summary>
    private static string GlowFactor_StringFormat => "0.0000";

    private static readonly PropertyChangedEventArgs sGlowFactor_TextChangedArgs = new(nameof(GlowFactor_Text));
    public string GlowFactor_Text
    {
        get => GlowFactor.ToString(GlowFactor_StringFormat);
    }

    private static readonly PropertyChangedEventArgs sMinGlowRadiusChangedArgs = new(nameof(MinGlowRadius));
    /// <summary>
    /// See explanation in <see cref="GlowFactorTooltip"/>.
    /// </summary>
    public double MinGlowRadius
    {
        get
        {
            var diagonalLength = (Model.DisplayBound1 - Model.DisplayBound0).Length;
            var minGlowRadius = GlowFactor * diagonalLength;
            return minGlowRadius;
        }
    }

    public string LeapTimeText =>
        "Leap time: " + Simulation.TimeToString(StepsPerLeapVM.CurrentValue * Model.PhysicsConfig.StepTime);

    private static readonly PropertyChangedEventArgs sIsAutoLeapingChangedArgs = new(nameof(IsAutoLeaping));
    public bool IsAutoLeaping
    {
        get;
        set
        {
            field = value;
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
        if (!IsAutoLeaping)
        {
            return;
        }

        DoingAutoLeap?.Invoke(this, EventArgs.Empty);

        // Check again; the event subscriber may have turned auto-leap off.
        if (!IsAutoLeaping)
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

    public void ExportBodyData(string filePath)
    {
        var rows = Model.GetBodyData();
        CsvHelper.WriteCsv(filePath, rows);
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
