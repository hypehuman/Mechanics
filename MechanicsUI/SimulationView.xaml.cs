using System.Windows;

namespace MechanicsUI;

partial class SimulationView
{
    public SimulationVM? SimulationVM => DataContext as SimulationVM;

    public SimulationView()
    {
        InitializeComponent();
    }

    private void LeapButton_Click(object sender, RoutedEventArgs e)
    {
        SimulationVM?.LeapAndRefresh();
    }
}
