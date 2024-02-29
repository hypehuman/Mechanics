using System.ComponentModel;
using System.Reflection;

namespace GuiByReflection.ViewModels;

public class MethodVM : IMethodVM
{
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(PropertyChangedEventArgs e) => PropertyChanged?.Invoke(this, e);

    private readonly object? _object;
    private readonly MethodBase _methodInfo;
    private IExceptionButtonVM? _latestExceptionVM;

    public IReadOnlyList<IParameterVM> ParameterVMs { get; }

    /// <param name="obj">The object on which to call the method; null if the method is static.</param>
    public MethodVM(object? obj, MethodBase methodInfo, IParameterVMSelector? parameterVMSelector = null)
    {
        _object = obj;
        _methodInfo = methodInfo;
        var parameters = _methodInfo.GetParameters();
        var parameterInfoVMs = new IParameterVM[parameters.Length];
        for (var i = 0; i < parameters.Length; i++)
        {
            var parameter = parameters[i];
            parameterInfoVMs[i] =
                parameterVMSelector == null ?
                new ParameterVM(parameter) :
                parameterVMSelector.SelectParameterVM(_methodInfo, i, parameter);
        }
        ParameterVMs = parameterInfoVMs;
    }

    private static readonly PropertyChangedEventArgs sLatestExceptionVMChangedArgs = new(nameof(LatestExceptionVM));
    public IExceptionButtonVM? LatestExceptionVM
    {
        get => _latestExceptionVM;
        private set
        {
            _latestExceptionVM = value;
            OnPropertyChanged(sLatestExceptionVMChangedArgs);
        }
    }

    private Exception? LatestException
    {
        set => LatestExceptionVM = value == null ? null : new ExceptionButtonVM(value);
    }

    public bool TrySetParameterValues(IReadOnlyList<object?> newParameterValues, out Exception? exception)
    {
        var actualN = newParameterValues.Count;
        var expectedN = ParameterVMs.Count;
        if (actualN != expectedN)
        {
            LatestException = exception = new ArgumentException($"Expected {expectedN} parameters, but got {actualN}");
            return false;
        }

        for (var i = 0; i < actualN; i++)
        {
            var newParameterValue = newParameterValues[i];
            var paramVM = ParameterVMs[i];
            // TODO: Check whether the value can be assigned to the type, perhaps using ParameterVM.CanAssignTypeFromValue.
            // .NET internally uses System.RuntimeType.CheckValue to do that when calling the method.
            // For now, we just set the value and wait for TryInvokeMethod to fail.
            paramVM.SetActualValue(newParameterValue, true);
        }

        exception = null;
        return true;
    }

    public bool TrySetParameterValues(IReadOnlyList<object?> newParameterValues)
    {
        var result = TrySetParameterValues(newParameterValues, out var exception);
        LatestException = exception;
        return result;
    }

    public bool TryInvokeMethod(out object? returnValue, out Exception? exception)
    {
        try
        {
            returnValue = InvokeMethod();
            exception = null;
            return true;
        }
        catch (Exception ex)
        {
            returnValue = null;
            exception = ex;
            return false;
        }
    }

    public bool TryInvokeMethod(out object? returnValue)
    {
        var result = TryInvokeMethod(out returnValue, out var exception);
        LatestException = exception;
        return result;
    }

    private object? InvokeMethod()
    {
        var parameterValues = ParameterVMs.Select(p => p.ActualValue).ToArray();
        return InvokeMethod(parameterValues);
    }

    protected virtual object? InvokeMethod(object?[]? parameterValues)
    {
        return _methodInfo.Invoke(_object, parameterValues);
    }
}
