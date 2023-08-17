using GuiByReflection.Models;
using MathNet.Spatial.Euclidean;

namespace MechanicsCore.Arrangements;

public class TwoBodies : RandomArrangement
{
    private readonly double _systemRadius;
    private readonly double _totalMass;
    private readonly double _totalBodyVolume;

    public override IEnumerable<string> GetConfigLines()
    {
        foreach (var b in base.GetConfigLines())
            yield return b;

        yield return $"System radius: {Simulation.DoubleToString(_systemRadius)}";
        yield return $"Total mass: {Simulation.DoubleToString(_totalMass)}";
        yield return $"Total volume: {Simulation.DoubleToString(_totalBodyVolume)}";
    }

    public override object?[] GetConstructorParameters()
    {
        return new object?[] { _systemRadius, _totalMass, _totalBodyVolume, _requestedSeed };
    }

    public TwoBodies(
        double systemRadius,
        double totalMass,
        double totalBodyVolume,
        [GuiName(RequestedSeedGuiName)]
        [GuiHelp(RequestedSeedGuiHelp)]
        int? requestedSeed = null
    )
        : base(requestedSeed)
    {
        if (!(systemRadius > 0))
            throw new ArgumentException("Must be greater than zero", nameof(systemRadius));
        _systemRadius = systemRadius;
        _totalMass = totalMass;
        _totalBodyVolume = totalBodyVolume;
    }

    public override IReadOnlyList<Body> GenerateInitialState(out Vector3D displayBound0, out Vector3D displayBound1)
    {
        var numBodies = 2;
        var solidRadius = Constants.SphereVolumeToRadius(_totalBodyVolume); // the radius we would get if all the bodies were to combine into one
        var bound = (_systemRadius + solidRadius) * 2;
        displayBound1 = new(bound, bound, bound);
        displayBound0 = -displayBound1;
        var bodies = new Body[numBodies];
        var fraction0 = Random.NextDouble();
        for (int i = 0; i < numBodies; i++)
        {
            var fraction = i == 0 ? fraction0 : 1 - fraction0;
            var bodyMass = _totalMass * fraction;
            var bodyVolume = _totalBodyVolume * fraction;
            var bodyRadius = Constants.SphereVolumeToRadius(bodyVolume);
            var position = Ball.RandomPointInBall(Random, _systemRadius);
            bodies[i] = new Body(NextBodyID,
                mass: bodyMass,
                radius: bodyRadius,
                position: position
            );
        }
        return bodies;
    }
}
