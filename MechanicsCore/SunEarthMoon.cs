using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace MechanicsCore;

public class SunEarthMoon : Simulation
{
    public override double dt_step => 16;
    private double leaps_per_year => 365.24;
    protected override int steps_per_leap => Convert.ToInt32(Constants.SecondsPerYear / leaps_per_year / dt_step);

    public override Vector<double> DisplayBound0 { get; }
    public override Vector<double> DisplayBound1 { get; }

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
                position: new DenseVector(3),
                // Give it the opposite momentum of the earth so that the system's center of mass stays in place
                velocity: new DenseVector(new[] { 0, -Constants.EarthOrbitSunSpeed * Constants.EarthMass / Constants.SunMass, 0 })
            ),
            new Body(this,
                name: "Earth",
                mass: Constants.EarthMass,
                displayRadius: Constants.EarthMoonDistance - moonDisplayRadius,
                position: new DenseVector(new[] { Constants.SunEarthDistance, 0, 0 }),
                velocity: new DenseVector(new[] { 0, Constants.EarthOrbitSunSpeed, 0 })
            ),
            new Body(this,
                name: "Moon",
                mass: Constants.MoonMass,
                displayRadius: moonDisplayRadius,
                position: new DenseVector(new[] { Constants.SunEarthDistance + Constants.EarthMoonDistance, 0, 0 }),
                velocity: new DenseVector(new[] { 0, Constants.EarthOrbitSunSpeed + Constants.MoonOrbitEarthSpeed, 0 })
            ),
        };

        var displayBoundPadding = Constants.SunEarthDistance / 64;
        DisplayBound0 = new DenseVector(new[] {
            bodies.Min(b => b.Position[0]) - displayBoundPadding,
            bodies.Min(b => b.Position[1]) - displayBoundPadding,
            bodies.Min(b => b.Position[2]) - displayBoundPadding,
        });
        DisplayBound1 = new DenseVector(new[] {
            bodies.Max(b => b.Position[0]) + displayBoundPadding,
            bodies.Max(b => b.Position[1]) + displayBoundPadding,
            bodies.Max(b => b.Position[2]) + displayBoundPadding,
        });
    }
}
