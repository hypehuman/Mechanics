using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace MechanicsCore;

public class Falling : RandomSimulation
{
    public override double dt_step => 512;
    protected override int steps_per_leap => 64;

    public override Vector<double> DisplayBound0 { get; }
    public override Vector<double> DisplayBound1 { get; }
    public override IReadOnlyList<Body> Bodies { get; }

    public Falling(double systemRadius, int numBodies, double systemMass, double totalVolume, double maxVelocity, int? seed = null)
        : base(seed)
    {
        DisplayBound1 = new DenseVector(new[] { systemRadius * 2, systemRadius * 2, systemRadius * 2 });
        DisplayBound0 = -DisplayBound1;
        var bodyMass = systemMass / numBodies;
        var bodyVolume = totalVolume / numBodies;
        var bodyRadius = Math.Pow(bodyVolume * 3 / Math.PI, 1d / 3);
        var bodies = new Body[numBodies];
        Vector<double> systemMomentum = new DenseVector(3);
        for (int i = 0; i < numBodies; i++)
        {
            var bodyPosition = RandomPointInBall(Random, systemRadius);
            var bodyVelocity = maxVelocity == 0 ? null : RandomPointInBall(Random, maxVelocity);
            bodies[i] = new Body(this,
                mass: bodyMass,
                radius: bodyRadius,
                position: bodyPosition,
                velocity: bodyVelocity
            );
            var bodyMomentum = bodyMass * bodyVelocity;
            systemMomentum.Add(bodyMomentum, systemMomentum);
        }

        // Adjust our frame of reference such that the system's center of mass is at rest
        var systemVelocity = systemMomentum / systemMass;
        foreach (var body in bodies)
        {
            body.Velocity.Subtract(systemVelocity, body.Velocity);
        }

        Bodies = bodies;
    }

    public static Vector<double> RandomPointInBall(Random random, double radius)
    {
        DenseVector v;
        do
        {
            var x = (random.NextDouble() * 2 - 1) * radius;
            var y = (random.NextDouble() * 2 - 1) * radius;
            var z = (random.NextDouble() * 2 - 1) * radius;
            v = new DenseVector(new[] { x, y, z });
        } while (v.L2Norm() > radius);
        return v;
    }
}
