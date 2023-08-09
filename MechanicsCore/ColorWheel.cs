using MathNet.Spatial.Euclidean;

namespace MechanicsCore;

public class ColorWheel : Simulation
{
    private readonly Func<int, BodyColor> _getColor;

    public override IEnumerable<string> GetConfigLines()
    {
        foreach (var b in base.GetConfigLines())
            yield return b;

        yield return $"Color getter: {_getColor.Method.Name}";
    }

    public override Vector3D DisplayBound0 { get; }
    public override Vector3D DisplayBound1 { get; }
    public override IReadOnlyList<Body> Bodies { get; }

    public ColorWheel(Func<int, BodyColor> getColor)
    {
        _getColor = getColor;

        const int numWheels = 3;
        const int numColorsPerWheel = 256;
        const double bodyRadius = Math.PI / numColorsPerWheel;

        var maxXY = GetWheelRadius(numWheels, bodyRadius);
        DisplayBound1 = new(maxXY, maxXY, bodyRadius);
        DisplayBound0 = -DisplayBound1;

        var bodies = new Body[numWheels * numColorsPerWheel];
        for (var wheelI = 0; wheelI < numWheels; wheelI++)
        {
            var wheelRadius = GetWheelRadius(wheelI, bodyRadius);
            for (var bodyJ = 0; bodyJ < numColorsPerWheel; bodyJ++)
            {
                var colorI = wheelI * numColorsPerWheel + bodyJ;
                var color = _getColor(colorI);
                var angle01 = (double)bodyJ / numColorsPerWheel;
                var angleRad = angle01 * 2 * Math.PI;
                var cos = Math.Cos(angleRad);
                var sin = Math.Sin(angleRad);
                bodies[colorI] = new(this,
                    color: color,
                    radius: bodyRadius,
                    position: new(wheelRadius * cos, wheelRadius * sin, 0)
                );
            }
        }

        Bodies = bodies;
    }

    private static double GetWheelRadius(int wheelI, double bodyRadius) => 1 + wheelI * 2 * bodyRadius;
}
