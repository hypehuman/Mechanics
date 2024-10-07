namespace MechanicsCore;

internal class BodyColors
{
    public static BodyColor Sun = new(0xFF, 0xD7, 0x00);
    public static BodyColor Earth = new(0x20, 0xB2, 0xAA);
    public static BodyColor Moon = new(0xC0, 0xC0, 0xC0);

    public static BodyColor FromID(int id, BodyHueOrder hueOrder, RingColorSpace colorSpace)
    {
        var hue_0_1 = hueOrder.GetBodyHue_0_1(id);
        var color = colorSpace.GetBodyColor(hue_0_1);
        return color;
    }
}
