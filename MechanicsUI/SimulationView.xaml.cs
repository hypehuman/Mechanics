using Microsoft.Win32;
using System;
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

    private void ExportBodyDataButton_Click(object sender, RoutedEventArgs e)
    {
        var vm = SimulationVM;
        if (vm == null)
        {
            return;
        }

        var dlg = new SaveFileDialog
        {
            DefaultExt = ".csv",
        };

        if (dlg.ShowDialog() != true)
        {
            return;
        }

        try
        {
            vm.ExportBodyData(dlg.FileName);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.ToString());
        }
    }
}
