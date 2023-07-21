using MechanicsCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MechanicsUI;

public class SimulationPickerVM
{
    private static readonly IReadOnlyList<string> sSimulationNames = typeof(Simulations).GetProperties().Select(p => p.Name).ToArray();

    public IReadOnlyList<string> SimulationNames => sSimulationNames;
    public string SelectedSimulationName { get; set; }

    public bool AutoStart { get; set; }

    public SimulationPickerVM()
    {
        SelectedSimulationName = SimulationNames[0];
    }

    public SimulationVM GetSimulationVM()
    {
        var sim = (Simulation)typeof(Simulations).GetProperty(SelectedSimulationName).GetValue(null)
            ?? throw new Exception("Reflected simulation was null");
        return new SimulationVM(sim, SelectedSimulationName)
        {
            IsAutoLeaping = AutoStart,
        };
    }
}
