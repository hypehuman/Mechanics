namespace MechanicsUI;

public class SimulationPickerVM
{
    public PreconfiguredSimulationGroupVM PreconfiguredVM { get; }
    public ConfigurableSimulationGroupVM ConfigurableVM { get; }
    public bool AutoStart { get; set; }

    public SimulationPickerVM()
    {
        PreconfiguredVM = new PreconfiguredSimulationGroupVM(this);
        ConfigurableVM = new ConfigurableSimulationGroupVM(this);
    }
}
