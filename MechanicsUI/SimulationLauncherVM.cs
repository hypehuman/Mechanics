﻿using GuiByReflection.ViewModels;
using MechanicsCore;
using MechanicsCore.PhysicsConfiguring;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace MechanicsUI;

public class SimulationLauncherVM : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(PropertyChangedEventArgs e) => PropertyChanged?.Invoke(this, e);

    private static readonly IReadOnlyList<PropertyInfo> sScenarioGalleryProperties =
        typeof(ScenarioGallery)
        .GetProperties();

    private static readonly IReadOnlyList<Type> sArrangementTypes =
        Utils.GetInstantiableTypes(typeof(Arrangement)).ToList();

    public IValidationTextBoxViewModel<int> StepsPerLeapUponLaunchVM { get; } = new StepsPerLeapTextBoxViewModel();
    public bool IsAutoLeapingUponLaunch { get; set; }

    public IReadOnlyList<IPropertyVM> SavedScenarioVMs { get; }

    public IReadOnlyList<ITypeVM> ArrangerVMs { get; }

    private ITypeVM _selectedArranger;
    private IMethodVM _arrangementConstructorVM;

    private static readonly PropertyChangedEventArgs sSelectedArrangerChangedArgs = new(nameof(SelectedArranger));
    public ITypeVM SelectedArranger
    {
        get => _selectedArranger;
        set
        {
            if (_selectedArranger == value) return;
            _selectedArranger = value;
            OnPropertyChanged(sSelectedArrangerChangedArgs);

            ArrangementConstructorVM = new ConstructorVM(ConstructorVM.GetLongestPublicConstructor(value.Model));
        }
    }

    private static readonly PropertyChangedEventArgs sArrangementConstructorVMChangedArgs = new(nameof(ArrangementConstructorVM));
    public IMethodVM ArrangementConstructorVM
    {
        get => _arrangementConstructorVM;
        set
        {
            if (_arrangementConstructorVM == value) return;
            _arrangementConstructorVM = value;
            OnPropertyChanged(sArrangementConstructorVMChangedArgs);
        }
    }

    public IMethodVM PhysicsConfigConstructorVM { get; } = new ConstructorVM(ConstructorVM.GetLongestPublicConstructor(typeof(PhysicsConfiguration)));

    public SimulationLauncherVM()
    {
        SavedScenarioVMs = sScenarioGalleryProperties.Select(pi => new PropertyVM(null, pi)).ToList();
        ArrangerVMs = sArrangementTypes.Select(at => new TypeVM(at)).ToList();
        SelectedArranger = ArrangerVMs[0];
    }

    public void LoadScenarioConfig(IPropertyVM savedScenarioVM)
    {
        var invoked = savedScenarioVM.GetValue()
            ?? throw new NullReferenceException($"'{savedScenarioVM.ActualGuiName}' was null");
        var scenario = (Scenario)invoked;

        LoadScenarioConfig(scenario);
    }

    public void LoadScenarioConfig(Scenario scenario)
    {
        var arrangementType = scenario.InitialArrangement.GetType();
        SelectedArranger = ArrangerVMs.First(avm => avm.Model == arrangementType);
        ArrangementConstructorVM.TrySetParameterValues(scenario.InitialArrangement.GetConstructorParameters());
        PhysicsConfigConstructorVM.TrySetParameterValues(scenario.PhysicsConfig.GetConstructorParameters());

        StepsPerLeapUponLaunchVM.CurrentValue = scenario.SuggestedStepsPerLeap;
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
        var simVM = new SimulationVM(sim)
        {
            IsAutoLeaping = IsAutoLeapingUponLaunch
        };
        simVM.StepsPerLeapVM.CurrentValue = StepsPerLeapUponLaunchVM.CurrentValue;
        return simVM;
    }
}
