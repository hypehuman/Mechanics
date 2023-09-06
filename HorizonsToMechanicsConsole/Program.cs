using HorizonsToMechanics;

namespace HorizonsToMechanicsConsole;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Body data from NASA Horizons:");
        var jsonDir = "../../../../HorizonsToMechanics/Downloaded/json";
        var txtDir = "../../../../HorizonsToMechanics/Downloaded/txt";
        foreach (var x in BodyDataIterator.IterateObjects(jsonDir, txtDir))
        {
            //Console.WriteLine(x);
        }
    }
}
