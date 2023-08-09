using MathNet.Spatial.Euclidean;
using MechanicsCore.Rust.mechanics_fast;
using MechanicsCore.StepConfiguring;

namespace MechanicsCore;

public class Body
{
    public int ID { get; }
    public string Name { get; }
    public BodyColor Color { get; set; }
    public double Mass { get; set; }
    public double Radius { get; set; }
    public bool Exists { get; set; } = true;

    public double Volume
    {
        get => Constants.SphereRadiusToVolume(Radius);
        set => Radius = Constants.SphereVolumeToRadius(value);
    }

    public double Density => Mass / Volume;

    public Body(int id, string? name = null, BodyColor? color = null, double mass = 0, double radius = 0, Vector3D position = default, Vector3D velocity = default)
    {
        ID = id;
        Name = name ?? ID.ToString();
        Color = color ?? BodyColors.GetSpacedCyclicColor(ID);
        Mass = mass;
        Radius = radius;
        Position = position;
        Velocity = velocity;
    }

    /// <summary>
    /// Helps us see small objects.
    /// All objects will have a glow applied, but smaller objects glow proportionally farther.
    /// An object with a radius of 0 will have a GlowRadius of <paramref name="minGlowRadius"/>.
    /// As radius approaches infinity, GlowRadius approaches radius.
    /// </summary>
    public double ComputeGlowRadius(double minGlowRadius) => Math.Sqrt(Radius * Radius + minGlowRadius * minGlowRadius);

    public Vector3D Position { get; set; }
    public Vector3D Velocity { get; set; }

    public Vector3D ComputeMomentum() => Mass * Velocity;

    public Vector3D ComputeAcceleration(IEnumerable<Body> allBodies, StepConfiguration config)
    {
        var a = default(Vector3D);
        foreach (var body2 in allBodies)
        {
            if (!body2.Exists || body2 == this)
            {
                continue;
            }
            a += GetAccelerationOn1DueTo2(this, body2, config);
        }
        return a;
    }

    public void ComputeStep(double dt, Vector3D a, out Vector3D p, out Vector3D v)
    {
        AssertIsReal(a);
        v = Velocity + dt * a;
        AssertIsReal(v);
        p = Position + dt * v;
        AssertIsReal(p);
    }

    private static void AssertIsReal(Vector3D vector)
    {
        if (
            !double.IsRealNumber(vector.X) ||
            !double.IsRealNumber(vector.Y) ||
            !double.IsRealNumber(vector.Z)
        )
        {
            throw new StepFailedException("Non-real vector");
        }
    }

    public void Step(Vector3D p, Vector3D v)
    {
        Position = p;
        Velocity = v;
    }

    private static Vector3D GetAccelerationOn1DueTo2(Body body1, Body body2, StepConfiguration config)
    {
        var displacement = body2.Position - body1.Position;
        var m2 = body2.Mass;

        if (config.CanTakeSimpleShortcut())
        {
            // Shortcut for simpler simulations
            return ComputePointlikeNewtonianGravitationalAcceleration(displacement, m2);
        }

        var distance = displacement.Length;
        var m1 = body1.Mass;
        var r1 = body1.Radius;
        var r2 = body2.Radius;

        var ag = ComputeGravitationalAcceleration(displacement, distance, m2, r1, r2, config.GravityConfig, config.BuoyantGravityRatio);

        if (distance > r1 + r2)
        {
            // The bodies are not touching.
            return ag;
        }

        if (m1 == 0 || m2 == 0)
        {
            // avoid infinities
            return ag;
        }

        var others = ComputeOtherForces(body1, body2, displacement, distance, config);
        others /= m1; // convert others from a force to an acceleration
        return ag + others;
    }

    private static Vector3D ComputeGravitationalAcceleration(
        Vector3D displacement,
        double distance,
        double m2,
        double r1,
        double r2,
        GravityType gravity,
        double buoyantGravityRatio
    )
    {
        switch (gravity)
        {
            case GravityType.None:
                return default;
            case GravityType.Newton_Pointlike:
                return ComputePointlikeNewtonianGravitationalAcceleration(displacement, m2, distance);
        }

        var kissingDistance = r1 + r2;
        if (distance >= kissingDistance)
        {
            // The bodies are not touching, which means we can treat them as points.
            return ComputePointlikeNewtonianGravitationalAcceleration(displacement, m2, distance);
        }

        var agAtKissingDistance = Constants.GravitationalConstant * m2 * displacement / (kissingDistance * kissingDistance * kissingDistance);

        switch (gravity)
        {
            case GravityType.Newton_LinearAfterTouching:
                // When you're inside an object, the gravity starts falling linearly with distance from the center.
                // Start with the force when the surfaces are barely touching:
                // Ag = displacement*G*m2/(r1+r2)^3
                // Then scale it by displacement/(r1+r2):
                return (distance / kissingDistance) * agAtKissingDistance;

            case GravityType.Newton_Buoyant:
                // When you're inside an object, the gravity starts falling linearly.
                // By the time you're fully engulfed, the gravity is the opposite of what it was when they were barely touching.
                // From there it drops linearly, to zero when the centers are overlapping.
                var engulfmentDistance = Math.Abs(r1 - r2);
                var agAtEngulfmentDistance = -buoyantGravityRatio * agAtKissingDistance;

                if (distance > engulfmentDistance)
                {
                    // the bodies are partially (but not fully) overlapping
                    return (distance - engulfmentDistance) / (kissingDistance - engulfmentDistance) * (agAtKissingDistance - agAtEngulfmentDistance) + agAtEngulfmentDistance;
                }

                // the smaller body is fully inside the larger
                return (distance / engulfmentDistance) * agAtEngulfmentDistance;
        }

        throw Utils.OutOfRange(nameof(gravity), gravity);
    }

