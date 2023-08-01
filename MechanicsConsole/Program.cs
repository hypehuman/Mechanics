using MathNet.Spatial.Euclidean;
using MechanicsCore;
using MechanicsCore.Rust.mechanics_fast;
using System.Diagnostics;

namespace MechanicsConsole;

internal class Program
{
    static void Main(string[] args)
    {
        ColorSpacing.FindBestColorSpacing();

        Console.WriteLine("Running performance tests:");

        Vector3D displacement = new(1, 1, 1);
        double m2 = 1;
        double distance = displacement.Length;
        Vector3D a() => mechanics_fast.compute_gravitational_acceleration(displacement, m2);
        Vector3D b()
        {
            var x = displacement.Length;
            return Constants.GravitationalConstant * m2 * displacement / (distance * distance * distance);
        }
        Vector3D c() => Constants.GravitationalConstant * m2 * displacement / (distance * distance * distance);
        HeadToHead(new[] { a, b, c }, 16, 100000000);

        TestPerformance(PreconfiguredSimulations.SunEarthMoon_Pointlike);
        TestPerformance(PreconfiguredSimulations.TwoBodies_Buoyant_Drag_0);
        TestPerformance(PreconfiguredSimulations.Falling_Buoyant_Drag_Huge_0);

        Console.WriteLine();
        Console.WriteLine("Press Enter to start the simulation:");
        Console.ReadLine();
        Run(PreconfiguredSimulations.Default());
    }

    private static void HeadToHead<T>(IReadOnlyList<Func<T>> funcs, int numGroupsPerFunc, int numRunsPerGroup)
    {
        var funcIDsByGroup = new List<int>();
        for (int i = 0; i < funcs.Count; i++)
        {
            funcIDsByGroup.AddRange(Enumerable.Repeat(i, numGroupsPerFunc));
        }
        Shuffle(funcIDsByGroup);

        var timeByFuncID = Enumerable.Range(0, funcs.Count).ToDictionary(i => i, i => (long)0);
        T x = default;
        var sw = new Stopwatch();
        foreach (var funcID in funcIDsByGroup)
        {
            var func = funcs[funcID];
            sw.Restart();
            for (int j = 0; j < numRunsPerGroup; j++)
            {
                var val = func();
                // bogus computation to ensure that the func() call doesn't get optimized away
                x = x.Equals(default(T)) ? val : x;
            }
            timeByFuncID[funcID] += sw.ElapsedTicks;
        }

        var avg = timeByFuncID.Values.Average();
        Console.WriteLine("Average: " + avg);
        foreach (var pair in timeByFuncID)
        {
            Console.WriteLine($"    {pair.Key}: {pair.Value / avg}");
        }
        // bogus computation to ensure that the func() call doesn't get optimized away
        Console.WriteLine(x.Equals(default(T)) ? "" : " ");
    }

    private static void Shuffle<T>(IList<T> list)
    {
        var rng = new Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    private static void TestPerformance(Simulation sim)
    {
        var numLeaps = 0;
        var sw = Stopwatch.StartNew();
        while (sw.ElapsedMilliseconds < 10000)
        {
            sim.Leap();
            numLeaps++;
        }
        Console.WriteLine($"{numLeaps} leaps in {sw.ElapsedMilliseconds} ms");
    }

    private static void Run(Simulation sim)
    {
        sim.DumpState();
        while (sim.t < Constants.SecondsPerYear)
        {
            sim.Leap();
            sim.DumpState();
        }
    }
}
