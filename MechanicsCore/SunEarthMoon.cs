using MathNet.Spatial.Euclidean;

namespace MechanicsCore;

public class SunEarthMoon : Simulation
{
    public override double dt_step => 16;
    private double leaps_per_year => 365.24;
    protected override int steps_per_leap => Convert.ToInt32(Constants.SecondsPerYear / leaps_per_year / dt_step);

    public override Vector3D DisplayBound0 { get; }
    public override Vector3D DisplayBound1 { get; }

    private readonly Body[] bodies;
    public override IReadOnlyList<Body> Bodies => bodies;

    public SunEarthMoon()
    {
        var moonDisplayRadius = Constants.EarthMoonDistance * 0.25;

        bodies = new[] {
            new Body(this,
                name: "Sun",
                mass: Constants.SunMass,
                displayRadius: Constants.SunEarthDistance / 64,
                position: default,
                // Give it the opposite momentum of the earth so that the system's center of mass stays in place
                velocity: new(0, -Constants.EarthOrbitSunSpeed * Constants.EarthMass / Constants.SunMass, 0)
            ),
            new Body(this,
                name: "Earth",
                mass: Constants.EarthMass,
                displayRadius: Constants.EarthMoonDistance - moonDisplayRadius,
                position: new(Constants.SunEarthDistance, 0, 0),
                velocity: new(0, Constants.EarthOrbitSunSpeed, 0)
            ),
            new Body(this,
                name: "Moon",
                mass: Constants.MoonMass,
                displayRadius: moonDisplayRadius,
                position: new(Constants.SunEarthDistance + Constants.EarthMoonDistance, 0, 0),
                velocity: new(0, Constants.EarthOrbitSunSpeed + Constants.MoonOrbitEarthSpeed, 0)
            ),
        };

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
