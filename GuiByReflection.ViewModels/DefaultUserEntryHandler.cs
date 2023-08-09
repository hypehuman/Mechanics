using System.Reflection;

namespace GuiByReflection.ViewModels;

internal class DefaultUserEntryHandler : IUserEntryHandler
{
    public static IUserEntryHandler Instance { get; } = new DefaultUserEntryHandler();

    public object? UserEntryToValue(object? userEntry, Type type, object? currentActualValue, out string? message)
    {
        // Handle values for which the user-entered Binding value needs no conversion, e.g.:
        //  - ActualValue is a string, and we're binding to a TextBox
        //  - ActualValue is a bool, and we're binding to a CheckBox
        //  - ActualValue is an enum, and we're binding to a ComboBox whose ItemsSource is Enum.GetValues
        if (ParameterVM.CanAssignTypeFromValue(type, userEntry))
        {
            message = string.Empty;
            return userEntry;
        }

        // Nullables are user-friendlier as a checkbox plus another friendly control for the value, but we support textboxes as well.
        var underlying = Nullable.GetUnderlyingType(type);
        if (underlying != null)
        {
            var underlyingValue = UserEntryToValue(userEntry, underlying, currentActualValue, out var underlyingMessage);
            if (string.IsNullOrWhiteSpace(underlyingMessage))
            {
                message = string.Empty;
                return underlyingValue;
            }

            // Having failed to parse as the underlying type, assume that a null/empty/whitespace string means null.
            if (userEntry == null || userEntry is string userString2 && string.IsNullOrWhiteSpace(userString2))
            {
                message = string.Empty;
                return null;
            }

            // Having failed to parse as null, return the message and result that we got when we attempted to parse a value.
            message = underlyingMessage;
            return underlyingValue;
        }

        if (userEntry is string userString)
        {
            // These two are IParsable and should therefore be handled by TryTryParseByReflection.
            // However, they are very common, so here are some optimized type checks.
            if (type == typeof(int))
                return TryParse<int>(userString, out message);
            if (type == typeof(double))
                return TryParse<double>(userString, out message);

            // Booleans are user-friendlier as checkboxes, but we support textboxes as well.
            // In .NET 8, System.Boolean will implement IParsable.
            if (type == typeof(bool))
            {
                if (bool.TryParse(userString, out var parsedBool))
                {
                    message = null;
                    return parsedBool;
                }

                message = TryParseReturnedFalseMessage(typeof(bool));
                return ParameterVM.GetDefaultValue(type);
            }

            // Enums are user-friendlier as dropdowns, but we support textboxes as well.
            if (type.IsEnum)
            {
                if (Enum.TryParse(type, userString, out var parsedEnum))
                {
                    message = string.Empty;
                    return parsedEnum;
                }

                message = TryParseReturnedFalseMessage(typeof(Enum));
                return ParameterVM.GetDefaultValue(type);
            }

            // Complicated reflection that should handle any IParsable but is probably very slow
            if (TryTryParseByReflection(type, userString, out var parsedParsable, out var possibleMessage))
            {
                message = possibleMessage;
                return parsedParsable;
            }
        }

        // give up
        var dumpUserEntry = userEntry == null ? "null" : $"{userEntry.GetType()} '{userEntry}'";
        message = $"Not sure how to convert {dumpUserEntry} to {type}";
        return ParameterVM.GetDefaultValue(type);
    }

    public static TSelf? TryParse<TSelf>(string userString, out string message)
        where TSelf : IParsable<TSelf>
    {
        if (TSelf.TryParse(userString, null, out var parsed))
        {
            message = string.Empty;
            return parsed;
        }

        message = TryParseReturnedFalseMessage(typeof(TSelf));
        return default;
    }

    public static string TryParseReturnedFalseMessage(Type type)
    {
        return $"{type}.TryParse returned false";
    }

    /// <summary>
    /// Returns false if the <see cref="IParsable{TSelf}.TryParse"/> method could not be found or if it threw an exception.
    /// </summary>
    public static bool TryTryParseByReflection(Type type, string userString, out object? parsed, out string message)
    {
        /* This doesn't seem to find the method:
        var tryParse = type.GetMethod(
            "TryParse",
            0,
            BindingFlags.Public | BindingFlags.Static,
            null,
            new[] { typeof(string), type },
            new[] { new ParameterModifier(2) { [0] = false, [1] = true } }
        );
        */

        /* This finds to many methods for, e.g., System.Double:
        var tryParses = type.GetMethods().Where(m => m.Name == "TryParse").ToList();
        */

        if (!TryGetTryParseMethod(type, out var tryParseMethod))
        {
            // TryParse method not found
            parsed = default;
            message = string.Empty;
            return false;
        }

        try
        {
            var parameters = new object?[] { userString, null, null };
            var resultObj = tryParseMethod.Invoke(null, parameters);
            parsed = parameters[2];
            if ((bool)resultObj)
            {
                // TryParse returned true
                message = string.Empty;
                return true;
            }

            // TryParse returned false
            message = TryParseReturnedFalseMessage(type);
            return true; // to indicate that we should display the message above and not try other means of conversion
        }
        catch
        {
            // TryParse method could not be called (probably because we messed up the reflection),
            // or the method itself threw an exception (unlikely)
            parsed = default;
            message = string.Empty;
            return false;
        }
    }

    public static bool TryGetTryParseMethod(Type type, out MethodInfo? tryParseMethod)
    {
        tryParseMethod = default;
        if (!type.IsAssignableTo(typeof(IParsable<>)))
            return false;

        Type tryParseInterface;
        try
        {
            tryParseInterface = typeof(IParsable<>).MakeGenericType(new[] { type });
        }
        catch
        {
            return false;
        }

        if (!type.IsAssignableTo(tryParseInterface))
            return false;

        var map = type.GetInterfaceMap(tryParseInterface);
        var tryParseInterfaceMethod = map.InterfaceMethods.SingleOrDefault(m =>
            m.Name == "TryParse" &&
            m.IsPublic &&
            m.IsStatic &&
            !m.ContainsGenericParameters
        /* && TODO: Figure out right properties to query for these checks:
        m.GetParameters() types are new[] { typeof(string), typeof(IFormatProvider), type }) &&
        m.GetParameters() byRef modifiers are new[] { new ParameterModifier(3) { [0] = false, [1] = false, [2] = true } */
        );
        if (tryParseInterfaceMethod == null)
            return false;

        var index = Array.IndexOf(map.InterfaceMethods, tryParseInterfaceMethod);
        if (index == -1 || index >= map.TargetMethods.Length)
            return false;

        tryParseMethod = map.TargetMethods[index];
        return tryParseMethod != null;
    }
}
