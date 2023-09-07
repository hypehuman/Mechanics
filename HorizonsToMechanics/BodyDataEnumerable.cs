using System.Text.Json;

namespace HorizonsToMechanics;

/// <summary>
/// List of APIs: https://ssd.jpl.nasa.gov/api.html
/// </summary>
public partial class BodyDataEnumerable : IAsyncEnumerable<BodyData>
{
    private readonly string? _jsonDir;
    private readonly string? _txtDir;

    public BodyDataEnumerable(string? jsonDir, string? txtDir)
    {
        _jsonDir = jsonDir;
        _txtDir = txtDir;
    }

    public async IAsyncEnumerator<BodyData> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(_jsonDir))
        {
            throw new Exception("Directory does not exist: " + (_jsonDir == null ? "" : Path.GetFullPath(_jsonDir)));
        }

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

                try
                {
                    if (_txtDir != null && responseObject.result != null)
                    {
                        var resultPath = Path.Combine(_txtDir, id + ".txt");
                        if (!File.Exists(resultPath))
                        {
                            // for debugging, since the newlines in the string make things hard to read
                            File.WriteAllText(resultPath, responseObject.result);
                        }
                    }
                }
                catch
                {
                    // These files are only for debugging anyway
                }

                if (responseObject.error != null)
                {
                    Console.WriteLine($"Object {id}: Response has error:");
                    Console.WriteLine(responseObject.error);
                    continue;
                }

                BodyData? bd;
                try
                {
                    bd = BodyDataParser.ParseBodyData(id, responseObject);
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
}
