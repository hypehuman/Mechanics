using MathNet.Spatial.Euclidean;
using MechanicsCore.PhysicsConfiguring;

namespace MechanicsCore.Tests;

/// <summary>
/// When bodies combine, the final body's volume shouldn't depend on the order in which they combine.
/// For example, these should all give the same final volume:
/// <list type="bullet">
/// <item>One at a time: (A, B, C, D) → (AB, C, D) → (ABC, D) → (ABCD)</item>
/// <item>In pairs: (A, B, C, D) → (AB, CD) → (ABCD)</item>
/// <item>All at once: (A, B, C, D) → (ABCD)</item>
/// </list>
/// </summary>
[TestClass]
public class CombineBodiesTests
{
    /// <summary>
    /// In theory, the order doesn't matter at all.
    /// In practice, floating-point rounding can differ in the last couple of digits.
    /// </summary>
    private const double RelativeTolerance = 1e-12;

    [TestMethod]
    [DataRow(0.5)]
    [DataRow(1.0)]
    [DataRow(2.0)]
    public void IdenticalBodies_FinalVolumeDoesNotDependOnOrder(double massVolumePower)
    {
        var config = new PhysicsConfiguration
        {
            MassVolumePower = massVolumePower,
        };

        const double mass = 3e24;
        const double volume = 1.1e21;
        static Body[] CreateBodies() => Enumerable.Range(0, 4).Select(id => CreateBody(id, mass, volume)).ToArray();

        // Every body has the same k = V/m^p, so the final body does too: V = k*(4m)^p = V*4^p.
        var expected = volume * Math.Pow(4, config.MassVolumePower);
        AssertVolume(expected, CombineOneAtATime(CreateBodies(), config), "one at a time");
        AssertVolume(expected, CombineInPairs(CreateBodies(), config), "in pairs");
        AssertVolume(expected, CombineAllAtOnce(CreateBodies(), config), "all at once");
    }

    [TestMethod]
    [DataRow(0.5)]
    [DataRow(1.0)]
    [DataRow(2.0)]
    public void DifferentBodies_FinalVolumeDoesNotDependOnOrder(double massVolumePower)
    {
        var config = new PhysicsConfiguration
        {
            MassVolumePower = massVolumePower,
        };

        static Body[] CreateBodies() =>
        [
            CreateBody(0, 1e24, 2e20),
            CreateBody(1, 4e24, 1e21),
            CreateBody(2, 2e24, 3e21),
            CreateBody(3, 3e24, 5e20),
        ];

        var expected = CombineAllAtOnce(CreateBodies(), config).Volume;
        AssertVolume(expected, CombineOneAtATime(CreateBodies(), config), "one at a time");
        AssertVolume(expected, CombineInPairs(CreateBodies(), config), "in pairs");
    }

    [TestMethod]
    public void MasslessBodies_PowerOfOne_AddTheirVolume()
    {
        var config = new PhysicsConfiguration
        {
            MassVolumePower = 1,
        };

        // When p = 1, Q = V*m^0 = V, even when m = 0, so the volumes simply add.
        var expected = CreateBodiesWithSomeMassless().Sum(b => b.Volume);
        AssertVolume(expected, CombineOneAtATime(CreateBodiesWithSomeMassless(), config), "one at a time");
        AssertVolume(expected, CombineInPairs(CreateBodiesWithSomeMassless(), config), "in pairs");
        AssertVolume(expected, CombineAllAtOnce(CreateBodiesWithSomeMassless(), config), "all at once");
    }

    [TestMethod]
    [DataRow(0.0)]
    [DataRow(0.5)]
    public void MasslessBodies_PowerBelowOne_AddNoVolume(double massVolumePower)
    {
        var config = new PhysicsConfiguration
        {
            MassVolumePower = massVolumePower,
        };

        // When p < 1, Q = V*m^(1-p) = 0 when m = 0, so massless bodies don't change the final volume.
        // It's as if only the bodies with mass (B and D) had combined.
        var bodies = CreateBodiesWithSomeMassless();
        var expected = Combine(config, bodies[1], bodies[3]).Volume;
        AssertVolume(expected, CombineOneAtATime(CreateBodiesWithSomeMassless(), config), "one at a time");
        AssertVolume(expected, CombineInPairs(CreateBodiesWithSomeMassless(), config), "in pairs");
        AssertVolume(expected, CombineAllAtOnce(CreateBodiesWithSomeMassless(), config), "all at once");
    }

    [TestMethod]
    [DataRow(0.5)]
    [DataRow(1.0)]
    [DataRow(2.0)]
    public void AllMasslessBodies_DoNotCombine(double massVolumePower)
    {
        var config = new PhysicsConfiguration
        {
            MassVolumePower = massVolumePower,
        };

        // Position, velocity, and color are mass-weighted averages, which are undefined when the total mass is zero,
        // so the bodies are left as they are, whatever p is.
        var bodies = CreateMasslessBodies();
        Simulation.CombineBodies(bodies.ToList(), config);

        var expected = CreateMasslessBodies();
        for (var i = 0; i < bodies.Length; i++)
        {
            var message = $"Body {i}.";
            Assert.IsTrue(bodies[i].Exists, message);
            Assert.AreEqual(expected[i].Name, bodies[i].Name, message);
            Assert.AreEqual(expected[i].Mass, bodies[i].Mass, message);
            Assert.AreEqual(expected[i].Volume, bodies[i].Volume, message);
            Assert.AreEqual(expected[i].Position, bodies[i].Position, message);
            Assert.AreEqual(expected[i].Velocity, bodies[i].Velocity, message);
            Assert.AreEqual(expected[i].Color, bodies[i].Color, message);
        }
    }

