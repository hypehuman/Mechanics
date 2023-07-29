namespace MechanicsCore;

public abstract class RandomSimulation : Simulation
{
    protected Random Random { get; }
    public int Seed { get; }

    protected RandomSimulation(int? seed)
    {
        Seed = seed ?? new Random().Next();
        Random = new Random(Seed);
    }

    public override IEnumerable<string> GetConfigLines()
    {
        yield return $"Seed: {Seed}";

        foreach (var b in base.GetConfigLines())
            yield return b;
    }
}
