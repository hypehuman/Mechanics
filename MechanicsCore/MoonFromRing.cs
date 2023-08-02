using MathNet.Spatial.Euclidean;

namespace MechanicsCore;

public class MoonFromRing : RandomSimulation
{
    private readonly int _numMoonFragments;

    public override Vector3D DisplayBound0 { get; }
    public override Vector3D DisplayBound1 { get; }
    public override IReadOnlyList<Body> Bodies { get; }

    public MoonFromRing(int numMoonFragments, int? seed = null)
        : base(seed)
    {
        StepConfig.StepTime = 1;
        StepConfig.StepsPerLeap = 128;

        _numMoonFragments = numMoonFragments;

        DisplayBound1 = new(Constants.EarthMoonDistance * 1.1, Constants.EarthMoonDistance * 1.1, Constants.EarthRadius * 1.1);
        DisplayBound0 = -DisplayBound1;
        var fragmentMass = Constants.MoonMass / _numMoonFragments;
        var fragmentVolume = Constants.MoonVolume / _numMoonFragments;
        var fragmentRadius = Constants.SphereVolumeToRadius(fragmentVolume);
        var fragmentDisplayRadius = Constants.SphereVolumeToRadius(fragmentVolume * 1000);
        var bodies = new Body[_numMoonFragments + 1];
        for (int i = 0; i < _numMoonFragments; i++)
        {
            var angle01 = Random.NextDouble();
            var angleRad = angle01 * 2 * Math.PI;
            var cos = Math.Cos(angleRad);
            var sin = Math.Sin(angleRad);
            bodies[i] = new(this,
                color: BodyColors.GetCloseCyclicColor((int)(angle01 * 256)),
                mass: fragmentMass,
                radius: fragmentRadius,
                displayRadius: fragmentDisplayRadius,
                position: new(Constants.EarthMoonDistance * cos, Constants.EarthMoonDistance * sin, 0),
                velocity: new(Constants.MoonOrbitEarthSpeed * -sin, Constants.MoonOrbitEarthSpeed * cos, 0)
            );
        }
        bodies[_numMoonFragments] = new(this,
            name: "Earth",
            color: BodyColors.Earth,
            mass: Constants.EarthMass,
            radius: Constants.EarthRadius
        );
        Bodies = bodies;
        BodySystem.SetNetZeroMomentum(bodies);
    }

    public override IEnumerable<string> GetConfigLines()
    {
        foreach (var b in base.GetConfigLines())
            yield return b;

        yield return $"Number of Moon fragments: {_numMoonFragments}";
    }
}
