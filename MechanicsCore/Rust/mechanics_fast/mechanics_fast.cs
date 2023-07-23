﻿using MechanicsCore.Rust.cgmath;
using System.Runtime.InteropServices;

namespace MechanicsCore.Rust.mechanics_fast;

/// <summary>
/// Source code is in this repo at Rust/mechanics_fast/src/lib.rs
/// </summary>
public static class mechanics_fast
{
    [DllImport("mechanics_fast.dll")]
    public static extern Vector3_64 compute_gravitational_acceleration(double[] displacement, double m2);
}
