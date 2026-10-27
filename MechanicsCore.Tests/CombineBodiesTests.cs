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
    public void IdenticalBodies_FinalVolumeDoesNotDependOnOrder()
    {
        const double mass = 3e24;
        const double volume = 1.1e21;
        static Body[] CreateBodies() => Enumerable.Range(0, 4).Select(id => CreateBody(id, mass, volume)).ToArray();

        var expected = volume * 4;
        AssertVolume(expected, CombineOneAtATime(CreateBodies()), "one at a time");
        AssertVolume(expected, CombineInPairs(CreateBodies()), "in pairs");
        AssertVolume(expected, CombineAllAtOnce(CreateBodies()), "all at once");
    }

    [TestMethod]
    public void DifferentBodies_FinalVolumeDoesNotDependOnOrder()
    {
        static Body[] CreateBodies() =>
        [
            CreateBody(0, 1e24, 2e20),
            CreateBody(1, 4e24, 1e21),
            CreateBody(2, 2e24, 3e21),
            CreateBody(3, 3e24, 5e20),
        ];

        var expected = CreateBodies().Sum(b => b.Volume);
        AssertVolume(expected, CombineOneAtATime(CreateBodies()), "one at a time");
        AssertVolume(expected, CombineInPairs(CreateBodies()), "in pairs");
        AssertVolume(expected, CombineAllAtOnce(CreateBodies()), "all at once");
    }

    /// <summary>
    /// (A, B, C, D) → (AB, C, D) → (ABC, D) → (ABCD)
    /// </summary>
    private static Body CombineOneAtATime(Body[] bodies)
    {
        var ab = Combine(bodies[0], bodies[1]);
        var abc = Combine(ab, bodies[2]);
        return Combine(abc, bodies[3]);
    }

    /// <summary>
    /// (A, B, C, D) → (AB, CD) → (ABCD)
    /// </summary>
    private static Body CombineInPairs(Body[] bodies)
    {
        var ab = Combine(bodies[0], bodies[1]);
        var cd = Combine(bodies[2], bodies[3]);
        return Combine(ab, cd);
    }

    /// <summary>
    /// (A, B, C, D) → (ABCD)
    /// </summary>
    private static Body CombineAllAtOnce(Body[] bodies) =>
        Combine(bodies);

    /// <summary>
    /// Combines the group and returns the one body that still exists.
    /// </summary>
    private static Body Combine(params Body[] group)
    {
        Simulation.CombineBodies(group.ToList());
        return group.Single(b => b.Exists);
    }

    private static Body CreateBody(int id, double mass, double volume) =>
        new(id, mass: mass) { Volume = volume };

    private static void AssertVolume(double expected, Body actual, string order) =>
        Assert.AreEqual(expected, actual.Volume, expected * RelativeTolerance, $"Final volume when combining {order}.");
}
