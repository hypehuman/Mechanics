#if !DISABLE_RUST
using MathNet.Spatial.Euclidean;
using System.Runtime.InteropServices;

namespace MechanicsCore.Rust.mechanics_fast;

/// <summary>
/// Source code is in this repo at Rust/mechanics_fast/src/lib.rs
/// </summary>
public static class mechanics_fast
{
    [DllImport("mechanics_fast.dll")]
    private static extern Vector3D pub_compute_gravitational_acceleration_one_on_one(Vector3D displacement, double m2);

    [DllImport("mechanics_fast.dll")]
    private static unsafe extern Vector3D pub_compute_gravitational_acceleration_many_on_one(double* masses, Vector3D* positions, UIntPtr num_bodies, UIntPtr index_of_self);

    [DllImport("mechanics_fast.dll")]
    private static unsafe extern IntPtr pub_compute_gravitational_acceleration_many_on_many(double* masses, Vector3D* positions, UIntPtr num_bodies, Vector3D* accelerationsOut);

    [DllImport("mechanics_fast.dll")]
    private static unsafe extern UIntPtr pub_try_leap(UIntPtr requested_num_steps, double stepDuration, double* masses, Vector3D* positions, Vector3D* velocities, UIntPtr num_bodies);

    public static Vector3D ComputeGravitationalAcceleration(Vector3D displacement, double m2)
    {
        return pub_compute_gravitational_acceleration_one_on_one(displacement, m2);
    }

    public static Vector3D ComputeGravitationalAcceleration(double[] masses, Vector3D[] positions, int numBodies, int index_of_self)
    {
        unsafe
        {
            fixed (double* massesPtr = masses)
            {
                fixed (Vector3D* positionsPtr = positions)
                {
                    return pub_compute_gravitational_acceleration_many_on_one(massesPtr, positionsPtr, (UIntPtr)numBodies, (UIntPtr)index_of_self);
                }
            }
        }
    }

    public static void ComputeGravitationalAcceleration(double[] masses, Vector3D[] positions, int numBodies, Vector3D[] accelerationsOut)
    {
        unsafe
        {
            fixed (double* massesPtr = masses)
            {
                fixed (Vector3D* positionsPtr = positions)
                {
                    fixed (Vector3D* accelerationsPtr = accelerationsOut)
                    {
                        pub_compute_gravitational_acceleration_many_on_many(massesPtr, positionsPtr, (UIntPtr)numBodies, accelerationsPtr);
                    }
                }
            }
        }
    }

    public static int TryLeap(int requestedNumSteps, double stepDuration, double[] masses, Vector3D[] positions, Vector3D[] velocities, int numBodies)
    {
        // TODO: When it fails, return a message like we do with StepFailedException.
        unsafe
        {
            fixed (double* massesPtr = masses)
            {
                fixed (Vector3D* positionsPtr = positions)
                {
                    fixed (Vector3D* velocitiesPtr = velocities)
                    {
                        var actualNumSteps = pub_try_leap((UIntPtr)requestedNumSteps, stepDuration, massesPtr, positionsPtr, velocitiesPtr, (UIntPtr)numBodies);
                        return (int)actualNumSteps;
                    }
                }
            }
        }
    }
}
#endif
