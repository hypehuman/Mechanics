using GuiByReflection.Models;
using MathNet.Spatial.Euclidean;

namespace MechanicsCore.Arrangements;

[GuiName("Solar System")]
[GuiHelp("Data from NASA Horizons")]
public partial class SolarSystem : Arrangement
{
    public override object?[] GetConstructorParameters()
    {
        return Array.Empty<object?>();
    }

    public override IReadOnlyList<Body> GenerateInitialState(out Vector3D displayBound0, out Vector3D displayBound1)
    {
        var bodies = CreateBodies();
        var maxDist = bodies.Select(p => p.Position.Length).Max();
        displayBound0 = new(-maxDist, -maxDist, -maxDist);
        displayBound1 = new(maxDist, maxDist, maxDist);
        return bodies;
    }
}
