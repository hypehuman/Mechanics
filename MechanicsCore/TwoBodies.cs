using MathNet.Spatial.Euclidean;

namespace MechanicsCore;

public class TwoBodies : RandomSimulation
{
    private readonly double _systemRadius;
    private readonly double _totalMass;
    private readonly double _totalVolume;

    public override double dt_step => 1;
    protected override int steps_per_leap => 1024;

    public override Vector3D DisplayBound0 { get; }
    public override Vector3D DisplayBound1 { get; }
    public override IReadOnlyList<Body> Bodies { get; }

    public TwoBodies(double systemRadius, double totalMass, double totalVolume, int? seed = null)
        : base(seed)
    {
        _systemRadius = systemRadius;
        _totalMass = totalMass;
        _totalVolume = totalVolume;

        var numBodies = 2;
        var solidRadius = Constants.SphereVolumeToRadius(_totalVolume); // the radius we would get if all the bodies were to combine into one
        var bound = (systemRadius + solidRadius) * 2;
        DisplayBound1 = new(bound, bound, bound);
        DisplayBound0 = -DisplayBound1;
        var bodies = new Body[numBodies];
        var fraction0 = Random.NextDouble();
        for (int i = 0; i < numBodies; i++)
        {
            var fraction = i == 0 ? fraction0 : 1 - fraction0;
            var bodyMass = _totalMass * fraction;
            var bodyVolume = _totalVolume * fraction;
            var bodyRadius = Constants.SphereVolumeToRadius(bodyVolume);
            var position = Falling.RandomPointInBall(Random, systemRadius);
            bodies[i] = new Body(this,
                mass: bodyMass,
                radius: bodyRadius,
                position: position
            );
        }
        Bodies = bodies;
    }

    public override IEnumerable<string> GetConfigLines()
    {
        foreach (var b in base.GetConfigLines())
            yield return b;

        yield return $"System radius: {DoubleToString(_systemRadius)}";
        yield return $"Total mass: {DoubleToString(_totalMass)}";
        yield return $"Total volume: {DoubleToString(_totalVolume)}";
    }
}
