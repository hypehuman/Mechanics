using MathNet.Spatial.Euclidean;

namespace MechanicsCore;

public class Line : Simulation
{
    public override double dt_step => 1;
    protected override int steps_per_leap => 128;

    public override Vector3D DisplayBound0 { get; }
    public override Vector3D DisplayBound1 { get; }
    public override IReadOnlyList<Body> Bodies { get; }

    public Line(int numBodies, double bodyMass, double bodyRadius)
    {
        var bodies = new Body[numBodies];
        for (var i = 0; i < numBodies; i++)
        {
            bodies[i] = new Body(this,
                mass: bodyMass,
                radius: bodyRadius,
                position: new((i * 2 + 1) * bodyRadius, 0, 0)
            );
        }
        Bodies = bodies;
        DisplayBound0 = new(0, -bodyRadius, -bodyRadius);
        DisplayBound1 = new(numBodies * 2 * bodyRadius, bodyRadius, bodyRadius);
    }
}
