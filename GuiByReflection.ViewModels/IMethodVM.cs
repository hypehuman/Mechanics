using System.ComponentModel;

namespace GuiByReflection.ViewModels;

public interface IMethodVM : INotifyPropertyChanged
{
    IReadOnlyList<IParameterVM> ParameterVMs { get; }
    IExceptionButtonVM? LatestExceptionVM { get; }

    bool TrySetParameterValues(IReadOnlyList<object?> newParameterValues, out Exception? exception);

    /// <summary>
    /// Any exception will be logged to <see cref="LatestExceptionVM"/>.
    /// </summary>
    bool TrySetParameterValues(IReadOnlyList<object?> newParameterValues);

    bool TryInvokeMethod(out object? returnValue, out Exception? exception);

    /// <summary>
    /// Any exception will be logged to <see cref="LatestExceptionVM"/>.
    /// </summary>
    bool TryInvokeMethod(out object? returnValue);
}
