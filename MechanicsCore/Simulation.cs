using MathNet.Spatial.Euclidean;

namespace MechanicsCore;

public abstract class Simulation
{
    private int sPrevBodyID = -1;
    public int NextBodyID => Interlocked.Increment(ref sPrevBodyID);

    public double t { get; private set; }
    public abstract double dt_step { get; }
    protected abstract int steps_per_leap { get; }
    public abstract Vector3D DisplayBound0 { get; }
    public abstract Vector3D DisplayBound1 { get; }

    public abstract IReadOnlyList<Body> Bodies { get; }
    public IEnumerable<Body> ExistingBodies => Bodies.Where(b => b.Exists);

    #region Configurable

    public GravityType GravityConfig { get; set; }
    public static double BuoyantGravityRatio => 1; // Increasing this to 10 lets us line up more moons without them collapsing in on each other.
    public bool CombineIfOverlapping { get; set; }
    public double DragCoefficient { get; set; }

    #endregion

    public bool TakeSimpleShortcut => GravityConfig == GravityType.Newton_Pointlike && DragCoefficient == 0;

    private void Step()
    {
        // To make deterministic, parallelizable, and order-insensitive:
        // first compute all accelerations, then move bodies.
        var n = Bodies.Count;
        var a = new Vector3D[n];
        for (var i = 0; i < n; i++)
        {
            var body = Bodies[i];
            if (!body.Exists) continue;
            a[i] = body.ComputeAcceleration(Bodies);
        };
        for (var i = 0; i < n; i++)
        {
            var body = Bodies[i];
            if (!body.Exists) continue;
            body.Step(dt_step, a[i]);
        };

        if (CombineIfOverlapping)
        {
            CombineOverlappingBodies();
        }

        t += dt_step;
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
        for (int i = 0; i < steps_per_leap; i++)
        {
            Step();
        }
    }

    public static string DoubleToString(double d) => $"{d:0.000e00}";

    private static string VectToString(Vector3D v) => $"{DoubleToString(v.X)}, {DoubleToString(v.Y)}, {DoubleToString(v.Z)}";

    private static string HeadingVectToString(Vector3D v) => $"{v.X:0.00}, {v.Y:0.00}, {v.Z:0.00}";

    public virtual IEnumerable<string> GetConfigLines()
    {
        if (GravityConfig != GravityType.None)
            yield return $"Gravity: {GravityConfig}";

        if (CombineIfOverlapping)
            yield return "Combine bodies if they overlap";

        if (DragCoefficient != 0)
            yield return $"Drag coefficient: {DragCoefficient}";
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
