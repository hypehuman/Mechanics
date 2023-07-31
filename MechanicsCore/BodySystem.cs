using MathNet.Spatial.Euclidean;

namespace MechanicsCore;

public static class BodySystem
{
    /// <summary>
    /// Adjust our frame of reference such that the system's center of mass is at rest
    /// </summary>
    public static void SetNetZeroMomentum(IEnumerable<Body> bodies)
    {
        var systemMass = 0d;
        var systemMomentum = default(Vector3D);
        foreach (var body in bodies)
        {
            systemMass += body.Mass;
            systemMomentum += body.ComputeMomentum();
        }
        var systemVelocity = systemMomentum / systemMass;
        foreach (var body in bodies)
        {
            body.Velocity -= systemVelocity;
        }
    }
}
