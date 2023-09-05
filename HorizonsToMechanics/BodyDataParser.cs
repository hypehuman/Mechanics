using System.Text.RegularExpressions;

namespace HorizonsToMechanics;

internal static partial class BodyDataParser
{
    public static BodyData ParseBodyData(int id, HorizonsResponseContent responseObject)
    {
        return new(
            id,
            ParseName(responseObject.result),
            ParseMass(responseObject.result)
        );
    }

    // Name ends with the ID in parentheses, or with the ID of a related object.
    // E.g., object 4 is Mars Barycenter (499), object 499 is Mars (499).
    // The name tends to be followed by "{source: "
    [GeneratedRegex(
        """\bTarget\s+body\s+name:\s*(?<name>[^{]+)""",
        RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US"
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
        """\bMass\s*x\s*10+\s*\^\s*(?<exponent>[-\d.])+\s*\(\s*kg\s*\)\s*=\s*(?<mantissa>[-\d.])+\b""",
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
