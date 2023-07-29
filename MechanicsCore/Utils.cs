namespace MechanicsCore;

public static class Utils
{
    public static ArgumentOutOfRangeException OutOfRange(string paramName, object? actualValue, string? startOfMessage = null)
    {
        // ArgumentOutOfRangeException.Message will append actualValue and paramName to the message.
        return new ArgumentOutOfRangeException(paramName, actualValue, startOfMessage);
    }
}
