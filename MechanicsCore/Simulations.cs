namespace MechanicsCore;

public static class Simulations
{
    public static Simulation Default() => TwoBodies_NoDrag_Fixed;

    public static Simulation SunEarthMoon => new SunEarthMoon();
    public static Simulation Falling_Cheap_Fixed => Falling(128, 0);
    public static Simulation Falling_Cheap_Random => Falling(128);
    public static Simulation Falling_Expensive_Fixed => Falling(512, 0);
    public static Simulation Falling_Expensive_Random => Falling(512);
    public static Simulation TwoBodies_NoDrag_Fixed => TwoBodies(false, 0);
    public static Simulation TwoBodies_NoDrag_Random => TwoBodies(false);
    public static Simulation TwoBodies_WithDrag_Fixed => TwoBodies(true, 0);
    public static Simulation TwoBodies_WithDrag_Random => TwoBodies(true);

    private static Simulation Falling(int numBodies, int? seed = null) => new Falling(
        Constants.EarthMoonDistance,
        numBodies,
        Constants.EarthMass,
        Constants.EarthVolume,
        seed
    )
    {
        DragCoefficient = 1e10,
    };

    private static Simulation TwoBodies(bool drag, int? seed = null) => new TwoBodies(
        Constants.EarthRadius * 2,
        Constants.EarthMass,
        Constants.EarthVolume,
        seed
    )
    {
        DragCoefficient = drag ? 1e10 : 0,
    };
}
