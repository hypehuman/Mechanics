namespace MechanicsCore.StepConfiguring;

/// <summary>
/// Configuration that determines how the simulation proceeds at each step
/// </summary>
public class StepConfiguration
{
    public StepConfiguration() { }

    public double StepTime { get; set; }
    public int StepsPerLeap { get; set; }

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
        yield return $"Steps per leap: {StepsPerLeap}";

        yield return $"Gravity: {GravityConfig}";
        if (GravityConfig == GravityType.Newton_Buoyant)
            yield return $"Buoyant gravity ratio: {Simulation.DoubleToString(BuoyantGravityRatio)}";

        yield return $"Collision handling: {CollisionConfig}";
        if (CollisionConfig == CollisionType.Drag)
            yield return $"Drag coefficient: {Simulation.DoubleToString(DragCoefficient)}";
    }
}
