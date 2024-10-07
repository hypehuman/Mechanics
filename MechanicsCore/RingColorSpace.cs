using GuiByReflection.Models;

namespace MechanicsCore;

public delegate BodyColor GetBodyColor(double hue_0_1);

/// <summary>
/// Most color spaces are 3-dimensional;
/// i.e., it takes three parameters to define a color.
/// I wanted a 1-dimensional color space for my bodies.
/// What's more, I wanted the space to be circular modulo 1,
/// i.e., -1, 0, and 1 would all be one color, -0.9, 0.1, and 1.1 would all be a different color, etc.
/// </summary>
public enum RingColorSpace
{
    [GuiHelp("Returns a color with the given sRGB hue angle with maximum full saturation.")]
    RgbSaturated,

    [GuiHelp("Returns a color with the given sRGB hue angle with a fixed radiance of 1.0 out of 3.0.")]
    RgbFixedRadiance,
}

public static class CircularColorSpaces
{
    public static GetBodyColor GetFunc(this RingColorSpace colorSpace) => colorSpace switch
    {
        RingColorSpace.RgbSaturated => RgbSaturated,
        RingColorSpace.RgbFixedRadiance => RgbFixedRadiance,
        _ => throw Utils.OutOfRange(nameof(colorSpace), colorSpace)
    };

    public static BodyColor GetBodyColor(this RingColorSpace colorSpace, double hue_0_1)
    {
        return colorSpace.GetFunc()(hue_0_1);
    }

    private static RgbDoubles_0_1 RgbSaturated_0_1(double hue_0_1)
    {
        return new(
            R: channelValue(5),
            G: channelValue(3),
            B: channelValue(1)
        );

        double channelValue(byte channelConstant)
        {
            var k = (channelConstant + hue_0_1 * 6) % 6;
            var darknessUnbounded = Math.Min(k, 4 - k);
            var darkness01 = Math.Max(0, Math.Min(1, darknessUnbounded));
            var luminance01 = 1 - darkness01;
            return luminance01;
        }
    }

    private static BodyColor RgbSaturated(double hue_0_1)
    {
        var sat01 = RgbSaturated_0_1(hue_0_1);
        var satBytes = sat01.ToBodyColor();
        return satBytes;
    }

    private static BodyColor RgbFixedRadiance(double hue_0_1)
    {
        var sat01 = RgbSaturated_0_1(hue_0_1);
        var normalized01 = NormalizeRadiance(sat01);
        var normalizedBytes = normalized01.ToBodyColor();
        return normalizedBytes;
    }

    /// <summary>
    /// Radiance is "the radiant flux emitted, reflected, transmitted or received by a given surface, per unit solid angle per unit projected area."
    /// For our purposes, let's define radiance as the sum of the R, G, and B values, each ranging from 0 to 1.
    /// Pure white would have a radiance of 1+1+1 = 3.
    /// Fully saturated primary colors (red, green, and blue) have a radiance of 1+0+0 = 1.
    /// Fully saturated secondary colors (cyan, magenta, and yellow) have a radiance of 1+1+0 = 2.
    /// Since _getColor returns only fully saturated colors, the minimum possible radiance is 1.
    /// So any colors that have a radiance over 1, darken them to normalize.
    /// </summary>
    private static RgbDoubles_0_1 NormalizeRadiance(RgbDoubles_0_1 input)
    {
        const double targetRadiance = 1f;
        var radiance = input.R + input.G + input.B;
        if (radiance > targetRadiance)
        {
            var factor = targetRadiance / radiance;
            return new(
                factor * input.R,
                factor * input.G,
                factor * input.B
            );
        }
        else
        {
            if (radiance < targetRadiance)
            {
                // TODO: Increase the radiance of darker colors.
                // This would make them whiter, e.g., red would become pink.
                // Currently irrelevant because RgbSaturated never returns radiances less than 1.
            }

            return input;
        }
    }

    public readonly record struct RgbDoubles_0_1(double R = 0, double G = 0, double B = 0)
    {
        public BodyColor ToBodyColor() => new(
            R: Double_0_1ToByte_0_255(R),
            G: Double_0_1ToByte_0_255(G),
            B: Double_0_1ToByte_0_255(B)
        );
    }

    private static byte Double_0_1ToByte_0_255(double double_0_1)
    {
        var double_0_255 = double_0_1 * 256f - 0.5f;
        var result =
            double_0_255 < byte.MinValue ? byte.MinValue :
            double_0_255 > byte.MaxValue ? byte.MaxValue :
            (byte)Math.Round(double_0_255);
        return result;
    }
}
