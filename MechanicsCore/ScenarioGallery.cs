using GuiByReflection.Models;
using MechanicsCore.Arrangements;
using MechanicsCore.PhysicsConfiguring;
using System.Diagnostics;

namespace MechanicsCore;

public static class ScenarioGallery
{
    public static Scenario Default() => TwoBodies_Buoyant_Drag_0;

    public static Scenario TwoBodies_Pointlike_0 => new(
        new TwoBodies(
            Constants.EarthRadius * 2,
            Constants.EarthMass,
            Constants.EarthVolume,
            requestedSeed: 0
        ),
        new()
        {
            StepTime = 1,
            GravityConfig = GravityType.Newton_Pointlike,
        },
        1024
    );

    public static Scenario TwoBodies_Buoyant_Drag_0 => new(
        new TwoBodies(
            Constants.EarthRadius * 2,
            Constants.EarthMass,
            Constants.EarthVolume,
            requestedSeed: 0
        ),
        new()
        {
            StepTime = 1,
            GravityConfig = GravityType.Newton_Buoyant,
            CollisionConfig = CollisionType.Drag,
        },
        1024
    );

    [GuiHelp("This is the same setup used for the Rust unit tests.")]
    public static Scenario SunEarthMoon => new(
        new SunEarthMoon(),
        new()
        {
            StepTime = 16,
            GravityConfig = GravityType.Newton_Pointlike,
        },
        TimeSpan.FromDays(1)
    );

    [GuiHelp("What would happen if the Earth and Moon lost their momentum around the Sun?")]
    public static Scenario SunEarthMoon_EarthStopped => new(
        new SunEarthMoon(
            earthVelocityY: 0,
            moonVelocityY: 0
        ),
        new()
        {
            StepTime = 1,
            GravityConfig = GravityType.Newton_LinearAfterTouching
        },
        900
    );

    [GuiName(Arrangements.ModernSolarSystem.MssGuiName)]
    [GuiHelp(Arrangements.ModernSolarSystem.MssGuiHelp)]
    public static Scenario ModernSolarSystem => new(
        new ModernSolarSystem(),
        new()
        {
            StepTime = 16,
            GravityConfig = GravityType.Newton_Pointlike,
        },
        TimeSpan.FromDays(1)
    );

    [GuiName("Collapsing: Earth and Moon")]
    [GuiHelp("What would happen if you smashed the Earth and Moon? Would they re-form?")]
    public static Scenario Collapsing_EarthMoon => new(
        new Ball(
            Constants.MoonOrbitEarthDistance,
            64,
            Constants.EarthMass + Constants.MoonMass,
            Constants.EarthVolume + Constants.MoonVolume,
            Constants.MoonOrbitEarthSpeed / Math.Sqrt(10)
        ),
        new()
        {
            StepTime = 8,
            GravityConfig = GravityType.Newton_Buoyant,
            CollisionConfig = CollisionType.Drag,
        },
        128
    );

    [GuiName("Collapsing: Earth and Moon (tiny, seed=287200931)")]
    [GuiHelp(
        "Around 3 days, forms a two-body 'planet' with two one-body 'moons'.",
        "Around 70 days, forms a three-body 'planet' with a one-body 'moon'.",
        "Around 1.7 years, collapses into a single four-body 'planet'."
    )]
    public static Scenario Collapsing_EarthMoon_Tiny_287200931 => new(
        new Ball(
            Constants.MoonOrbitEarthDistance,
            4,
            Constants.EarthMass + Constants.MoonMass,
            Constants.EarthVolume + Constants.MoonVolume,
            Constants.MoonOrbitEarthSpeed / Math.Sqrt(10),
            287200931
        ),
        new()
        {
            StepTime = 8,
            GravityConfig = GravityType.Newton_Buoyant,
            CollisionConfig = CollisionType.Drag,
        },
        128
    );

