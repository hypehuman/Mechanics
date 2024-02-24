using System.Text.RegularExpressions;

namespace HorizonsToMechanics;

internal static partial class BodyDataParser
{
    /// <summary>
    /// A null return value means that I know what I'm looking at, and it's not relevant.
    /// </summary>
    public static BodyData? ParseBodyData(int id, HorizonsResponseContent responseObject)
    {
        var category = IdentifyFormat(id, responseObject.result);
        switch (category)
        {
            case ObjectDataFormat.Nonexistant:
            case ObjectDataFormat.DynamicalPoint:
            case ObjectDataFormat.SimulatedAsteroid:
            case ObjectDataFormat.Lagrange:
            case ObjectDataFormat.RecentlyDiscovered:
                return null;
        }

        var name = ParseName(responseObject.result);
        if (name.ContainsIgnoreCase("Barycenter"))
            return null;

        var mass = ParseMass(id, responseObject.result);
        if (mass == null)
            return null;

        var pv = ParsePositionAndVelocity(responseObject.result);

        return new(
            id,
            name,
            mass,
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

    // 401 Phobos is interesting.
    // There is no GM listed, only mass.
    // The mass of 1.08e16 kg is represented as "Mass (10^20 kg )        =  1.08 (10^-4)"
    private static readonly Regex sMassPattern = MassPattern();
    [GeneratedRegex(
        """(?<!Rocky core |Charon )\bMass\b(?! ratio| of atmosphere| layers|-energy conv rate),?\s*(?<units>[^=]*)\s*\=(?<value>\s*[\S]+\s*(\([^)]*\))?([^A-Z]|(?<!\s)[A-Z])+)""",
        RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.ExplicitCapture
    )]
    private static partial Regex MassPattern();

    // |\) is to not get confused by 852 Wladilena (A916 GM)
    // 2013 Siding Spring has the units explained in its Comet Physical line "Comet physical (GM= km^3/s^2; RAD= km):  "
    // End with """([^A-Z]|(?<!\s)[A-Z])+""", which keeps matching anything until we find a capital letter preceded by a space,
    //     which probably signals the start of a new property name.
    //     E.g.: GM (km^3/s^2)          = 5959.9155+- 0.004 Geometric Albedo  = 0.63  +- 0.02
    private static readonly Regex sGmPattern = GmPattern();
    [GeneratedRegex(
        """(?<!Comet physical \()\bGM\b( \(planet\))?(?! 1-sigma|\)),?\s*(?<units>[^=]*)\s*\=(?<value>\s*[\S]+\s*(\([^)]*\))?([^A-Z]|(?<!\s)[A-Z])+)""",
        RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.ExplicitCapture
    )]
    private static partial Regex GmPattern();

    private static readonly Regex sMassUnitsPattern = MassUnitsPattern();
    [GeneratedRegex(
        """^\s*\(?\s*x?\s*10\s*\^\s*(?<exponent>[-\d.]+)\s*\(?\s*(?<unit>k?g)\s*\)?\s*$""",
        RegexOptions.IgnoreCase | RegexOptions.Compiled
    )]
    private static partial Regex MassUnitsPattern();

    private static readonly Regex sMassValuePattern = MassValuePattern();
    [GeneratedRegex(
        """^\s*~?\s*(?<mantissa>[-\d.]+)\s*(\+\s*/?\s*\-\s*[\d.]+)?\s*(\(\s*10\s*\^\s*(?<exponent>[-\d.]+)\s*\))?\s*$""",
        RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.ExplicitCapture
    )]
    private static partial Regex MassValuePattern();

    private static readonly Regex sGmUnitsPattern = GmUnitsPattern();
    [GeneratedRegex(
        """^\s*\(?\s*km\s*\^\s*3\s*/\s*s\s*\^\s*2\s*\)?\s*$""",
        RegexOptions.IgnoreCase | RegexOptions.Compiled
    )]
    private static partial Regex GmUnitsPattern();

    private static readonly Regex sGmValuePattern = GmValuePattern();
    [GeneratedRegex(
        """^\s*~?\s*(?<mantissa>[-\d.]+)\s*(\+\s*/?\s*\-\s*[\d.]+)?\s*$""",
        RegexOptions.IgnoreCase | RegexOptions.Compiled
    )]
    private static partial Regex GmValuePattern();

    /// <summary>
    /// Null return value means that the mass is explicitly not available.
    /// If not found or couldn't parse, throws an exception.
    /// </summary>
    private static double? ParseMass(int id, string dataResult)
    {
        var parsedMasses = sMassPattern.Matches(dataResult).Select(ParseMass).Distinct().ToList();
        if (parsedMasses.Count > 1)
            throw new("Expected 0 or 1 mass, found " + parsedMasses.Count);

        // 11 Parthenope has the "Asteroid physical parameters" listed twice.
        var parsedGms = sGmPattern.Matches(dataResult).Select(ParseGm).Distinct().ToList();
        if (parsedGms.Count > 1)
            throw new("Expected 0 or 1 GM, found " + parsedGms.Count);

        if (parsedMasses.Count == 0 && parsedGms.Count == 0)
            throw new("Found neither mass nor GM.");

        var massesFromGm = parsedGms.Select(gm => gm / 6.6743e-20); // e-20 instead of e-11 because we're using km^3 instead of m^3

        var convertedMasses = parsedMasses.Concat(massesFromGm).Where(m => m != null).Select(m => m.Value).ToList();
        double? prevMass = null;
        foreach (var mass in convertedMasses)
        {
            if (prevMass.HasValue && Math.Abs((mass - prevMass.Value) / prevMass.Value) > 0.01)
                Console.WriteLine($"Masses {prevMass} and {mass} disagree: taking average.");
            prevMass = mass;
        }

        return convertedMasses.Any()? convertedMasses.Average() : null;
    }

    private static double? ParseMass(Match match)
    {
        // 507 Elara just has whitespace for the mass (i.e., it's missing).
        if (match.Groups["value"].Value.StartsWith("     ") && (
                match.Groups["value"].Value.Trim().StartsWith("Geometric") ||
                match.Groups["value"].Value.Trim().StartsWith("Hill")
            ))
            return null;

        var valueStr = match.Groups["value"].Value.Trim();
        var unitsStr = match.Groups["units"].Value.Trim();

        var unitsMatch = sMassUnitsPattern.Match(unitsStr);
        if (!unitsMatch.Success || !double.TryParse(unitsMatch.Groups["exponent"].Value, out var unitsExponent))
            throw new("Could not parse mass units: " + unitsStr);
        var unitFactor = unitsMatch.Groups["unit"].Value switch
        {
            "kg" => 1,
            "g" => 0.001,
            "" => 1, // if missing, assume kg
            _ => throw new("Unusual mass units: " + unitsMatch.Groups["unit"].Value)
        };

        var valueMatch = sMassValuePattern.Match(valueStr);
        if (!valueMatch.Success || !double.TryParse(valueMatch.Groups["mantissa"].Value, out var mantissa))
            throw new("Could not parse mass value: " + valueStr);

        var valueExponentGroup = valueMatch.Groups["exponent"];
        double valueExponent;
        if (!valueExponentGroup.Success)
            valueExponent = 0;
        else if (!double.TryParse(valueExponentGroup.Value, out valueExponent))
            throw new("Could not parse mass value exponent: " + valueExponentGroup.Value);

        return mantissa * Math.Pow(10, unitsExponent + valueExponent) * unitFactor;
    }

    private static double? ParseGm(Match match)
    {
        var valueStr = match.Groups["value"].Value.Trim();

        if (valueStr == "n.a.")
            return null;

        var unitsStr = match.Groups["units"].Value.Trim();
        if (unitsStr != "") // if empty units, just assume the default
        {
            var unitsMatch = sGmUnitsPattern.Match(unitsStr);
            if (!unitsMatch.Success /*|| !double.TryParse(unitsMatch.Groups["exponent"].Value, out var unitsExponent)*/)
                throw new("Could not parse GM units: " + unitsStr);
        }

        var valueMatch = sGmValuePattern.Match(valueStr);
        if (!valueMatch.Success || !double.TryParse(valueMatch.Groups["mantissa"].Value, out var mantissa))
            throw new("Could not parse GM value: " + valueStr);

        return mantissa /* * Math.Pow(10, unitsExponent)*/;
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
        // Ephemeris uses kilometers and seconds, but we want SI units (meters and seconds)
        return (
            1000 * double.Parse(pvMatch.Groups["px"].Value),
            1000 * double.Parse(pvMatch.Groups["py"].Value),
            1000 * double.Parse(pvMatch.Groups["pz"].Value),
            1000 * double.Parse(pvMatch.Groups["vx"].Value),
            1000 * double.Parse(pvMatch.Groups["vy"].Value),
            1000 * double.Parse(pvMatch.Groups["vz"].Value)
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
