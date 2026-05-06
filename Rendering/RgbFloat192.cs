namespace Rendering;

public readonly partial struct RgbFloat192 : IEquatable<RgbFloat192>
{
    public static RgbFloat192 Black => default;

    public readonly double R;
    public readonly double G;
    public readonly double B;

    public RgbFloat192(double r, double g, double b)
    {
        R = r;
        G = g;
        B = b;
    }

    public static bool operator !=(RgbFloat192 left, RgbFloat192 right)
    {
        return !(left == right);
    }

    public static bool operator ==(RgbFloat192 left, RgbFloat192 right)
    {
        return left.Equals(right);
    }

    public override bool Equals(object? obj)
    {
        return obj is RgbFloat192 other && Equals(other);
    }

    public bool Equals(RgbFloat192 other) =>
        R == other.R && G == other.G && B == other.B;

    public override int GetHashCode() =>
        HashCode.Combine(R, G, B);

    public override string ToString()
    {
        return $"{nameof(RgbFloat192)} {{ {nameof(R)} = {R}, {nameof(G)} = {G}, {nameof(B)} = {B} }}";
    }

    public static RgbFloat192 operator +(RgbFloat192 left, RgbFloat192 right)
    {
        return new(
            left.R + right.R,
            left.G + right.G,
            left.B + right.B
        );
    }

    public static RgbFloat192 operator *(RgbFloat192 left, double right)
    {
        return new(
            left.R * right,
            left.G * right,
            left.B * right
        );
    }

    public static RgbFloat192 operator *(double left, RgbFloat192 right)
    {
        return right * left;
    }

    public static RgbFloat192 operator /(RgbFloat192 left, double right)
    {
        return new(
            left.R / right,
            left.G / right,
            left.B / right
        );
    }

    /// <summary>
    /// Returns the simple mean of each of the values.
    /// </summary>
    public static RgbFloat192 Average(IEnumerable<RgbFloat192> values, int count)
    {
        if (count < 1)
        {
            throw new ArgumentOutOfRangeException($"Expected: {nameof(count)}>=1. Actual: {nameof(count)}={count}");
        }

        unchecked
        {
            RgbFloat192 sum = default;
            foreach (var value in values)
            {
                sum += value;
            }

            var avg = sum / count;
            return avg;
        }
    }

    public Rgb24 TransferSrgb24() => new(
        R.TransferSrgb8(),
        G.TransferSrgb8(),
        B.TransferSrgb8()
    );
}
