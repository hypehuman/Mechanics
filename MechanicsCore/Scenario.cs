using MechanicsCore.PhysicsConfiguring;

namespace MechanicsCore;

public record Scenario(
    Arrangement InitialArrangement,
    PhysicsConfiguration PhysicsConfig,
    int SuggestedStepsPerLeap = 1
)
{
    public Scenario(
        Arrangement InitialArrangement,
        PhysicsConfiguration PhysicsConfig,
        TimeSpan ApproxSuggestedLeapDuration
    )
        : this(
              InitialArrangement,
              PhysicsConfig,
              ComputeNumSteps(PhysicsConfig.StepTime, ApproxSuggestedLeapDuration)
        )
    {
    }

    private static int ComputeNumSteps(double stepTime, TimeSpan approxLeapDuration)
    {
        var numSteps = approxLeapDuration.TotalSeconds / stepTime;
        return Convert.ToInt32(numSteps);
    }
}
