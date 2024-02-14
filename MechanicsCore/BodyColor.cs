namespace MechanicsCore;

public readonly record struct BodyColor(byte R, byte G, byte B)
{
    /// <summary>
    /// Radiance is "the radiant flux emitted, reflected, transmitted or received by a given surface, per unit solid angle per unit projected area."
    /// For our purposes, let's define radiance as the sum of the R, G, and B values.
    /// Pure white would have a radiance of 255+255+255 = 765.
    /// Fully saturated primary colors (red, green, and blue) have a radiance of 255+0+0 = 255.
    /// Fully saturated secondary colors (cyan, magenta, and yellow) have a radiance of 255+255+0 = 510.
    /// </summary>
    public int ComputeRadiance() => R + G + B;
}
