using MathNet.Spatial.Euclidean;
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

    private Vector3D PositionOnPanel => SimulationVM.Perspective.SimToPanel(Model.Position);

    private static readonly PropertyChangedEventArgs sPanelCenterXYChangedArgs = new(nameof(PanelCenterXY));
    private Point _panelCenterXY;
    public Point PanelCenterXY
    {
        get => _panelCenterXY;
        private set
        {
            if (_panelCenterXY == value)
                return;
            _panelCenterXY = value;
            PropertyChanged?.Invoke(this, sPanelCenterXYChangedArgs);
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

    private Point ComputePanelCenterXY()
    {
        var panelPosition = PositionOnPanel;
        return new(panelPosition.X, panelPosition.Y);
    }

    private int ComputePanelZIndex()
    {
        // Compute Z scaled to the range [0,1] relative to the simulation bounds.
        var panelZUnscaled = PositionOnPanel.Z;
        SimulationVM.Sort(SimulationVM.PanelDisplayBound0.Z, SimulationVM.PanelDisplayBound1.Z, out var minPanelZUnscaled, out var maxPanelZUnscaled);
        var panelZScaled = (panelZUnscaled - minPanelZUnscaled) / (maxPanelZUnscaled - minPanelZUnscaled);

        // special cases if out of bounds
        if (panelZScaled < 0)
            return int.MinValue;
        if (panelZScaled > 1)
            return int.MaxValue;

        // Compute Z scaled to half the range of int.
        // Using only half the range to avoid conversion exceptions.
        var minOut = int.MinValue / 2d;
        var maxOut = int.MaxValue / 2d;
        var doubleOut = panelZScaled * (maxOut - minOut) + minOut;
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
        PanelCenterXY = ComputePanelCenterXY();
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
