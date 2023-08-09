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

    public BodyVM(Body model, SimulationVM simulationVM)
    {
        Model = model;
        SimulationVM = simulationVM;

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

    public Point CenterXY => Model.Exists ? new(Model.Position.X, Model.Position.Y) : new(double.NaN, double.NaN);

    public int PanelZIndex
    {
        get
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
    }

    public Color WinMediaColor => GetWinMediaColor(Model.Color);

    private static Color GetWinMediaColor(BodyColor bc)
    {
        // 75% opacity lets us see to the next object behind
        return Color.FromArgb(192, bc.R, bc.G, bc.B);
    }

    public double GlowRadius => Model.ComputeGlowRadius(SimulationVM.MinGlowRadius);
    public double TrueRadiusOverGlowRadius => Model.Radius / GlowRadius;

    public event PropertyChangedEventHandler? PropertyChanged;
    private static readonly PropertyChangedEventArgs CenterXYChangedArgs = new(nameof(CenterXY));
    private static readonly PropertyChangedEventArgs PanelZIndexChangedArgs = new(nameof(PanelZIndex));
    private static readonly PropertyChangedEventArgs WinMediaColorChangedArgs = new(nameof(WinMediaColor));
    private static readonly PropertyChangedEventArgs GlowRadiusChangedArgs = new(nameof(GlowRadius));
    private static readonly PropertyChangedEventArgs TrueRadiusOverGlowRadiusChangedArgs = new(nameof(TrueRadiusOverGlowRadius));

    public void Refresh()
    {
        PropertyChanged?.Invoke(this, CenterXYChangedArgs);
        PropertyChanged?.Invoke(this, PanelZIndexChangedArgs);
        PropertyChanged?.Invoke(this, WinMediaColorChangedArgs);
        RefreshRadii();
    }

    private void RefreshRadii()
    {
        PropertyChanged?.Invoke(this, GlowRadiusChangedArgs);
        PropertyChanged?.Invoke(this, TrueRadiusOverGlowRadiusChangedArgs);
    }
}
