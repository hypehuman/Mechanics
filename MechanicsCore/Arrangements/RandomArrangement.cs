namespace MechanicsCore.Arrangements;

public abstract class RandomArrangement : Arrangement
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

    protected RandomArrangement(int? requestedSeed)
    {
        _requestedSeed = requestedSeed;
        Seed = requestedSeed ?? new Random().Next();
        Random = new Random(Seed);
    }

    protected const string RequestedSeedGuiName = "Fixed Seed";
    protected const string RequestedSeedGuiHelp = "Leave empty to make it random. Enter a number to repeat a previous run.";
}
