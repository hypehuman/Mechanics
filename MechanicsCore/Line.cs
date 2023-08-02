using MathNet.Spatial.Euclidean;

namespace MechanicsCore;

public class Line : Simulation
{
    private readonly int _numBodies;
    private readonly double _bodyMass;
    private readonly double _bodyRadius;

    public override Vector3D DisplayBound0 { get; }
    public override Vector3D DisplayBound1 { get; }
    public override IReadOnlyList<Body> Bodies { get; }

    public Line(int numBodies, double bodyMass, double bodyRadius)
    {
        StepConfig.StepTime = 1;
        StepConfig.StepsPerLeap = 128;

        _numBodies = numBodies;
        _bodyMass = bodyMass;
        _bodyRadius = bodyRadius;

        var bodies = new Body[_numBodies];
        for (var i = 0; i < _numBodies; i++)
        {
            bodies[i] = new Body(this,
                mass: _bodyMass,
                radius: _bodyRadius,
                position: new((i * 2 + 1) * _bodyRadius, 0, 0)
            );
        }
        Bodies = bodies;
        DisplayBound0 = new(0, -_bodyRadius, -_bodyRadius);
        DisplayBound1 = new(_numBodies * 2 * _bodyRadius, _bodyRadius, _bodyRadius);
    }

    public override IEnumerable<string> GetConfigLines()
    {
        foreach (var b in base.GetConfigLines())
            yield return b;

        yield return $"Number of bodies: {_numBodies}";
        yield return $"Body mass: {DoubleToString(_bodyMass)}";
        yield return $"Body radius: {DoubleToString(_bodyRadius)}";
    }
}
