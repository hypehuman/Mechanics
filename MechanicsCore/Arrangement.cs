using GuiByReflection.Models;
using MathNet.Spatial.Euclidean;

namespace MechanicsCore;

/// <summary>
/// Configuration that determines the initial state of the simulation
/// </summary>
public abstract class Arrangement : IGetConstructorParameters
{
    private int _prevBodyID = -1;
    public int NextBodyID => Interlocked.Increment(ref _prevBodyID);

    public virtual IEnumerable<string> GetConfigLines()
    {
        yield return $"Initial Arrangement: {GetType().Name}";
    }

    public abstract object?[] GetConstructorParameters();

    public abstract IReadOnlyList<Body> GenerateInitialState(out Vector3D displayBound0, out Vector3D displayBound1);

    public static bool ComputeBoundingBox(IEnumerable<Body> bodies, out Vector3D min, out Vector3D max)
    {
        var anyBodies = false;
        var xMin = double.PositiveInfinity;
        var yMin = double.PositiveInfinity;
        var zMin = double.PositiveInfinity;
        var xMax = double.NegativeInfinity;
        var yMax = double.NegativeInfinity;
        var zMax = double.NegativeInfinity;
        foreach (var body in bodies)
        {
            anyBodies = true;
            xMin = Math.Min(xMin, body.Position.X - body.Radius);
            yMin = Math.Min(yMin, body.Position.Y - body.Radius);
            zMin = Math.Min(zMin, body.Position.Z - body.Radius);
            xMax = Math.Max(xMax, body.Position.X + body.Radius);
            yMax = Math.Max(yMax, body.Position.Y + body.Radius);
            zMax = Math.Max(zMax, body.Position.Z + body.Radius);
        }
        min = anyBodies ? new(xMin, yMin, zMin) : new(-1, -1, -1);
        max = anyBodies ? new(xMax, yMax, zMax) : new(+1, +1, +1);
        return anyBodies;
    }
}
