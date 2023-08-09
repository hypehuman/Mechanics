using System.Reflection;

namespace GuiByReflection.ViewModels;

public class ConstructorVM : MethodVM
{
    private readonly ConstructorInfo _constructorInfo;

    public ConstructorVM(ConstructorInfo constructorInfo, IParameterVMSelector? parameterVMSelector = null)
        : base(null, constructorInfo, parameterVMSelector)
    {
        _constructorInfo = constructorInfo;
    }

    protected override object? InvokeMethod(object?[]? parameterValues)
    {
        // For static methods, MethodInfo.Invoke(null, parameters) works just fine.
        // But for constructors, that throws "Non-static method requires a target".
        // So let's do it this way:
        return _constructorInfo.Invoke(parameterValues);
    }

    public static ConstructorInfo GetLongestPublicConstructor(Type type)
    {
        return type.GetConstructors().Where(c => c.IsPublic).MaxBy(c => c.GetParameters().Length)
            ?? throw new Exception($"No public constructors found for {type}");
    }
}
