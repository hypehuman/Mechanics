using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace MechanicsCore;

public class Body
{
    public Simulation Simulation { get; }
    public int ID { get; }
    public string Name { get; }
    public double Mass { get; set; }
    public double Radius { get; set; }
    public double DisplayRadius { get; set; }

    public double Volume => 4d / 3 * Math.PI * Radius * Radius * Radius;
    public double Density => Mass / Volume;

    public Body(Simulation simulation, string name = "b", double mass = 0, double radius = 0, double? displayRadius = null, Vector<double>? position = null, Vector<double>? velocity = null)
    {
        Simulation = simulation;
        ID = Simulation.NextBodyID;
        Name = name;
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

        var ag = ComputeGravitationalAcceleration(displacement, distance, m2, r1, r2, body1.Simulation.BuoyantGravity);
        a.Add(ag, a);

        if (distance > r1 + r2)
        {
            // The bodies are not touching.
            return;
        }

        if (m1 == 0 || m2 == 0)
        {
            // avoid infinities
            return;
        }

        var others = ComputeOtherForces(body1, body2, displacement, distance);
        others.Divide(m1, others); // convert others from a force to an acceleration
        a.Add(others, a);
    }

    private static Vector<double> ComputeGravitationalAcceleration(Vector<double> displacement, double distance, double m2, double r1, double r2, bool buoyant)
    {
        var kissingDistance = r1 + r2;
        if (distance >= kissingDistance)
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

        var agAtKissingDistance = displacement * Constants.GravitationalConstant * m2 / (kissingDistance * kissingDistance * kissingDistance);

        if (!buoyant)
        {
            // When you're inside an object, the gravity starts falling linearly with distance from the center.
            // Start with the force when the surfaces are barely touching:
            // Ag = displacement*G*m2/(r1+r2)^3
            // Then scale it by displacement/(r1+r2):
            return agAtKissingDistance * (distance / kissingDistance);
        }

        // When you're inside an object, the gravity starts falling linearly.
        // By the time you're fully engulfed, the gravity is the opposite of what it was when they were barely touching.
        // From there it drops linearly, to zero when the centers are overlapping.
        var engulfmentDistance = Math.Abs(r1 - r2);
        var agAtEngulfmentDistance = -agAtKissingDistance;

        if (distance > engulfmentDistance)
        {
            // the bodies are partially (but not fully) overlapping
            return (distance - engulfmentDistance) / (kissingDistance - engulfmentDistance) * (agAtKissingDistance - agAtEngulfmentDistance) + agAtEngulfmentDistance;
        }

        // the smaller body is fully inside the larger
        return agAtEngulfmentDistance * (distance / engulfmentDistance);
    }

    private static Vector<double> ComputeOtherForces(Body body1, Body body2, Vector<double> displacement, double distance)
    {
        var f = new DenseVector(3);

        var fDrag = ComputeDragForce(body1, body2, displacement, distance);
        f.Add(fDrag, f);
        return f;
    }

    private static Vector<double> ComputeDragForce(Body body1, Body body2, Vector<double> displacement, double distance)
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

        var fdMagnitude =
            0.5 *
            (body1.Density + body2.Density) *
            (relativeVelocity * relativeVelocity) *
            dragCoefficient * crossSectionalArea;

        // compute the unit vector of the force's direction
        var vector = relativeVelocity / relativeSpeed;
        // compute the force vector
        vector.Multiply(fdMagnitude, vector);

        // In theory, vector is currently the correct drag force.
        // Also in theory, the higher DragCoefficient value is, the more the bodies should clump up.
        // But in practice, at high DragCoefficients, further increasing DragCoefficients
        // actually flings the bodies apart more.
        // I think the instantaneous acceleration ends up being too high and then the time step is too long,
        // whereas in theory, the acceleration should drop off rapidly within that time step.
        // Therefore, we cap the force at an amount that would bring the bodies' relative velocity to zero in one time step.

        if (WillBounce(body1, body2, relativeVelocity, vector, false))
        {
            // Instead, apply a force sufficient to make both velocities the same while conserving momentum
            // (assuming the opposite force is applied to body2)
            var m1 = body1.Mass;
            var m2 = body2.Mass;
            var v1 = body1.Velocity;
            var v2 = body2.Velocity;
            var t = body1.Simulation.dt_step;
            vector = ((m1 * v1 + m2 * v2) / (m1 + m2) - v1) * m1 / t;

            // Check again!
            WillBounce(body1, body2, relativeVelocity, vector, true);
        }

        // Now we have computed the drag.
        // However, I want things to be able to roll/slide past each other,
        // so return only the radial component of drag.
        var component = vector * displacement * displacement / distance / distance;

        return component;
    }

    private static bool WillBounce(Body body1, Body body2, Vector<double> relativeVelocity, Vector<double> force, bool isDoubleCheck)
    {
        var nextAcceleration1 = force / body1.Mass;
        var nextAcceleration2 = -force / body2.Mass;
        var changeInVelocity1 = nextAcceleration1 * body1.Simulation.dt_step;
        var changeInVelocity2 = nextAcceleration2 * body2.Simulation.dt_step;
        var nextVelocity1 = body1.Velocity + changeInVelocity1;
        var nextVelocity2 = body2.Velocity + changeInVelocity2;
        var nextRelativeVelocity = nextVelocity2 - nextVelocity1;
        var dot = relativeVelocity * nextRelativeVelocity;
        if (isDoubleCheck && dot < -0.0001)
        {
            throw new Exception("They bounced off each other. This should have been prevented by the capping formula.");
        }
        if (isDoubleCheck && dot > 0.0001)
        {
            throw new Exception("They bounced the first time, but now they're still moving.");
        }
        return dot < 0;
    }
}
