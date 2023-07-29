﻿using MathNet.Spatial.Euclidean;

namespace MechanicsCore;

public class Falling : RandomSimulation
{
    public override double dt_step => 512;
    protected override int steps_per_leap => 64;

    public override Vector3D DisplayBound0 { get; }
    public override Vector3D DisplayBound1 { get; }
    public override IReadOnlyList<Body> Bodies { get; }

    public Falling(double systemRadius, int numBodies, double systemMass, double totalVolume, double maxVelocity, int? seed = null)
        : base(seed)
    {
        DisplayBound1 = new(systemRadius * 2, systemRadius * 2, systemRadius * 2 );
        DisplayBound0 = -DisplayBound1;
        var bodyMass = systemMass / numBodies;
        var bodyVolume = totalVolume / numBodies;
        var bodyRadius = Constants.SphereVolumeToRadius(bodyVolume);
        var bodies = new Body[numBodies];
        Vector3D systemMomentum = default;
        for (int i = 0; i < numBodies; i++)
        {
            var bodyPosition = RandomPointInBall(Random, systemRadius);
            var bodyVelocity = maxVelocity == 0 ? default : RandomPointInBall(Random, maxVelocity);
            bodies[i] = new Body(this,
                mass: bodyMass,
                radius: bodyRadius,
                position: bodyPosition,
                velocity: bodyVelocity
            );
            var bodyMomentum = bodyMass * bodyVelocity;
            systemMomentum += bodyMomentum;
        }

        // Adjust our frame of reference such that the system's center of mass is at rest
        var systemVelocity = systemMomentum / systemMass;
        foreach (var body in bodies)
        {
            body.Velocity -= systemVelocity;
        }

        Bodies = bodies;
    }

    public static Vector3D RandomPointInBall(Random random, double radius)
    {
        Vector3D v;
        do
        {
            var x = (random.NextDouble() * 2 - 1) * radius;
            var y = (random.NextDouble() * 2 - 1) * radius;
            var z = (random.NextDouble() * 2 - 1) * radius;
            v = new(x, y, z);
        } while (v.Length > radius);
        return v;
    }
}
