using MechanicsCore;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace MechanicsUI;

public class BodyVM : INotifyPropertyChanged
{
    public Body Model { get; }
    public SimulationVM SimulationVM { get; }
    public event PropertyChangedEventHandler? PropertyChanged;

    public BodyVM(Body model, SimulationVM simulationVM)
    {
        Model = model;
        SimulationVM = simulationVM;

        Refresh();
        SimulationVM.PropertyChanged += SimulationVM_PropertyChanged;
    }

    private void SimulationVM_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MechanicsUI.SimulationVM.MinGlowRadius))
        {
            RefreshRadii();
        }
    }

    private Simulation Simulation => SimulationVM.Model;

    private static readonly PropertyChangedEventArgs sCenterXYChangedArgs = new(nameof(CenterXY));
    private Point _centerXY;
    public Point CenterXY
    {
        get => _centerXY;
        private set
        {
            if (_centerXY == value)
                return;
            _centerXY = value;
            PropertyChanged?.Invoke(this, sCenterXYChangedArgs);
        }
    }

    private static readonly PropertyChangedEventArgs sPanelZIndexChangedArgs = new(nameof(PanelZIndex));
    private int _panelZIndex;
    public int PanelZIndex
    {
        get => _panelZIndex;
        private set
        {
            if (_panelZIndex == value)
                return;
            _panelZIndex = value;
            PropertyChanged?.Invoke(this, sPanelZIndexChangedArgs);
        }
    }

    private static readonly PropertyChangedEventArgs sWinMediaColorChangedArgs = new(nameof(WinMediaColor));
    private Color _winMediaColor;
    public Color WinMediaColor
    {
        get => _winMediaColor;
        private set
        {
            if (_winMediaColor == value)
                return;
            _winMediaColor = value;
            PropertyChanged?.Invoke(this, sWinMediaColorChangedArgs);
        }
    }

    private static readonly PropertyChangedEventArgs sGlowRadiusChangedArgs = new(nameof(GlowRadius));
    private double _glowRadius;
    public double GlowRadius
    {
        get => _glowRadius;
        private set
        {
            if (_glowRadius == value)
                return;
            _glowRadius = value;
            PropertyChanged?.Invoke(this, sGlowRadiusChangedArgs);
        }
    }

    private static readonly PropertyChangedEventArgs sTrueRadiusOverGlowRadiusChangedArgs = new(nameof(TrueRadiusOverGlowRadius));
    private double _trueRadiusOverGlowRadius;
    public double TrueRadiusOverGlowRadius
    {
        get => _trueRadiusOverGlowRadius;
        set
        {
            if (value == _trueRadiusOverGlowRadius)
                return;
            _trueRadiusOverGlowRadius = value;
            PropertyChanged?.Invoke(this, sTrueRadiusOverGlowRadiusChangedArgs);
        }
    }

    private Point ComputeCenterXY()
    {
        return Model.Exists ? new(Model.Position.X, Model.Position.Y) : new(double.NaN, double.NaN);
    }

    private int ComputePanelZIndex()
    {
        if (!Model.Exists)
        {
            return 0;
        }

        // Compute Z scaled to the range [0,1] relative to the simulation bounds.
        var z = Model.Position.Z;
        SimulationVM.Sort(Simulation.DisplayBound0.Z, Simulation.DisplayBound1.Z, out var zMin, out var zMax);
        var relativeZ = (z - zMin) / (zMax - zMin);

        // special cases if out of bounds
        if (relativeZ < 0)
            return int.MinValue;
        if (relativeZ > 1)
            return int.MaxValue;

        // Compute Z scaled to half the range of int.
        // Using only half the range to avoid conversion exceptions.
        var minOut = int.MinValue / 2d;
        var maxOut = int.MaxValue / 2d;
        var doubleOut = relativeZ * (maxOut - minOut) + minOut;
        return
            double.IsNaN(doubleOut) ? 0 : // z was probably NaN
            doubleOut < int.MinValue + 1 ? int.MinValue : // z was probably negative infinity
            doubleOut > int.MaxValue - 1 ? int.MaxValue : // z was probably positive infinity
            Convert.ToInt32(doubleOut);
    }

    private Color ComputeWinMediaColor()
    {
        var bc = Model.Color;
        // 75% opacity lets us see to the next object behind
        return Color.FromArgb(192, bc.R, bc.G, bc.B);
    }

    private double ComputeGlowRadius()
    {
        return Model.ComputeGlowRadius(SimulationVM.MinGlowRadius);
    }

    private double ComputeTrueRadiusOverGlowRadius()
    {
        return Model.Radius / GlowRadius;
    }

    public void Refresh()
    {
        CenterXY = ComputeCenterXY();
        PanelZIndex = ComputePanelZIndex();
        WinMediaColor = ComputeWinMediaColor();

        RefreshRadii();
    }

    private void RefreshRadii()
    {
        GlowRadius = ComputeGlowRadius();
        TrueRadiusOverGlowRadius = ComputeTrueRadiusOverGlowRadius();
    }
}
