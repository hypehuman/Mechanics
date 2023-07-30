namespace MechanicsCore;

public static class PreconfiguredSimulations
{
    public static Simulation Default()
    {
        var sim = TwoBodies_Pointlike_0;
        sim.GravityConfig = GravityType.Newton_Buoyant;
        sim.DragCoefficient = 1;
        return sim;
    }

    public static Simulation TwoBodies_Pointlike_0
    {
        get
        {
            var sim = Simulations.TwoBodies(0);
            sim.GravityConfig = GravityType.Newton_Pointlike;
            return sim;
        }
    }

    public static Simulation TwoBodies_Buoyant_0
    {
        get
        {
            var sim = Simulations.TwoBodies(0);
            sim.GravityConfig = GravityType.Newton_Buoyant;
            return sim;
        }
    }

    public static Simulation TwoBodies_Buoyant_Drag_0
    {
        get
        {
            var sim = Simulations.TwoBodies(0);
            sim.GravityConfig = GravityType.Newton_Buoyant;
            sim.DragCoefficient = 1;
            return sim;
        }
    }

    public static Simulation SunEarthMoon_Pointlike
    {
        get
        {
            var sim = Simulations.SunEarthMoon();
            sim.GravityConfig = GravityType.Newton_Pointlike;
            return sim;
        }
    }

    public static Simulation Falling_Buoyant_Drag_Tiny_0
    {
        get
        {
            var sim = Simulations.Falling_Tiny(0);
            sim.GravityConfig = GravityType.Newton_Buoyant;
            sim.DragCoefficient = 1;
            return sim;
        }
    }

    // Around 3 days, forms a two-body "planet" with two one-body "moons".
    // Around 70 days, forms a three-body "planet" with a one-body "moon".
    // Around 2.5 years, collapses into a single four-body "planet".
    public static Simulation Falling_Buoyant_Drag_Tiny_287200931
    {
        get
        {
            var sim = Simulations.Falling_Tiny(287200931);
            sim.GravityConfig = GravityType.Newton_Buoyant;
            sim.DragCoefficient = 1;
            return sim;
        }
    }

    public static Simulation Falling_Buoyant_Drag_Small_0
    {
        get
        {
            var sim = Simulations.Falling_Small(0);
            sim.GravityConfig = GravityType.Newton_Buoyant;
            sim.DragCoefficient = 1;
            return sim;
        }
    }

    public static Simulation Falling_Buoyant_Drag_Large_0
    {
        get
        {
            var sim = Simulations.Falling_Large(0);
            sim.GravityConfig = GravityType.Newton_Buoyant;
            sim.DragCoefficient = 1;
            return sim;
        }
    }

    public static Simulation Falling_Buoyant_Drag_Huge_0
    {
        get
        {
            var sim = Simulations.Falling_Huge(0);
            sim.GravityConfig = GravityType.Newton_Buoyant;
            sim.DragCoefficient = 1;
            return sim;
        }
    }
}
