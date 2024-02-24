using System.Text.Json;

namespace HorizonsToMechanics;

/// <summary>
/// List of APIs: https://ssd.jpl.nasa.gov/api.html
/// </summary>
public partial class BodyDataEnumerable : IAsyncEnumerable<BodyData>
{
    /// <summary>
    /// Archive the data that Horizons sends back.
    /// </summary>
    private readonly string _jsonDir;

    /// <summary>
    /// For debugging only; this directory is gitignored.
    /// </summary>
    private readonly string? _txtDir;

    public BodyDataEnumerable(string jsonDir, string? txtDir)
    {
        _jsonDir = jsonDir;
        _txtDir = txtDir;
    }

    public async IAsyncEnumerator<BodyData> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(_jsonDir))
        {
            Directory.CreateDirectory(_jsonDir);
        }
        Console.WriteLine($"Downloading json responses to: '{Path.GetFullPath(_jsonDir)}'");

        try
        {
            if (!Directory.Exists(_txtDir))
            {
                Directory.CreateDirectory(_txtDir);
            }
            Console.WriteLine($"We will attempt to write debugging data to: '{Path.GetFullPath(_txtDir)}'");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Cannot write debugging data to: '{_txtDir}'");
            Console.WriteLine(ex);
        }

        // This timestamp is the birthday that Suhas and I stargazed at Glas-Iglu Braunwald.
        // The Milky Way was clearly visible with the naked eye.
        // We saw Jupiter's moons and Saturn's rings with our cheapy tiny telescope; big surprise!
        var time = new DateTime(2023, 08, 12, 0, 0, 0, DateTimeKind.Utc);
        using (var querier = new HorizonsQuerier())
        {
            for (var id = -2; id < 623829; id++)
            {
                var contentPath = Path.Combine(_jsonDir, id + ".json");

                HorizonsResponseContent responseObject;
                try
                {
                    if (File.Exists(contentPath))
                    {
                        using var fileStream = File.OpenRead(contentPath);
                        responseObject = await JsonSerializer.DeserializeAsync<HorizonsResponseContent>(fileStream, cancellationToken: cancellationToken)
                        ?? throw new NullReferenceException("The serializer returned null.");
                    }
                    else
                    {
                        var response = await querier.GetAsync(id, time, cancellationToken: cancellationToken);
                        if (response.IsSuccessStatusCode)
                        {
                            responseObject = await response.Content.ReadAsAsync<HorizonsResponseContent>(cancellationToken: cancellationToken);
                        }
                        else
                        {
                            throw new Exception($"{(int)response.StatusCode} ({response.ReasonPhrase})");
                        }

                        try
                        {
                            using (var fileStream = File.Create(contentPath))
                            {
                                await response.Content.CopyToAsync(fileStream, cancellationToken: cancellationToken);
                            }
                        }
                        catch
                        {
                            // Get rid of any partially written file
                            File.Delete(contentPath);
                            throw;
                        }
                    }
                }
                catch (TaskCanceledException)
                {
                    yield break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Object {id}: Failed to get response:");
                    Console.WriteLine(ex);
                    continue;
                }

                if (responseObject.error != null)
                {
                    Console.WriteLine($"Object {id}: Response has error:");
                    Console.WriteLine(responseObject.error);
                    continue;
                }

                TryWithTextPath(id, txtPath =>
                {
                    if (!File.Exists(txtPath))
                    {
                        // for debugging, since the newlines in the string make things hard to read
                        File.WriteAllText(txtPath, responseObject.result);
                    }
                });

                BodyData? bd;
                try
                {
                    bd = BodyDataParser.ParseBodyData(id, responseObject);
                    if (bd == null)
                    {
                        // I know what I'm looking at, and it's not relevant.
                        TryWithTextPath(id, File.Delete);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Object {id}: Failed to parse:");
                    Console.WriteLine(ex);
                    bd = null;
                }

                if (bd != null)
                    yield return bd;
            }
        }
    }

    private void TryWithTextPath(int id, Action<string> pathAction)
    {
        try
        {
            if (_txtDir != null)
            {
                var resultPath = Path.Combine(_txtDir, id + ".txt");
                pathAction(resultPath);
            }
        }
        catch
        {
            // These files are only for debugging anyway
        }
    }
}
