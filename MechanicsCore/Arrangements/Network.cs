using MathNet.Spatial.Euclidean;

namespace MechanicsCore.Arrangements;

public class Network(int numBodies, double bodyMass, double bodyRadius, double linkStrength) : Arrangement
{
    public override IEnumerable<string> GetConfigLines()
    {
        foreach (var b in base.GetConfigLines())
            yield return b;

        yield return $"Number of bodies: {numBodies}";
        yield return $"Body mass: {Simulation.DoubleToString(bodyMass)}";
        yield return $"Body radius: {Simulation.DoubleToString(bodyRadius)}";
        yield return $"Link strength: {Simulation.DoubleToString(linkStrength)}";
    }

    public override object?[] GetConstructorParameters()
    {
        return new object?[] { numBodies, bodyMass, bodyRadius, linkStrength };
    }

    public override InitialState GenerateInitialState(out Vector3D displayBound0, out Vector3D displayBound1)
    {
        var bodies = new Body[numBodies];
        for (var i = 0; i < numBodies; i++)
        {
            bodies[i] = new Body(NextBodyID,
                mass: bodyMass,
                radius: bodyRadius,
                position: new((i * 2 + 1) * bodyRadius, 0, 0)
            );
        }
        displayBound0 = new(0, -bodyRadius, -bodyRadius);
        displayBound1 = new(numBodies * 2 * bodyRadius, bodyRadius, bodyRadius);

        var links = new List<Link>();
        for (var i = 0; i < numBodies - 1; i++)
        {
            links.Add(new Link(bodies[i], bodies[i + 1], linkStrength));
        }

        return new(bodies, links);
    }
}