    private static Vector3D ComputePointlikeNewtonianGravitationalAcceleration(Vector3D displacement, double m2)
    {
#if DISABLE_RUST
        var distance = displacement.Length;
        return ComputePointlikeNewtonianGravitationalAcceleration(displacement, m2, distance);
#else
        return mechanics_fast.compute_gravitational_acceleration(displacement, m2);
#endif
    }

    private static Vector3D ComputePointlikeNewtonianGravitationalAcceleration(Vector3D displacement, double m2, double distance)
    {
        // Acceleration due to gravity (Newton's law of gravitation) in scalar form:
        // Ag = G*m2/distance^2
        // To make it a vector, we need to multiply by the unit vector of the displacement (displacement/|displacement|):
        // Ag = (displacement/|displacement|)*G*m2/distance^2
        // Since |displacement| is the distance, this simplifies to:
        // Ag = displacement*G*m2/distance^3
#if DISABLE_RUST
        return Constants.GravitationalConstant * m2 * displacement / (distance * distance * distance);
#else
        return mechanics_fast.compute_gravitational_acceleration(displacement, m2);
#endif
    }

    private static Vector3D ComputeOtherForces(Body body1, Body body2, Vector3D displacement, double distance, StepConfiguration config)
    {
        return ComputeDragForce(body1, body2, displacement, distance, config);
    }

    private static Vector3D ComputeDragForce(Body body1, Body body2, Vector3D displacement, double distance, StepConfiguration config)
    {
        if (config.CollisionConfig != CollisionType.Drag)
        {
            return default;
        }
        var dragCoefficient = config.DragCoefficient;

        // Assume the bodies are fully overlapping, which might not actually be true.
        var relativeVelocity = body2.Velocity - body1.Velocity;
        var relativeSpeed = relativeVelocity.Length;
        var minRadius = Math.Min(body1.Radius, body2.Radius);
        var crossSectionalArea = Math.PI * minRadius * minRadius;

        var fdMagnitude =
            0.5 *
            (body1.Density + body2.Density) *
            (relativeVelocity * relativeVelocity) *
            dragCoefficient * crossSectionalArea;

        // compute the unit vector of the force's direction
        var fdDirectionUnit = relativeVelocity / relativeSpeed;
        // compute the force vector
        var fd = fdMagnitude * fdDirectionUnit;

        // In theory, vector is currently the correct drag force.
        // Also in theory, the higher DragCoefficient value is, the more the bodies should clump up.
        // But in practice, at high DragCoefficients, further increasing DragCoefficients
        // actually flings the bodies apart more.
        // I think the instantaneous acceleration ends up being too high and then the time step is too long,
        // whereas in theory, the acceleration should drop off rapidly within that time step.
        // Therefore, we cap the force at an amount that would bring the bodies' relative velocity to zero in one time step.

        if (WillBounce(body1, body2, relativeVelocity, fd, config.StepTime, false))
        {
            // Instead, apply a force sufficient to make both velocities the same while conserving momentum
            // (assuming the opposite force is applied to body2)
            var m1 = body1.Mass;
            var m2 = body2.Mass;
            var v1 = body1.Velocity;
            var v2 = body2.Velocity;
            var t = config.StepTime;
            fd = m1 / t * ((m1 * v1 + m2 * v2) / (m1 + m2) - v1);

            // Check again!
            WillBounce(body1, body2, relativeVelocity, fd, config.StepTime, true);
        }

        // Now we have computed the drag.
        // However, I want things to be able to roll/slide past each other,
        // so return only the radial component of drag.
        var component = fd * displacement * displacement / distance / distance;

        // Will be NaN if the relative velocity was 0.
        return double.IsFinite(component.Length) ? component : default;
    }

    private static bool WillBounce(Body body1, Body body2, Vector3D relativeVelocity, Vector3D force, double stepTime, bool isDoubleCheck)
    {
        var nextAcceleration1 = force / body1.Mass;
        var nextAcceleration2 = -force / body2.Mass;
        var changeInVelocity1 = stepTime * nextAcceleration1;
        var changeInVelocity2 = stepTime * nextAcceleration2;
        var nextVelocity1 = body1.Velocity + changeInVelocity1;
        var nextVelocity2 = body2.Velocity + changeInVelocity2;
        var nextRelativeVelocity = nextVelocity2 - nextVelocity1;
        var dot = relativeVelocity * nextRelativeVelocity;
        if (isDoubleCheck && dot < -0.0001)
        {
            throw new StepFailedException("They bounced off each other. This should have been prevented by the capping formula.");
        }
        if (isDoubleCheck && dot > 0.0001)
        {
            throw new StepFailedException("They bounced the first time, but now they're still moving.");
        }
        return dot < 0;
    }
}
