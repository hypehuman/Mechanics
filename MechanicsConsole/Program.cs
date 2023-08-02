using MathNet.Spatial.Euclidean;
using MechanicsCore;
using MechanicsCore.Rust.mechanics_fast;
using System.Diagnostics;

namespace MechanicsConsole;

internal class Program
{
    static void Main(string[] args)
    {
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

        SeeHowLongItTakes(PreconfiguredSimulations.MoonFromRing_Pointlike_Combine_Insane_102691847, 5);
        SeeHowFarItGoes(PreconfiguredSimulations.SunEarthMoon_Pointlike, 10000);
        SeeHowFarItGoes(PreconfiguredSimulations.TwoBodies_Buoyant_Drag_0, 10000);
        SeeHowFarItGoes(PreconfiguredSimulations.Falling_Buoyant_Drag_Huge_0, 10000);

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

    private static void SeeHowLongItTakes(Simulation sim, int numLeaps)
    {
        var sw = Stopwatch.StartNew();
        for (int leapI = 0; leapI < 5; leapI++)
        {
            sim.Leap();
            Console.WriteLine("Leap " + leapI);
            foreach (var line in sim.GetStateSummaryLines())
            {
                Console.WriteLine(line);
            }
            Console.WriteLine();
        }
        Console.WriteLine($"{numLeaps} leaps in {sw.ElapsedMilliseconds} ms");
    }

    private static void SeeHowFarItGoes(Simulation sim, int ms)
    {
        var numLeaps = 0;
        var sw = Stopwatch.StartNew();
        while (sw.ElapsedMilliseconds < ms)
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