    [TestMethod]
    [DataRow(1.5)]
    [DataRow(2.0)]
    public void MasslessBodies_PowerAboveOne_CannotCombine(double massVolumePower)
    {
        var config = new PhysicsConfiguration
        {
            MassVolumePower = massVolumePower,
        };

        // When p > 1, a massless body's Q = V*0^(1-p) would be ∞, so massless bodies can't combine.
        // CombineBodies refuses them, before changing any body.
        var bodies = CreateBodiesWithSomeMassless();
        Assert.ThrowsExactly<ArgumentException>(() => Simulation.CombineBodies(bodies.ToList(), config));
        Assert.IsTrue(bodies.All(b => b.Exists));
    }

    [TestMethod]
    [DataRow(0.5, 1)]
    [DataRow(1.0, 1)]
    [DataRow(1.5, 3)]
    [DataRow(2.0, 3)]
    public void MasslessBodyTouchingTwoOthers_CombinesOnlyWhenPowerIsAtMostOne(double massVolumePower, int expectedNumBodies)
    {
        // A and C have mass and don't touch each other. B is massless, and touches both of them.
        // When p <= 1, B links all three into one group, which combines into one body.
        // When p > 1, B can't combine, so it doesn't count as touching anything, and all three bodies are left as they are.
        var simulation = new Simulation(new Scenario(
            new FixedArrangement(
                CreateBody(0, 4e24, 1e21, new(0, 0, 0)),
                CreateBody(1, 0, 1e21, new(1e7, 0, 0)),
                CreateBody(2, 3e24, 1e21, new(2e7, 0, 0))
            ),
            new PhysicsConfiguration
            {
                StepTime = 1,
                CollisionConfig = CollisionType.Combine,
                MassVolumePower = massVolumePower,
            }
        ));
        simulation.Leap(1);
        Assert.AreEqual(expectedNumBodies, simulation.Bodies.Count);
    }

    /// <summary>
    /// A and C are massless. In every order, each of them combines with at least one body that has mass.
    /// (A group of only massless bodies doesn't combine at all; see <see cref="AllMasslessBodies_DoNotCombine"/>.)
    /// </summary>
    private static Body[] CreateBodiesWithSomeMassless() =>
    [
        CreateBody(0, 0, 2e20),
        CreateBody(1, 4e24, 1e21),
        CreateBody(2, 0, 3e21),
        CreateBody(3, 3e24, 5e20),
    ];

    /// <summary>
    /// Four massless bodies with different volumes, positions and velocities.
    /// Each one overlaps the next, so in a simulation they would form one group.
    /// </summary>
    private static Body[] CreateMasslessBodies() =>
    [
        CreateBody(0, 0, 1e20, new(0, 0, 0), new(0, 4e3, 0)),
        CreateBody(1, 0, 2e20, new(1e6, 0, 0), new(0, 3e3, 0)),
        CreateBody(2, 0, 3e20, new(2e6, 0, 0), new(0, 2e3, 0)),
        CreateBody(3, 0, 4e20, new(3e6, 0, 0), new(0, 1e3, 0)),
    ];

    /// <summary>
    /// (A, B, C, D) → (AB, C, D) → (ABC, D) → (ABCD)
    /// </summary>
    private static Body CombineOneAtATime(Body[] bodies, PhysicsConfiguration config)
    {
        var ab = Combine(config, bodies[0], bodies[1]);
        var abc = Combine(config, ab, bodies[2]);
        return Combine(config, abc, bodies[3]);
    }

    /// <summary>
    /// (A, B, C, D) → (AB, CD) → (ABCD)
    /// </summary>
    private static Body CombineInPairs(Body[] bodies, PhysicsConfiguration config)
    {
        var ab = Combine(config, bodies[0], bodies[1]);
        var cd = Combine(config, bodies[2], bodies[3]);
        return Combine(config, ab, cd);
    }

    /// <summary>
    /// (A, B, C, D) → (ABCD)
    /// </summary>
    private static Body CombineAllAtOnce(Body[] bodies, PhysicsConfiguration config) =>
        Combine(config, bodies);

    /// <summary>
    /// Combines the group and returns the one body that still exists.
    /// </summary>
    private static Body Combine(PhysicsConfiguration config, params Body[] group)
    {
        Simulation.CombineBodies(group.ToList(), config);
        return group.Single(b => b.Exists);
    }

    private static Body CreateBody(int id, double mass, double volume, Vector3D position = default, Vector3D velocity = default) =>
        new(id, mass: mass, position: position, velocity: velocity) { Volume = volume };

    private static void AssertVolume(double expected, Body actual, string order) =>
        Assert.AreEqual(expected, actual.Volume, expected * RelativeTolerance, $"Final volume when combining {order}.");

    /// <summary>
    /// An arrangement whose initial state is just the given bodies.
    /// </summary>
    private class FixedArrangement(params Body[] bodies) : Arrangement
    {
        public override object?[] GetConstructorParameters() => [];

        public override IReadOnlyList<Body> GenerateInitialState(out Vector3D displayBound0, out Vector3D displayBound1)
        {
            displayBound0 = default;
            displayBound1 = default;
            return bodies;
        }
    }
}
