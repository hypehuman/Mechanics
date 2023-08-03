using MathNet.Spatial.Euclidean;
using MechanicsCore;
using MechanicsCore.Rust.mechanics_fast;
using MechanicsCore.StepConfiguring;
using System.Diagnostics;

namespace MechanicsConsole;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Running performance tests:");

        var sim = PreconfiguredSimulations.Falling_Buoyant_Drag_Huge_0;
        sim.StepConfig.GravityConfig = GravityType.Newton_Pointlike;
        sim.StepConfig.CollisionConfig = CollisionType.None;
        if (!sim.TakeSimpleShortcut) { throw new Exception("Config should have made it simple."); }
        var n = sim.Bodies.Count;
        var m = new double[n];
        var p = new Vector3D[n];
        for (var i = 0; i < n; i++)
        {
            m[i] = sim.Bodies[i].Mass;
            p[i] = sim.Bodies[i].Position;
        };
        Vector3D[] a()
        {
            var a = new Vector3D[n];
            for (var i = 0; i < n; i++)
            {
                a[i] = mechanics_fast.ComputeAcceleration(m, p, i);
            };
            return a;
        }
        Vector3D[] b()
        {
            var n = sim.Bodies.Count;
            var m = new double[n];
            var p = new Vector3D[n];
            for (var i = 0; i < n; i++)
            {
                m[i] = sim.Bodies[i].Mass;
                p[i] = sim.Bodies[i].Position;
            };
            var a = new Vector3D[n];
            for (var i = 0; i < n; i++)
            {
                a[i] = mechanics_fast.ComputeAcceleration(m, p, i);
            };
            return a;
        }
        Vector3D[] c()
        {
            var a = new Vector3D[n];
            for (var i = 0; i < n; i++)
            {
                a[i] = sim.Bodies[i].ComputeAcceleration(sim.Bodies);
            };
            return a;
        };
        HeadToHead(new[] { a, b, c }, 8, 32);

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
        T? x = default;
        var sw = new Stopwatch();
        foreach (var funcID in funcIDsByGroup)
        {
            var func = funcs[funcID];
            sw.Restart();
            for (int j = 0; j < numRunsPerGroup; j++)
            {
                var val = func();
                // bogus computation to ensure that the func() call doesn't get optimized away
                x = object.Equals(x, default(T)) ? val : x;
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
