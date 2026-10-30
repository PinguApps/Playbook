using System.Globalization;

namespace Playbook.Samples;

// snippet:policy
public sealed record ArchiveDate(DateTime GregorianDate, int? Era, int? EraYear, string Outcome);

public static class CalendarPolicy
{
    public static ArchiveDate Import(DateTime storedDate, JapaneseCalendar calendar)
    {
        // Query this runtime's boundary; never clamp or rewrite historical source data.
        if (storedDate < calendar.MinSupportedDateTime || storedDate > calendar.MaxSupportedDateTime)
            return new(storedDate, null, null, "RetainGregorian");

        return new(storedDate, calendar.GetEra(storedDate), calendar.GetYear(storedDate), "JapaneseDate");
    }
}
// endsnippet:policy
