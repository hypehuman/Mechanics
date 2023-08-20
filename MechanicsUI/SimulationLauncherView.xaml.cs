using AdonisUI.Controls;
using GuiByReflection.ViewModels;
using MechanicsCore;
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
        simWindow.Closed += delegate
        {
            // Stop the simulation from running in the background
            simVm.IsAutoLeaping = false;
        };
        simWindow.Show();
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
