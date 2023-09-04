using System.Text.RegularExpressions;

namespace HorizonsToMechanics;

/// <summary>
/// List of APIs: https://ssd.jpl.nasa.gov/api.html
/// </summary>
public class Class1
{
    public record BodyData
    (
        int ID,
        string? Name,
        double? Mass

    );

    public record HorizonsObjectData(
        string error,
        string result
    );

    // Name ends with the ID in parentheses, or with the ID of a related object.
    // E.g., object 4 is Mars Barycenter (499), object 499 is Mars (499).
    // The name tends to be followed by "{source: "
    private static readonly Regex sNamePattern = new Regex(
        @"\bTarget\s+body\s+name:\s*(?<name>[^{]+)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    private static readonly Regex sMassPattern = new Regex(
        @"\bMass\s*x\s*10+\s*\^\s*(?<exponent>[-\d.])+\s*\(\s*kg\s*\)\s*=\s*(?<mantissa>[-\d.])+\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    public static IEnumerable<BodyData> IterateObjects()
    {
        var time = new DateTime(2023, 08, 12, 0, 0, 0, DateTimeKind.Utc);
        using (var client = new HttpClient
        {
            BaseAddress = new("https://ssd.jpl.nasa.gov/api/horizons.api"),
        })
        {
            for (var id = 0; id < 623829; id++)
            {
                var parameters = GetUrlParameters(id, time);
                Console.WriteLine(parameters);

                HttpResponseMessage response = client.GetAsync(parameters).Result;
                if (response.IsSuccessStatusCode)
                {
                    using (var fileStream = File.Create(id + ".json"))
                    {
                        response.Content.CopyToAsync(fileStream).Wait();
                    }
                    //var horizonsData = .ReadAsAsync<HorizonsObjectData>().Result;
                    //yield return new BodyData(
                    //    id,
                    //    ParseName(horizonsData.result),
                    //    ParseMass(horizonsData.result)
                    //);
                }
                else
                {
                    Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
                    yield return new BodyData(id, null, null);
                }

                Console.WriteLine();
            }
        }
    }

    /// <summary>
    /// Minimum and maximum ID# are unclear.
    /// -1 says "No such record, positive values only",
    /// but there are sporadic spacecraft entires at least down to -111.
    /// Maximum object ID at time of this comment is 623827
    /// Higher will give a descriptive error message
    /// (json will have an "error" element)
    /// </summary>
    private static string GetUrlParameters(int objectID, DateTime time)
    {
        return
            "?" +
            "MAKE_EPHEM=YES"
            + "&" +
            $"COMMAND={objectID}"
            + "&" +
            "EPHEM_TYPE=VECTORS"
            + "&" +
            "CENTER='500@0'"
            + "&" +
            $"START_TIME='{time:O}'"
            + "&" +
            $"STOP_TIME='{time + TimeSpan.FromDays(1):O}'"
            + "&" +
            "STEP_SIZE='2 DAYS'"
            + "&" +
            "VEC_TABLE='2'"
            + "&" +
            "REF_SYSTEM='ICRF'"
            + "&" +
            "REF_PLANE='ECLIPTIC'"
            + "&" +
            "VEC_CORR='NONE'"
            + "&" +
            "CAL_TYPE='M'"
            + "&" +
            "OUT_UNITS='KM-S'"
            + "&" +
            "VEC_LABELS='YES'"
            + "&" +
            "VEC_DELTA_T='NO'"
            + "&" +
            "CSV_FORMAT='NO'"
            + "&" +
            "OBJ_DATA='YES'"
            ;
    }

    private static string? ParseName(string dataResult)
    {
        var match = sNamePattern.Match(dataResult);
        if (!match.Success)
            return null;

        return match.Groups["name"].Value.Trim();
    }

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
