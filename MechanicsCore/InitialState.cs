namespace MechanicsCore;

public readonly record struct InitialState(
    IReadOnlyList<Body> Bodies,
    IReadOnlyList<Link> Links
)
{
    public InitialState(IReadOnlyList<Body> bodies)
        : this(bodies, [])
    {
    }
}
