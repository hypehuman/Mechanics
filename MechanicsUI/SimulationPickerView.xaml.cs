using System.Windows;
using System.Windows.Controls;

namespace MechanicsUI;

public partial class SimulationPickerView : UserControl
{
    public SimulationPickerVM? ViewModel => DataContext as SimulationPickerVM;

    public SimulationPickerView()
    {
        InitializeComponent();
    }

    private void PreconfiguredButton_Click(object sender, RoutedEventArgs e)
    {
        var vm = ViewModel;
        if (vm == null)
            return;

        var preconfigName = (string)((FrameworkElement)sender).DataContext;

        // TODO: Instead of starting the simulation, just set the config values.
        var simVm = vm.StartPreconfiguredSimulation(preconfigName);
        ShowSimWindow(simVm);
    }

    private void LaunchButton_Click(object sender, RoutedEventArgs e)
    {
        var vm = ViewModel;
        if (vm == null)
            return;

        var simVm = vm.StartSimulation();
        ShowSimWindow(simVm);
    }

    private static void ShowSimWindow(SimulationVM simVm)
    {
        var simWindow = new Window
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
