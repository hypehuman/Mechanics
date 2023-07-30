namespace MechanicsCore;

public static class Constants
{
    public const double SecondsPerYear = 31556925;

    public const double GravitationalConstant = 6.6743e-11;

    public const double SunMass = 1.98847e30;

    public const double SunEarthDistance = 1.4960e11;

    public const double EarthMass = 5.9722e24;

    public const double EarthOrbitSunSpeed = 29780;

    public const double EarthMoonDistance = 3.84399e8;

    public const double MoonMass = 7.34767309e22;

    public const double MoonOrbitEarthSpeed = 1.022e3;

    public const double EarthVolume = 1.03e21;

    public static readonly double EarthRadius = SphereVolumeToRadius(EarthVolume);

    private const double FourThirdsPi = 4 * Math.PI / 3;

    /// <summary>
    /// Precompute the inverse of <see cref="FourThirdsPi"/> because multiplication is faster than division.
    /// </summary>
    private const double ThreeOverFourPi = 1 / FourThirdsPi;

    public static double SphereRadiusToVolume(double radius) => FourThirdsPi * radius * radius * radius;

    public static double SphereVolumeToRadius(double volume) => Math.Cbrt(volume * ThreeOverFourPi);
}
