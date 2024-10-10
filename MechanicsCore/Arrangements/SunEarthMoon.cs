using GuiByReflection.Models;
using MathNet.Spatial.Euclidean;
using System.Reflection;

namespace MechanicsCore.Arrangements;

[GuiHelp(
    "The default values match the scenario used for the Rust unit tests." +
    "Can be configured to simulate any three-body starting arrangement."
)]
public class SunEarthMoon : Arrangement
{
    private readonly double _sunMass;
    private readonly double _sunRadius;
    private readonly double _sunPositionX;
    private readonly double _sunPositionY;
    private readonly double _sunPositionZ;
    private readonly double _sunVelocityX;
    private readonly double _sunVelocityY;
    private readonly double _sunVelocityZ;
    private readonly double _earthMass;
    private readonly double _earthRadius;
    private readonly double _earthPositionX;
    private readonly double _earthPositionY;
    private readonly double _earthPositionZ;
    private readonly double _earthVelocityX;
    private readonly double _earthVelocityY;
    private readonly double _earthVelocityZ;
    private readonly double _moonMass;
    private readonly double _moonRadius;
    private readonly double _moonPositionX;
    private readonly double _moonPositionY;
    private readonly double _moonPositionZ;
    private readonly double _moonVelocityX;
    private readonly double _moonVelocityY;
    private readonly double _moonVelocityZ;

    public override IEnumerable<string> GetConfigLines()
    {
        foreach (var b in base.GetConfigLines())
            yield return b;

        foreach (var field in GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
        {
            var value = field.GetValue(this);
            string? valueString;
            if (value is double dbl)
            {
                if (dbl == 0)
                    continue;
                valueString = Simulation.DoubleToString(dbl);
            }
            else
            {
                valueString = value?.ToString();
            }
            yield return $"{field.Name.Trim('_')}: {valueString}";
        }
    }

    public override object?[] GetConstructorParameters()
    {
        return GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic).Select(f => f.GetValue(this)).ToArray();
    }

    /// <summary>
    /// The default parameters here are used in the Rust unit tests.
    /// The orbits will not be perfectly circular because the setup
    /// does not compensate for the effects of the smaller bodies on
    /// the larger bodies.
    /// </summary>
    public SunEarthMoon(
        double sunMass = Constants.SunMass,
        double sunRadius = Constants.SunRadius,
        double sunPositionX = 0,
        double sunPositionY = 0,
        double sunPositionZ = 0,
        double sunVelocityX = 0,
        double sunVelocityY = 0,
        double sunVelocityZ = 0,
        double earthMass = Constants.EarthMass,
        double earthRadius = Constants.EarthRadius,
        double earthPositionX = Constants.EarthOrbitSunDistance,
        double earthPositionY = 0,
        double earthPositionZ = 0,
        double earthVelocityX = 0,
        double earthVelocityY = Constants.EarthOrbitSunSpeed,
        double earthVelocityZ = 0,
        double moonMass = Constants.MoonMass,
        double moonRadius = Constants.MoonRadius,
        double moonPositionX = Constants.EarthOrbitSunDistance,
        double moonPositionY = Constants.MoonOrbitEarthDistance,
        double moonPositionZ = 0,
        double moonVelocityX = -Constants.MoonOrbitEarthSpeed,
        double moonVelocityY = Constants.EarthOrbitSunSpeed,
        double moonVelocityZ = 0
    )
    {
        _sunMass = sunMass;
        _sunRadius = sunRadius;
        _sunPositionX = sunPositionX;
        _sunPositionY = sunPositionY;
        _sunPositionZ = sunPositionZ;
        _sunVelocityX = sunVelocityX;
        _sunVelocityY = sunVelocityY;
        _sunVelocityZ = sunVelocityZ;
        _earthMass = earthMass;
        _earthRadius = earthRadius;
        _earthPositionX = earthPositionX;
        _earthPositionY = earthPositionY;
        _earthPositionZ = earthPositionZ;
        _earthVelocityX = earthVelocityX;
        _earthVelocityY = earthVelocityY;
        _earthVelocityZ = earthVelocityZ;
        _moonMass = moonMass;
        _moonRadius = moonRadius;
        _moonPositionX = moonPositionX;
        _moonPositionY = moonPositionY;
        _moonPositionZ = moonPositionZ;
        _moonVelocityX = moonVelocityX;
        _moonVelocityY = moonVelocityY;
        _moonVelocityZ = moonVelocityZ;
    }

    public override IReadOnlyList<Body> GenerateInitialState(out Vector3D displayBound0, out Vector3D displayBound1)
    {
        var bodies = new Body[] {
            new(NextBodyID,
                name: "Sun",
                color: BodyColors.Sun,
                mass: _sunMass,
                radius: _sunRadius,
                position: new(_sunPositionX, _sunPositionY, _sunPositionZ),
                velocity: new(_sunVelocityX, _sunVelocityY, _sunVelocityZ)
            ),
            new(NextBodyID,
                name: "Earth",
                color: BodyColors.Earth,
                mass: _earthMass,
                radius: _earthRadius,
                position: new(_earthPositionX, _earthPositionY, _earthPositionZ),
                velocity: new(_earthVelocityX, _earthVelocityY, _earthVelocityZ)
            ),
            new(NextBodyID,
                name: "Moon",
                color: BodyColors.Moon,
                mass: _moonMass,
                radius: _moonRadius,
                position: new(_moonPositionX, _moonPositionY, _moonPositionZ),
                velocity: new(_moonVelocityX, _moonVelocityY, _moonVelocityZ)
            ),
        };

        ComputeBoundingBox(bodies, out displayBound0, out displayBound1);
        var displaySize = displayBound1 - displayBound0;
        const double displayBoundPaddingFactor = 0.05;
        displayBound0 -= displayBoundPaddingFactor * displaySize;
        displayBound1 += displayBoundPaddingFactor * displaySize;

        return bodies;
    }
}
