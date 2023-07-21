namespace MechanicsCore;

public static class Simulations
{
    public static Simulation Default() => TwoBodies_NoDrag_0;

    public static Simulation SunEarthMoon => new SunEarthMoon();
    public static Simulation Falling_Tiny_Random => Falling(16);
    public static Simulation Falling_Tiny_0 => Falling(16, 0);
    public static Simulation Falling_Cheap_Random => Falling(128);
    public static Simulation Falling_Cheap_0 => Falling(128, 0);
    public static Simulation Falling_Expensive_Random => Falling(512);
    public static Simulation Falling_Expensive_0 => Falling(512, 0);
    public static Simulation TwoBodies_NoDrag_Random => TwoBodies(false);
    public static Simulation TwoBodies_NoDrag_0 => TwoBodies(false, 0);
    public static Simulation TwoBodies_WithDrag_Random => TwoBodies(true);
    public static Simulation TwoBodies_WithDrag_0 => TwoBodies(true, 0);
    public static Simulation TwoBodies_WithDrag_916536409 => TwoBodies(true, 916536409);
    public static Simulation Falling_2_Random => Falling(2);
    public static Simulation Falling_3_Random => Falling(3);

    private static Simulation Falling(int numBodies, int? seed = null) => new Falling(
        Constants.EarthMoonDistance * 10,
        numBodies,
        Constants.EarthMass,
        Constants.EarthVolume * 10000,
        Constants.MoonOrbitEarthSpeed / Math.Sqrt(10),
        seed
    )
    {
        DragCoefficient = 1,
        BuoyantGravity = true,
    };

    private static Simulation TwoBodies(bool drag, int? seed = null) => new TwoBodies(
        Constants.EarthRadius * 2,
        Constants.EarthMass,
        Constants.EarthVolume,
        seed
    )
    {
        DragCoefficient = drag ? 1 : 0,
        BuoyantGravity = true,
    };
}
