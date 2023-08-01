using MechanicsCore;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace MechanicsUI;

public class BodyVM : INotifyPropertyChanged
{
    public Body Model { get; }
    public Simulation Simulation { get; }
    public double RadiusPix => Model.DisplayRadius;
    private BodyColor _latestColor; // brushes are expensive, so only make a new one if necessary
    public Brush Fill { get; private set; }

    public BodyVM(Body model, Simulation simulation)
    {
        Model = model;
        Simulation = simulation;
        Fill = MakeBrush(Model.Color);
    }

    private static Brush MakeBrush(BodyColor bc)
    {
        var winMediaColor = Color.FromRgb(bc.R, bc.G, bc.B);
        var brush = new SolidColorBrush(winMediaColor);
        brush.Freeze();
        return brush;
    }

    public Point CenterPix => Model.Exists ? new(Model.Position.X, Model.Position.Y) : new(double.NaN, double.NaN);

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
            var intOut = Convert.ToInt32(doubleOut);
            return intOut;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private static readonly PropertyChangedEventArgs CenterPointChangedArgs = new(nameof(CenterPix));
    private static readonly PropertyChangedEventArgs PanelZIndexChangedArgs = new(nameof(PanelZIndex));
    private static readonly PropertyChangedEventArgs RadiusPixChangedArgs = new(nameof(RadiusPix));
    private static readonly PropertyChangedEventArgs FillChangedArgs = new(nameof(Fill));

    public void Refresh()
    {
        PropertyChanged?.Invoke(this, CenterPointChangedArgs);
        PropertyChanged?.Invoke(this, PanelZIndexChangedArgs);
        PropertyChanged?.Invoke(this, RadiusPixChangedArgs);
        RefreshColor();
    }

    private void RefreshColor()
    {
        var newColor = Model.Color;
        if (newColor == _latestColor)
        {
            // Brushes are expensive, so only make a new one if necessary.
            return;
        }

        _latestColor = newColor;
        Fill = MakeBrush(newColor);
        PropertyChanged?.Invoke(this, FillChangedArgs);
    }
}
