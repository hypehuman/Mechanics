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

    private void ShrinkOrExpandBoundsToFitBodiesButton_Click(object sender, RoutedEventArgs e)
    {
        SimulationVM?.ShrinkOrExpandBoundsToFitBodies();
    }

    private void ExpandBoundsToFitBodiesButton_Click(object sender, RoutedEventArgs e)
    {
        SimulationVM?.ExpandBoundsToFitBodies();
    }

    private void ResetBoundsButton_Click(object sender, RoutedEventArgs e)
    {
        SimulationVM?.ResetBounds();
    }
}
