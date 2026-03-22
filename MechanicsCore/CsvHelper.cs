using System.Globalization;
using System.Text;
using System.Xml;

namespace MechanicsCore;

public static class CsvHelper
{
    public static void WriteCsv(string csvPath, IEnumerable<IEnumerable<string>> rows)
    {
        using (var writer = File.CreateText(csvPath))
        {
            var sb = new StringBuilder();
            foreach (var row in rows)
            {
                sb.Clear();
                var appendCommaBeforeNextCell = false;
                foreach (var cellData in row)
                {
                    if (appendCommaBeforeNextCell)
                    {
                        sb.Append(',');
                    }

                    AppendCsvCell(sb, cellData);

                    appendCommaBeforeNextCell = true;
                }

                writer.WriteLine(sb);
            }
        }
    }

    /// <summary>
    /// If <paramref name="str"/> is null, appends nothing.
    /// Otherwise, appends <paramref name="str"/> wrapped in quotes and with any internal quotes escaped.
    /// Adapted from https://stackoverflow.com/a/6377656
    /// </summary>
    private static void AppendCsvCell(StringBuilder sb, string? str)
    {
        if (str == null)
        {
            return;
        }

        sb.Append('"');
        foreach (char nextChar in str)
        {
            sb.Append(nextChar);
            if (nextChar == '"')
            {
                sb.Append('"');
            }
        }
        sb.Append('"');
        XmlConvert.ToString(1);
    }

    public static string ToRoundTripString(this int value) => value.ToString(CultureInfo.InvariantCulture);
    public static int RoundTripStringToInt(this string str) => int.Parse(str, CultureInfo.InvariantCulture);

    public static string ToRoundTripString(this byte value) => value.ToString(CultureInfo.InvariantCulture);
    public static byte RoundTripStringToByte(this string str) => byte.Parse(str, CultureInfo.InvariantCulture);

    public static string ToRoundTripString(this double value) => value.ToString("G17", CultureInfo.InvariantCulture);
    public static double RoundTripStringToDouble(this string str) => double.Parse(str, CultureInfo.InvariantCulture);
}
