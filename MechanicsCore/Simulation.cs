﻿using MathNet.Spatial.Euclidean;
using MechanicsCore.PhysicsConfiguring;
using MechanicsCore.Rust.mechanics_fast;

namespace MechanicsCore;

public class Simulation
{
    public Arrangement InitialArrangement { get; }
    public PhysicsConfiguration PhysicsConfig { get; }

    #region Current state

    public long NumStepsPerformed { get; private set; }
    public double t => NumStepsPerformed * PhysicsConfig.StepTime;
    public Vector3D DisplayBound0 { get; }
    public Vector3D DisplayBound1 { get; }

    public IReadOnlyList<Body> Bodies { get; }
    public IEnumerable<Body> ExistingBodies => Bodies.Where(b => b.Exists);

    public bool HasError { get; private set; }
    public string? LatestErrorMessage { get; private set; }

    #endregion

    public Simulation(Scenario config)
    {
        InitialArrangement = config.InitialArrangement;
        Bodies = InitialArrangement.GenerateInitialState(out var displayBound0, out var displayBound1);
        DisplayBound0 = displayBound0;
        DisplayBound1 = displayBound1;
        PhysicsConfig = config.PhysicsConfig;

        p = new Vector3D[Bodies.Count];
        v = new Vector3D[Bodies.Count];
        a = new Vector3D[Bodies.Count];
    }

    // Reuse these collections on each step to reduce garbage
    private readonly Vector3D[] p;
    private readonly Vector3D[] v;
    private readonly Vector3D[] a;

    private bool TryComputeStep()
    {
        try
        {
            ComputeStep();
            return true;
        }
        catch (StepFailedException caughtEx)
        {
            HasError = true;
            LatestErrorMessage = caughtEx.Message;
            return false;
        }
    }

    /// <summary>
    /// Should not change anything but the values in <see cref="p"/>, <see cref="v"/>, and <see cref="a"/>.
    /// </summary>
    private void ComputeStep()
    {
        // To make deterministic, parallelizable, and order-insensitive:
        // first compute all accelerations, then move bodies.
        var n = Bodies.Count;
#if !DISABLE_RUST
        if (PhysicsConfig.CanTakeSimpleShortcut())
        {
            // The indexing in Bodies will be different from what we pass to Rust,
            // since Rust doesn't yet know to ignore bodies that have stopped existing.
            var numExistingBodies = ExistingBodies.Count();
            var rustMasses = new double[numExistingBodies];
            var rustPositions = new Vector3D[numExistingBodies];
            int rustI = 0;
            foreach (var body in ExistingBodies)
            {
                rustMasses[rustI] = body.Mass;
                rustPositions[rustI] = body.Position;
                rustI++;
            };
            rustI = 0;
            for (var i = 0; i < n; i++)
            {
                var body = Bodies[i];
                if (!body.Exists) continue;
                a[i] = mechanics_fast.ComputeAcceleration(rustMasses, rustPositions, rustI);
                rustI++;
            };
        }
        else
#endif
        {
            for (var i = 0; i < n; i++)
            {
                var body = Bodies[i];
                if (!body.Exists) continue;
                a[i] = body.ComputeAcceleration(Bodies, PhysicsConfig);
            };
        }
        for (var i = 0; i < n; i++)
        {
            var body = Bodies[i];
            if (!body.Exists) continue;
            body.ComputeStep(PhysicsConfig.StepTime, a[i], out p[i], out v[i]);
        };
    }

    private void ApplyStep()
    {
        // Now we've started editing the bodies, so there's no way to recover from an exception.

        var n = Bodies.Count;
        for (var i = 0; i < n; i++)
        {
            var body = Bodies[i];
            if (!body.Exists) continue;
            body.Step(p[i], v[i]);
        };

        if (PhysicsConfig.CollisionConfig == CollisionType.Combine)
        {
            CombineOverlappingBodies();
        }

        NumStepsPerformed++;
    }

    private void CombineOverlappingBodies()
    {
        var n = Bodies.Count;
        var overlapping = new Dictionary<Body, List<Body>>();
        for (int i = 0; i < n; i++)
        {
            var body1 = Bodies[i];
            if (!body1.Exists) continue;
            for (int j = i + 1; j < n; j++)
            {
                var body2 = Bodies[j];
                if (!body2.Exists) continue;
                if ((body1.Position - body2.Position).Length < (body1.Radius + body2.Radius))
                {
                    if (!overlapping.ContainsKey(body1)) overlapping.Add(body1, new());
                    if (!overlapping.ContainsKey(body2)) overlapping.Add(body2, new());
                    overlapping[body1].Add(body2);
                    overlapping[body2].Add(body1);
                }
            }
        }

        while (overlapping.Any())
        {
            var group = new List<Body>();
            void Check(Body body)
            {
                if (overlapping.Remove(body, out var touching))
                {
                    group.Add(body);
                    foreach (var other in touching)
                    {
                        Check(other);
                    }
                }
            }
            Check(overlapping.Keys.First());
            CombineBodies(group);
        }
    }

