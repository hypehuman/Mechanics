using MechanicsCore.PhysicsConfiguring;

namespace MechanicsCore;

public record Scenario(
    Arrangement InitialArrangement,
    PhysicsConfiguration PhysicsConfig,
    int SuggestedStepsPerLeap = 1
)
{
}
