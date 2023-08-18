using System.Reflection;

namespace MechanicsCore;

public static class Utils
{
    public static ArgumentOutOfRangeException OutOfRange(string paramName, object? actualValue, string? startOfMessage = null)
    {
        // ArgumentOutOfRangeException.Message will append actualValue and paramName to the message.
        return new ArgumentOutOfRangeException(paramName, actualValue, startOfMessage);
    }

    public static IEnumerable<Type> GetInstantiableTypes(Type baseType)
    {
        return baseType.Assembly
            .GetTypes()
            .Where(baseType.IsAssignableFrom)
            .Where(t => !t.IsAbstract)
            ?? Type.EmptyTypes;
    }
}
