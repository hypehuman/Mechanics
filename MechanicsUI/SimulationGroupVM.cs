using MechanicsCore;
using MechanicsCore.StepConfiguring;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;

namespace MechanicsUI;

public abstract class SimulationGroupVM
{
    public abstract IReadOnlyList<string> SimulationNames { get; }
    public SimulationPickerVM PickerVM { get; }
    public string SelectedSimulationName { get; set; }

    public SimulationGroupVM(SimulationPickerVM pickerVM)
    {
        PickerVM = pickerVM;
        SelectedSimulationName = SimulationNames[0];
    }

    public SimulationVM GetSimulationVM()
    {
        var name = SelectedSimulationName;
        Simulation sim;
        try
        {
            sim = MakeSimulation(name) ?? throw new NullReferenceException("returned simulation was null");
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to make simulation '{name}': {ex.Message}", ex);
        }
        SetStepConfiguration(sim);
        return new SimulationVM(sim, name)
        {
            IsAutoLeaping = PickerVM.AutoStart,
        };
    }

    protected abstract Simulation? MakeSimulation(string name);

    protected virtual void SetStepConfiguration(Simulation sim) { }
}

public class PreconfiguredSimulationGroupVM : SimulationGroupVM
{
    private static readonly IReadOnlyList<string> sSimulationNames = typeof(PreconfiguredSimulations).GetProperties().Select(p => p.Name).ToArray();
    public override IReadOnlyList<string> SimulationNames => sSimulationNames;

    public PreconfiguredSimulationGroupVM(SimulationPickerVM pickerVM) : base(pickerVM) { }

    protected override Simulation? MakeSimulation(string name)
    {
        var getter = typeof(PreconfiguredSimulations).GetProperty(SelectedSimulationName);
        var sim = (Simulation)getter.GetValue(null);
        return sim;
    }
}

public class ConfigurableSimulationGroupVM : SimulationGroupVM
{
    private static readonly IReadOnlyList<string> sSimulationNames = typeof(Simulations).GetMethods().Where(m => m.ReturnType == typeof(Simulation)).Select(p => p.Name).ToArray();
    public override IReadOnlyList<string> SimulationNames => sSimulationNames;

    private static readonly IReadOnlyList<GravityType> sGravityTypes = Enum.GetValues<GravityType>();
    public static IReadOnlyList<GravityType> GravityTypes => sGravityTypes;
    public GravityType GravityConfig { get; set; }

    private static readonly IReadOnlyList<CollisionType> sCollisionTypes = Enum.GetValues<CollisionType>();
    public static IReadOnlyList<CollisionType> CollisionTypes => sCollisionTypes;
    public CollisionType CollisionConfig { get; set; }

    public string SeedString { get; set; } = string.Empty;
    private int? Seed => TryParseSeed(SeedString, out var seed) ? seed : null;

    public ConfigurableSimulationGroupVM(SimulationPickerVM pickerVM) : base(pickerVM) { }

    protected override Simulation? MakeSimulation(string name)
    {
        var getter = typeof(Simulations).GetMethod(SelectedSimulationName);
        var sim = (Simulation)getter.Invoke(null, new object[] { Seed });
        return sim;
    }

    protected override void SetStepConfiguration(Simulation sim)
    {
        base.SetStepConfiguration(sim);

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

public class SeedTextValidationRule : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        return
            value is string str && ConfigurableSimulationGroupVM.TryParseSeed(str, out _)
            ? new ValidationResult(true, null)
            : new ValidationResult(false, null);
    }
}
