namespace MechanicsCore;

public static class Simulations
{
    public static Simulation TwoBodies(int? seed = null) => new TwoBodies(
        Constants.EarthRadius * 2,
        Constants.EarthMass,
        Constants.EarthVolume,
        seed
    );

    /// <summary>
    /// Seed is unused, but this signature is important so that it can be found via reflection.
    /// </summary>
    public static Simulation SunEarthMoon(int? seed = null) => new SunEarthMoon();

    public static Simulation Falling_Tiny(int? seed = null) => Falling(4, seed);
    public static Simulation Falling_Small(int? seed = null) => Falling(16, seed);
    public static Simulation Falling_Large(int? seed = null) => Falling(128, seed);
    public static Simulation Falling_Huge(int? seed = null) => Falling(512, seed);

    private static Simulation Falling(int numBodies, int? seed = null) => new Falling(
        Constants.EarthRadius,
        numBodies,
        Constants.EarthMass,
        Constants.EarthVolume,
        100,
        seed
    );
}
