using GuiByReflection.Models;
using MathNet.Spatial.Euclidean;

namespace MechanicsCore.Arrangements;

[GuiHelp(
    "Just for testing the distribution of colors that we choose for bodies.",
    "Step/Leap does nothing."
)]
public class zColorWheel : Arrangement
{
    private readonly ColorMapping _colorMapping;
    /// <summary>
    /// See <see cref="BodyColor.ComputeRadiance"/>
    /// </summary>
    private readonly int? _radiance;

    public override IEnumerable<string> GetConfigLines()
    {
        foreach (var b in base.GetConfigLines())
            yield return b;

        yield return $"Color mapping: {_colorMapping}";
        yield return $"Radiance: {_radiance?.ToString() ?? "[varying]"}";
    }

    public override object?[] GetConstructorParameters()
    {
        return new object?[] { _colorMapping, _radiance };
    }

    public zColorWheel(
        ColorMapping colorMapping,
        [GuiHelp(
            "null: No adjustment. Secondary colors are brighter than primary colors.",
            "<=0: Always black.",
            "1-254: Darken all colors.",
            "255: Darken secondary colors to match the radiance of primary colors.",
            "256-509: Darken secondary colors, brighten primary colors.",
            "510: Brighten primary colors to match the radiance of secondary colors.",
            "1-254: Brighten all colors.",
            ">=765: Always white."
        )]
        int? radiance
    )
    {
        // Call this to validate the argument
        var func = GetColorMappingFunction(colorMapping);

        _colorMapping = colorMapping;
        _radiance = radiance;
    }

    public enum ColorMapping
    {
        CloseCyclic,
        SpacedCyclic,
        HashedPseudorandom,
    }

    public override IReadOnlyList<Body> GenerateInitialState(out Vector3D displayBound0, out Vector3D displayBound1)
    {
        const int numWheels = 3;
        const int numColorsPerWheel = 256;
        const double bodyRadius = Math.PI / numColorsPerWheel;

        var maxXY = GetWheelRadius(numWheels, bodyRadius);
        displayBound1 = new(maxXY, maxXY, bodyRadius);
        displayBound0 = -displayBound1;

        var getColor = GetColorMappingFunction(_colorMapping);

        var bodies = new Body[numWheels * numColorsPerWheel];
        for (var wheelI = 0; wheelI < numWheels; wheelI++)
        {
            var wheelRadius = GetWheelRadius(wheelI, bodyRadius);
            for (var bodyJ = 0; bodyJ < numColorsPerWheel; bodyJ++)
            {
                var colorI = wheelI * numColorsPerWheel + bodyJ;
                var color = getColor(colorI);
                if (_radiance.HasValue)
                {
                    color = BodyColors.NormalizeRadiance(color, _radiance.Value);
                }
                var angle01 = (double)bodyJ / numColorsPerWheel;
                var angleRad = angle01 * 2 * Math.PI;
                var cos = Math.Cos(angleRad);
                var sin = Math.Sin(angleRad);
                bodies[colorI] = new(NextBodyID,
                    color: color,
                    radius: bodyRadius,
                    position: new(wheelRadius * cos, wheelRadius * sin, 0)
                );
            }
        }

        return bodies;
    }

    private static double GetWheelRadius(int wheelI, double bodyRadius) => 1 + wheelI * 2 * bodyRadius;

    private static Func<int, BodyColor> GetColorMappingFunction(ColorMapping colorMapping)
    {
        return colorMapping switch
        {
            ColorMapping.CloseCyclic => BodyColors.GetCloseCyclicColor,
            ColorMapping.SpacedCyclic => BodyColors.GetSpacedCyclicColor,
            ColorMapping.HashedPseudorandom => BodyColors.GetHashedPseudorandomColor,
            _ => throw Utils.OutOfRange(nameof(colorMapping), colorMapping)
        };
    }
}
