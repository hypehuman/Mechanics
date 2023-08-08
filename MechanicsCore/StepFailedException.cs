namespace MechanicsCore;

internal class StepFailedException : Exception
{
    public StepFailedException(string message)
        : base(message)
    {
    }
}
