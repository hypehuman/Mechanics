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
        yield return $"Scenario: {GetType().Name}";
    }

    public abstract object?[] GetConstructorParameters();

    public abstract IReadOnlyList<Body> GenerateInitialState(out Vector3D displayBound0, out Vector3D displayBound1);
}
