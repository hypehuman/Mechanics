using GuiByReflection.ViewModels;
using MechanicsCore;
using MechanicsCore.StepConfiguring;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace MechanicsUI;

public class SimulationPickerVM : INotifyPropertyChanged
{
    private static readonly IReadOnlyList<string> sPreconfigNames =
        typeof(PreconfiguredSimulations)
        .GetProperties()
        .Select(p => p.Name)
        .ToList();

    public event PropertyChangedEventHandler? PropertyChanged;

    public bool AutoStart { get; set; }

    public IReadOnlyList<string> PreconfigNames => sPreconfigNames;

    public IReadOnlyList<Scenario> Scenarios => Simulations.Scenarios;

    private Scenario _selectedScenario;
    public Scenario SelectedScenario
    {
        get => _selectedScenario;
        set
        {
            _selectedScenario = value;
            OnPropertyChanged();

            InitConfigConstructorVM = new ConstructorVM(ConstructorVM.GetLongestPublicConstructor(value.InitializerType));
        }
    }

    private IMethodVM _initConfigConstructorVM;
    public IMethodVM InitConfigConstructorVM
    {
        get => _initConfigConstructorVM;
        set
        {
            _initConfigConstructorVM = value;
            OnPropertyChanged();
        }
    }

    public IMethodVM StepConfigConstructorVM { get; } = new ConstructorVM(ConstructorVM.GetLongestPublicConstructor(typeof(StepConfiguration)));

    public SimulationPickerVM()
    {
        SelectedScenario = Scenarios[0];
    }

    public void SetPreconfiguredValues(string preconfigName)
    {
        var invoked = typeof(PreconfiguredSimulations).GetStaticPropertyValue(preconfigName)
            ?? throw new NullReferenceException($"'{preconfigName}' was null");
        var preconfig = (FullConfiguration)invoked;

        var initializerType = preconfig.InitConfig.GetType();
        SelectedScenario = Simulations.ScenariosByType[initializerType];
        InitConfigConstructorVM.TrySetParameterValues(preconfig.InitConfig.GetConstructorParameters());
        StepConfigConstructorVM.TrySetParameterValues(preconfig.StepConfig.GetConstructorParameters());
    }

    public SimulationVM? StartSimulation()
    {
        if (
            !InitConfigConstructorVM.TryInvokeMethod(out var initConfigObj)
            | // bitwise OR so that we can display both exceptions
            !StepConfigConstructorVM.TryInvokeMethod(out var stepConfigObj)
        )
        {
            return null;
        }
        var initConfig = (SimulationInitializer)initConfigObj;
        var stepConfig = (StepConfiguration)stepConfigObj;
        var sim = new Simulation(new(initConfig, stepConfig));
        return new SimulationVM(sim)
        {
            IsAutoLeaping = AutoStart,
        };
    }

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
