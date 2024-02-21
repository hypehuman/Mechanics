using MechanicsCore;
using MechanicsCore.Arrangements;
using MechanicsCore.PhysicsConfiguring;
using System.Diagnostics;

namespace MechanicsConsole;

internal class Program
{
    static void Main(string[] args)
    {
        //Dump the initial conditions and expected results for the Rust unit test.
        var sim = new Simulation(ScenarioGallery.SunEarthMoon);
        sim.DumpState();
        sim.Leap(5);
        sim.DumpState();

        Console.WriteLine("Running performance tests:");
        var sw = Stopwatch.StartNew();
        var horizonsScenario = ScenarioGallery.ModernSolarSystem; 
        SeeHowLongItTakes(horizonsScenario, 3, 200_000);
        Utils.WritePerformanceResults(sw);
        return;

        var ballHuge = new Scenario(
            new Ball(
                Constants.MoonOrbitEarthDistance,
                512,
                Constants.EarthMass + Constants.MoonMass,
                Constants.EarthVolume + Constants.MoonVolume,
                Constants.MoonOrbitEarthSpeed / Math.Sqrt(10)
            ),
            new PhysicsConfiguration
            {
                StepTime = 8,
                GravityConfig = GravityType.Newton_Pointlike,
                CollisionConfig = CollisionType.None,
            },
            SuggestedStepsPerLeap: 128
        );
        SeeHowLongItTakes(ScenarioGallery.MoonFromRing_Insane_102691847, 5);
        SeeHowFarItGoes(ScenarioGallery.SunEarthMoon, 10000);
        SeeHowFarItGoes(ScenarioGallery.TwoBodies_Buoyant_Drag_0, 10000);
        SeeHowFarItGoes(ballHuge, 10000);

        Console.WriteLine();
        Console.WriteLine("Press Enter to start the simulation:");
        Console.ReadLine();
        Run(ScenarioGallery.Default());
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

    private static void SeeHowLongItTakes(Scenario config, int numLeaps, int? stepsPerLeap = null)
    {
        stepsPerLeap ??= config.SuggestedStepsPerLeap;
        var sim = new Simulation(config);
        var sw = Stopwatch.StartNew();
        for (int leapI = 0; leapI < numLeaps; leapI++)
        {
            sim.Leap(stepsPerLeap.Value);
            Console.WriteLine("Leap " + leapI);
            foreach (var line in sim.GetStateSummaryLines())
            {
                Console.WriteLine(line);
            }
            Console.WriteLine();
        }
        Console.WriteLine($"{numLeaps} leaps in {sw.ElapsedMilliseconds} ms");
    }

    private static void SeeHowFarItGoes(Scenario config, int ms)
    {
        var sim = new Simulation(config);
        var numLeaps = 0;
        var sw = Stopwatch.StartNew();
        while (sw.ElapsedMilliseconds < ms)
        {
            sim.Leap(config.SuggestedStepsPerLeap);
            numLeaps++;
        }
        Console.WriteLine($"{numLeaps} leaps in {sw.ElapsedMilliseconds} ms");
    }

    private static void Run(Scenario config)
    {
        var sim = new Simulation(config);
        sim.DumpState();
        while (sim.ElapsedTime < Constants.SecondsPerYear)
        {
            sim.Leap(config.SuggestedStepsPerLeap);
            sim.DumpState();
        }
    }
}
