using GuiByReflection.Models;
using MathNet.Spatial.Euclidean;

namespace MechanicsCore.Scenarios;

public class Falling : RandomSimulationInitializer
{
    private readonly double _systemRadius;
    private readonly int _numBodies;
    private readonly double _totalMass;
    private readonly double _totalVolume;
    private readonly double _maxVelocity;

    public override IEnumerable<string> GetConfigLines()
    {
        foreach (var b in base.GetConfigLines())
            yield return b;

        yield return $"System radius: {Simulation.DoubleToString(_systemRadius)}";
        yield return $"Number of bodies: {_numBodies}";
        yield return $"Total mass: {Simulation.DoubleToString(_totalMass)}";
        yield return $"Total volume: {Simulation.DoubleToString(_totalVolume)}";
        yield return $"Max velocity: {Simulation.DoubleToString(_maxVelocity)}";
    }

    public override object?[] GetConstructorParameters()
    {
        return new object?[] { _systemRadius, _numBodies, _totalMass, _totalVolume, _maxVelocity, _requestedSeed };
    }

    public Falling(
        double systemRadius,
        int numBodies,
        double totalMass,
        double totalVolume,
        double maxVelocity,
        [GuiTitle(RequestedSeedGuiTitle)]
        [GuiHelp(RequestedSeedGuiHelp)]
        int? requestedSeed = null
    )
        : base(requestedSeed)
    {
        _systemRadius = systemRadius;
        _numBodies = numBodies;
        _totalMass = totalMass;
        _totalVolume = totalVolume;
        _maxVelocity = maxVelocity;
    }

    public override IReadOnlyList<Body> GenerateInitialState(out Vector3D displayBound0, out Vector3D displayBound1)
    {
        var bodyMass = _totalMass / _numBodies;
        var bodyVolume = _totalVolume / _numBodies;
        var bodyRadius = Constants.SphereVolumeToRadius(bodyVolume);
        var solidRadius = Constants.SphereVolumeToRadius(_totalVolume); // the radius we would get if all the bodies were to combine into one
        var bound = (_systemRadius + solidRadius) * 2;
        displayBound1 = new(bound, bound, bound);
        displayBound0 = -displayBound1;
        var bodies = new Body[_numBodies];
        for (int i = 0; i < _numBodies; i++)
        {
            var bodyPosition = RandomPointInBall(Random, _systemRadius);
            var bodyVelocity = _maxVelocity == 0 ? default : RandomPointInBall(Random, _maxVelocity);
            bodies[i] = new Body(NextBodyID,
                mass: bodyMass,
                radius: bodyRadius,
                position: bodyPosition,
                velocity: bodyVelocity
            );
        }
        BodySystem.SetNetZeroMomentum(bodies);
        return bodies;
    }

    public static Vector3D RandomPointInBall(Random random, double radius)
    {
        Vector3D v;
        do
        {
            var x = (random.NextDouble() * 2 - 1) * radius;
            var y = (random.NextDouble() * 2 - 1) * radius;
            var z = (random.NextDouble() * 2 - 1) * radius;
            v = new(x, y, z);
        } while (v.Length > radius);
        return v;
    }
}
