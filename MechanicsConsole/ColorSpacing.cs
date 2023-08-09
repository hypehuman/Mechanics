namespace MechanicsConsole;

internal static class ColorSpacing
{
    public static void FindBestColorSpacing()
    {
        var goodSpacings = new List<int>();
        for (int spacing = 1; spacing < 256; spacing++)
        {
            if (HitsAllValuesIn256(spacing))
            {
                goodSpacings.Add(spacing);
            }
        }
        var minDiffSum = goodSpacings.ToDictionary(s => s, s => 0);
        for (int maxN = 2; maxN <= 256; maxN++)
        {
            for (int n = 2; n <= maxN; n++)
            {
                foreach (var spacing in goodSpacings)
                {
                    var minDiff = GetDiffs(spacing, n).Min();
                    minDiffSum[spacing] += minDiff;
                }
            }
            var best = minDiffSum.GroupBy(pair => pair.Value).MaxBy(grouping => grouping.Key);
            Console.WriteLine($"n=2..{maxN} best min diffs sum={best.Key} by spacings = {string.Join(',', best.Select(pair => pair.Key))}");
        }
    }

    public static bool HitsAllValuesIn256(int spacing)
    {
        var hashset = new HashSet<int>();
        var val = 0;
        for (int i = 0; i < 256; i++)
        {
            val += spacing;
            val %= 256;
            if (!hashset.Add(val))
                return false;
        }
        return true;
    }

    public static IEnumerable<int> GetDiffs(int spacing, int n)
    {
        var values = new List<int>();
        var val = 0;
        for (int i = 0; i < n; i++)
        {
            val += spacing;
            val %= 256;
            values.Add(val);
        }
        for (int i = 0; i < values.Count; i++)
        {
            for (int j = i + 1; j < values.Count; j++)
            {
                var diff = Math.Abs(values[i] - values[j]);
                // look the other way around as well
                diff = Math.Min(diff, 256 - diff);
                yield return diff;
            }
        }
    }
}
