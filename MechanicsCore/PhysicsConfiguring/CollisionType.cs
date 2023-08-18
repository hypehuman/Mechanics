using GuiByReflection.Models;

namespace MechanicsCore.PhysicsConfiguring;

public enum CollisionType
{
    None,
    [GuiHelp("When bodies come into contact, they form a single body with the combined mass and volume.")]
    Combine,
    [GuiHelp(
        "When bodies are in contact, they experience a force that reduces their relative velocity.",
        "We attempt to prevent this force from flinging objects apart from one another (due to the nonzero step time), " +
        "but something in the math is still off. If that happens, the simulation will stop."
    )]
    Drag,
}
