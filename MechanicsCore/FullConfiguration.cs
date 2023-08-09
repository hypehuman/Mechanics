using MechanicsCore.StepConfiguring;

namespace MechanicsCore;

public record FullConfiguration(
    SimulationInitializer InitConfig,
    StepConfiguration StepConfig
)
{
}
