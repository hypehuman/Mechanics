using MechanicsCore.Scenarios;
using System.Reflection;

namespace MechanicsCore;

public record Scenario(Type InitializerType, string Title, string Description);

public static class Simulations
{
    private static readonly List<Scenario> sScenarios = new();
    public static IReadOnlyList<Scenario> Scenarios => sScenarios;

    private static readonly Dictionary<Type, Scenario> sScenariosByType = new();
    public static IReadOnlyDictionary<Type, Scenario> ScenariosByType => sScenariosByType;

    static Simulations()
    {
        // Add some scenarios in a defined order with names and descriptions
        AddScenario(new(typeof(TwoBodies), "Two Bodies", ""));
        AddScenario(new(typeof(Line), "Line", ""));
        AddScenario(new(typeof(SunEarthMoon), "Sun, Earth, and Moon", ""));
        AddScenario(new(typeof(Falling), "Falling", ""));
        AddScenario(new(typeof(MoonFromRing), "Moon from Ring", "Start with the moon broken into fragments orbiting the Earth."));

        // Add the remaining scenarios in an arbitrary order with default names and no descriptions
        var allInitializerTypes = Assembly.GetCallingAssembly().GetTypes()
            .Where(t => !t.IsAbstract && typeof(SimulationInitializer).IsAssignableFrom(t));
        foreach (var initializerType in allInitializerTypes)
        {
            if (!sScenariosByType.ContainsKey(initializerType))
            {
                AddScenario(new Scenario(initializerType, initializerType.Name, ""));
            }
        }
    }

    private static void AddScenario(Scenario scenario)
    {
        sScenarios.Add(scenario);
        sScenariosByType.Add(scenario.InitializerType, scenario);
    }
}
