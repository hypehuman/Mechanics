using GuiByReflection.Models;
using System.Security.Cryptography;

namespace MechanicsCore;

public delegate double GetBodyHue_0_1(int bodyID);

public enum BodyHueOrder
{
    /// <summary>
    /// Used only for testing; throws an exception when attempting to use it.
    /// </summary>
    [GuiHelp("Choose this to let each body's position determine its hue angle.")]
    Explicit,

    [GuiHelp("Returns hues in an order where small groups of adjacent IDs have well-spread hues.")]
    GoldenSpaced,

    [GuiHelp("Returns a predictable but random-looking and non-repeating series of hues.")]
    HashedPseudorandom,
}

public static class BodyHueOrders
{
    public static GetBodyHue_0_1 GetFunc(this BodyHueOrder hueOrder) => hueOrder switch
    {
        BodyHueOrder.Explicit => throw new Exception($"{hueOrder} has no predefined order."),
        BodyHueOrder.GoldenSpaced => GoldenSpaced,
        BodyHueOrder.HashedPseudorandom => HashedPseudorandom,
        _ => throw Utils.OutOfRange(nameof(hueOrder), hueOrder)
    };

    public static double GetBodyHue_0_1(this BodyHueOrder hueOrder, int bodyID)
    {
        return hueOrder.GetFunc()(bodyID);
    }

    private static double GoldenSpaced(int id)
    {
        var hue_0_1 = (Constants.NegPhiMod1 * id) % 1;
        return hue_0_1;
    }

    private static double HashedPseudorandom(int id)
    {
        var idBytes = BitConverter.GetBytes(id);
        var hash256 = SHA256.HashData(idBytes);

        // As described in http://prng.di.unimi.it/:
        // "A standard double (64-bit) floating-point number in IEEE floating point format has 52 bits of significand,
        //  plus an implicit bit at the left of the significand. Thus, the representation can actually store numbers with
        //  53 significant binary digits. Because of this fact, in C99 a 64-bit unsigned integer x should be converted to
        //  a 64-bit double using the expression
        //  (x >> 11) * 0x1.0p-53"
        var hash64 = BitConverter.ToUInt64(hash256, 0);
        var hue_0_1 = (hash64 >> 11) * (1.0 / (1ul << 53));

        return hue_0_1;
    }
}
