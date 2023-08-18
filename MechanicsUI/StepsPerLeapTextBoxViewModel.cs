namespace MechanicsUI;

public class StepsPerLeapTextBoxViewModel : ValidationTextBoxViewModel<int>
{
    public StepsPerLeapTextBoxViewModel()
        : base(TryParseStepsPerLeap, initialValue: 1)
    {
    }

    private static bool TryParseStepsPerLeap(string s, out int parsed, out string message)
    {
        if (!int.TryParse(s, out parsed) || !(parsed > 0))
        {
            message = $"Value must be a positive {typeof(int)}.";
            return false;
        }

        message = string.Empty;
        return true;
    }
}
