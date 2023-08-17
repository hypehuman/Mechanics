using GuiByReflection.Models;
using MathNet.Spatial.Euclidean;

namespace MechanicsCore.Arrangements;

[GuiHelp(
    "Arranges identical bodies in a line, each touching the other.",
    "Useful for testing the equilibrium behavior of collision properties like elasticity, deformation, and pressure."
)]
public class Line : Arrangement
{
    private readonly int _numBodies;
    private readonly double _bodyMass;
    private readonly double _bodyRadius;

    public override IEnumerable<string> GetConfigLines()
    {
        foreach (var b in base.GetConfigLines())
            yield return b;

        yield return $"Number of bodies: {_numBodies}";
        yield return $"Body mass: {Simulation.DoubleToString(_bodyMass)}";
        yield return $"Body radius: {Simulation.DoubleToString(_bodyRadius)}";
    }

    public override object?[] GetConstructorParameters()
    {
        return new object?[] { _numBodies, _bodyMass, _bodyRadius };
    }

    public Line(int numBodies, double bodyMass, double bodyRadius)
    {
        _numBodies = numBodies;
        _bodyMass = bodyMass;
        _bodyRadius = bodyRadius;
    }

    public override IReadOnlyList<Body> GenerateInitialState(out Vector3D displayBound0, out Vector3D displayBound1)
    {
        var bodies = new Body[_numBodies];
        for (var i = 0; i < _numBodies; i++)
        {
            bodies[i] = new Body(NextBodyID,
                mass: _bodyMass,
                radius: _bodyRadius,
                position: new((i * 2 + 1) * _bodyRadius, 0, 0)
            );
        }
        displayBound0 = new(0, -_bodyRadius, -_bodyRadius);
        displayBound1 = new(_numBodies * 2 * _bodyRadius, _bodyRadius, _bodyRadius);
        return bodies;
    }
}
