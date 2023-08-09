using System.Reflection;

namespace MechanicsCore;

public static class ReflectionHelper
{
    public static PropertyInfo GetPropertyOrThrow(this Type type, string name)
    {
        return type.GetProperty(name) ?? throw new Exception($"Property '{name}' not found in type '{type}'");
    }

    public static EventInfo GetEventOrThrow(this Type type, string name)
    {
        return type.GetEvent(name) ?? throw new Exception($"Event '{name}' not found in type '{type}'");
    }

    public static MethodInfo GetMethodOrThrow(this Type type, string name)
    {
        return type.GetMethod(name) ?? throw new Exception($"Method '{name}' not found in type '{type}'");
    }

    public static MethodInfo GetMethodOrThrow(this Type type, string name, Type[] types)
    {
        types ??= Type.EmptyTypes;
        return type.GetMethod(name, types) ?? throw new Exception($"Method {name}({string.Join(",", types.Select(t => t.ToString()))}) not found in type '{type}'");
    }

    public static object? GetStaticPropertyValue(this Type type, string name)
    {
        return GetPropertyOrThrow(type, name).GetValue(null);
    }

    public static object? GetPropertyValue(this object instance, string name)
    {
        return GetPropertyOrThrow(instance.GetType(), name).GetValue(instance);
    }

    public static object? InvokeStatic(this Type type, string name)
    {
        var method = type.GetMethodOrThrow(name, Type.EmptyTypes);
        return method.Invoke(null, Array.Empty<object>());
    }

    public static object? InvokeStatic<TP1>(this Type type, string name, TP1 param1)
    {
        var method = type.GetMethodOrThrow(name, new[] { typeof(TP1) });
        return method.Invoke(null, new object?[] { param1 });
    }

    public static object? InvokeStatic<TP1, TP2>(this Type type, string name, TP1 param1, TP2 param2)
    {
        var method = type.GetMethodOrThrow(name, new[] { typeof(TP1), typeof(TP2) });
        return method.Invoke(null, new object?[] { param1, param2 });
    }

    public static object? Invoke(this object instance, string name)
    {
        var method = instance.GetType().GetMethodOrThrow(name, Type.EmptyTypes);
        return method.Invoke(instance, Array.Empty<object>());
    }

    public static object? Invoke<TP1>(this object instance, string name, TP1 param1)
    {
        var method = instance.GetType().GetMethodOrThrow(name, new[] { typeof(TP1) });
        return method.Invoke(instance, new object?[] { param1 });
    }

    public static object? Invoke<TP1, TP2>(this object instance, string name, TP1 param1, TP2 param2)
    {
        var method = instance.GetType().GetMethodOrThrow(name, new[] { typeof(TP1), typeof(TP2) });
        return method.Invoke(instance, new object?[] { param1, param2 });
    }
}