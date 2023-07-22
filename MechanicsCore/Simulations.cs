namespace MechanicsCore;

public static class Simulations
{
    public static Simulation Default() => TwoBodies_NoDrag_0;

    public static Simulation TwoBodies_NoDrag_0 => TwoBodies(false, 0);
    public static Simulation TwoBodies_NoDrag_Random => TwoBodies(false);
    public static Simulation TwoBodies_WithDrag_0 => TwoBodies(true, 0);
    public static Simulation TwoBodies_WithDrag_Random => TwoBodies(true);

    public static Simulation SunEarthMoon => new SunEarthMoon();

    public static Simulation Falling_Tiny_0 => Falling(4, 0);
    public static Simulation Falling_Tiny_287200931 => Falling(4, 287200931);
    public static Simulation Falling_Tiny_Random => Falling(4);
    public static Simulation Falling_Small_0 => Falling(16, 0);
    public static Simulation Falling_Small_1002345669 => Falling(16, 0); // Something fun happened between 30 and 35 years.
    public static Simulation Falling_Small_Random => Falling(16);
    public static Simulation Falling_Large_0 => Falling(128, 0);
    public static Simulation Falling_Large_Random => Falling(128);
    public static Simulation Falling_Huge_0 => Falling(512, 0);
    public static Simulation Falling_Huge_Random => Falling(512);

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
