using AdonisUI.Controls;
using GuiByReflection.ViewModels;
using System.Windows;

namespace MechanicsUI;

partial class SimulationLauncherView
{
    public SimulationLauncherVM? ViewModel => DataContext as SimulationLauncherVM;

    public SimulationLauncherView()
    {
        InitializeComponent();
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
        var vm = ViewModel;
        if (vm == null)
            return;

        var simVm = vm.LaunchSimulation();
        if (simVm == null)
            return;

        ShowSimWindow(simVm);
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
}