    [GuiName("Earth Lattice")]
    [GuiHelp(
        "I chose 13 bodies because then we often get 12 (the 3D kissing number) surrounding the middle one.",
        "4 and 6 bodies are also pretty because of their symmetry.",
        "I gave them just enough speed that the combined body tends to rotate comparably to Earth."
    )]
    public static Scenario EarthLattice => new(
        new Ball(
            Constants.EarthRadius,
            13,
            Constants.EarthMass,
            Constants.EarthVolume,
            1000
        ),
        new()
        {
            StepTime = 0.1,
            GravityConfig = GravityType.Newton_Buoyant,
            BuoyantGravityRatio = 1000,
            CollisionConfig = CollisionType.Drag,
            DragCoefficient = 10,
        },
        128
    );

    [GuiName("Moon forms from a ring")]
    public static Scenario MoonFromRing => new(
        new MoonFromRing(64),
        new()
        {
            StepTime = 8,
            GravityConfig = GravityType.Newton_Pointlike,
            CollisionConfig = CollisionType.Combine,
        },
        128
    );

    [GuiHelp(
        "This starts with an oddity: By 17 minutes two fragments are ejected from the bottom-right, leaving a gap.",
        "After about a day or two, a large body falls out of the gap and collides with Earth after 5-6 days!",
        "Reducing the step time from 8 to 1 causes the oddity to disappear, revealing the oddity to be a simulation artifact."
    )]
    public static Scenario MoonFromRing_Insane_102691847 => new(
        new MoonFromRing(1024, false, 102691847),
        new()
        {
            StepTime = 8,
            GravityConfig = GravityType.Newton_Pointlike,
            CollisionConfig = CollisionType.Combine,
        },
        128
    );

    [GuiName("Collapsing: Solar System")]
    [GuiHelp(
        "Formation of the solar system, assuming it's all gas.",
        "I had hoped to see an accretion disc form, but I think that would require some repulsive forces.",
        "Dividing the solar system's mass by the regional mass density gives us the approximate volume of the gas that formed the solar system.",
        "Matter outside this region would have fallen into other stars.",
        "We need a lot of bodies; with 1000 bodies, none of them will weigh less than Jupiter."
    )]
    public static Scenario Collapsing_SolarSystem_Puffy => Get_Collapsing_SolarSystem_Puffy();

    public static Scenario Get_Collapsing_SolarSystem_Puffy(int? requestedSeed = null)
    {
        var sunAndPlanets = Constants.PlanetaryPhysicalParameters
            .Select(x => new Body(-1, x.name, mass: x.massE24kg * 1e24, radius: x.radiusKm * 1e3))
            .Append(new Body(-1, "Sun", mass: Constants.SunMass, radius: Constants.SunRadius))
            .OrderBy(x => x.Mass) // ascending order improves the precision of the sum
            .ToList();
        var solarSystemMass = sunAndPlanets.Sum(b => b.Mass);
        var volumeOfSolarSystemRegion = solarSystemMass / Constants.MassDensityInSolarNeighborhood;
        var radiusOfSolarSystemRegion = Constants.SphereVolumeToRadius(volumeOfSolarSystemRegion);

        // I arbitrarily choose a coefficient x in the range (0,1).
        // The stuff is in clumps that take up fraction x of the space,
        // and the remaining (1-x) of the space is empty.
        // More realistic might be to start with x near 1 (uniformly distributed gas).
        // In today's solar system, x=2.8e-24 (the sun's volume divided by the region's volume).
        const double x = 0.001;
        var stuffVolume = x * volumeOfSolarSystemRegion;

        // Maximum speed to keep most objects from escaping
        var orbitalSpeedAtBoundary = Math.Sqrt(Constants.GravitationalConstant * solarSystemMass / radiusOfSolarSystemRegion);

        // Find the p where GetRelativeErrorOfVolume is close to zero.
        var p = FindZero(GetRelativeErrorOfVolume, 1e-4, 100);

        return new(GetInitialArrangement(), GetConfig(p));

        Arrangement GetInitialArrangement() => new Ball(
            radiusOfSolarSystemRegion,
            2048,
            solarSystemMass,
            stuffVolume,
            orbitalSpeedAtBoundary, // Also try with 0 speed, which might be more realistic?
            requestedSeed
        );

        static PhysicsConfiguration GetConfig(double p) => new()
        {
            StepTime = 1000000000000 / 4,
            GravityConfig = GravityType.Newton_Pointlike,
            CollisionConfig = CollisionType.Combine,
            MassVolumePower = p,
        };

        // return the relative difference in combined volume between GetScenario's bodies and sunAndPlanets.
        double GetRelativeErrorOfVolume(double p)
        {
            var initialBodies = GetInitialArrangement().GenerateInitialState(out _, out _);
            var config = GetConfig(p);
            var combinedVolume0 = Simulation.GetCombinedVolume(sunAndPlanets, config, solarSystemMass);
            var combinedVolume1 = Simulation.GetCombinedVolume(initialBodies, config, solarSystemMass);
            return (combinedVolume1 - combinedVolume0) / combinedVolume0;
        }
    }

