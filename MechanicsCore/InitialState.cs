namespace MechanicsCore;

public readonly record struct InitialState(
    IReadOnlyList<Body> Bodies
)
{
}
