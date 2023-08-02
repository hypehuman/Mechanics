namespace MechanicsCore;

public static class Simulations
{
    public static Simulation TwoBodies(int? seed = null) => new TwoBodies(
        Constants.EarthRadius * 2,
        Constants.EarthMass,
        Constants.EarthVolume,
        seed
    );

    public static Simulation Line3Moons(int? seed = null) => new Line(
        3,
        Constants.MoonMass,
        Constants.MoonRadius
    );

    public static Simulation Line4Moons(int? seed = null) => new Line(
        4,
        Constants.MoonMass,
        Constants.MoonRadius
    );

    public static Simulation SunEarthMoon(int? seed = null) => new SunEarthMoon();

    public static Simulation Falling_Tiny(int? seed = null) => Falling(4, seed);
    public static Simulation Falling_Small(int? seed = null) => Falling(16, seed);
    public static Simulation Falling_Large(int? seed = null) => Falling(128, seed);
    public static Simulation Falling_Huge(int? seed = null) => Falling(512, seed);

    private static Simulation Falling(int numBodies, int? seed = null) => new Falling(
        Constants.EarthMoonDistance,
        numBodies,
        Constants.EarthMass + Constants.MoonMass,
        Constants.EarthVolume + Constants.MoonVolume,
        Constants.MoonOrbitEarthSpeed / Math.Sqrt(10),
        seed
    );

    public static Simulation MoonFromRing_Sane(int? seed = null) => new MoonFromRing(64, seed);
    public static Simulation MoonFromRing_Insane(int? seed = null) => new MoonFromRing(1024, seed);
}
