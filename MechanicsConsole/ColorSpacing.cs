using MechanicsCore;

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

    public static void ExportColorWheel(int numColors = 1024)
    {
        var numColorsNumChars = numColors.ToString().Length;
        var getColor = RingColorSpace.Cam16UcsRing.GetFunc();
        var distSqrFreq = new Dictionary<double, int>();
        BodyColor prev = default;
        var outBytes = new byte[numColors * 3];
        for (var i = 0; i < numColors + 1; i++)
        {
            var curr = getColor((double)i / numColors);
            if (i != numColors)
            {
                var label = (i + 1).ToString().PadLeft(numColorsNumChars);
                System.Diagnostics.Debug.WriteLine($"{label}: #{curr.R:X2}{curr.G:X2}{curr.B:X2}");
                outBytes[3 * i + 0] = curr.R;
                outBytes[3 * i + 1] = curr.G;
                outBytes[3 * i + 2] = curr.B;
            }
            if (i != 0)
            {
                var dr = curr.R - prev.R;
                var dg = curr.G - prev.G;
                var db = curr.B - prev.B;
                var distSqr = dr * dr + dg * dg + db * db;
                distSqrFreq.TryAdd(distSqr, 0);
                distSqrFreq[distSqr]++;
            }
            prev = curr;
        }
        var distSqrNumChars = distSqrFreq.Keys.Max().ToString().Length;
        var nNumChars = distSqrFreq.Values.Max().ToString().Length;
        foreach (var pair in distSqrFreq.OrderBy(x => x.Key))
        {
            var distSqrStr = pair.Key.ToString().PadLeft(distSqrNumChars);
            var nStr = pair.Value.ToString().PadLeft(nNumChars);
            Console.WriteLine($"d² {distSqrStr} n {nStr}");
        }
        var outPath = @"colors.bin";
        File.Delete(outPath);
        File.WriteAllBytes(outPath, outBytes);
        Console.WriteLine("Wrote to " + Path.GetFullPath(outPath));
    }
}
