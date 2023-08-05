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
    private static unsafe extern Vector3D compute_acceleration_wrapper(double* masses, Vector3D* positions, UIntPtr num_bodies, UIntPtr index_of_self);

    public static Vector3D ComputeAcceleration(double[] masses, Vector3D[] positions, int index_of_self)
    {
        unsafe
        {
            fixed (double* massesPtr = masses)
            {
                fixed (Vector3D* positionsPtr = positions)
                {
                    return compute_acceleration_wrapper(massesPtr, positionsPtr, (UIntPtr)masses.Length, (UIntPtr)index_of_self);
                }
            }
        }
    }
}
