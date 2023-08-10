using MechanicsCore.StepConfiguring;

namespace MechanicsCore;

public static class PreconfiguredSimulations
{
    public static FullConfiguration Default() => TwoBodies_Buoyant_Drag_0;

    public static FullConfiguration TwoBodies_Pointlike_0
    {
        get
        {
            var initConfig = Simulations.TwoBodies(0);
            var stepConfig = new StepConfiguration
            {
                StepTime = 1,
                StepsPerLeap = 1024,
                GravityConfig = GravityType.Newton_Pointlike,
            };
            return new(initConfig, stepConfig);
        }
    }

    public static FullConfiguration TwoBodies_Buoyant_0
    {
        get
        {
            var initConfig = Simulations.TwoBodies(0);
            var stepConfig = new StepConfiguration
            {
                StepTime = 1,
                StepsPerLeap = 1024,
                GravityConfig = GravityType.Newton_Buoyant,
            };
            return new(initConfig, stepConfig);
        }
    }

    public static FullConfiguration TwoBodies_Buoyant_Drag_0
    {
        get
        {
            var initConfig = Simulations.TwoBodies(0);
            var stepConfig = new StepConfiguration
            {
                StepTime = 1,
                StepsPerLeap = 1024,
                GravityConfig = GravityType.Newton_Buoyant,
                CollisionConfig = CollisionType.Drag,
            };
            return new(initConfig, stepConfig);
        }
    }

    public static FullConfiguration Line3Moons_Buoyant_Drag
    {
        get
        {
            var initConfig = Simulations.Line3Moons();
            var stepConfig = new StepConfiguration
            {
                StepTime = 1,
                StepsPerLeap = 128,
                GravityConfig = GravityType.Newton_Buoyant,
                CollisionConfig = CollisionType.Drag,
            };
            return new(initConfig, stepConfig);
        }
    }

    public static FullConfiguration Line4Moons_Buoyant_Drag
    {
        get
        {
            var initConfig = Simulations.Line4Moons();
            var stepConfig = new StepConfiguration
            {
                StepTime = 1,
                StepsPerLeap = 128,
                GravityConfig = GravityType.Newton_Buoyant,
                CollisionConfig = CollisionType.Drag,
            };
            return new(initConfig, stepConfig);
        }
    }

    public static FullConfiguration SunEarthMoon_Pointlike
    {
        get
        {
            const double step_time = 16;
            const double leaps_per_year = 365.24;
            const double steps_per_leap = Constants.SecondsPerYear / leaps_per_year / step_time;
            var initConfig = Simulations.SunEarthMoon();
            var stepConfig = new StepConfiguration
            {
                StepTime = step_time,
                StepsPerLeap = Convert.ToInt32(steps_per_leap),
                GravityConfig = GravityType.Newton_Pointlike,
            };
            return new(initConfig, stepConfig);
        }
    }

    public static FullConfiguration Falling_Buoyant_Drag_Tiny_0
    {
        get
        {
            var initConfig = Simulations.Falling_Tiny(0);
            var stepConfig = new StepConfiguration
            {
                StepTime = 8,
                StepsPerLeap = 128,
                GravityConfig = GravityType.Newton_Buoyant,
                CollisionConfig = CollisionType.Drag,
            };
            return new(initConfig, stepConfig);
        }
    }

    // Around 3 days, forms a two-body "planet" with two one-body "moons".
    // Around 70 days, forms a three-body "planet" with a one-body "moon".
    // Around 1.7 years, collapses into a single four-body "planet".
    public static FullConfiguration Falling_Buoyant_Drag_Tiny_287200931
    {
        get
        {
            var initConfig = Simulations.Falling_Tiny(287200931);
            var stepConfig = new StepConfiguration
            {
                StepTime = 8,
                StepsPerLeap = 128,
                GravityConfig = GravityType.Newton_Buoyant,
                CollisionConfig = CollisionType.Drag,
            };
            return new(initConfig, stepConfig);
        }
    }

    public static FullConfiguration Falling_Buoyant_Drag_Small_0
    {
        get
        {
            var initConfig = Simulations.Falling_Small(0);
            var stepConfig = new StepConfiguration
            {
                StepTime = 8,
                StepsPerLeap = 128,
                GravityConfig = GravityType.Newton_Buoyant,
                CollisionConfig = CollisionType.Drag,
            };
            return new(initConfig, stepConfig);
        }
    }

    public static FullConfiguration Falling_Buoyant_Drag_Large_0
    {
        get
        {
            var initConfig = Simulations.Falling_Large(0);
            var stepConfig = new StepConfiguration
            {
                StepTime = 8,
                StepsPerLeap = 128,
                GravityConfig = GravityType.Newton_Buoyant,
                CollisionConfig = CollisionType.Drag,
            };
            return new(initConfig, stepConfig);
        }
    }

    public static FullConfiguration Falling_Buoyant_Drag_Huge_0
    {
        get
        {
            var initConfig = Simulations.Falling_Huge(0);
            var stepConfig = new StepConfiguration
            {
                StepTime = 8,
                StepsPerLeap = 128,
                GravityConfig = GravityType.Newton_Buoyant,
                CollisionConfig = CollisionType.Drag,
            };
            return new(initConfig, stepConfig);
        }
    }

    public static FullConfiguration MoonFromRing_Pointlike_Combine_Sane_0
    {
        get
        {
            var initConfig = Simulations.MoonFromRing_Sane(0);
            var stepConfig = new StepConfiguration
            {
                StepTime = 8,
                StepsPerLeap = 128,
                GravityConfig = GravityType.Newton_Pointlike,
                CollisionConfig = CollisionType.Combine,
            };
            return new(initConfig, stepConfig);
        }
    }

    public static FullConfiguration MoonFromRing_Pointlike_Combine_Insane_102691847
    {
        get
        {
            var initConfig = Simulations.MoonFromRing_Insane(102691847);
            var stepConfig = new StepConfiguration
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
