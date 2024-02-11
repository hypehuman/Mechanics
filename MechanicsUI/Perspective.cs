using MathNet.Spatial.Euclidean;
using MechanicsCore;

namespace MechanicsUI;

/// <summary>
/// A WPF Panel uses left-handed coordinates where:
/// +X is right, -X is left.
/// +Y is down, -Y is up.
/// +Z (Panel.ZIndex) is the direction from the screen to your eyes, -Z is from your eyes to the screen.
/// </summary>
public enum Perspective
{
    /// <summary>
    /// "From Above" is a perspective from (0,0,+∞) with your eyes pointing -Z, your right ear pointing +X and the tip of your head pointing +Y.
    /// Simulation +X is Panel +X (right), Panel +X (right) is Simulation +X.
    /// Simulation -Y is Panel +Y (down), Panel +Y (down) is Simulation -Y.
    /// Simulation +Z is Panel +Z (screen to eyes), Panel +Z (screen to eyes) is Simulation +Z.
    /// </summary>
    Orthogonal_FromAbove,

    /// <summary>
    /// "From Front" is a perspective from (0,-∞,0) with your eyes pointing +Y, your right ear pointing +X and the tip of your head pointing +Z.
    /// Simulation +X is Panel +X (right), Panel +X (right is Simulation +X.
    /// Simulation +Y is panel -Z (eyes to screen), Panel +Y (down) is Simulation -Z.
    /// Simulation +Z is panel -Y (up), Panel +Z (screen to eyes) is Simulation -Y.
    /// </summary>
    Orthogonal_FromFront,

    /// <summary>
    /// "From Right" is a perspective from (+∞,0,0) with your eyes pointing -X, your right ear pointing +Y, and the tip of your head pointing +Z.
    /// Simulation +X is Panel +Z (screen to eyes), Panel +X (right) is Simulation +Y.
    /// Simulation +Y is Panel +X (right), Panel +Y (down) is Simulation -Z.
    /// Simulation +Z is Panel -Y (up), Panel +Z (screen to eyes) is Simulation +X.
    /// </summary>
    Orthogonal_FromRight,
}

public static class PerspectiveExtensions
{
    public static Vector3D SimToPanel(this Perspective perspective, Vector3D sim)
    {
        return perspective switch
        {
            Perspective.Orthogonal_FromAbove => new(+sim.X, -sim.Y, +sim.Z),
            Perspective.Orthogonal_FromFront => new(+sim.X, -sim.Z, -sim.Y),
            Perspective.Orthogonal_FromRight => new(+sim.Y, -sim.Z, +sim.X),
            _ => throw Utils.OutOfRange(nameof(perspective), perspective),
        };
    }
}
