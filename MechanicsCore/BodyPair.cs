namespace MechanicsCore;

public readonly struct BodyPair : IEquatable<BodyPair>
{
    Body A { get; }
    Body B { get; }

    public BodyPair(Body body1, Body body2)
    {
        if (body2.ID > body1.ID)
        {
            A = body2;
            B = body1;
        }
        else
        {
            A = body1;
            B = body2;
        }
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(A, B);
    }

    public bool Equals(BodyPair other)
    {
        return A.Equals(other.A) && B.Equals(other.B);
    }

    public override bool Equals(object? obj)
    {
        return obj is BodyPair other && Equals(other);
    }

    public static bool operator ==(BodyPair left, BodyPair right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(BodyPair left, BodyPair right)
    {
        return !(left == right);
    }
}
