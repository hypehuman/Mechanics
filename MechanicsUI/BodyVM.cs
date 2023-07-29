using MechanicsCore;
using System;
using System.ComponentModel;
using System.Linq;
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
        Fill = GetBrush();
    }

    private Brush GetBrush()
    {
        // Some magic names have specially selected brushes.
        switch (Model.Name)
        {
            case "Sun": return Brushes.Gold;
            case "Earth": return Brushes.LightSeaGreen;
            case "Moon": return Brushes.Silver;
        }

        // Get an arbitrary hue based on the model's ID.
        var hueByte = GetArbitraryHue(Model.ID);
        var brush = sBrushByHue[hueByte];
        return brush;
    }

    private static byte GetArbitraryHue(int id)
    {
        // The first few IDs get nicely distributed contrasting hues.
        if (id < 8)
        {
            var hue01 = id switch
            {
                0 => 0d / 8,
                1 => 4d / 8,
                2 => 2d / 8,
                3 => 6d / 8,
                4 => 1d / 8,
                5 => 5d / 8,
                6 => 3d / 8,
                7 => 7d / 8,
            };
            var hueByte = (byte)(hue01 * 256);
            return hueByte;
        }

        // Then we start hashing them.
        return Hash8(BitConverter.GetBytes(id));
    }

    private static byte Hash8(byte[] input)
    {
        byte hash = 0;
        int q = 33149;
        foreach (byte b in input)
        {
            hash += (byte)(b * q);
        }
        return hash;
    }

    private static readonly Brush[] sBrushByHue = Enumerable.Range(0, 256).Select(hueByte =>
    {
        var hue01 = hueByte / 256d;
        var brush = new SolidColorBrush(HueToRGB(hue01));
        brush.Freeze();
        return brush;
    }).ToArray();

    private static Color HueToRGB(double hue01)
    {
        var kr = (5 + hue01 * 6) % 6;
        var kg = (3 + hue01 * 6) % 6;
        var kb = (1 + hue01 * 6) % 6;

        var r = 1 - Math.Max(Min3(kr, 4 - kr, 1), 0);
        var g = 1 - Math.Max(Min3(kg, 4 - kg, 1), 0);
        var b = 1 - Math.Max(Min3(kb, 4 - kb, 1), 0);

        return Color.FromRgb(Convert.ToByte(r * byte.MaxValue), Convert.ToByte(g * byte.MaxValue), Convert.ToByte(b * byte.MaxValue));
    }

    private static double Min3(double a, double b, double c)
    {
        return Math.Min(Math.Min(a, b), c);
    }

    public Point CenterPix => new(Model.Position.X, Model.Position.Y);

    public int PanelZIndex
    {
        get
        {
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

    public void Refresh()
    {
        PropertyChanged?.Invoke(this, CenterPointChangedArgs);
        PropertyChanged?.Invoke(this, PanelZIndexChangedArgs);
    }
}
