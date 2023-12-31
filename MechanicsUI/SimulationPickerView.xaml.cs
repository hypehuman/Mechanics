﻿using System.Windows;
using System.Windows.Controls;

namespace MechanicsUI;
/// <summary>
/// Interaction logic for SimulationPickerView.xaml
/// </summary>
public partial class SimulationPickerView : UserControl
{
    public SimulationPickerVM? ViewModel => DataContext as SimulationPickerVM;

    public SimulationPickerView()
    {
        InitializeComponent();
    }

    private void LaunchButton_Click(object sender, RoutedEventArgs e)
    {
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
