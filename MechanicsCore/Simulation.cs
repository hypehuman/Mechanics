﻿using MathNet.Numerics.LinearAlgebra;

namespace MechanicsCore;

public abstract class Simulation
{
    private int sPrevBodyID = -1;
    public int NextBodyID => Interlocked.Increment(ref sPrevBodyID);

    public double t { get; private set; }
    public abstract double dt_step { get; }
    protected abstract int steps_per_leap { get; }
    public abstract Vector<double> DisplayBound0 { get; }
    public abstract Vector<double> DisplayBound1 { get; }

    public abstract IReadOnlyList<Body> Bodies { get; }

    #region Configurable

    public double DragCoefficient { get; set; }
    public bool BuoyantGravity { get; set; }

    #endregion

    private void Step()
    {
        // To make deterministic, parallelizable, and order-insensitive:
        // first compute all accelerations, then move bodies.
        var n = Bodies.Count;
        var a = new Vector<double>[n];
        for (var i = 0; i < n; i++)
        {
            a[i] = Bodies[i].ComputeAcceleration(Bodies);
        };
        for (var i = 0; i < n; i++)
        {
            Bodies[i].Step(dt_step, a[i]);
        };

        t += dt_step;
    }

    public void Leap()
    {
        for (int i = 0; i < steps_per_leap; i++)
        {
            Step();
        }
    }

    private static string DoubleToString(double d) => $"{d:0.000e00}";

    private static string VectToString(Vector<double> v) => string.Join(", ", v.Select(DoubleToString));

    private static string HeadingVectToString(Vector<double> v) => string.Join(", ", v.Select(x => $"{x:0.00}"));

    public string GetTimeString()
    {
        var secStr = $"t = {DoubleToString(t)} seconds";
        if (t < 1d / 1000)
        {
            // TimeSpan.FromSeconds is only accurate to the nearest millisecond.
            return secStr;
        }

        if (t < Constants.SecondsPerYear)
        {
            var ts = TimeSpan.FromSeconds(t);
            if (ts.TotalMinutes < 1)
                return $"{secStr} = {ts.TotalSeconds:0.000} seconds";
            if (ts.TotalHours < 1)
                return $"{secStr} = {ts.TotalMinutes:0.000} minutes";
            if (ts.TotalDays < 1)
                return $"{secStr} = {ts.TotalHours:0.000} hours";
            return $"{secStr} = {ts.TotalDays:0.000} days";
        }

        return $"{secStr} = {DoubleToString(t / Constants.SecondsPerYear)} years";
    }

    public void Dump()
    {
        Console.WriteLine(GetTimeString());
        foreach (var b in Bodies)
        {
            Console.WriteLine($"{b.Name} : {VectToString(b.Position)}");
        }
        for (int i = 0; i < Bodies.Count - 1; i++)
        {
            var a = Bodies[i];
            var b = Bodies[i + 1];
            var displacement = b.Position - a.Position;
            var distance = displacement.L2Norm();
            var heading = displacement / distance;
            Console.WriteLine($"{a.Name}->{b.Name} : {DoubleToString(distance)} ({HeadingVectToString(heading)})");
        }
        Console.WriteLine();
    }
}
