﻿using HorizonsToMechanics;
using System.Reflection;

namespace HorizonsToMechanicsConsole;

internal class Program
{
    private static readonly CancellationTokenSource sCancelSource = new();

    static async Task Main(string[] args)
    {
        Console.WriteLine("Writing C# file. Ctrl+C to end. Do NOT just close the console, or the file will not finish writing.");
        Console.WriteLine();

        Console.CancelKeyPress += (sender, eventArgs) =>
        {
            Console.WriteLine("Cancel event triggered");
            sCancelSource.Cancel();
            eventArgs.Cancel = true;
        };

        Console.WriteLine("Body data from NASA Horizons:");
        var jsonDir = "../../../../HorizonsToMechanics/Downloaded/json";
        var txtDir = "../../../../HorizonsToMechanics/Downloaded/txt";
        var csFile = "../../../../MechanicsCore/Arrangements/ModernSolarSystem.horizons.cs";
        using (var csWriter = File.CreateText(csFile))
        {
            csWriter.WriteLine("// Autogenerated by " + Assembly.GetCallingAssembly());
            csWriter.WriteLine();
            csWriter.WriteLine("namespace MechanicsCore.Arrangements;");
            csWriter.WriteLine();
            csWriter.WriteLine("partial class ModernSolarSystem");
            csWriter.WriteLine("{");
            csWriter.WriteLine("    private List<Body> CreateBodies() => new()");
            csWriter.WriteLine("    {");
            await foreach (var bodyData in new BodyDataEnumerable(jsonDir, txtDir).WithCancellation(sCancelSource.Token))
            {
                if (!sCancelSource.IsCancellationRequested)
                {
                    await WriteBody(csWriter, bodyData);
                }
            }

            Console.WriteLine("Closing file...");
            csWriter.WriteLine("    };");
            csWriter.WriteLine("}");
        }
    }

    /// <summary>
    /// Do not check for cancellation within this method.
    /// Once we start writing a body, we should finish it.
    /// </summary>
    static async Task WriteBody(StreamWriter csWriter, BodyData bd)
    {
        await csWriter.WriteLineAsync($"        new({Lit(bd.ID)},");
        await csWriter.WriteLineAsync($"            name: {Lit(bd.Name)},");
        await csWriter.WriteLineAsync($"            mass: {Lit(bd.Mass)},");
        await csWriter.WriteLineAsync($"            position: new({Lit(bd.PX)}, {Lit(bd.PY)}, {Lit(bd.PZ)}),");
        await csWriter.WriteLineAsync($"            velocity: new({Lit(bd.VX)}, {Lit(bd.VY)}, {Lit(bd.VZ)})");
        await csWriter.WriteLineAsync($"        ),");
    }

    private static string Lit(string? value)
    {
        return value == null ? "null" : $"\"{value}\"";
    }

    private static string Lit(int? value)
    {
        return value == null ? "null" : $"{value:R}";
    }

    private static string Lit(double? value)
    {
        return value == null ? "null" : $"{value:R}";
    }
}
