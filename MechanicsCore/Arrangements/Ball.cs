using GuiByReflection.Models;
using MathNet.Spatial.Euclidean;

namespace MechanicsCore.Arrangements;

[GuiHelp(
    "Identical bodies with their centers randomly distributed throughout a ball.",
    "Distribution is such that any region of the ball with a given volume has the same chance of containing a given number of body centers."
)]
public class Ball : RandomArrangement
{
    private readonly double _systemRadius;
    private readonly int _numBodies;
    private readonly double _totalMass;
    private readonly double _totalBodyVolume;
    private readonly double _maxSpeed;

    public override IEnumerable<string> GetConfigLines()
    {
        foreach (var b in base.GetConfigLines())
            yield return b;

        yield return $"System radius: {Simulation.DoubleToString(_systemRadius)}";
        yield return $"Number of bodies: {_numBodies}";
        yield return $"Total mass: {Simulation.DoubleToString(_totalMass)}";
        yield return $"Total body volume: {Simulation.DoubleToString(_totalBodyVolume)}";
        yield return $"Max speed: {Simulation.DoubleToString(_maxSpeed)}";
    }

    public override object?[] GetConstructorParameters()
    {
        return new object?[] { _systemRadius, _numBodies, _totalMass, _totalBodyVolume, _maxSpeed, _requestedSeed };
    }

    public Ball(
        [GuiHelp("radius of the ball in which the bodies are arranged")]
        double systemRadius,
        int numBodies,
        double totalMass,
        double totalBodyVolume,
        [GuiHelp("Initial velocities are distributed with speeds from 0 to maxSpeed (flat distribution) and random headings.")]
        double maxSpeed,
        [GuiName(RequestedSeedGuiName)]
        [GuiHelp(RequestedSeedGuiHelp)]
        int? requestedSeed = null
    )
        : base(requestedSeed)
    {
        _systemRadius = systemRadius;
        _numBodies = numBodies;
        _totalMass = totalMass;
        _totalBodyVolume = totalBodyVolume;
        _maxSpeed = maxSpeed;
    }

    public override IReadOnlyList<Body> GenerateInitialState(out Vector3D displayBound0, out Vector3D displayBound1)
    {
        var bodyMass = _totalMass / _numBodies;
        var bodyVolume = _totalBodyVolume / _numBodies;
        var bodyRadius = Constants.SphereVolumeToRadius(bodyVolume);
        var solidRadius = Constants.SphereVolumeToRadius(_totalBodyVolume); // the radius we would get if all the bodies were to combine into one
        var bound = (_systemRadius + solidRadius) * 2;
        displayBound1 = new(bound, bound, bound);
        displayBound0 = -displayBound1;
        var bodies = new Body[_numBodies];
        for (int i = 0; i < _numBodies; i++)
        {
            var bodyPosition = RandomPointInBall(Random, _systemRadius);
            var bodyVelocity = _maxSpeed == 0 ? default : RandomPointInBall(Random, _maxSpeed);
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
