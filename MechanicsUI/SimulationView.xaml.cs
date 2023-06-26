using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace MechanicsUI;

public partial class SimulationView : UserControl
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

    private void AutoLeapButton_Click(object sender, RoutedEventArgs e)
    {
        SimulationVM?.SetAutoLeap(Dispatcher, ((ToggleButton)sender).IsChecked == true);
    }
}
