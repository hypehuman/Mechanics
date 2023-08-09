using System.Diagnostics.CodeAnalysis;
namespace GuiByReflection.Models;

/// <summary>
/// Specifies a string that describes to the user this parameter / property / etc.'s meaning and usage.
/// By default, displayed as a tooltip in the GUI.
/// </summary>
[AttributeUsage(AttributeTargets.All)]
public class GuiHelpAttribute : Attribute
{
    public static readonly GuiHelpAttribute Default = new();

    public string Value { get; }

    public GuiHelpAttribute()
        : this(string.Empty)
    {
    }

    public GuiHelpAttribute(string value)
    {
        Value = value;
    }

    public override bool Equals([NotNullWhen(true)] object? obj) =>
        obj is GuiHelpAttribute other && other.Value == Value;

    public override int GetHashCode() => Value?.GetHashCode() ?? 0;

    public override bool IsDefaultAttribute() => Equals(Default);
}
