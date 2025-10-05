namespace Rendering;

public static class PixelMath
{
    /// <summary>
    /// Encodes luminance as an sRGB color channel value.
    /// From <a href="https://www.color.org/srgb.pdf">Specification of sRGB</a> from the International Color Consortium:
    /// <para>
    /// 8. Color component transfer function:
    /// Note: Produces sRGB digital values with a range 0 to 1, which must then be multiplied by 2^(bit depth)–1 and quantized.
    /// If RL, GL, BL are less than or equal to 0.0031308
    ///     R = 12.92 * RL
    ///     G = 12.92 * GL
    ///     B = 12.92 * BL
    /// If RL, GL, BL are greater than 0.0031308
    ///     R = 1.055 * RL^(1/2.4) – 0.055
    ///     G = 1.055 * GL^(1/2.4) – 0.055
    ///     B = 1.055 * BL^(1/2.4) – 0.055
    /// </para>
    /// </summary>
    /// <param name="luminance_0_1">the relative luminance in the range [0,1]</param>
    /// <returns>the value of 64-bit floating point sRGB color channel in the range [0,1]</returns>
    private static double ComputeTransferSrgbFloat64(double luminance_0_1)
    {
        var value_0_1 =
            luminance_0_1 <= 0.0031308 ?
                12.92 * luminance_0_1 :
                (1.055 * Math.Pow(luminance_0_1, 1 / 2.4) - 0.055);
        return value_0_1;
    }

    /// <summary>
    /// Decodes the luminance from an sRGB color channel value.
    /// From <a href="https://www.color.org/srgb.pdf">Specification of sRGB</a> from the International Color Consortium:
    /// <para>
    /// 1. Inverting the color component transfer function
    /// Note: Starting from sRGB values in the range 0 to 1, which are obtained by dividing integer values by 2^(bit depth)–1.
    /// It is common practice (but not required) to use black and white normalized colorimetry when making ICC v2 display profiles. When this is done, the inversion of the color component transfer function is accomplished using 1D LUTs created using the inverse color component transfer function without inverse black normalization (scaling), which is provided in IEC 61966-2-1 as follows:
    /// If R, G, B are less than or equal to 0.04045
    ///     RL = R/12.92
    ///     GL = G/12.92
    ///     BL = B/12.02
    /// If R, G, B are greater than 0.04045
    ///     RL = ((R + 0.055)/1.055)^2.4
    ///     GL = ((R + 0.055)/1.055)^2.4
    ///     BL = ((R + 0.055)/1.055)^2.4
    /// </para>
    /// </summary>
    /// <param name="value_0_1">the value of 64-bit floating point sRGB color channel in the range [0,1]</param>
    /// <returns>the relative luminance in the range [0,1]</returns>
    public static double ComputeInverseTransferSrgbFloat64(double value_0_1)
    {
        var luminance_0_1 =
            value_0_1 <= 0.04045 ?
                value_0_1 / 12.92 :
                Math.Pow((value_0_1 + 0.055) / 1.055, 2.4);
        return luminance_0_1;
    }

    /// <summary>
    /// Ordered dithering according to <paramref name="dit"/>.
    /// </summary>
    /// <param name="luminance_0_1">the relative luminance in the range [0,1]</param>
    /// <param name="maxValue">the quantized value corresponding to a luminance of 1.0</param>
    /// <returns>the value of an sRGB color channel in the range [0,<paramref name="maxValue"/>]</returns>
    public static int TransferAndQuantize(double luminance_0_1, int maxValue)
    {
        // Compute the value in the range [0,maxValue], but don't quantize yet.
        var value_0_1 = ComputeTransferSrgbFloat64(luminance_0_1);
        var value_0_maxValue = value_0_1 * maxValue;

        // Find the two closest integer values and the luminances that they round-trip back to.
        var value_quantized_low = Math.Floor(value_0_maxValue);
        if (value_quantized_low == value_0_maxValue)
            return checked((int)value_quantized_low); // transfer happens to be exact
        var value_quantized_high = Math.Ceiling(value_0_maxValue);
        var luminance_0_1_roundTripLow = ComputeInverseTransferSrgbFloat64(value_quantized_low / maxValue);
        var luminance_0_1_roundTripHigh = ComputeInverseTransferSrgbFloat64(value_quantized_high / maxValue);

        // Choose the value whose inverse transfer is closest to luminance_0_1,
        // i.e., the one that round-trips closest to the original input.
        var errorLow = luminance_0_1 - luminance_0_1_roundTripLow;
        var errorHigh = luminance_0_1_roundTripHigh - luminance_0_1;
        double value_quantized;
        if (errorLow < errorHigh)
            value_quantized = value_quantized_low;
        else if (errorHigh < errorLow)
            value_quantized = value_quantized_high;
        else // midpoint rounding: towards even
            value_quantized = errorLow % 2 == 0 ? errorLow : errorHigh;
        return checked((int)value_quantized);
    }
    #region 8 bit(s) per channel

    /// <inheritdoc cref="ComputeTransferSrgbFloat64"/>
    /// <returns>the value of a(n) 8-bit sRGB color channel in the range [0,<see cref="byte.MaxValue"/>]</returns>
    public static byte ComputeTransferSrgb8(double luminance_0_1)
    {
        var value_quantized = TransferAndQuantize(luminance_0_1, byte.MaxValue);
        return checked((byte)value_quantized);
    }

    /// <inheritdoc cref="ComputeInverseTransferSrgbFloat64"/>
    /// <param name="value_quantized">the value of a(n) 8-bit sRGB color channel in the range [0,<see cref="byte.MaxValue"/>]</param>
    public static double ComputeInverseTransferSrgb8(byte value_quantized)
    {
        var value_0_1 = value_quantized / (double)byte.MaxValue;
        var luminance_0_1 = ComputeInverseTransferSrgbFloat64(value_0_1);
        return luminance_0_1;
    }

    /// <inheritdoc cref="ComputeTransferSrgb8"/>
    public static byte TransferSrgb8(this double luminance_0_1)
    {
        // optimization not yet implemented
        return ComputeTransferSrgb8(luminance_0_1);
    }

    /// <summary>
    /// cached outputs of <see cref="ComputeInverseTransferSrgb8"/> for every possible <see cref="byte"/> value, computed at runtime
    /// </summary>
    private static readonly double[] sInverseTransferSrgb8 = PreComputeInverseTransferSrgb8();

    private static double[] PreComputeInverseTransferSrgb8()
    {
        var result = new double[byte.MaxValue + 1];
        for (var i = 0; i < result.Length; i++)
        {
            result[i] = ComputeInverseTransferSrgb8((byte)i);
        }
        return result;
    }

    /// <inheritdoc cref="ComputeInverseTransferSrgb8(byte)"/>
    public static double InverseTransferSrgb8(byte value_quantized)
    {
        return sInverseTransferSrgb8[value_quantized];
    }

    #endregion 8 bit(s) per channel
}

#region 8 bit(s) per channel

public static class UInt8Extensions
{
    public static double InverseTransferSrgb(this byte value) => PixelMath.InverseTransferSrgb8(value);
}

public static class Rgb24Extensions
{
    public static RgbFloat192 InverseTransferSrgb(this Rgb24 value) => new(
        value.R.InverseTransferSrgb(),
        value.G.InverseTransferSrgb(),
        value.B.InverseTransferSrgb()
    );
}

#endregion 8 bit(s) per channel
