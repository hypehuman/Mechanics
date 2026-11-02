using GuiByReflection.Models;

namespace MechanicsCore.PhysicsConfiguring;

/// <summary>
/// Configuration that determines how the simulation proceeds at each step
/// </summary>
public class PhysicsConfiguration : IGetConstructorParameters
{
    public PhysicsConfiguration() { }

    public object?[] GetConstructorParameters()
    {
        return new object?[] { StepTime, GravityConfig, BuoyantGravityRatio, CollisionConfig, DragCoefficient, MassVolumePower };
    }

    public PhysicsConfiguration(
        [GuiName("Step time")]
        [GuiHelp("The number of seconds per simulation step. Increasing this makes the simulation faster but less accurate.")]
        double stepTime,
        GravityType gravity,
        [GuiHelp(
            "Only relevant if gravity is Buoyant.",
            "Determines the maximum strength of the buoyant force.",
            "If set to 1, the maximum repulsion will have the same magnitude as the maximum attraction " +
            "(maximum attraction being when the bodies are just barely touching)."
        )]
        double buoyantGravityRatio,
        CollisionType collisionConfig,
        [GuiHelp("Only relevant if collisionConfig is Drag.")]
        double dragCoefficient,
        [GuiHelp(
            $"Volume = k * Mass^{nameof(MassVolumePower)}.",
            $"Only relevant if {nameof(collisionConfig)} is {nameof(CollisionType.Combine)}.",
            "Below 1: Heavier bodies are denser. When a light body combines with a heavy one, " +
            "it contributes less than its own volume, and a massless body contributes none.",
            "At 1 (the default): Density doesn't depend on mass, so volumes simply add, even for massless bodies.",
            "Above 1: Heavier bodies are less dense. When a light body combines with a heavy one, " +
            "it contributes more than its own volume, and massless bodies don't combine."
        )]
        double massVolumePower
    )
    {
        StepTime = stepTime;
        GravityConfig = gravity;
        BuoyantGravityRatio = buoyantGravityRatio;
        CollisionConfig = collisionConfig;
        DragCoefficient = dragCoefficient;
        MassVolumePower = massVolumePower;
    }

    public double StepTime { get; set; }

    public GravityType GravityConfig { get; set; }

    /// <summary>
    /// Only relevant if <see cref="GravityConfig"/> is <see cref="GravityType.Newton_Buoyant"/>
    /// Increasing this to 10 lets us line up more moons without them collapsing in on each other.
    /// </summary>
    public double BuoyantGravityRatio { get; set; } = 1;

    public CollisionType CollisionConfig { get; set; }

    /// <summary>
    /// Only relevant if <see cref="CollisionConfig"/> is <see cref="CollisionType.Drag"/>
    /// </summary>
    public double DragCoefficient { get; set; } = 1;

    /// <summary>
    /// Volume = k * Mass^<see cref="MassVolumePower"/>.
    /// Only relevant if <see cref="CollisionConfig"/> is <see cref="CollisionType.Combine"/>
    /// </summary>
    /// <remarks>
    /// When bodies combine, each one contributes Vᵢ*(mᵢ/m)^(1-p) to the combined volume,
    /// where mᵢ and Vᵢ are its own mass and volume, m is the combined mass, and p is <see cref="MassVolumePower"/>.
    /// So for a light body (mᵢ much smaller than m), there are three regimes:
    /// <list type="bullet">
    /// <item>
    /// p &lt; 1: Heavier bodies are denser.
    /// A light body contributes less than its own volume, and a massless body contributes none.
    /// (The heavy body's own contribution also shrinks a little, since it's now part of a heavier, denser body.)
    /// If p &lt; 0, heavier bodies are smaller, so bodies with the same k shrink when they combine.
    /// At p = 0, V = k, so the combined volume is just the mass-weighted average of the initial bodies' volumes.
    /// With masses like the Sun's (about 2e30 kg), the powers of mass leave the range of a double
    /// once p is below about -8, and the combined volume comes out as ∞ or NaN.
    /// </item>
    /// <item>
    /// p = 1 (the default): Density doesn't depend on mass, so the volumes simply add, even for massless bodies.
    /// </item>
    /// <item>
    /// p &gt; 1: Heavier bodies are less dense.
    /// A light body contributes more than its own volume, without limit as its mass approaches 0,
    /// so light bodies can make the combined volume enormous:
    /// at p = 2, the 1 kg pebble in the table below contributes 5.5 times the volume of the Earth-like body.
    /// A massless body would make the combined volume ∞, so we don't allow it to combine.
    /// With masses like the Sun's (about 2e30 kg), the powers of mass leave the range of a double
    /// once p is above about 11, and the combined volume comes out as ∞ or NaN.
    /// </item>
    /// </list>
    /// Here's how much a 1-liter pebble contributes in each regime, in m³,
    /// when it combines with an Earth-like body (5.97e24 kg, 1.08e21 m³):
    /// <code>
    /// pebble mass  p = -0.01  p = 0    p = 0.01  p = 0.9  p = 0.99  p = 1  p = 1.01  p = 1.1  p = 2
    /// 1 kg         9.5e-29    1.7e-28  3.0e-28   3.3e-6   5.7e-4    1e-3   1.8e-3    0.3      6e21 (5.5 Earths)
    /// 1e-30 kg     4.7e-59    1.7e-58  5.9e-58   3.3e-9   2.8e-4    1e-3   3.5e-3    300      6e51
    /// 0 kg         0          0        0         0        0         1e-3   doesn't combine (would contribute ∞)
    /// </code>
    /// For any body with mass, the contribution changes smoothly with p,
    /// but the lighter the body, the more steeply it changes around p = 1.
    /// A massless body is the extreme case: the contribution is the limit as mᵢ → 0 (with p and Vᵢ fixed),
    /// so it changes stepwise with p. (When p > 1, that limit is ∞, so we don't allow it to combine.)
    /// </remarks>
    public double MassVolumePower { get; set; } = 1;

    public IEnumerable<string> GetConfigLines()
    {
        yield return $"Step time: {Simulation.DoubleToString(StepTime)}";

        yield return $"Gravity: {GravityConfig}";
        if (GravityConfig == GravityType.Newton_Buoyant)
            yield return $"Buoyant gravity ratio: {Simulation.DoubleToString(BuoyantGravityRatio)}";

        yield return $"Collision handling: {CollisionConfig}";
        if (CollisionConfig == CollisionType.Drag)
            yield return $"Drag coefficient: {Simulation.DoubleToString(DragCoefficient)}";
        else if (CollisionConfig == CollisionType.Combine)
            yield return $"Mass-volume relation power: {Simulation.DoubleToString(MassVolumePower)}";
    }

    public bool CanTakeSimpleShortcut() =>
        GravityConfig == GravityType.Newton_Pointlike &&
        CollisionConfig != CollisionType.Drag;

    public bool CanLeapWithRust() =>
        CanTakeSimpleShortcut() &&
        CollisionConfig == CollisionType.None;
}
