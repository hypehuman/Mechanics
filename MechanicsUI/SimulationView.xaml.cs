using System.Windows;
using System.Windows.Controls;

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
}
