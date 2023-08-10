namespace GuiByReflection.Models;

public interface IGetConstructorParameters
{
    /// <summary>
    /// Returns a list of the same parameters that were passed to this object's constructor
    /// </summary>
    object?[] GetConstructorParameters();
}

public static class GetConstructorParametersExtensions
{
    public static T Clone<T>(this T orig)
        where T : IGetConstructorParameters
    {
        var args = orig.GetConstructorParameters();
        var copy = Activator.CreateInstance(typeof(T), args);
        return (T)copy;
    }
}
