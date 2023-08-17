﻿using GuiByReflection.Models;
using MechanicsCore.Arrangements;
using MechanicsCore.PhysicsConfiguring;

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

    public static Scenario SunEarthMoon
    {
        get
        {
            const double step_time = 16;
            const double leaps_per_year = 365.24;
            const double steps_per_leap = Constants.SecondsPerYear / leaps_per_year / step_time;
            return new(
                new SunEarthMoon(),
                new()
                {
                    StepTime = step_time,
                    GravityConfig = GravityType.Newton_Pointlike,
                },
                Convert.ToInt32(steps_per_leap)
            );
        }
    }

    [GuiName("Collapsing: Earth and Moon")]
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
        "4 and 6 are also pretty because of their symmetry.",
        "I gave them just enough speed that the combined body tends rotate comparably to Earth."
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
        new MoonFromRing(1024, 102691847),
        new()
        {
            StepTime = 8,
            GravityConfig = GravityType.Newton_Pointlike,
            CollisionConfig = CollisionType.Combine,
        },
        128
    );
}
