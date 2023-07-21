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
    public string Title { get; }
    public BodyVM[] BodyVMs { get; }
    public string SimTimeString => Model.GetTimeString();
    public double CanvasTranslateX { get; private set; }
    public double CanvasTranslateY { get; private set; }
    public double CanvasScaleX { get; private set; } = 1;
    public double CanvasScaleY { get; private set; } = -1;
    bool _isAutoLeaping;
    public bool IsAutoLeaping
    {
        get => _isAutoLeaping;
        set
        {
            _isAutoLeaping = value;
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

    public SimulationVM(Simulation model, string name)
    {
        Model = model;
        Title = name + (model is RandomSimulation rnd ? $" seed: {rnd.Seed}" : null);
        BodyVMs = Model.Bodies.Select(b => new BodyVM(b, Model)).ToArray();
    }

    public void LeapAndRefresh()
    {
        Model.Leap();
        RefreshSim();
    }

    private void DoAutoLeap(Dispatcher dispatcher)
    {
        if (!_isAutoLeaping)
        {
            return;
        }

        LeapAndRefresh();
        dispatcher.InvokeAsync(() => DoAutoLeap(dispatcher), DispatcherPriority.Background);
    }


    public event PropertyChangedEventHandler? PropertyChanged;
    private static readonly PropertyChangedEventArgs SimTimeStringChangedArgs = new(nameof(SimTimeString));
    private static readonly PropertyChangedEventArgs CanvasTranslateXChangedArgs = new(nameof(CanvasTranslateX));
    private static readonly PropertyChangedEventArgs CanvasTranslateYChangedArgs = new(nameof(CanvasTranslateY));
    private static readonly PropertyChangedEventArgs CanvasScaleXChangedArgs = new(nameof(CanvasScaleX));
    private static readonly PropertyChangedEventArgs CanvasScaleYChangedArgs = new(nameof(CanvasScaleY));

    private void RefreshSim()
    {
        PropertyChanged?.Invoke(this, SimTimeStringChangedArgs);
        foreach (var bodyVM in BodyVMs)
        {
            bodyVM.Refresh();
        }
    }

    private void RefreshBounds()
    {
        Sort(Model.DisplayBound0[0], Model.DisplayBound1[0], out var xMin, out var xMax);
        Sort(Model.DisplayBound0[1], Model.DisplayBound1[1], out var yMin, out var yMax);
        var systemWidth = xMax - xMin;
        var systemHeight = yMax - yMin;
        var xScale = _availableSizePix.Width / systemWidth;
        var yScale = _availableSizePix.Height / systemHeight;
        xScale = Math.Min(xScale, yScale);
        yScale = -xScale;
        CanvasScaleX = xScale;
        CanvasScaleY = yScale;
        CanvasTranslateX = -xMin * xScale;
        CanvasTranslateY = -yMin * yScale;
        PropertyChanged?.Invoke(this, CanvasTranslateXChangedArgs);
        PropertyChanged?.Invoke(this, CanvasTranslateYChangedArgs);
        PropertyChanged?.Invoke(this, CanvasScaleXChangedArgs);
        PropertyChanged?.Invoke(this, CanvasScaleYChangedArgs);
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
        : base(Simulations.Default(), "Default")
    {
    }
}
