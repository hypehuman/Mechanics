using GuiByReflection.Models;
using MathNet.Spatial.Euclidean;

namespace MechanicsCore.Arrangements;

[GuiName(MssGuiName)]
[GuiHelp(MssGuiHelp)]
public partial class ModernSolarSystem : Arrangement
{
    public const string MssGuiName = "Modern Solar System";
    public const string MssGuiHelp = "Data from NASA Horizons";

    private readonly int? _numBodies;

    public override IEnumerable<string> GetConfigLines()
    {
        foreach (var b in base.GetConfigLines())
            yield return b;

        yield return $"Number of bodies: {_numBodies}";
    }

    public override object?[] GetConstructorParameters()
    {
        return new object?[] { _numBodies };
    }

    public ModernSolarSystem(
        [GuiHelp("if not null, only take the most massive n bodies")]
        int? numBodies = null
    )
    {
        _numBodies = numBodies;
    }

    public override IReadOnlyList<Body> GenerateInitialState(out Vector3D displayBound0, out Vector3D displayBound1)
    {
        var bodies = CreateBodies();
        var toMirror = bodies.Where(b => b.Name.Contains("Earth") || b.Name.Contains("Moon")).ToList();
        foreach (var orig in toMirror)
        {
            bodies.Add(new(
                -orig.ID,
                new string(orig.Name.Reverse().ToArray()),
                new ((byte)(byte.MaxValue - orig.Color.R), (byte)(byte.MaxValue - orig.Color.G), (byte)(byte.MaxValue - orig.Color.B)),
                orig.Mass,
                orig.Radius,
                -orig.Position,
                -orig.Velocity
            ));
        }
        if (_numBodies != null)
        {
            bodies = bodies.OrderByDescending(b => b.Mass).Take(_numBodies.Value).OrderBy(b => b.ID).ToList();
        }
        var maxDist = 1.1 * Constants.EarthOrbitSunDistance;
        displayBound0 = new(-maxDist, -maxDist, -maxDist);
        displayBound1 = new(maxDist, maxDist, maxDist);
        return bodies;
    }
}
