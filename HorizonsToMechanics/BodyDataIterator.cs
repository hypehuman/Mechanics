using System.Text.Json;

namespace HorizonsToMechanics;

/// <summary>
/// List of APIs: https://ssd.jpl.nasa.gov/api.html
/// </summary>
public partial class BodyDataIterator
{
    public static IEnumerable<BodyData> IterateObjects(string? jsonDir, string? txtDir)
    {
        if (!Directory.Exists(jsonDir))
        {
            throw new Exception("Directory does not exist: " + Path.GetFullPath(jsonDir));
        }

        var time = new DateTime(2023, 08, 12, 0, 0, 0, DateTimeKind.Utc);
        using (var querier = new HorizonsQuerier())
        {
            for (var id = -2; id < 623829; id++)
            {
                var contentPath = Path.Combine(jsonDir, id + ".json");

                HorizonsResponseContent responseObject;
                try
                {
                    if (File.Exists(contentPath))
                    {
                        using var fileStream = File.OpenRead(contentPath);
                        responseObject = JsonSerializer.Deserialize<HorizonsResponseContent>(fileStream)
                        ?? throw new NullReferenceException("The serializer returned null.");
                    }
                    else
                    {
                        var response = querier.Get(id, time).Result;
                        if (response.IsSuccessStatusCode)
                        {
                            responseObject = response.Content.ReadAsAsync<HorizonsResponseContent>().Result;
                        }
                        else
                        {
                            throw new Exception($"{(int)response.StatusCode} ({response.ReasonPhrase})");
                        }
                        using (var fileStream = File.Create(contentPath))
                        {
                            response.Content.CopyToAsync(fileStream).Wait();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Object {id}: Failed to get response:");
                    Console.WriteLine(ex);
                    continue;
                }

                try
                {
                    if (txtDir != null && responseObject.result != null)
                    {
                        var resultPath = Path.Combine(txtDir, id + ".txt");
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
