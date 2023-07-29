﻿using MathNet.Spatial.Euclidean;

namespace MechanicsCore;

public class TwoBodies : RandomSimulation
{
    public override double dt_step => 1;
    protected override int steps_per_leap => 1024;

    public override Vector3D DisplayBound0 { get; }
    public override Vector3D DisplayBound1 { get; }
    public override IReadOnlyList<Body> Bodies { get; }

    public TwoBodies(double systemRadius, double totalMass, double totalVolume, int? seed = null)
        : base(seed)
    {
        var numBodies = 2;
        DisplayBound1 = new(systemRadius * 2, systemRadius * 2, systemRadius * 2);
        DisplayBound0 = -DisplayBound1;
        var bodies = new Body[numBodies];
        var fraction0 = Random.NextDouble();
        for (int i = 0; i < numBodies; i++)
        {
            var fraction = i == 0 ? fraction0 : 1 - fraction0;
            var bodyMass = totalMass * fraction;
            var bodyVolume = totalVolume * fraction;
            var bodyRadius = Constants.SphereVolumeToRadius(bodyVolume);
            var position = Falling.RandomPointInBall(Random, systemRadius);
            bodies[i] = new Body(this,
                mass: bodyMass,
                radius: bodyRadius,
                position: position
            );
        }
        Bodies = bodies;
    }
}
