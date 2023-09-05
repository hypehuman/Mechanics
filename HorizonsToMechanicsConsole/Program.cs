using HorizonsToMechanics;

namespace HorizonsToMechanicsConsole;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Body data from NASA Horizons:");
        var path = "../../../../HorizonsToMechanics/Downloaded/";
        foreach (var x in BodyDataIterator.IterateObjects(path))
        {
            Console.WriteLine(x);
        }
    }
}
