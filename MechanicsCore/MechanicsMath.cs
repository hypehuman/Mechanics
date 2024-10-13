namespace MechanicsCore;

public static class MechanicsMath
{
    public static double SphereRadiusToVolume(double radius) => Constants.FourThirdsPi * radius * radius * radius;

    public static double SphereVolumeToRadius(double volume) => Math.Cbrt(volume * Constants.ThreeOverFourPi);
}
