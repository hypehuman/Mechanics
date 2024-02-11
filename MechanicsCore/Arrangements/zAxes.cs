using MathNet.Spatial.Euclidean;

namespace MechanicsCore.Arrangements;

public class zAxes : Arrangement
{
    public override object?[] GetConstructorParameters()
    {
        return Array.Empty<object?[]>();
    }

    public override IReadOnlyList<Body> GenerateInitialState(out Vector3D displayBound0, out Vector3D displayBound1)
    {
        displayBound1 = new(1.5, 1.5, 1.5);
        displayBound0 = -displayBound1;
        return new Body[]
        {
            // origin: small black
            NewBody(0.0, 0.0, 0.0, false),
            // +x: large dark red, then small bright red
            NewBody(0.5, 0.0, 0.0, true),
            NewBody(1.0, 0.0, 0.0, false),
            // +y: large dark green, then small bright green
            NewBody(0.0, 0.5, 0.0, true),
            NewBody(0.0, 1.0, 0.0, false),
            // +z: large dark blue, then small bright blue
            NewBody(0.0, 0.0, 0.5, true),
            NewBody(0.0, 0.0, 1.0, false),
        };
    }

    private Body NewBody(double x, double y, double z, bool big)
    {
        return new(NextBodyID,
            position: new(x, y, z),
            color: new(
                (byte)(255 * x),
                (byte)(255 * y),
                (byte)(255 * z)
            ),
            radius: big ? 0.2 : 0.1
        );
    }
}
