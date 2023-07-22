using MechanicsCore;
using MechanicsCore.Rust.mechanics_fast;
using System.Diagnostics;

namespace MechanicsConsole;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Calling Rust method:");
        Console.WriteLine(mechanics_fast.add(3, 4));

        Console.WriteLine("Running performance tests:");
        TestPerformance(Simulations.TwoBodies_NoDrag_0);
        TestPerformance(Simulations.Falling_Huge_0);

        Console.WriteLine();
        Console.WriteLine("Press Enter to start the simulation:");
        Console.ReadLine();
        Run(Simulations.Default());
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
        sim.Dump();
        while (sim.t < Constants.SecondsPerYear)
        {
            sim.Leap();
            sim.Dump();
        }
    }
}
