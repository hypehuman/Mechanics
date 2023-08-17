using MechanicsCore.Arrangements;
using System.Reflection;

namespace MechanicsCore;

public record GalleryItem(Type ArrangementType, string Title, string Description);

public static class Simulations
{
    private static readonly List<GalleryItem> sScenarios = new();
    public static IReadOnlyList<GalleryItem> Scenarios => sScenarios;

    private static readonly Dictionary<Type, GalleryItem> sScenariosByType = new();
    public static IReadOnlyDictionary<Type, GalleryItem> ScenariosByType => sScenariosByType;

    static Simulations()
    {
        // Add some scenarios in a defined order with names and descriptions
        AddScenario(new(typeof(TwoBodies), "Two Bodies", ""));
        AddScenario(new(typeof(Line), "Line", ""));
        AddScenario(new(typeof(SunEarthMoon), "Sun, Earth, and Moon", ""));
        AddScenario(new(typeof(Ball), "Falling", ""));
        AddScenario(new(typeof(MoonFromRing), "Moon from Ring", "Start with the moon broken into fragments orbiting the Earth."));

        // Add the remaining scenarios in an arbitrary order with default names and no descriptions
        var allArrangementTypes = Assembly.GetCallingAssembly().GetTypes()
            .Where(t => !t.IsAbstract && typeof(Arrangement).IsAssignableFrom(t));
        foreach (var arrangmentType in allArrangementTypes)
        {
            if (!sScenariosByType.ContainsKey(arrangmentType))
            {
                AddScenario(new GalleryItem(arrangmentType, arrangmentType.Name, ""));
            }
        }
    }

    private static void AddScenario(GalleryItem scenario)
    {
        sScenarios.Add(scenario);
        sScenariosByType.Add(scenario.ArrangementType, scenario);
    }
}
