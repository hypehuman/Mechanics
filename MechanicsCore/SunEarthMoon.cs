using MathNet.Spatial.Euclidean;

namespace MechanicsCore;

public class SunEarthMoon : Simulation
{
    public override Vector3D DisplayBound0 { get; }
    public override Vector3D DisplayBound1 { get; }

    public override IReadOnlyList<Body> Bodies { get; }

    public SunEarthMoon()
    {
        StepConfig.StepTime = 16;
        const double leaps_per_year = 365.24;
        StepConfig.StepsPerLeap = Convert.ToInt32(Constants.SecondsPerYear / leaps_per_year / StepConfig.StepTime);

        var bodies = new Body[] {
            new(this,
                name: "Sun",
                color: BodyColors.Sun,
                mass: Constants.SunMass,
                radius: Constants.SunRadius,
                position: default,
                // Give it the opposite momentum of the earth so that the system's center of mass stays in place
                velocity: new(0, -Constants.EarthOrbitSunSpeed * Constants.EarthMass / Constants.SunMass, 0)
            ),
            new(this,
                name: "Earth",
                color: BodyColors.Earth,
                mass: Constants.EarthMass,
                radius: Constants.EarthRadius,
                position: new(Constants.SunEarthDistance, 0, 0),
                velocity: new(0, Constants.EarthOrbitSunSpeed, 0)
            ),
            new(this,
                name: "Moon",
                color: BodyColors.Moon,
                mass: Constants.MoonMass,
                radius: Constants.MoonRadius,
                position: new(Constants.SunEarthDistance + Constants.EarthMoonDistance, 0, 0),
                velocity: new(0, Constants.EarthOrbitSunSpeed + Constants.MoonOrbitEarthSpeed, 0)
            ),
        };
        Bodies = bodies;

        var displayBoundPadding = Constants.SunEarthDistance / 64;
        DisplayBound0 = new(
            bodies.Min(b => b.Position.X) - displayBoundPadding,
            bodies.Min(b => b.Position.Y) - displayBoundPadding,
            bodies.Min(b => b.Position.Z) - displayBoundPadding
        );
        DisplayBound1 = new(
            bodies.Max(b => b.Position.X) + displayBoundPadding,
            bodies.Max(b => b.Position.Y) + displayBoundPadding,
            bodies.Max(b => b.Position.Z) + displayBoundPadding
        );
    }
}
