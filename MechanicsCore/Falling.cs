using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace MechanicsCore;

public class Falling : Simulation
{
    public override double dt_step => 512;
    protected override int steps_per_leap => 4;

    public override Vector<double> DisplayBound0 { get; }
    public override Vector<double> DisplayBound1 { get; }
    public override IReadOnlyList<Body> Bodies { get; }

    public Falling(double systemRadius, int numBodies, double totalMass, double totalVolume, int? seedIn = null)
    {
        var seed = seedIn ?? new Random().Next();
        System.Diagnostics.Debug.WriteLine($"{GetType().Name} seed: {seed}");
        var random = new Random(seed);

        DisplayBound1 = new DenseVector(new[] { systemRadius * 2, systemRadius * 2, systemRadius * 2 });
        DisplayBound0 = -DisplayBound1;
        var bodyMass = totalMass / numBodies;
        var bodyVolume = totalVolume / numBodies;
        var bodyRadius = Math.Pow(bodyVolume * 3 / Math.PI, 1d / 3);
        var bodies = new Body[numBodies];
        for (int i = 0; i < numBodies; i++)
        {
            DenseVector position;
            do
            {
                var x = (random.NextDouble() * 2 - 1) * systemRadius;
                var y = (random.NextDouble() * 2 - 1) * systemRadius;
                var z = (random.NextDouble() * 2 - 1) * systemRadius;
                position = new DenseVector(new[] { x, y, z });
            } while (position.L2Norm() > systemRadius);
            bodies[i] = new Body(this,
                mass: bodyMass,
                radius: bodyRadius,
                position: position
            );
        }
        Bodies = bodies;
    }
}
