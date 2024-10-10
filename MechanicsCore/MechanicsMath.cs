namespace MechanicsCore;

public static class MechanicsMath
{
    public static double SphereRadiusToVolume(double radius) => Constants.FourThirdsPi * radius * radius * radius;

    public static double SphereVolumeToRadius(double volume) => Math.Cbrt(volume * Constants.ThreeOverFourPi);

    public static double Min(double val1, double val2, double val3)
    {
        return Math.Min(Math.Min(val1, val2), val3);
    }

    public static double Max(double val1, double val2, double val3)
    {
        return Math.Max(Math.Max(val1, val2), val3);
    }
}
