namespace HorizonsToMechanics;

/// <summary>
/// Report to NASA:
///  - object 553 "recently recovered"; should be "recently discovered" to match 551, 552, etc?
///  - https://ssd.jpl.nasa.gov/sats/phys_par/ link "glossary definition of GM" points to https://ssd.jpl.nasa.gov/glossary/albedo.html should be https://ssd.jpl.nasa.gov/glossary/GM.html
///  All where Mass and GM are more than 1% different; we know G better than that!
/// </summary>
internal class HorizonsQuerier : IDisposable
{
    HttpClient? _client;

    private HttpClient Client
    {
        get
        {
            _client ??= new()
            {
                BaseAddress = new("https://ssd.jpl.nasa.gov/api/horizons.api"),
            };
            return _client;
        }
    }

    public async Task<HttpResponseMessage> GetAsync(int id, DateTime time, CancellationToken cancellationToken = default)
    {
        var parameters = GetUrlParameters(id, time);
        Console.WriteLine(parameters);

        return await Client.GetAsync(parameters, cancellationToken: cancellationToken);
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

    public void Dispose()
    {
        _client?.Dispose();
    }
}
