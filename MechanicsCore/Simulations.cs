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

    public static SimulationInitializer TwoBodies(int? requestedSeed = null) => new TwoBodies(
        Constants.EarthRadius * 2,
        Constants.EarthMass,
        Constants.EarthVolume,
        requestedSeed
    );

    public static SimulationInitializer Line3Moons(int? requestedSeed = null) => new Line(
        3,
        Constants.MoonMass,
        Constants.MoonRadius
    );

    public static SimulationInitializer Line4Moons(int? requestedSeed = null) => new Line(
        4,
        Constants.MoonMass,
        Constants.MoonRadius
    );

    public static SimulationInitializer SunEarthMoon(int? requestedSeed = null) => new SunEarthMoon();

    public static SimulationInitializer Falling_Tiny(int? requestedSeed = null) => Falling(4, requestedSeed);
    public static SimulationInitializer Falling_Small(int? requestedSeed = null) => Falling(16, requestedSeed);
    public static SimulationInitializer Falling_Large(int? requestedSeed = null) => Falling(128, requestedSeed);
    public static SimulationInitializer Falling_Huge(int? requestedSeed = null) => Falling(512, requestedSeed);

    private static SimulationInitializer Falling(int numBodies, int? requestedSeed = null) => new Falling(
        Constants.EarthMoonDistance,
        numBodies,
        Constants.EarthMass + Constants.MoonMass,
        Constants.EarthVolume + Constants.MoonVolume,
        Constants.MoonOrbitEarthSpeed / Math.Sqrt(10),
        requestedSeed
    );

    public static SimulationInitializer MoonFromRing_Sane(int? requestedSeed = null) => new MoonFromRing(64, requestedSeed);
    public static SimulationInitializer MoonFromRing_Insane(int? requestedSeed = null) => new MoonFromRing(1024, requestedSeed);
}
