using MathNet.Spatial.Euclidean;
using MechanicsCore.Rust.mechanics_fast;
using MechanicsCore.StepConfiguring;

namespace MechanicsCore;

public abstract class Simulation
{
    #region Current state

    private int sPrevBodyID = -1;
    public int NextBodyID => Interlocked.Increment(ref sPrevBodyID);

    public double t { get; private set; }
    public abstract Vector3D DisplayBound0 { get; }
    public abstract Vector3D DisplayBound1 { get; }

    public abstract IReadOnlyList<Body> Bodies { get; }
    public IEnumerable<Body> ExistingBodies => Bodies.Where(b => b.Exists);

    #endregion

    public InitialConfiguration InitConfig { get; } = new InitialConfiguration();
    public StepConfiguration StepConfig { get; } = new StepConfiguration();

    public bool TakeSimpleShortcut =>
        StepConfig.GravityConfig == GravityType.Newton_Pointlike &&
        StepConfig.CollisionConfig != CollisionType.Drag;

    private void Step()
    {
        // To make deterministic, parallelizable, and order-insensitive:
        // first compute all accelerations, then move bodies.
        var n = Bodies.Count;
        var a = new Vector3D[n];
#if !DISABLE_RUST
        if (TakeSimpleShortcut)
        {
            // The indexing in Bodies will be different from what we pass to Rust,
            // since Rust doesn't yet know to ignore bodies that have stopped existing.
            var numExistingBodies = ExistingBodies.Count();
            var m = new double[numExistingBodies];
            var p = new Vector3D[numExistingBodies];
            int rustI = 0;
            foreach (var body in ExistingBodies)
            {
                m[rustI] = body.Mass;
                p[rustI] = body.Position;
                rustI++;
            };
            rustI = 0;
            for (var i = 0; i < n; i++)
            {
                var body = Bodies[i];
                if (!body.Exists) continue;
                a[i] = mechanics_fast.ComputeAcceleration(m, p, rustI);
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
                a[i] = body.ComputeAcceleration(Bodies);
            };
        }
        for (var i = 0; i < n; i++)
        {
            var body = Bodies[i];
            if (!body.Exists) continue;
            body.Step(StepConfig.StepTime, a[i]);
        };

        if (StepConfig.CollisionConfig == CollisionType.Combine)
        {
            CombineOverlappingBodies();
        }

        t += StepConfig.StepTime;
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
                body.DisplayVolume = group.Sum(b => b.DisplayVolume);
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

    public void Leap()
    {
        for (int i = 0; i < StepConfig.StepsPerLeap; i++)
        {
            Step();
        }
    }

    public static string DoubleToString(double d) => $"{d:0.000e00}";

    private static string VectToString(Vector3D v) => $"{DoubleToString(v.X)}, {DoubleToString(v.Y)}, {DoubleToString(v.Z)}";

    private static string HeadingVectToString(Vector3D v) => $"{v.X:0.00}, {v.Y:0.00}, {v.Z:0.00}";

    public virtual IEnumerable<string> GetConfigLines()
    {
        foreach (var s in StepConfig.GetConfigLines())
            yield return s;
    }

    public IEnumerable<string> GetStateSummaryLines()
    {
        yield return GetTimeString();
        yield return $"{ExistingBodies.Count()} bodies";
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
