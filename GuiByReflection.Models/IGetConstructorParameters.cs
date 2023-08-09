namespace GuiByReflection.Models;

public interface IGetConstructorParameters
{
    /// <summary>
    /// Returns a list of the same parameters that were passed to this object's constructor
    /// </summary>
    object?[] GetConstructorParameters();
}
