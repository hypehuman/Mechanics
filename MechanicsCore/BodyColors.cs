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
    /// Returns colors with high contrast between adjacent values.
    /// Repeats every 256 values.
    /// </summary>
    public static BodyColor GetSpacedCyclicColor(int id)
    {
        var hueByte = sSpacedBytes[id % 256];
        return sSatBrightColorsByHue[hueByte];
    }

    private static readonly IReadOnlyList<byte> sSpacedBytes = GetSpaced01s(256).Select(d => d * 256).Select(Convert.ToByte).ToArray();

    private static double[] GetSpaced01s(int n)
    {
        var result = new double[n];
        // start from 1, since the value at i=0 is 0 by default
        int i = 1;
        for (int setI = 0; true; setI++)
        {
            foreach (var value in GetNextRotated(setI, result))
            {
                result[i++] = value;
                if (i == n)
                {
                    return result;
                }
            }
        }
    }

    private static IEnumerable<double> GetNextRotated(int setI, IReadOnlyList<double> precomputed)
    {
        var n = 1 << setI;
        var offset = 0.5 / n;
        for (int i = 0; i < n; i++)
        {
            yield return (offset + precomputed[i]) % 1;
        }
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
        byte hash = 0;
        int q = 33149;
        foreach (byte b in input)
        {
            hash += (byte)(b * q);
        }
        return hash;
    }

    #endregion
}
