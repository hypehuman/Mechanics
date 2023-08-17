using MechanicsCore.Arrangements;
using MechanicsCore.PhysicsConfiguring;

namespace MechanicsCore;

public static class ScenarioGallery
{
    public static Scenario Default() => TwoBodies_Buoyant_Drag_0;

    public static Scenario TwoBodies_Pointlike_0
    {
        get
        {
            var initConfig = new TwoBodies(
                Constants.EarthRadius * 2,
                Constants.EarthMass,
                Constants.EarthVolume,
                requestedSeed: 0
            );
            var stepConfig = new PhysicsConfiguration
            {
                StepTime = 1,
                StepsPerLeap = 1024,
                GravityConfig = GravityType.Newton_Pointlike,
            };
            return new(initConfig, stepConfig);
        }
    }

    public static Scenario TwoBodies_Buoyant_Drag_0
    {
        get
        {
            var initConfig = new TwoBodies(
                Constants.EarthRadius * 2,
                Constants.EarthMass,
                Constants.EarthVolume,
                requestedSeed: 0
            );
            var stepConfig = new PhysicsConfiguration
            {
                StepTime = 1,
                StepsPerLeap = 1024,
                GravityConfig = GravityType.Newton_Buoyant,
                CollisionConfig = CollisionType.Drag,
            };
            return new(initConfig, stepConfig);
        }
    }

    public static Scenario SunEarthMoon
    {
        get
        {
            const double step_time = 16;
            const double leaps_per_year = 365.24;
            const double steps_per_leap = Constants.SecondsPerYear / leaps_per_year / step_time;
            var initConfig = new SunEarthMoon();
            var stepConfig = new PhysicsConfiguration
            {
                StepTime = step_time,
                StepsPerLeap = Convert.ToInt32(steps_per_leap),
                GravityConfig = GravityType.Newton_Pointlike,
            };
            return new(initConfig, stepConfig);
        }
    }

    public static Scenario Falling_EarthMoon
    {
        get
        {
            var initConfig = new Ball(
                Constants.MoonOrbitEarthDistance,
                64,
                Constants.EarthMass + Constants.MoonMass,
                Constants.EarthVolume + Constants.MoonVolume,
                Constants.MoonOrbitEarthSpeed / Math.Sqrt(10)
            );
            var stepConfig = new PhysicsConfiguration
            {
                StepTime = 8,
                StepsPerLeap = 128,
                GravityConfig = GravityType.Newton_Buoyant,
                CollisionConfig = CollisionType.Drag,
            };
            return new(initConfig, stepConfig);
        }
    }

    /// <summary>
    /// Around 3 days, forms a two-body "planet" with two one-body "moons".
    /// Around 70 days, forms a three-body "planet" with a one-body "moon".
    /// Around 1.7 years, collapses into a single four-body "planet".
    /// </summary>
    public static Scenario Falling_EarthMoon_Tiny_287200931
    {
        get
        {
            var initConfig = new Ball(
                Constants.MoonOrbitEarthDistance,
                4,
                Constants.EarthMass + Constants.MoonMass,
                Constants.EarthVolume + Constants.MoonVolume,
                Constants.MoonOrbitEarthSpeed / Math.Sqrt(10),
                287200931
            );
            var stepConfig = new PhysicsConfiguration
            {
                StepTime = 8,
                StepsPerLeap = 128,
                GravityConfig = GravityType.Newton_Buoyant,
                CollisionConfig = CollisionType.Drag,
            };
            return new(initConfig, stepConfig);
        }
    }

    /// <summary>
    /// Picked 13 because then you have often get 12 (the 3D kissing number) the middle one.
    /// 4 and 6 are also pretty because of their symmetry.
    /// I gave them just enough speed that the combined body tends rotate.
    /// </summary>
    public static Scenario EarthLattice
    {
        get
        {
            var initConfig = new Ball(
                Constants.EarthRadius,
                13,
                Constants.EarthMass,
                Constants.EarthVolume,
                1000
            );
            var stepConfig = new PhysicsConfiguration
            {
                StepTime = 0.1,
                StepsPerLeap = 128,
                GravityConfig = GravityType.Newton_Buoyant,
                BuoyantGravityRatio = 1000,
                CollisionConfig = CollisionType.Drag,
                DragCoefficient = 10,
            };
            return new(initConfig, stepConfig);
        }
    }

    public static Scenario MoonFromRing
    {
        get
        {
            var initConfig = new MoonFromRing(64);
            var stepConfig = new PhysicsConfiguration
            {
                StepTime = 8,
                StepsPerLeap = 128,
                GravityConfig = GravityType.Newton_Pointlike,
                CollisionConfig = CollisionType.Combine,
            };
            return new(initConfig, stepConfig);
        }
    }

    public static Scenario MoonFromRing_Insane_102691847
    {
        get
        {
            var initConfig = new MoonFromRing(1024, 102691847);
            var stepConfig = new PhysicsConfiguration
            {
                StepTime = 8,
                StepsPerLeap = 128,
                GravityConfig = GravityType.Newton_Pointlike,
                CollisionConfig = CollisionType.Combine,
            };
            return new(initConfig, stepConfig);
        }
    }
}