    /// <summary>
    /// Attempts to find a zero of function <paramref name="f"/>(x).
    /// Assumes that <paramref name="f"/>(x) is monotonic.
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    private static double FindZero(Func<double, double> f, double tolerance, int maxIterations)
    {
        var x0 = 0d;
        var y0 = f(x0);
        if (Math.Abs(y0) <= tolerance)
        {
            return x0;
        }

        var x1 = 1d;
        var y1 = f(x1);
        if (Math.Abs(y1) <= tolerance)
        {
            return x1;
        }

        for (var i = 0; i < maxIterations; i++)
        {
            if (y1 - y0 == 0)
            {
                throw new InvalidOperationException($"Gradient is flat after {maxIterations} iterations. x0={x0}, y0={y0}, x1={x1}, y1={y1}");
            }

            // g(x) is a linear approximation of f(x).
            // It's a line passing through (x0,y0) and (x1,y1), which are both known to be points on f(x).
            // Find x where g(x) is 0, and use that as our next prediction of where f(x) ought to be 0.
            // g(x)-y0 = (y1-y0)/(x1-x0)*(x-x0)
            // g(x) = 0
            // 0-y0 = (y1-y0)/(x1-x0)*(x-x0)
            // (y1-y0)/(x1-x0)*(x-x0) = 0-y0
            // (y1-y0)/(x1-x0)*(x-x0) = -y0
            // (y1-y0)*(x-x0) = -y0*(x1-x0)
            // x-x0 = -y0*(x1-x0)/(y1-y0)
            // x = -y0*(x1-x0)/(y1-y0)+x0
            var xNext = -y0 * (x1 - x0) / (y1 - y0) + x0;
            var yNext = f(xNext);
            Debug.WriteLine($"x0={x0}, y0={y0}, x1={x1}, y1={y1}, xNext={xNext}, yNext={yNext}");
            if (Math.Abs(yNext) <= tolerance)
            {
                return xNext;
            }

            if (Math.Sign(y0) != Math.Sign(y1))
            {
                // y0 and y1 have opposite signs, so xNext should be between x0 and x1.
                // Replace the one that has the same sign as yNext.
                if (Math.Sign(yNext) == Math.Sign(y0))
                {
                    x0 = xNext;
                    y0 = yNext;
                }
                else
                {
                    x1 = xNext;
                    y1 = yNext;
                }
            }
            else
            {
                // y0 and y1 have the same sign, so xNext should be outside of them.
                // Replace whichever one is farther from xNext.
                if (Math.Abs(xNext - x0) > Math.Abs(xNext - x1))
                {
                    x0 = xNext;
                    y0 = yNext;
                }
                else
                {
                    x1 = xNext;
                    y1 = yNext;
                }
            }
        }

        throw new InvalidOperationException($"Failed to find zero after {maxIterations} iterations. x0={x0}, y0={y0}, x1={x1}, y1={y1}");
    }
}
