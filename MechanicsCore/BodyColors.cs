using System.Security.Cryptography;

namespace MechanicsCore;

internal class BodyColors
{
    public static BodyColor Sun = new(0xFF, 0xD7, 0x00);
    public static BodyColor Earth = new(0x20, 0xB2, 0xAA);
    public static BodyColor Moon = new(0xC0, 0xC0, 0xC0);

    #region Saturated bright colors

    /// <summary>
    /// Returns colors with hues marching incrementally around the color wheel.
    /// Repeats every 256 values.
    /// </summary>
    public static BodyColor GetCloseCyclicColor(int id)
    {
        var hueByte = id % 256;
        return sSatBrightColorsByHue[hueByte];
    }

    private static readonly IReadOnlyList<BodyColor> sSatBrightColorsByHue = Enumerable.Range(0, 256).Select(Convert.ToByte).Select(HueToRGB).ToArray();

    /// <summary>
    /// Returns a color with the given hue angle, full saturation, and full brightness.
    /// </summary>
    private static BodyColor HueToRGB(byte hueByte)
    {
        var hue01 = hueByte / 256d;

        var kr = (5 + hue01 * 6) % 6;
        var kg = (3 + hue01 * 6) % 6;
        var kb = (1 + hue01 * 6) % 6;

        var r = 1 - Math.Max(Min3(kr, 4 - kr, 1), 0);
        var g = 1 - Math.Max(Min3(kg, 4 - kg, 1), 0);
        var b = 1 - Math.Max(Min3(kb, 4 - kb, 1), 0);

        return new(Convert.ToByte(r * byte.MaxValue), Convert.ToByte(g * byte.MaxValue), Convert.ToByte(b * byte.MaxValue));
    }

    private static double Min3(double a, double b, double c)
    {
        return Math.Min(Math.Min(a, b), c);
    }

    #endregion

    #region Spaced Cyclic Colors

    /// <summary>
    /// Returns colors with high contrast between small groups of adjacent values.
    /// Repeats every 256 values.
    /// </summary>
    public static BodyColor GetSpacedCyclicColor(int id)
    {
        const double increment = 99;
        var hueByte = (byte)((increment * id) % 256);
        return sSatBrightColorsByHue[hueByte];
    }

    #endregion

    #region Hashed Colors

    /// <summary>
    /// Returns a predictable but random-looking and non-repeating series of colors.
    /// </summary>
    public static BodyColor GetHashedPseudorandomColor(int id)
    {
        var hueByte = Hash8(BitConverter.GetBytes(id));
        return sSatBrightColorsByHue[hueByte];
    }

    private static byte Hash8(byte[] input)
    {
        var hash256 = SHA256.HashData(input);
        var hash8 = hash256[0];
        return hash8;
    }

    #endregion

    /// <summary>
    /// "Radiance" is defined here: <see cref="BodyColor.ComputeRadiance"/>.
    /// Due to rounding error, the output's radiance might be slightly off from the target radiance.
    /// </summary>
    public static BodyColor NormalizeRadiance(BodyColor inputColor, int targetRadiance)
    {
        targetRadiance = Math.Max(0, Math.Min(765, targetRadiance));
        var inputRadiance = inputColor.ComputeRadiance();

        if (inputRadiance == targetRadiance)
            return inputColor;

        // Select an "adjustment" with which to average the input color
        // (either black or white).
        var adjustmentColor = inputRadiance < targetRadiance
            ? new BodyColor(byte.MaxValue, byte.MaxValue, byte.MaxValue)
            : new BodyColor(byte.MinValue, byte.MinValue, byte.MinValue);
        var adjustmentRadiance = adjustmentColor.ComputeRadiance();

        // Compute the weights that will result in the correct output radiance.
        var inputWeight = ((float)targetRadiance - adjustmentRadiance) / (inputRadiance - adjustmentRadiance);
        var adjustmentWeight = 1 - inputWeight; 

        // Perform the weighted average to obtain the output.
        var output = new BodyColor(
            Convert.ToByte(inputWeight * inputColor.R + adjustmentWeight * adjustmentColor.R),
            Convert.ToByte(inputWeight * inputColor.G + adjustmentWeight * adjustmentColor.G),
            Convert.ToByte(inputWeight * inputColor.B + adjustmentWeight * adjustmentColor.B)
        );

        return output;
    }
}
