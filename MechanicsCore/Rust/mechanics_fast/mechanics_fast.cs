using MathNet.Spatial.Euclidean;
using System.Runtime.InteropServices;

namespace MechanicsCore.Rust.mechanics_fast;

/// <summary>
/// Source code is in this repo at Rust/mechanics_fast/src/lib.rs
/// </summary>
public static class mechanics_fast
{
    [DllImport("mechanics_fast.dll")]
    public static extern Vector3D compute_gravitational_acceleration(Vector3D displacement, double m2);

    [DllImport("mechanics_fast.dll")]
    public static extern Vector3D compute_acceleration(double[] masses, Vector3D[] positions, int index_of_self);
}