    private static void CombineBodies(List<Body> group)
    {
        var kept = group.MaxBy(b => b.Mass);
        foreach (var body in group)
        {
            if (body == kept)
            {
                body.Position = WeightedAverage(group, b => b.Position, b => b.Mass);
                body.Velocity = WeightedAverage(group, b => b.Velocity, b => b.Mass);
                body.Color = new(
                    WeightedAverage(group, b => b.Color.R, b => b.Mass),
                    WeightedAverage(group, b => b.Color.G, b => b.Mass),
                    WeightedAverage(group, b => b.Color.B, b => b.Mass)
                );
                body.Mass = group.Sum(b => b.Mass);
                body.Volume = group.Sum(b => b.Volume);
            }
            else
            {
                body.Exists = false;
            }
        }
    }

    /// <summary>
    /// Adapted from https://stackoverflow.com/a/3604761
    /// </summary>
    public static Vector3D WeightedAverage<T>(IEnumerable<T> records, Func<T, Vector3D> value, Func<T, double> weight)
    {
        if (records == null)
            throw new ArgumentNullException(nameof(records), $"{nameof(records)} is null.");

        int count = 0;
        Vector3D valueSum = default;
        double weightSum = 0;

        foreach (var record in records)
        {
            count++;
            double recordWeight = weight(record);

            valueSum += recordWeight * value(record);
            weightSum += recordWeight;
        }

        if (count == 0)
            throw new ArgumentException($"{nameof(records)} is empty.");

        if (count == 1)
            return value(records.Single());

        if (weightSum != 0)
            return valueSum / weightSum;
        else
            throw new DivideByZeroException($"Division of {valueSum} by zero.");
    }

    /// <summary>
    /// Adapted from https://stackoverflow.com/a/3604761
    /// </summary>
    public static byte WeightedAverage<T>(IEnumerable<T> records, Func<T, byte> value, Func<T, double> weight)
    {
        if (records == null)
            throw new ArgumentNullException(nameof(records), $"{nameof(records)} is null.");

        int count = 0;
        double valueSum = 0;
        double weightSum = 0;

        foreach (var record in records)
        {
            count++;
            double recordWeight = weight(record);

            valueSum += recordWeight * value(record);
            weightSum += recordWeight;
        }

        if (count == 0)
            throw new ArgumentException($"{nameof(records)} is empty.");

        if (count == 1)
            return value(records.Single());

        if (weightSum != 0)
            return Convert.ToByte(valueSum / weightSum);
        else
            throw new DivideByZeroException($"Division of {valueSum} by zero.");
    }

    public void Leap(int numSteps)
    {
        if (!TryLeap(numSteps))
        {
            throw new Exception(LatestErrorMessage);
        }
    }

    public bool TryLeap(int numSteps)
    {
        for (int i = 0; i < numSteps; i++)
        {
            if (!TryComputeStep())
            {
                return false;
            }
            ApplyStep();
        }

        return true;
    }

    public static string DoubleToString(double d) => $"{d:0.000e00}";

    private static string VectToString(Vector3D v) => $"{DoubleToString(v.X)}, {DoubleToString(v.Y)}, {DoubleToString(v.Z)}";

    private static string HeadingVectToString(Vector3D v) => $"{v.X:0.00}, {v.Y:0.00}, {v.Z:0.00}";

    public virtual IEnumerable<string> GetConfigLines()
    {
        foreach (var i in InitialArrangement.GetConfigLines())
            yield return i;

        foreach (var s in PhysicsConfig.GetConfigLines())
            yield return s;
    }

    public IEnumerable<string> GetStateSummaryLines()
    {
        yield return $"Step {NumStepsPerformed}";
        yield return GetTimeString();
        yield return $"{ExistingBodies.Count()} bodies";
        if (HasError)
        {
            yield return LatestErrorMessage;
        }
    }

    private string GetTimeString()
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

    public void DumpState()
    {
        foreach (var s in GetStateSummaryLines())
        {
            Console.WriteLine(s);
        }
        foreach (var b in ExistingBodies)
        {
            Console.WriteLine($"{b.Name} : {VectToString(b.Position)}");
        }
        for (int i = 0; i < Bodies.Count - 1; i++)
        {
            var a = Bodies[i];
            if (!a.Exists) continue;
            var b = Bodies[i + 1];
            if (!b.Exists) continue;
            var displacement = b.Position - a.Position;
            var distance = displacement.Length;
            var heading = displacement / distance;
            Console.WriteLine($"{a.Name}->{b.Name} : {DoubleToString(distance)} ({HeadingVectToString(heading)})");
        }
        Console.WriteLine();
    }
}
