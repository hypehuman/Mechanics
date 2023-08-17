using MathNet.Spatial.Euclidean;

namespace MechanicsCore.Arrangements;

public class SunEarthMoon : Arrangement
{
    public override object?[] GetConstructorParameters()
    {
        return Array.Empty<object?>();
    }

    public SunEarthMoon()
    {
    }

    public override IReadOnlyList<Body> GenerateInitialState(out Vector3D displayBound0, out Vector3D displayBound1)
    {
        var bodies = new Body[] {
            new(NextBodyID,
                name: "Sun",
                color: BodyColors.Sun,
                mass: Constants.SunMass,
                radius: Constants.SunRadius,
                position: default,
                // Give it the opposite momentum of the earth so that the system's center of mass stays in place
                velocity: new(0, -Constants.EarthOrbitSunSpeed * Constants.EarthMass / Constants.SunMass, 0)
            ),
            new(NextBodyID,
                name: "Earth",
                color: BodyColors.Earth,
                mass: Constants.EarthMass,
                radius: Constants.EarthRadius,
                position: new(Constants.EarthOrbitSunDistance, 0, 0),
                velocity: new(0, Constants.EarthOrbitSunSpeed, 0)
            ),
            new(NextBodyID,
                name: "Moon",
                color: BodyColors.Moon,
                mass: Constants.MoonMass,
                radius: Constants.MoonRadius,
                position: new(Constants.EarthOrbitSunDistance + Constants.MoonOrbitEarthDistance, 0, 0),
                velocity: new(0, Constants.EarthOrbitSunSpeed + Constants.MoonOrbitEarthSpeed, 0)
            ),
        };

        var displayBoundPadding = Constants.EarthOrbitSunDistance / 64;
        displayBound0 = new(
            bodies.Min(b => b.Position.X) - displayBoundPadding,
            bodies.Min(b => b.Position.Y) - displayBoundPadding,
            bodies.Min(b => b.Position.Z) - displayBoundPadding
        );
        displayBound1 = new(
            bodies.Max(b => b.Position.X) + displayBoundPadding,
            bodies.Max(b => b.Position.Y) + displayBoundPadding,
            bodies.Max(b => b.Position.Z) + displayBoundPadding
        );

        return bodies;
    }
}
