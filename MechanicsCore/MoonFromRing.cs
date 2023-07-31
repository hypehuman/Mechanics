using MathNet.Spatial.Euclidean;

namespace MechanicsCore;

public class MoonFromRing : RandomSimulation
{
    public override double dt_step => 8;
    protected override int steps_per_leap => 128;

    public override Vector3D DisplayBound0 { get; }
    public override Vector3D DisplayBound1 { get; }
    public override IReadOnlyList<Body> Bodies { get; }

    public MoonFromRing(int numMoonFragments, int? seed = null)
        : base(seed)
    {
        DisplayBound1 = new(Constants.EarthMoonDistance * 1.1, Constants.EarthMoonDistance * 1.1, Constants.EarthRadius * 1.1);
        DisplayBound0 = -DisplayBound1;
        var fragmentMass = Constants.MoonMass / numMoonFragments;
        var fragmentVolume = Constants.MoonVolume / numMoonFragments;
        var fragmentRadius = Constants.SphereVolumeToRadius(fragmentVolume);
        var fragmentDisplayRadius = Constants.SphereVolumeToRadius(fragmentVolume * 1000);
        var bodies = new Body[numMoonFragments + 1];
        for (int i = 0; i < numMoonFragments; i++)
        {
            var angle01 = Random.NextDouble();
            var angleRad = angle01 * 2 * Math.PI;
            var cos = Math.Cos(angleRad);
            var sin = Math.Sin(angleRad);
            bodies[i] = new(this,
                mass: fragmentMass,
                radius: fragmentRadius,
                displayRadius: fragmentDisplayRadius,
                position: new(Constants.EarthMoonDistance * cos, Constants.EarthMoonDistance * sin, 0),
                velocity: new(Constants.MoonOrbitEarthSpeed * -sin, Constants.MoonOrbitEarthSpeed * cos, 0)
            );
        }
        bodies[numMoonFragments] = new(this,
            name: "Earth",
            mass: Constants.EarthMass,
            radius: Constants.EarthRadius
        );
        Bodies = bodies;
        BodySystem.SetNetZeroMomentum(bodies);
    }
}
