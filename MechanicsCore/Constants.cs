namespace MechanicsCore;

/// <summary>
/// Assume SI units unless otherwise specified.
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

    /// <summary>
    /// "The true stellar density near the Sun is estimated as 0.004 stars per cubic light year, or 0.14 stars pc−3. When combined with estimates of the stellar masses, this yields a mass density estimate of 4×10−24 g/cm3 or 0.059 solar masses per cubic parsec. The density estimate varies across space, with the density decreasing rapidly in the direction out of the galactic plane."
    /// Source: https://en.wikipedia.org/wiki/Stellar_density
    /// Source cited by source: Gregersen, Erik (2010). The Milky Way and beyond. The Rosen Publishing Group. pp. 35–36. ISBN 1-61530-053-8.
    /// We represent this value in SI units (kg per cubic meter).
    /// </summary>
    public const double MassDensityInSolarNeighborhood = 4e-21;

    /// <summary>
    /// Taken from to https://ssd.jpl.nasa.gov/planets/phys_par.html
    /// "Mean Radius" here means Volumetric Radius.
    /// Represented in the non-SI units that are used in the source table.
    /// </summary>
    public static IReadOnlyList<(string name, double radiusKm, double massE24kg)> PlanetaryPhysicalParameters =>
    [
        // Planet | Mean Radius (km) | Mass (×10^24 kg) |
        ("Mercury",  2439.4          ,    0.330103      ),
        //        | +-  0.1          | +- 0.000021      |
        ("Venus"  ,  6051.8          ,    4.86731       ),
        //        | +-  1.0          | +- 0.00023       |
        ("Earth"  ,  6371.0084       ,    5.97217       ),
        //        | +-  0.0001       | +- 0.00028       |
        ("Mars"   ,  3389.50         ,    0.641691      ),
        //        | +-  0.2          | +- 0.000030      |
        ("Jupiter", 69911            , 1898.125         ),
        //        | +-  6            | +- 0.088         |
        ("Saturn" , 58232            ,  568.317         ),
        //        | +-  6            | +- 0.026         |
        ("Uranus" , 25362            ,   86.8099        ),
        //        | +-  7            | +- 0.0040        |
        ("Neptune", 24622            ,  102.4092        ),
        //        | +- 19            | +- 0.0048        |
    ];

    #region Math

    private const double FourThirdsPi = 4 * Math.PI / 3;

    /// <summary>
    /// Precompute the inverse of <see cref="FourThirdsPi"/> because multiplication is faster than division.
    /// </summary>
    private const double ThreeOverFourPi = 1 / FourThirdsPi;

    public static double SphereRadiusToVolume(double radius) => FourThirdsPi * radius * radius * radius;

    public static double SphereVolumeToRadius(double volume) => Math.Cbrt(volume * ThreeOverFourPi);

    /// <summary>
    /// The golden ratio, (1 + Sqrt(5)) / 2.
    /// The literal representation here is far more precise than a double can represent.
    /// </summary>
    public const double Phi = 1.618033988749894848204586834365638117720309179805762862;

    /// <summary>
    /// The additive inverse of the golden ratio, modulo 1.
    /// The literal representation here is far more precise than a double can represent.
    /// The last digit is rounded up; with more precision, it would be 7, not 8.
    /// </summary>
    public const double NegPhiMod1 = 0.381966011250105151795413165634361882279690820194237138;

    #endregion
}
