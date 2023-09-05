using HorizonsToMechanics;

namespace HorizonsToMechanicsConsole;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        var path = "../../../../HorizonsToMechanics/Downloaded/";
        foreach (var x in BodyDataIterator.IterateObjects(path))
        {
            Console.WriteLine(x);
        }
    }
}
