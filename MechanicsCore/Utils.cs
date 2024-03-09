using MathNet.Spatial.Euclidean;
using System.Diagnostics;
using System.Reflection;
using System.Xml;

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

    /// <summary>
    /// Returns false if any component is NaN, negative infinity, or positive infinity.
    /// Otherwise, returns true.
    /// </summary>
    public static bool IsFinite(this Vector3D v)
    {
        return
            double.IsFinite(v.X) &&
            double.IsFinite(v.Y) &&
            double.IsFinite(v.Z);
    }

    public static void WritePerformanceResults(Stopwatch sw)
    {
        var timestamp = XmlConvert.ToString(DateTime.Now, XmlDateTimeSerializationMode.RoundtripKind);
        WritePerformanceLine($"{timestamp}\t{sw.ElapsedMilliseconds}\tms");
    }

    public static void WritePerformanceLine(string line)
    {
        File.AppendAllLines("performance test results.txt", new[] { line });
    }
}
