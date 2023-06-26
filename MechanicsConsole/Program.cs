using MechanicsCore;

namespace MechanicsConsole;

internal class Program
{
    static void Main(string[] args)
    {
        var model = Simulations.Default();
        model.Dump();
        while (model.t < Constants.SecondsPerYear)
        {
            model.Leap();
            model.Dump();
        }
    }
}
