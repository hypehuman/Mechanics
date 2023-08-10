namespace MechanicsCore;

/// <summary>
/// Our simulation treats all bodies as spheres of uniform density.
/// But in reality, they tend to be more ellipsoidal.
/// Therefore the radii here are the "volumetric radius",
/// i.e., the radius of a sphere of volume equal to the ellipsoid.
/// Sources:
///  - https://nssdc.gsfc.nasa.gov/planetary/factsheet/sunfact.html
///  - https://nssdc.gsfc.nasa.gov/planetary/factsheet/moonfact.html
/// </summary>
public static class Constants
{
    public const double SecondsPerYear = 31556925;

    public const double GravitationalConstant = 6.6743e-11;

    public const double SunMass = 1.9885e30;
    public const double SunRadius = 6.957e8;
    public const double SunVolume = 1.412e27;

    public const double EarthMass = 5.9724e24;
    public const double EarthRadius = 6.371e6;
    public const double EarthVolume = 1.08321e21;

    public const double MoonMass = 7.3476e22;
    public const double MoonRadius = 1.7374e6;
    public const double MoonVolume = 2.1968e19;

    public const double EarthOrbitSunDistance = 1.4960e11;
    public const double EarthOrbitSunSpeed = 2.978e4;

    public const double MoonOrbitEarthDistance = 3.84399e8;
    public const double MoonOrbitEarthSpeed = 1.022e3;

    #region Math

    private const double FourThirdsPi = 4 * Math.PI / 3;

    /// <summary>
    /// Precompute the inverse of <see cref="FourThirdsPi"/> because multiplication is faster than division.
    /// </summary>
    private const double ThreeOverFourPi = 1 / FourThirdsPi;

    public static double SphereRadiusToVolume(double radius) => FourThirdsPi * radius * radius * radius;

    public static double SphereVolumeToRadius(double volume) => Math.Cbrt(volume * ThreeOverFourPi);

    public const double GoldenRatio = 1.61803398874989484820458683436;

    #endregion
}
