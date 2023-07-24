using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System.Runtime.InteropServices;

namespace MechanicsCore.Rust.cgmath;

/// <summary>
/// Adapted from <see href="https://docs.rs/cgmath/latest/cgmath/struct.Vector3.html"/>
/// Adapted using <see href="https://dev.to/living_syn/calling-rust-from-c-6hk"/>
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly record struct Vector3_64(double x, double y, double z)
{
    /// <summary>
    /// If you change this, also change <see cref="Simulation.VectToString"/>
    /// </summary>
    public override readonly string ToString()
    {
        return $"{Simulation.DoubleToString(x)}, {Simulation.DoubleToString(y)}, {Simulation.DoubleToString(z)}";
    }

    public Vector<double> ToMathNet()
    {
        return new DenseVector(new[] { x, y, z });
    }

    public void ToMathNet(Vector<double> result)
    {
        result[0] = x;
        result[1] = y;
        result[2] = z;
    }
}
