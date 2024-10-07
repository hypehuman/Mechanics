using GuiByReflection.Models;
using MathNet.Spatial.Euclidean;

namespace MechanicsCore.Arrangements;

[GuiHelp(
    "Just for testing the distribution of colors that we choose for bodies.",
    "Step/Leap does nothing."
)]
public class zColorWheel : Arrangement
{
    private readonly BodyHueOrder _hueOrder;
    private readonly RingColorSpace _colorSpace;
    private readonly bool _spiral;
    private readonly int _numWheels = 3;
    private readonly int _numColorsPerWheel = 256;

    public override IEnumerable<string> GetConfigLines()
    {
        foreach (var b in base.GetConfigLines())
            yield return b;

        yield return $"Hue order: {_hueOrder}";
        yield return $"Color space: {_colorSpace}";
        yield return $"Spiral: {_spiral}";
    }

    public override object?[] GetConstructorParameters()
    {
        return new object?[]
        {
            _hueOrder,
            _colorSpace,
            _spiral,
        };
    }

    public zColorWheel(
        BodyHueOrder hueOrder = BodyHueOrder.GoldenSpaced,
        RingColorSpace colorSpace = RingColorSpace.RgbSaturated,
        bool spiral = false,
        int numWheels = 3,
        int numColorsPerWheel = 256
    )
    {
        _hueOrder = hueOrder;
        _colorSpace = colorSpace;
        _spiral = spiral;
        _numWheels = numWheels;
        _numColorsPerWheel = numColorsPerWheel;
    }

    public override IReadOnlyList<Body> GenerateInitialState(out Vector3D displayBound0, out Vector3D displayBound1)
    {
        var bodyRadius = Math.PI / _numColorsPerWheel;

        var maxXY = GetDisanceFromCenter(_numWheels, 1, bodyRadius);
        displayBound1 = new(maxXY, maxXY, bodyRadius);
        displayBound0 = -displayBound1;

        var getColor = _colorSpace.GetFunc();

        var bodies = new Body[_numWheels * _numColorsPerWheel];
        for (var wheelI = 0; wheelI < _numWheels; wheelI++)
        {
            for (var bodyJ = 0; bodyJ < _numColorsPerWheel; bodyJ++)
            {
                var bodyID = NextBodyID;
                var angle01 = (double)bodyJ / _numColorsPerWheel;
                var wheelRadius = GetDisanceFromCenter(wheelI, angle01, bodyRadius);
                var hue_0_1 = _hueOrder == BodyHueOrder.Explicit ? angle01 : _hueOrder.GetBodyHue_0_1(bodyID);
                var color = getColor(hue_0_1);
                var angleRad = angle01 * 2 * Math.PI;
                var cos = Math.Cos(angleRad);
                var sin = Math.Sin(angleRad);
                bodies[bodyID] = new(bodyID,
                    color: color,
                    radius: bodyRadius,
                    position: new(wheelRadius * cos, wheelRadius * sin, 0)
                );
            }
        }

        return bodies;
    }

    private double GetDisanceFromCenter(int wheelI, double angle01, double bodyRadius) =>
        1 + (wheelI + (_spiral ? angle01 : 0d)) * 2 * bodyRadius;
}
