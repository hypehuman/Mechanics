using System;
using System.ComponentModel;

namespace MechanicsUI;

/// <summary>
/// View Model for a slider whose linear movements result in exponential changes to a value.
/// As the slider position ranges linearly from <see cref="MinSliderPosition"/> (0) to <see cref="MaxSliderPosition"/> (1),
/// the value ranges exponentially from <see cref="MinValue"/> to <see cref="MaxValue"/>.
/// </summary>
public class ExponentialSliderVM : INotifyPropertyChanged
{
    public static double MinSliderPosition => 0;
    public static double MaxSliderPosition => 1;

    public double MinValue { get; }
    public double MaxValue { get; }
    private readonly Func<double> _getValue;
    private readonly Action<double> _setValue;
    private readonly double _exponentCoefficient;

    public ExponentialSliderVM(double minValue, double maxValue, Func<double> getValue, Action<double> setValue)
    {
        MinValue = minValue;
        MaxValue = maxValue;
        _getValue = getValue;
        _setValue = setValue;
        _exponentCoefficient = Math.Log(MaxValue / MinValue);
    }

    public double SliderPosition
    {
        get => GetSliderPosition(_getValue());
        set => _setValue(GetValue(value));
    }

    public double GetValue(double sliderPosition)
    {
        var value = MinValue * Math.Exp(_exponentCoefficient * sliderPosition);
        return value;
    }

    public double GetSliderPosition(double value)
    {
        var sliderPosition = Math.Log(value / MinValue) / _exponentCoefficient;
        return sliderPosition;
    }

    /// <summary>
    /// We don't yet support ongoing updates of the slider position from the value,
    /// so we don't yet ever need to raise this event.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged { add { } remove { } }
}
