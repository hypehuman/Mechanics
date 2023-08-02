using System.Globalization;
using System.Windows.Controls;

namespace MechanicsUI;

public class SeedTextValidationRule : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        return
            value is string str && SimulationPickerVM.TryParseSeed(str, out _)
            ? new ValidationResult(true, null)
            : new ValidationResult(false, null);
    }
}
