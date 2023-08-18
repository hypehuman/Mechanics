using System.Diagnostics.CodeAnalysis;

namespace GuiByReflection.Models;

/// <summary>
/// Specifies a pseudonym under which to display this parameter, property, etc. in the GUI.
/// If not specified, the property or parameter name used in the code will appear.
/// </summary>
[AttributeUsage(AttributeTargets.All)]
public class GuiNameAttribute : Attribute
{
    public static readonly GuiNameAttribute Default = new();

    public string Value { get; }

    public GuiNameAttribute()
        : this(string.Empty)
    {
    }

    public GuiNameAttribute(string value)
    {
        Value = value;
    }

    public override bool Equals([NotNullWhen(true)] object? obj) =>
        obj is GuiNameAttribute other && other.Value == Value;

    public override int GetHashCode() => Value?.GetHashCode() ?? 0;

    public override bool IsDefaultAttribute() => Equals(Default);
}
