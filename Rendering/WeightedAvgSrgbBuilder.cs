namespace Rendering;

/// <summary>
/// Warning: mutable struct!
/// Stores the running total of a calculation of the weighted average by luminance of <see cref="Rgb24"/> values.
/// By computing the avearge by luminance instead of the simple average,
/// we acknowledge the nonlinear relationship between pixel values and luminance.
/// </summary>
public struct WeightedAvgSrgbBuilder
{
    /// <summary>the sum of the weights</summary>
    private double _w;

    /// <summary name="R">the weighted sum of the luminances of each channel</summary>
    private RgbFloat192 _l;

    /// <summary>
    /// Warning: mutates the struct!
    /// </summary>
    public void Append(double weight, Rgb24 value)
    {
        AppendLuminance(weight, value.InverseTransferSrgb());
    }

    /// <summary>
    /// Warning: mutates the struct!
    /// </summary>
    public void AppendLuminance(double weight, RgbFloat192 luminance)
    {
        _w += weight;
        _l += weight * luminance;
    }

    public readonly RgbFloat192 AverageLuminance()
    {
        return _l / _w;
    }
}
