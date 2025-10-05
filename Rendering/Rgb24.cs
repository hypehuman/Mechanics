using System.Runtime.InteropServices;

namespace Rendering;

[StructLayout(LayoutKind.Explicit)]
public readonly struct Rgb24 : IEquatable<Rgb24>
{
    public static Rgb24 Black => default;

    [FieldOffset(2)]
    public readonly byte R;

    [FieldOffset(1)]
    public readonly byte G;

    [FieldOffset(0)]
    public readonly byte B;

    public Rgb24(byte r, byte g, byte b)
    {
        R = r;
        G = g;
        B = b;
    }

    public static bool operator !=(Rgb24 left, Rgb24 right)
    {
        return !(left == right);
    }

    public static bool operator ==(Rgb24 left, Rgb24 right)
    {
        return left.Equals(right);
    }

    public override bool Equals(object? obj)
    {
        return obj is Rgb24 other && Equals(other);
    }

    public bool Equals(Rgb24 other) =>
        R == other.R && G == other.G && B == other.B;

    public override int GetHashCode() =>
        (R << 16) ^ (G << 8) ^ B;
}
