namespace MechanicsCore.Scenarios;

public abstract class RandomSimulationInitializer : SimulationInitializer
{
    protected Random Random { get; }
    protected int? _requestedSeed;
    public int Seed { get; }

    public override IEnumerable<string> GetConfigLines()
    {
        foreach (var b in base.GetConfigLines())
            yield return b;

        yield return $"Seed: {Seed}";
    }

    protected RandomSimulationInitializer(int? requestedSeed)
    {
        _requestedSeed = requestedSeed;
        Seed = requestedSeed ?? new Random().Next();
        Random = new Random(Seed);
    }

    protected const string RequestedSeedGuiTitle = "Fixed Seed";
    protected const string RequestedSeedGuiHelp = "Leave empty to make it random. Enter a number to repeat a previous run.";
}
