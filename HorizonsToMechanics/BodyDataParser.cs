using System.Text.RegularExpressions;

namespace HorizonsToMechanics;

internal static partial class BodyDataParser
{
    /// <summary>
    /// A null return value means that I know what I'm looking at, and it's not interesting.
    /// </summary>
    public static BodyData? ParseBodyData(int id, HorizonsResponseContent responseObject)
    {
        var category = IdentifyFormat(id, responseObject.result);
        if (category == ObjectDataFormat.Nonexistant || category == ObjectDataFormat.DynamicalPoint)
            return null;

        var name = ParseName(responseObject.result);
        if (name.ContainsIgnoreCase("Barycenter"))
            return null;

        var pv = ParsePositionAndVelocity(responseObject.result);

        return new(
            id,
            name,
            ParseMass(responseObject.result),
            pv.Item1, pv.Item2, pv.Item3, pv.Item4, pv.Item5, pv.Item6
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

        /// <summary>
        /// Careful: objects 1 (Mercury Barycenter) and 199 (Mercury)
        /// differ only in the "Target body name" property.
        /// Even the name in the body data section is identical.
        /// </summary>
        PhysicalData,
        PhysicalProperties,
        GeophysicalData,
        GeophysicalProperties,
        SatellitePhysicalProperties,
        SatellitePhysicalP,
        SatelliteGeneralPhysicalProperties,

        AsteroidPhysicalParameters,
        CometPhysical,
        SimulatedAsteroid,
        Lagrange,
        RecentlyDiscovered,

        /// <summary>
        /// My code could not figure it out
        /// </summary>
        Unrecognized,
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

        if (bdTrimStartsWith(1, $"Multiple major-bodies match string \"{id}\""))
            return ObjectDataFormat.Nonexistant;

        if (bdTrimStartsWith(4, "http://nssdc.gsfc.nasa.gov/nmc/spacecraftDisplay"))
            return ObjectDataFormat.Spacecraft;

        // Sometimes has a colon, sometimes not.
        if (bdTrimStartsWith(3, "Dynamical point"))
            return ObjectDataFormat.DynamicalPoint;

        if (
            bdTrimStartsWith(3, "PHYSICAL DATA") ||
            bdTrimStartsWith(6, "PHYSICAL DATA")
        )
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
        {
            // TODO: Many of these have more info that can be looked up at https://ssd.jpl.nasa.gov/sats/phys_par/
            // Some, e.g., object 716 (Caliban) don't appear in the table, but you can look it up by the
            // {source: ura116} and make the URL https://ssd.jpl.nasa.gov/ftp/eph/satellites/bsp/ura116.bsp
            return ObjectDataFormat.SatellitePhysicalProperties;
        }

        if (bdTrimStartsWith(6, "SATELLITE GENERAL PHYSICAL PROPERTIES"))
        {
            return ObjectDataFormat.SatelliteGeneralPhysicalProperties;
        }

        if (bdTrimStartsWith(3, "SATELLITE PHYSICAL P"))
            return ObjectDataFormat.SatellitePhysicalP;

        if (bdTrimStartsWith(14, "Asteroid physical parameters"))
            return ObjectDataFormat.AsteroidPhysicalParameters;

        if (bdTrimStartsWith(22, "Comet physical"))
            return ObjectDataFormat.CometPhysical;

        if (bdLineOrEmpty(3).ContainsIgnoreCase("SIMULATED ASTEROID"))
            return ObjectDataFormat.SimulatedAsteroid;

        if (bodyDataCloseI <= 13 && bdLines().Any(l =>
                l.ContainsIgnoreCase("discover") ||
                l.ContainsIgnoreCase("recently recovered") || // I'm guessing they meant "recently discovered"
                l.ContainsIgnoreCase("Initial designation") ||
                l.ContainsIgnoreCase("irregular") ||
                l.ContainsIgnoreCase("fit to ")
            ))
        {
            return ObjectDataFormat.RecentlyDiscovered;
        }

        if (sLagrangePattern.IsMatch(bdLineOrEmpty(2)))
            return ObjectDataFormat.Lagrange;

        Console.WriteLine($"Object {id}: Unrecognized category");
        return ObjectDataFormat.Unrecognized;
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

    private static string ParseName(string dataResult)
    {
        var matches = sNamePattern.Matches(dataResult);
        var names = matches.Select(m => m.Groups["name"].Value.Trim()).Distinct().ToList();
        if (names.Count == 1)
            return names[0];

        if (names.Count > 1)
            throw new("Multiple names: " + string.Join(", ", names));

        throw new("Could not file name");
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

    // $$SOE = "start of ephemeris"
    // $$EOE = "end of ephemeris"
    private static readonly Regex sEphemerisPattern = EphemerisPattern();
    [GeneratedRegex(
        """\n\$\$SOE\n.*\n\$\$EOE\n""",
        RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline
    )]
    private static partial Regex EphemerisPattern();

    private static readonly Regex sPositionVelocityPattern = PositionVelocityPattern();
    [GeneratedRegex(
        """\s*X\s*\=\s*(?<px>\S+)\s+Y\s*\=\s*(?<py>\S+)\s+Z\s*\=\s*(?<pz>\S+)\s*VX\s*\=\s*(?<vx>\S+)\s+VY\s*\=\s*(?<vy>\S+)\s+VZ\s*\=\s*(?<vz>\S+)\s*""",
        RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline
    )]
    private static partial Regex PositionVelocityPattern();

    private static (double, double, double, double, double, double) ParsePositionAndVelocity(string dataResult)
    {
        var ephemerisMatches = sEphemerisPattern.Matches(dataResult);
        if (ephemerisMatches.Count != 1)
            throw new("Expected 1 ephemeris, found " + ephemerisMatches.Count);
        var ephemeris = ephemerisMatches[0].Value;
        var pvMatches = sPositionVelocityPattern.Matches(ephemeris);
        if (pvMatches.Count != 1)
            throw new("Expected 1 position/velocity entry, found " + pvMatches.Count);
        var pvMatch = pvMatches[0];
        return (
            double.Parse(pvMatch.Groups["px"].Value),
            double.Parse(pvMatch.Groups["py"].Value),
            double.Parse(pvMatch.Groups["pz"].Value),
            double.Parse(pvMatch.Groups["vx"].Value),
            double.Parse(pvMatch.Groups["vy"].Value),
            double.Parse(pvMatch.Groups["vz"].Value)
        );
    }
}

internal static class StringExtensions
{
    public static bool ContainsIgnoreCase(this string self, string value)
    {
        return self.Contains(value, StringComparison.InvariantCultureIgnoreCase);
    }
}
