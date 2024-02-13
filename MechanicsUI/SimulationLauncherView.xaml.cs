using AdonisUI.Controls;
using GuiByReflection.ViewModels;
using MechanicsCore;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace MechanicsUI;

partial class SimulationLauncherView
{
    private bool _isLoaded;
    public SimulationLauncherVM? ViewModel => DataContext as SimulationLauncherVM;

    public SimulationLauncherView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void LoadScenarioConfigButton_Click(object sender, RoutedEventArgs e)
    {
        var vm = ViewModel;
        if (vm == null)
            return;

        if (sender is not FrameworkElement senderFE || senderFE.DataContext is not IPropertyVM scenarioVM)
            return;

        vm.LoadScenarioConfig(scenarioVM);
    }

    private void LaunchButton_Click(object sender, RoutedEventArgs e)
    {
        LaunchButton_Click();
    }

    private SimulationVM? LaunchButton_Click()
    {
        var vm = ViewModel;
        if (vm == null)
            return null;

        var simVm = vm.LaunchSimulation();
        if (simVm == null)
            return null;

        ShowSimWindow(simVm);
        return simVm;
    }

    private static void ShowSimWindow(SimulationVM simVm)
    {
        var simWindow = new AdonisWindow
        {
            Title = simVm.Title,
            Content = new SimulationView
            {
                DataContext = simVm
            },
        };
        simWindow.Closed += SimWindow_Closed;
        simWindow.Show();
    }

    private static void SimWindow_Closed(object? sender, EventArgs e)
    {
        if (sender is not AdonisWindow simWindow)
            return;

        simWindow.Closed -= SimWindow_Closed;

        if ((simWindow.Content as FrameworkElement)?.DataContext is not SimulationVM simVm)
            return;

        // Stop the simulation from running in the background
        simVm.IsAutoLeaping = false;

        // The window has a memory leak. The leak appears to be through a DependencyObjectPropertyDescriptor
        // created in AdonisWindow.HandleTitleBarActualHeightChanged.
        // See https://github.com/benruehl/adonis-ui/issues/207
        // The following code at least allows the SimulationVM to be released.
        var simView = simWindow.Content as FrameworkElement;
        simWindow.Content = null; // Trying to free up SimulationView, but it's still leaked.
        if (simView != null)
            simView.DataContext = null; // Frees up SimulationVM.
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_isLoaded)
            return;

        _isLoaded = true;
        OnFirstLoaded();
    }

    private void OnFirstLoaded()
    {
        return;
        // Run performance test
        var launcherVM = ViewModel;
        var scenario = ScenarioGallery.Get_Collapsing_SolarSystem_Puffy(requestedSeed: 0);
        launcherVM.LoadScenarioConfig(scenario);
        var simVM = LaunchButton_Click();
        var sw = new Stopwatch();
        simVM.DoingAutoLeap += (sender, e) =>
        {
            if (simVM.Model.NumStepsPerformed >= 100000)
            {
                simVM.IsAutoLeaping = false;
                File.WriteAllText("performance test results.txt", sw.ElapsedMilliseconds.ToString());
                Dispatcher.BeginInvokeShutdown(DispatcherPriority.Send);
            }
        };
        sw.Start();
        simVM.IsAutoLeaping = true;
    }
}
