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
        return new object?[] { StepTime, GravityConfig, BuoyantGravityRatio, CollisionConfig, DragCoefficient };
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
        double dragCoefficient
    )
    {
        StepTime = stepTime;
        GravityConfig = gravity;
        BuoyantGravityRatio = buoyantGravityRatio;
        CollisionConfig = collisionConfig;
        DragCoefficient = dragCoefficient;
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

    public IEnumerable<string> GetConfigLines()
    {
        yield return $"Step time: {Simulation.DoubleToString(StepTime)}";

        yield return $"Gravity: {GravityConfig}";
        if (GravityConfig == GravityType.Newton_Buoyant)
            yield return $"Buoyant gravity ratio: {Simulation.DoubleToString(BuoyantGravityRatio)}";

        yield return $"Collision handling: {CollisionConfig}";
        if (CollisionConfig == CollisionType.Drag)
            yield return $"Drag coefficient: {Simulation.DoubleToString(DragCoefficient)}";
    }

    public bool CanTakeSimpleShortcut() => false;
        //GravityConfig == GravityType.Newton_Pointlike &&
        //CollisionConfig != CollisionType.Drag;
}
