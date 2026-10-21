// snippet:utility
using System.Globalization;

string[] rows = args.Length == 0 ? ["east:3", "west:5"] : args;
int total = 0;
foreach (string row in rows)
{
    string[] cells = row.Split(':');
    if (cells.Length != 2 || cells[0].Length == 0 ||
        !int.TryParse(cells[1], NumberStyles.None, CultureInfo.InvariantCulture, out int count))
    {
        throw new ArgumentException("Expected a named rack and a nonnegative slot count.");
    }

    total = checked(total + count);
}

Console.WriteLine($"slots={total}");
// endsnippet:utility
