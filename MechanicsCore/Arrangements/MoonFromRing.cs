using GuiByReflection.Models;
using MathNet.Spatial.Euclidean;

namespace MechanicsCore.Arrangements;

[GuiName("Moon from Ring")]
[GuiHelp("Start with the moon broken into identical fragments orbiting the Earth, randomly distributed on a circle.")]
public class MoonFromRing : RandomArrangement
{
    private readonly int _numMoonFragments;
    private readonly double _spreadFactor;

    public override IEnumerable<string> GetConfigLines()
    {
        foreach (var b in base.GetConfigLines())
            yield return b;

        yield return $"Number of Moon fragments: {_numMoonFragments}";
        yield return $"Z-spread factor: {_spreadFactor}";
    }

    public override object?[] GetConstructorParameters()
    {
        return new object?[] { _numMoonFragments, _spreadFactor, _requestedSeed };
    }

    public MoonFromRing(
        [GuiName("Number of Moon fragments")]
        int numMoonFragments,
        [GuiName("Z-spread factor")]
        [GuiHelp("0 will form a perfect circle. 1 will spread them out up or down up to the moon's radius, forming a cylinder.")]
        double spreadFactor,
        [GuiName(RequestedSeedGuiName)]
        [GuiHelp(RequestedSeedGuiHelp)]
        int? requestedSeed = null
    )
        : base(requestedSeed)
    {
        _numMoonFragments = numMoonFragments;
        _spreadFactor = Math.Min(1, spreadFactor);
    }

    public override IReadOnlyList<Body> GenerateInitialState(out Vector3D displayBound0, out Vector3D displayBound1)
    {
        displayBound1 = new(Constants.MoonOrbitEarthDistance * 1.1, Constants.MoonOrbitEarthDistance * 1.1, Constants.EarthRadius * 1.1);
        displayBound0 = -displayBound1;
        var fragmentMass = Constants.MoonMass / _numMoonFragments;
        var fragmentVolume = Constants.MoonVolume / _numMoonFragments;
        var fragmentRadius = Constants.SphereVolumeToRadius(fragmentVolume);
        var bodies = new Body[_numMoonFragments + 1];
        for (int i = 0; i < _numMoonFragments; i++)
        {
            var angle01 = Random.NextDouble();
            var angleRad = angle01 * 2 * Math.PI;
            var cos = Math.Cos(angleRad);
            var sin = Math.Sin(angleRad);
            bodies[i] = new(NextBodyID,
                color: BodyColors.GetCloseCyclicColor((int)(angle01 * 256)),
                mass: fragmentMass,
                radius: fragmentRadius,
                position: new(Constants.MoonOrbitEarthDistance * cos, Constants.MoonOrbitEarthDistance * sin, 0),
                velocity: new(Constants.MoonOrbitEarthSpeed * -sin, Constants.MoonOrbitEarthSpeed * cos, 0)
            );
        }

        var spreadMagnitude = _spreadFactor * Constants.MoonRadius;
        for (int i = 0; i < _numMoonFragments; i++)
        {
            bodies[i].Position += new Vector3D(0, 0, spreadMagnitude * Random.NextDouble());
        }

        bodies[_numMoonFragments] = new(NextBodyID,
            name: "Earth",
            color: BodyColors.Earth,
            mass: Constants.EarthMass,
            radius: Constants.EarthRadius
        );
        BodySystem.SetNetZeroMomentum(bodies);
        return bodies;
    }
}
