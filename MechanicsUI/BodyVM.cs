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
    public RenderVM RenderVM { get; }
    public event PropertyChangedEventHandler? PropertyChanged;

    public BodyVM(Body model, RenderVM renderVM)
    {
        Model = model;
        RenderVM = renderVM;
    }

    private SimulationVM SimulationVM => RenderVM.SimulationVM;

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

    private static readonly PropertyChangedEventArgs sLabelTextChangedArgs = new(nameof(LabelText));
    private string _labelText;
    public string LabelText
    {
        get => _labelText;
        private set
        {
            if (value == _labelText)
                return;
            _labelText = value;
            PropertyChanged?.Invoke(this, sLabelTextChangedArgs);
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
        private set
        {
            if (value == _trueRadiusOverGlowRadius)
                return;
            _trueRadiusOverGlowRadius = value;
            PropertyChanged?.Invoke(this, sTrueRadiusOverGlowRadiusChangedArgs);
        }
    }

    private Vector3D ComputePositionInPerspective(Func<Body, double> getGlowRadius)
    {
        var pushedPos = !SimulationVM.GlowPush || SimulationVM.MinGlowRadius == 0
            ? Model.Position
            : Model.ComputePushedPosition(SimulationVM.Model.Bodies, getGlowRadius);
        return RenderVM.Perspective.SimToPanel(pushedPos);
    }

    private Point ComputePanelCenterXY(Vector3D positionInPerspective)
    {
        return new(positionInPerspective.X, positionInPerspective.Y);
    }

    private int ComputePanelZIndex(Vector3D positionInPerspective)
    {
        // Compute Z scaled to the range [0,1] relative to the simulation bounds.
        var panelZUnscaled = positionInPerspective.Z;
        RenderVM.Sort(RenderVM.PanelDisplayBound0.Z, RenderVM.PanelDisplayBound1.Z, out var minPanelZUnscaled, out var maxPanelZUnscaled);
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

    public void Refresh_1_of_2()
    {
        RefreshRadii();
    }

    /// <summary>
    /// Because of "glow push", where we display bodies as farther apart than they really are,
    /// the displayed position depends on each body knowing its glow radius.
    /// </summary>
    public void Refresh_2_of_2(Func<Body, double> getGlowRadius)
    {
        RefreshPosition(getGlowRadius);

        RefreshLabel();
        RefreshColor();
    }

    /// <inheritdoc <see cref="Refresh_2_of_2"/>/>
    private void RefreshRadii()
    {
        if (SimulationVM.MinGlowRadius == 0)
        {
            GlowRadius = Model.Radius;
            TrueRadiusOverGlowRadius = 1;
        }
        else
        {
            GlowRadius = Model.ComputeGlowRadius(SimulationVM.MinGlowRadius);
            TrueRadiusOverGlowRadius = Model.Radius / GlowRadius;
        }
    }

    private void RefreshPosition(Func<Body, double> getGlowRadius)
    {
        var positionInPerspective = ComputePositionInPerspective(getGlowRadius);
        PanelCenterXY = ComputePanelCenterXY(positionInPerspective);
        PanelZIndex = ComputePanelZIndex(positionInPerspective);
    }

    private void RefreshLabel()
    {
        LabelText = Model.Name;
    }

    private void RefreshColor()
    {
        WinMediaColor = ComputeWinMediaColor();
    }
}
