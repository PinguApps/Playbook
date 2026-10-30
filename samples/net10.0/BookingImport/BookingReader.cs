using System.Globalization;

namespace Playbook.Samples;

// snippet:reader
public sealed class BookingReader
{
    private readonly string[] dateFormats;
    private readonly string[] timeFormats;
    private readonly DateTimeStyles styles;

    public BookingReader(string[] dateFormats, string[] timeFormats, DateTimeStyles styles)
    {
        // This import contract accepts a small known set, not arbitrary .NET formats.
        if ((styles & ~DateTimeStyles.AllowWhiteSpaces) != 0)
            throw new ArgumentException("Only whitespace styles are supported.", nameof(styles));
        this.dateFormats = CopyFormats(dateFormats, ["yyyyMMdd", "dd'/'MM'/'yyyy"]);
        this.timeFormats = CopyFormats(timeFormats, ["HHmm", "HH':'mm"]);
        this.styles = styles;
    }

    private static string[] CopyFormats(string[] formats, string[] supported)
    {
        ArgumentNullException.ThrowIfNull(formats);
        string[] copy = (string[])formats.Clone();
        if (copy.Length == 0 || copy.Any(format => !supported.Contains(format, StringComparer.Ordinal)))
            throw new ArgumentException("Select at least one supported booking format.", nameof(formats));
        return copy;
    }

    public bool TryRead(string? day, string? time, out DateOnly date, out TimeOnly arrival) =>
        TryRead(day.AsSpan(), time.AsSpan(), out date, out arrival);

    public bool TryRead(ReadOnlySpan<char> day, ReadOnlySpan<char> time, out DateOnly date, out TimeOnly arrival)
    {
        bool validDate = DateOnly.TryParseExact(day, dateFormats, CultureInfo.InvariantCulture, styles, out date);
        bool validTime = TimeOnly.TryParseExact(time, timeFormats, CultureInfo.InvariantCulture, styles, out arrival);
        return validDate && validTime;
    }
}
// endsnippet:reader
