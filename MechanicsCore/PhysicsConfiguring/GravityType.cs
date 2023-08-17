using GuiByReflection.Models;

namespace MechanicsCore.PhysicsConfiguring;

public enum GravityType
{
    None,

    [GuiHelp(
        "Newton's law of gravitation for two pointlike bodies: Ag = G*m2/distance^2.",
        "Acceleration approaches infinity as the distance approaches zero."
    )]
    Newton_Pointlike,

    [GuiHelp(
        "When not overlapping, uses the formula for two pointlike bodies.",
        "When overlapping, uses the same as solved for a pointlike body that might be inside another spherical body of uniform density, " +
            "where the acceleration drops linearly with distance from the center.",
        "We will (incorrectly) apply this to two overlapping spherical bodies."
    )]
    Newton_LinearAfterTouching,

    [GuiHelp(
        "Something I made up to approximate a buoyant force that drives two overlapping bodies apart.",
        "Once touching, attraction drops linearly to a negative (repulsive) value with distance, " +
            "then increases to zero as the distance approaches zero."
    )]
    Newton_Buoyant,
}
