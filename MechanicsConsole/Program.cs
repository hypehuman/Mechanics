using MechanicsCore;
using System.Diagnostics;

namespace MechanicsConsole;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Running performance tests:");
        TestPerformance(PreconfiguredSimulations.SunEarthMoon_Pointlike);
        TestPerformance(PreconfiguredSimulations.TwoBodies_Buoyant_Drag_0);
        TestPerformance(PreconfiguredSimulations.Falling_Buoyant_Drag_Huge_0);

        Console.WriteLine();
        Console.WriteLine("Press Enter to start the simulation:");
        Console.ReadLine();
        Run(PreconfiguredSimulations.Default());
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
