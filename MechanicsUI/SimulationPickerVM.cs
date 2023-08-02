using MechanicsCore;
using MechanicsCore.StepConfiguring;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MechanicsUI;

public class SimulationPickerVM
{
    public IReadOnlyList<string> PreconfigNames { get; } = typeof(PreconfiguredSimulations).GetProperties().Select(p => p.Name).ToArray();
    public IReadOnlyList<string> ScenarioNames { get; } = typeof(Simulations).GetMethods().Where(m => m.ReturnType == typeof(Simulation)).Select(p => p.Name).ToArray();

    public bool AutoStart { get; set; }

    #region Init Config

    public string SelectedScenarioName { get; set; }

    public string SeedString { get; set; } = string.Empty;
    /* make private */
    public int? GetSeed() => TryParseSeed(SeedString, out var seed) ? seed : null;

    #endregion

    #region Step Config

    private static readonly IReadOnlyList<GravityType> sGravityTypes = Enum.GetValues<GravityType>();
    public static IReadOnlyList<GravityType> GravityTypes => sGravityTypes;
    public GravityType GravityConfig { get; set; }

    private static readonly IReadOnlyList<CollisionType> sCollisionTypes = Enum.GetValues<CollisionType>();
    public static IReadOnlyList<CollisionType> CollisionTypes => sCollisionTypes;
    public CollisionType CollisionConfig { get; set; }

    #endregion

    public SimulationPickerVM()
    {
        SelectedScenarioName = ScenarioNames[0];
    }

    /// <summary>
    /// TODO: Replace with a method that sets the config values instead of starting a new simulation
    /// </summary>
    public SimulationVM StartPreconfiguredSimulation(string name)
    {
        var invoked = typeof(PreconfiguredSimulations).GetStaticPropertyValue(name)
            ?? throw new NullReferenceException($"'{name}' was null");
        var sim = (Simulation)invoked;
        return new SimulationVM(sim, name)
        {
            IsAutoLeaping = AutoStart,
        };
    }

    public SimulationVM StartSimulation()
    {
        string name = SelectedScenarioName;
        var invoked = typeof(Simulations).InvokeStatic(name, GetSeed())
            ?? throw new NullReferenceException($"'{name}' was null");
        var sim = (Simulation)invoked;
        SetStepConfiguration(sim);
        return new SimulationVM(sim, name)
        {
            IsAutoLeaping = AutoStart,
        };
    }

    private void SetStepConfiguration(Simulation sim)
    {
        // TODO: Configure step time
        // TODO: Configure steps per leap
        sim.StepConfig.GravityConfig = GravityConfig;
        // TODO: Configure buoyancy ratio if relevant
        sim.StepConfig.CollisionConfig = CollisionConfig;
        // TODO: Configure drag coefficient if relevant
    }

    public static bool TryParseSeed(string seedString, out int? seed)
    {
        if (string.IsNullOrWhiteSpace(seedString))
        {
            // Null is intentional; seed will be random.
            seed = null;
            return true;
        }

        if (int.TryParse(seedString, out var parsedSeed))
        {
            // Got an integer; seed will be fixed to the specified value.
            seed = parsedSeed;
            return true;
        }

        // Parsing failed.
        seed = null;
        return false;
    }
}
