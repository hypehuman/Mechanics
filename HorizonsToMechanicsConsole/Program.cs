using HorizonsToMechanics;

namespace HorizonsToMechanicsConsole;

internal class Program
{
    private static readonly CancellationTokenSource sCancelSource = new();

    static async Task Main(string[] args)
    {
        Console.WriteLine("Downloading Horizons data. Ctrl+C to end");

        Console.CancelKeyPress += (sender, eventArgs) =>
        {
            Console.WriteLine("Cancel event triggered");
            sCancelSource.Cancel();
            eventArgs.Cancel = true;
        };

        Console.WriteLine("Body data from NASA Horizons:");
        var jsonDir = "../../../../HorizonsToMechanics/Downloaded/json";
        var txtDir = "../../../../HorizonsToMechanics/Downloaded/txt";
        await foreach (var bodyData in new BodyDataEnumerable(jsonDir, txtDir).WithCancellation(sCancelSource.Token))
        {
            if (!sCancelSource.IsCancellationRequested)
            {
                Console.WriteLine($"Object {bodyData.ID}: Success!");
            }
        }
    }
}
