using MechanicsCore;
using System;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace MechanicsUI;

public class BodyVM : INotifyPropertyChanged
{
    public Body Model { get; }
    public Simulation Simulation { get; }
    public double RadiusPix { get; }
    public Brush Fill { get; }

    public BodyVM(Body model, Simulation simulation)
    {
        Model = model;
        Simulation = simulation;
        RadiusPix = Math.Max(1, Model.DisplayRadius);
        Fill = Model.Name switch
        {
            "Sun" => Brushes.Gold,
            "Earth" => Brushes.LightSeaGreen,
            "Moon" => Brushes.Silver,
            _ => MakeHashBrush(Model.Name),
        };
    }

    /// <summary>
    /// Returns an arbitrary fully-saturated solid-color brush
    /// whose hue depends on a hash of <paramref name="name"/>
    /// </summary>
    private static Brush MakeHashBrush(string name)
    {
        byte hash = 0;
        int q = 33149;
        foreach (byte b in Encoding.UTF8.GetBytes(name))
        {
            hash += (byte)(b * q);
        }

        var hue = hash / 255f;
        var brush = new SolidColorBrush(HueToRGB(hue));
        brush.Freeze();
        return brush;
    }

    private static Color HueToRGB(double h)
    {
        var kr = (5 + h * 6) % 6;
        var kg = (3 + h * 6) % 6;
        var kb = (1 + h * 6) % 6;

        var r = 1 - Math.Max(Min3(kr, 4 - kr, 1), 0);
        var g = 1 - Math.Max(Min3(kg, 4 - kg, 1), 0);
        var b = 1 - Math.Max(Min3(kb, 4 - kb, 1), 0);

        return Color.FromRgb(Convert.ToByte(r * byte.MaxValue), Convert.ToByte(g * byte.MaxValue), Convert.ToByte(b * byte.MaxValue));
    }

    private static double Min3(double a, double b, double c)
    {
        return Math.Min(Math.Min(a, b), c);
    }

    public Point CenterPix => new(Model.X, Model.Y);

    public int PanelZIndex
    {
        get
        {
            // Compute Z scaled to the range [0,1] relative to the simulation bounds.
            var z = Model.Z;
            SimulationVM.Sort(Simulation.DisplayBound0[2], Simulation.DisplayBound1[2], out var zMin, out var zMax);
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

    public void Refresh()
    {
        PropertyChanged?.Invoke(this, CenterPointChangedArgs);
        PropertyChanged?.Invoke(this, PanelZIndexChangedArgs);
    }
}
