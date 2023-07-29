using System.Windows;
using System.Windows.Controls;

namespace MechanicsUI;

public partial class SimulationGroupView : UserControl
{
    public SimulationGroupVM? ViewModel => DataContext as SimulationGroupVM;

    public SimulationGroupView()
    {
        InitializeComponent();
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        var vm = ViewModel;
        if (vm == null)
            return;

        vm.SelectedSimulationName = (string)((FrameworkElement)sender).DataContext;
        var simVm = vm.GetSimulationVM();

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
