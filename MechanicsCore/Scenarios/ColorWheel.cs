using MathNet.Spatial.Euclidean;

namespace MechanicsCore.Scenarios;

public class ColorWheel : SimulationInitializer
{
    private readonly Func<int, BodyColor> _getColor;
    private readonly bool _normalizeRadiance;

    public override IEnumerable<string> GetConfigLines()
    {
        foreach (var b in base.GetConfigLines())
            yield return b;

        yield return $"Color getter: {_getColor.Method.Name}";
        yield return $"Normalize radiance: {_normalizeRadiance}";
    }

    public override object?[] GetConstructorParameters()
    {
        return new object?[] { _getColor, _normalizeRadiance };
    }

    public ColorWheel(Func<int, BodyColor> getColor, bool normalizeRadiance)
    {
        _getColor = getColor ?? throw new ArgumentNullException(nameof(getColor));
        _normalizeRadiance = normalizeRadiance;
    }

    public override IReadOnlyList<Body> GenerateInitialState(out Vector3D displayBound0, out Vector3D displayBound1)
    {
        const int numWheels = 3;
        const int numColorsPerWheel = 256;
        const double bodyRadius = Math.PI / numColorsPerWheel;

        var maxXY = GetWheelRadius(numWheels, bodyRadius);
        displayBound1 = new(maxXY, maxXY, bodyRadius);
        displayBound0 = -displayBound1;

        var bodies = new Body[numWheels * numColorsPerWheel];
        for (var wheelI = 0; wheelI < numWheels; wheelI++)
        {
            var wheelRadius = GetWheelRadius(wheelI, bodyRadius);
            for (var bodyJ = 0; bodyJ < numColorsPerWheel; bodyJ++)
            {
                var colorI = wheelI * numColorsPerWheel + bodyJ;
                var color = _getColor(colorI);
                if (_normalizeRadiance)
                {
                    color = NormalizeRadiance(color);
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

    /// <summary>
    /// Radiance is "the radiant flux emitted, reflected, transmitted or received by a given surface, per unit solid angle per unit projected area."
    /// For our purposes, let's define radiance as the sum of the R, G, and B values.
    /// Pure white would have a radiance of 255+255+255 = 765.
    /// Fully saturated primary colors (red, green, and blue) have a radiance of 255+0+0 = 255.
    /// Fully saturated secondary colors (cyan, magenta, and yellow) have a radiance of 255+255+0 = 255.
    /// Since _getColor returns only fully saturated colors, the minimum possible radiance is 255.
    /// So any colors that have a radiance over 255, darken them to normalize.
    /// </summary>
    private static BodyColor NormalizeRadiance(BodyColor input)
    {
        const int targetRadiance = 255;
        var radiance = input.R + input.G + input.B;
        if (radiance > targetRadiance)
        {
            var factor = (double)targetRadiance / radiance;
            return new(
                Convert.ToByte(factor * input.R),
                Convert.ToByte(factor * input.G),
                Convert.ToByte(factor * input.B)
            );
        }
        else
        {
            if (radiance < targetRadiance)
            {
                // TODO: Increase the radiance of darker colors.
                // This would make them whiter, e.g., red would become pink.
                // Currently irrelevant because _getColors never returns radiances less than 510.
            }

            return input;
        }
    }
}
