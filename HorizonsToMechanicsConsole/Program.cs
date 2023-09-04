using HorizonsToMechanics;

namespace HorizonsToMechanicsConsole;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        foreach (var x in Class1.IterateObjects())
        {
            Console.WriteLine(x);
        }
    }
}
