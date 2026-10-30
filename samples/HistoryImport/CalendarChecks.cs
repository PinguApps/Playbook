using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Playbook.Samples;

Console.WriteLine($"runtime={RuntimeInformation.FrameworkDescription}; os={RuntimeInformation.OSDescription}; arch={RuntimeInformation.ProcessArchitecture}; culture={CultureInfo.CurrentCulture.Name}");
// Reflection records the measured runtime mode, not an application dependency.
var mode = typeof(CultureInfo).Assembly.GetType("System.Globalization.GlobalizationMode", throwOnError: true)!;
foreach (var name in new[] { "Invariant", "UseNls" })
    Console.WriteLine($"globalization {name}={mode.GetProperty(name, BindingFlags.Static | BindingFlags.NonPublic)?.GetValue(null) ?? "not-exposed"}");
var japaneseCulture = CultureInfo.GetCultureInfo("ja-JP");
var calendar = new JapaneseCalendar(); // A missing globalization capability must fail this check.
Console.WriteLine($"culture={japaneseCulture.Name}; calendar={calendar.GetType().Name}; minimum={calendar.MinSupportedDateTime:yyyy-MM-dd}");
foreach (var type in new[] { typeof(JapaneseCalendar) })
{
    var path = type.Assembly.Location;
    Console.WriteLine($"RUNTIME ARTIFACT {path} sha256={Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)))}");
}
#if NET11_0
var expectedMinimum = new DateTime(1868, 10, 23);
#else
var expectedMinimum = new DateTime(1868, 9, 8);
#endif
Check(calendar.MinSupportedDateTime == expectedMinimum, "actual calendar minimum");
foreach (var text in new[] { "1868-09-08", "1868-10-01", "1868-10-22", "1868-10-23" })
{
    var original = DateTime.ParseExact(text, "yyyy-MM-dd", CultureInfo.InvariantCulture);
    var supported = original >= expectedMinimum;
    var result = CalendarPolicy.Import(original, calendar);
    Check(result.GregorianDate == original && result.GregorianDate.Kind == original.Kind, $"preserve stored date {text}");
    Check(result.Outcome == (supported ? "JapaneseDate" : "RetainGregorian"), $"explicit routing {text}");
    if (supported)
    {
        Check(calendar.GetYear(original) == 1 && calendar.GetEra(original) == 1, $"real calendar operation {text}");
        Check(result.EraYear == 1 && result.Era == 1, $"imported era/year {text}");
    }
    else
    {
        Check(result.Era is null && result.EraYear is null, $"no invented Japanese date {text}");
        try { calendar.GetYear(original); throw new InvalidOperationException($"Calendar accepted {text}"); }
        catch (ArgumentOutOfRangeException) { Console.WriteLine($"PASS real calendar rejection {text}"); }
    }
    Console.WriteLine($"INPUT {text}; outcome={result.Outcome}; stored={result.GregorianDate:O}; era={result.Era}; year={result.EraYear}");
}
var belowOldMinimum = new DateTime(1868, 9, 7);
Check(CalendarPolicy.Import(belowOldMinimum, calendar).Outcome == "RetainGregorian", "older unsupported date remains Gregorian");
Check(CalendarPolicy.Import(calendar.MaxSupportedDateTime, calendar).Outcome == "JapaneseDate", "inclusive maximum");
Console.WriteLine("PASS historical-record importer boundaries and value preservation");

static void Check(bool condition, string name)
{
    if (!condition) throw new InvalidOperationException(name);
    Console.WriteLine($"PASS {name}");
}
