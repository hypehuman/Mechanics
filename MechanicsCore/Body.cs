using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace MechanicsCore;

public class Body
{
    public Simulation Simulation { get; }
    public string Name { get; }
    public double Mass { get; set; }
    public double Radius { get; set; }
    public double DisplayRadius { get; set; }

    public double Volume => 4d / 3 * Math.PI * Radius * Radius * Radius;
    public double Density => Mass / Volume;

    public Body(Simulation simulation, string? name = null, double mass = 0, double radius = 0, double? displayRadius = null, Vector<double>? position = null, Vector<double>? velocity = null)
    {
        Simulation = simulation;
        Name = name ?? Simulation.NextBodyID.ToString();
        Mass = mass;
        Radius = radius;
        DisplayRadius = displayRadius ?? radius;
        Position = position ?? new DenseVector(3);
        Velocity = velocity ?? new DenseVector(3);
    }

    public Vector<double> Position { get; set; }
    public Vector<double> Velocity { get; set; }

    public double X => Position[0];
    public double Y => Position[1];
    public double Z => Position[2];

    public Vector<double> ComputeAcceleration(IEnumerable<Body> allBodies)
    {
        var a = new DenseVector(3);
        foreach (var body2 in allBodies)
        {
            if (body2 == this)
            {
                continue;
            }
            AddAccelerationOn1DueTo2(this, body2, a);
        }
        return a;
    }

    public void Step(double dt, Vector<double> a)
    {
        var v = Velocity;
        v.Add(a * dt, v);

        var p = Position;
        p.Add(v * dt, p);
    }

    private static void AddAccelerationOn1DueTo2(Body body1, Body body2, Vector<double> a)
    {
        var displacement = body2.Position - body1.Position;
        var distance = displacement.L2Norm();
        var m1 = body1.Mass;
        var m2 = body2.Mass;
        var r1 = body1.Radius;
        var r2 = body2.Radius;
        var radii = r1 + r2;

        var ag = ComputeGravitationalAcceleration(displacement, distance, m2, radii);
        a.Add(ag, a);

        if (distance > radii)
        {
            // The bodies are not touching.
            return;
        }

        if (m1 == 0 || m2 == 0)
        {
            // avoid infinities
            return;
        }

        var others = ComputeOtherForces(body1, body2);
        others.Divide(m1, others); // convert others from a force to an acceleration
        a.Add(others, a);
    }

    private static Vector<double> ComputeGravitationalAcceleration(Vector<double> displacement, double distance, double m2, double radii)
    {
        if (distance >= radii)
        {
            // The bodies are not touching.
            // Acceleration due to gravity (Newton's law of gravitation) in scalar form:
            // Ag = G*m2/distance^2
            // To make it a vector, we need to multiply by the unit vector of the displacement (displacement/|displacement|):
            // Ag = (displacement/|displacement|)*G*m2/distance^2
            // Since |displacement| is the distance, this simplifies to:
            // Ag = displacement*G*m2/distance^3
            return displacement * Constants.GravitationalConstant * m2 / (distance * distance * distance);
        }
        else
        {
            // The bodies are overlapping.
            // When you're inside an object, the gravity starts falling linearly with distance from the center.
            // Start with the force when the surfaces are barely touching:
            // Ag = displacement*G*m2/radii^3
            // Then scale it by (displacement/radii):
            // Ag = displacement*distance*G*m2/radii^4
            var radii_2 = radii * radii;
            return displacement * distance * Constants.GravitationalConstant * m2 / (radii_2 * radii_2);
        }
    }

    private static Vector<double> ComputeOtherForces(Body body1, Body body2)
    {
        var f = new DenseVector(3);

        var fDrag = ComputeDragForce(body1, body2);
        f.Add(fDrag, f);
        return f;
    }

    private static Vector<double> ComputeDragForce(Body body1, Body body2)
    {
        var dragCoefficient = body1.Simulation.DragCoefficient;
        if (dragCoefficient == 0)
        {
            return new SparseVector(3);
        }

        // Assume the bodies are fully overlapping, which might not actually be true.
        var relativeVelocity = body2.Velocity - body1.Velocity;
        var relativeSpeed = relativeVelocity.L2Norm();
        var minRadius = Math.Min(body1.Radius, body2.Radius);
        var crossSectionalArea = Math.PI * minRadius * minRadius;

        var fdMagnitudeNaive =
            0.5 *
            (body1.Density + body2.Density) *
            (relativeVelocity * relativeVelocity) *
            dragCoefficient * crossSectionalArea;

        // In theory, fdMagnitudeNaive is the correct magnitude of the drag force.
        // Also in theory, the higher DragCoefficient value is, the more the bodies should clump up.
        // But in practice, at high DragCoefficients, further increasing DragCoefficients
        // actually flings the bodies apart more.
        // I think the instantaneous acceleration ends up being too high and then the time step is too long,
        // whereas in theory, the acceleration should drop off rapidly within that time step.
        // Therefore, we cap the force at an amount that would bring the bodies' relative velocity to zero in one time step.
        var fdMagnitudeMax = body1.Mass * body2.Mass * relativeSpeed * body1.Simulation.dt_step / (body1.Mass + body2.Mass);
        var fdMagnitude = Math.Min(fdMagnitudeMax, fdMagnitudeNaive);

        // compute the unit vector of the force's direction
        var vector = relativeVelocity / relativeSpeed;
        // compute the force vector
        vector.Multiply(fdMagnitude, vector);

        // DEBUG: Another way to compute the cap.
        var nextAcceleration1 = vector / body1.Mass;
        var nextAcceleration2 = -vector / body2.Mass;
        var changeInVelocity1 = nextAcceleration1 * body1.Simulation.dt_step;
        var changeInVelocity2 = nextAcceleration2 * body2.Simulation.dt_step;
        var nextVelocity1 = body1.Velocity + changeInVelocity1;
        var nextVelocity2 = body2.Velocity + changeInVelocity2;
        var nextRelativeVelocity = nextVelocity2 - nextVelocity1;
        var dot = relativeVelocity * nextRelativeVelocity;
        if (dot < -0.001)
        {
            throw new Exception("The thing turned around. This should have been prevented by the capping formula.");
        }
        if (dot < 0)
        {
            // TODO : Make the correction here.
        }

        return vector;
    }
}
