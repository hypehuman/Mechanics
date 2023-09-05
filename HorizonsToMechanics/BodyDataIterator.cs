using System.Text.Json;

namespace HorizonsToMechanics;

/// <summary>
/// List of APIs: https://ssd.jpl.nasa.gov/api.html
/// </summary>
public partial class BodyDataIterator
{
    public static IEnumerable<BodyData> IterateObjects(string outPath)
    {
        if (!Directory.Exists(outPath))
        {
            throw new Exception("Directory does not exist: " + Path.GetFullPath(outPath));
        }

        var time = new DateTime(2023, 08, 12, 0, 0, 0, DateTimeKind.Utc);
        using (var querier = new HorizonsQuerier())
        {
            for (var id = -2; id < 623829; id++)
            {
                var contentPath = Path.Combine(outPath, id + ".json");

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
                    Console.WriteLine($"Failed to get data for object {id}: ");
                    Console.WriteLine(ex);
                    continue;
                }

                yield return BodyDataParser.ParseBodyData(id, responseObject);
                Console.WriteLine();
            }
        }
    }
}
