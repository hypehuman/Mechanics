using MathNet.Spatial.Euclidean;
using MechanicsCore.PhysicsConfiguring;

namespace MechanicsCore;

public class Link
{
    public BodyPair Bodies { get; }
    public double Strength { get; set; }

    public Link(Body body1, Body body2, double strength = 0)
    {
        Bodies = new BodyPair(body1, body2);
        Strength = strength;
    }

    internal Vector3D ComputeForce(Vector3D displacement, double distance, PhysicsConfiguration config)
    {
        return Strength * displacement;
    }
}
