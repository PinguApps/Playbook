using System.Globalization;

namespace Playbook.Samples;

// snippet:policy
public sealed record ArchiveDate(DateTime GregorianDate, int? Era, int? EraYear, string Outcome);

public static class CalendarPolicy
{
    public static ArchiveDate Import(DateTime storedDate, JapaneseCalendar calendar)
    {
        // Preserve the original Gregorian value even when the calendar cannot represent it.
        if (storedDate < calendar.MinSupportedDateTime || storedDate > calendar.MaxSupportedDateTime)
            return new(storedDate, null, null, "RetainGregorian");

        return new(storedDate, calendar.GetEra(storedDate), calendar.GetYear(storedDate), "JapaneseDate");
    }
}
// endsnippet:policy
