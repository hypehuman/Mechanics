using GuiByReflection.ViewModels;
using MechanicsCore;
using MechanicsCore.PhysicsConfiguring;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace MechanicsUI;

public class SimulationLauncherVM : INotifyPropertyChanged
{
    private static readonly IReadOnlyList<string> sPreconfigNames =
        typeof(ScenarioGallery)
        .GetProperties()
        .Select(p => p.Name)
        .ToList();

    public event PropertyChangedEventHandler? PropertyChanged;

    public bool IsAutoLeapingUponLaunch { get; set; }

    public IReadOnlyList<string> PreconfigNames => sPreconfigNames;

    public IReadOnlyList<GalleryItem> Scenarios => Simulations.Scenarios;

    private GalleryItem _selectedScenario;
    public GalleryItem SelectedScenario
    {
        get => _selectedScenario;
        set
        {
            _selectedScenario = value;
            OnPropertyChanged();

            ArrangementConstructorVM = new ConstructorVM(ConstructorVM.GetLongestPublicConstructor(value.ArrangementType));
        }
    }

    private IMethodVM _arrangementConstructorVM;
    public IMethodVM ArrangementConstructorVM
    {
        get => _arrangementConstructorVM;
        set
        {
            _arrangementConstructorVM = value;
            OnPropertyChanged();
        }
    }

    public IMethodVM PhysicsConfigConstructorVM { get; } = new ConstructorVM(ConstructorVM.GetLongestPublicConstructor(typeof(PhysicsConfiguration)));

    public SimulationLauncherVM()
    {
        SelectedScenario = Scenarios[0];
    }

    public void LoadScenarioConfig(string preconfigName)
    {
        var invoked = typeof(ScenarioGallery).GetStaticPropertyValue(preconfigName)
            ?? throw new NullReferenceException($"'{preconfigName}' was null");
        var scenario = (Scenario)invoked;

        var arrangementType = scenario.InitialArrangement.GetType();
        SelectedScenario = Simulations.ScenariosByType[arrangementType];
        ArrangementConstructorVM.TrySetParameterValues(scenario.InitialArrangement.GetConstructorParameters());
        PhysicsConfigConstructorVM.TrySetParameterValues(scenario.PhysicsConfig.GetConstructorParameters());
    }

    public SimulationVM? LaunchSimulation()
    {
        if (
            !ArrangementConstructorVM.TryInvokeMethod(out var arrangementObj)
            | // bitwise OR so that we can display both exceptions
            !PhysicsConfigConstructorVM.TryInvokeMethod(out var physicsObj)
        )
        {
            return null;
        }
        var arrangement = (Arrangement)arrangementObj;
        var physics = (PhysicsConfiguration)physicsObj;
        var sim = new Simulation(new(arrangement, physics));
        return new SimulationVM(sim)
        {
            IsAutoLeaping = IsAutoLeapingUponLaunch,
        };
    }

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
