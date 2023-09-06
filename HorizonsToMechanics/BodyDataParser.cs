using System.Text.RegularExpressions;

namespace HorizonsToMechanics;

internal static partial class BodyDataParser
{
    public static BodyData ParseBodyData(int id, HorizonsResponseContent responseObject)
    {
        var category = IdentifyFormat(id, responseObject.result);
        return new(
            id,
            ParseName(responseObject.result),
            ParseMass(responseObject.result)
        );
    }

    /// <summary>
    /// The result is a user-readable string that apparently has several different formats.
    /// Each one of these represents an observed format of the object data
    /// (the data describing the body's static properties, not the ephemeris).
    /// </summary>
    private enum ObjectDataFormat
    {
        Nonexistant,
        DynamicalPoint,
        Spacecraft,

        PhysicalData,
        PhysicalProperties,
        GeophysicalData,
        GeophysicalProperties,
        SatellitePhysicalProperties,
        SatellitePhysicalP,

        AsteroidPhysicalParameters,
        Lagrange,
        RecentlyDiscovered,
    }

    [GeneratedRegex(
        """^\s*\*+\s*$""",
        RegexOptions.Compiled
    )]
    private static partial Regex SectionSeparatorPattern();
    private static readonly Regex sSectionSeparatorPattern = SectionSeparatorPattern();

    [GeneratedRegex(
        """\bLagrange\b""",
        RegexOptions.IgnoreCase | RegexOptions.Compiled
    )]
    private static partial Regex LagrangePattern();
    private static readonly Regex sLagrangePattern = LagrangePattern();

    private static ObjectDataFormat IdentifyFormat(int id, string dataResult)
    {
        if (dataResult.StartsWith("No such record"))
            return ObjectDataFormat.Nonexistant;

        var lines = dataResult.Split('\n');
        var bodyDataOpenI = 0;
        if (lines == null || lines.Length <= bodyDataOpenI || !sSectionSeparatorPattern.IsMatch(lines[bodyDataOpenI]))
            throw new("Body data opening line not found");
        int bodyDataCloseI = -1;
        for (int lineI = bodyDataOpenI + 1; lineI < lines.Length; lineI++)
        {
            if (sSectionSeparatorPattern.IsMatch(lines[lineI]))
            {
                bodyDataCloseI = lineI;
                break;
            }
        }
        if (bodyDataCloseI <= bodyDataOpenI)
            throw new("Body data closing line not found");

        string bdLineOrEmpty(int lineI) => bodyDataOpenI < lineI && lineI < bodyDataCloseI ? lines[lineI] : string.Empty;
        bool bdTrimStartsWith(int lineI, string start) => bdLineOrEmpty(lineI).Trim().StartsWith(start);
        IEnumerable<string> bdLines() => Enumerable.Range(bodyDataOpenI + 1, bodyDataCloseI - bodyDataOpenI - 1).Select(i => lines[i]);

        if (bdTrimStartsWith(4, "http://nssdc.gsfc.nasa.gov/nmc/spacecraftDisplay"))
            return ObjectDataFormat.Spacecraft;

        // Sometimes has a colon, sometimes not.
        if (bdTrimStartsWith(3, "Dynamical point"))
            return ObjectDataFormat.DynamicalPoint;

        if (bdTrimStartsWith(3, "PHYSICAL DATA"))
            return ObjectDataFormat.PhysicalData;

        if (bdTrimStartsWith(3, "PHYSICAL PROPERTIES"))
            return ObjectDataFormat.PhysicalProperties;

        if (bdTrimStartsWith(3, "GEOPHYSICAL DATA"))
            return ObjectDataFormat.GeophysicalData;

        if (bdTrimStartsWith(3, "GEOPHYSICAL PROPERTIES"))
            return ObjectDataFormat.GeophysicalProperties;

        if (
            bdTrimStartsWith(3, "SATELLITE PHYSICAL PROPERTIES") ||
            bdTrimStartsWith(4, "SATELLITE PHYSICAL PROPERTIES") ||
            bdTrimStartsWith(5, "SATELLITE PHYSICAL PROPERTIES") ||
            bdTrimStartsWith(6, "SATELLITE PHYSICAL PROPERTIES")
        )
            return ObjectDataFormat.SatellitePhysicalProperties;

        if (bdTrimStartsWith(3, "SATELLITE PHYSICAL P"))
            return ObjectDataFormat.SatellitePhysicalP;

        if (bdTrimStartsWith(14, "Asteroid physical parameters"))
            return ObjectDataFormat.AsteroidPhysicalParameters;

        if (bodyDataCloseI <= 8 && bdLines().Any(l =>
                l.ContainsIgnoreCase("discover") ||
                l.ContainsIgnoreCase("recently recovered") || // I'm guessing they meant "recently discovered"
                l.ContainsIgnoreCase("Initial designation") ||
                l.ContainsIgnoreCase("irregular") ||
                l.ContainsIgnoreCase("Solution fit to all data")
            ))
        {
            return ObjectDataFormat.RecentlyDiscovered;
        }

        if (sLagrangePattern.IsMatch(bdLineOrEmpty(2)))
            return ObjectDataFormat.Lagrange;

        throw new($"Unrecognized category for object {id}");
    }

    // Name ends with the ID in parentheses, or with the ID of a related object.
    // E.g., object 4 is Mars Barycenter (499), object 499 is Mars (499).
    // The name tends to be followed by "{source: "
    [GeneratedRegex(
        """\bTarget\s+body\s+name:\s*(?<name>[^{]+)""",
        RegexOptions.IgnoreCase | RegexOptions.Compiled
    )]
    private static partial Regex NamePattern();
    private static readonly Regex sNamePattern = NamePattern();

    private static string? ParseName(string dataResult)
    {
        var match = sNamePattern.Match(dataResult);
        if (!match.Success)
            return null;

        return match.Groups["name"].Value.Trim();
    }

    private static readonly Regex sMassPattern = MassPattern();
    [GeneratedRegex(
        """\bMass\s*x\s*10+\s*\^\s*(?<exponent>[-\d.]+)\s*\(\s*kg\s*\)\s*=\s*(?<mantissa>[-\d.]+)\b""",
        RegexOptions.IgnoreCase | RegexOptions.Compiled
    )]
    private static partial Regex MassPattern();

    private static double? ParseMass(string dataResult)
    {
        var match = sMassPattern.Match(dataResult);
        if (!match.Success)
            return null;

        if (!double.TryParse(match.Groups["mantissa"].Value, out var mantissa))
            return null;

        if (!double.TryParse(match.Groups["exponent"].Value, out var exponent))
            return null;

        return mantissa * Math.Pow(10, exponent);
    }
}

internal static class StringExtensions
{
    public static bool ContainsIgnoreCase(this string self, string value)
    {
        return self.Contains(value, StringComparison.InvariantCultureIgnoreCase);
    }
}
