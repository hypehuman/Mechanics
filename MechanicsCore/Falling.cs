using MathNet.Spatial.Euclidean;

namespace MechanicsCore;

public class Falling : RandomSimulation
{
    private readonly double _systemRadius;
    private readonly int _numBodies;
    private readonly double _totalMass;
    private readonly double _totalVolume;
    private readonly double _maxVelocity;

    public override Vector3D DisplayBound0 { get; }
    public override Vector3D DisplayBound1 { get; }
    public override IReadOnlyList<Body> Bodies { get; }

    public Falling(double systemRadius, int numBodies, double totalMass, double totalVolume, double maxVelocity, int? seed = null)
        : base(seed)
    {
        StepConfig.StepTime = 8;
        StepConfig.StepsPerLeap = 128;

        _systemRadius = systemRadius;
        _numBodies = numBodies;
        _totalMass = totalMass;
        _totalVolume = totalVolume;
        _maxVelocity = maxVelocity;

        var bodyMass = _totalMass / _numBodies;
        var bodyVolume = _totalVolume / _numBodies;
        var bodyRadius = Constants.SphereVolumeToRadius(bodyVolume);
        var solidRadius = Constants.SphereVolumeToRadius(_totalVolume); // the radius we would get if all the bodies were to combine into one
        var bound = (_systemRadius + solidRadius) * 2;
        DisplayBound1 = new(bound, bound, bound);
        DisplayBound0 = -DisplayBound1;
        var bodies = new Body[_numBodies];
        for (int i = 0; i < _numBodies; i++)
        {
            var bodyPosition = RandomPointInBall(Random, _systemRadius);
            var bodyVelocity = _maxVelocity == 0 ? default : RandomPointInBall(Random, _maxVelocity);
            bodies[i] = new Body(this,
                mass: bodyMass,
                radius: bodyRadius,
                position: bodyPosition,
                velocity: bodyVelocity
            );
        }
        Bodies = bodies;
        BodySystem.SetNetZeroMomentum(bodies);
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

    public override IEnumerable<string> GetConfigLines()
    {
        foreach (var b in base.GetConfigLines())
            yield return b;

        yield return $"System radius: {DoubleToString(_systemRadius)}";
        yield return $"Number of bodies: {_numBodies}";
        yield return $"Total mass: {DoubleToString(_totalMass)}";
        yield return $"Total volume: {DoubleToString(_totalVolume)}";
        yield return $"Max velocity: {DoubleToString(_maxVelocity)}";
    }
}
