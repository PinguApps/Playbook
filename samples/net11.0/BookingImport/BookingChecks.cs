using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Playbook.Samples;

string[] dateSingle = ["yyyyMMdd"];
string[] timeSingle = ["HHmm"];
string[] dateMultiple = ["yyyyMMdd", "dd'/'MM'/'yyyy"];
string[] timeMultiple = ["HHmm", "HH':'mm"];
int checks = 0;
void Require(bool condition, string claim)
{
    if (!condition) throw new InvalidOperationException(claim);
    checks++;
}

void Observe(string claim, Func<bool> parse, string expected)
{
    string actual;
    try { actual = parse() ? "true" : "false"; }
    catch (ArgumentException error) { actual = error.GetType().Name; }
    catch (FormatException error) { actual = error.GetType().Name; }
    Console.WriteLine($"{claim}: {actual}");
    Require(actual == expected, $"{claim}: expected {expected}, got {actual}");
}

#if NET11_0_OR_GREATER
const string invalidStyle = "ArgumentException";
const string invalidFormat = "FormatException";
const string nullFormats = "ArgumentException";
#else
const string invalidStyle = "false";
const string invalidFormat = "false";
const string nullFormats = "false";
#endif

// All input here is non-null: string overloads can return before option validation for null data.
foreach (bool span in new[] { false, true })
{
    string entry = span ? "span" : "string";
    foreach (DateTimeStyles style in new[] { DateTimeStyles.AssumeUniversal, (DateTimeStyles)1024 })
    {
        Observe($"DateOnly {entry} TryParse invalid style {style}", () => span
            ? DateOnly.TryParse("20320417".AsSpan(), CultureInfo.InvariantCulture, style, out _)
            : DateOnly.TryParse("20320417", CultureInfo.InvariantCulture, style, out _), invalidStyle);
        Observe($"TimeOnly {entry} TryParse invalid style {style}", () => span
            ? TimeOnly.TryParse("1845".AsSpan(), CultureInfo.InvariantCulture, style, out _)
            : TimeOnly.TryParse("1845", CultureInfo.InvariantCulture, style, out _), invalidStyle);
        foreach (bool multiple in new[] { false, true })
        {
            Observe($"DateOnly {entry} exact multiple={multiple} invalid style", () => multiple
                ? (span ? DateOnly.TryParseExact("20320417".AsSpan(), dateSingle, CultureInfo.InvariantCulture, style, out _)
                        : DateOnly.TryParseExact("20320417", dateSingle, CultureInfo.InvariantCulture, style, out _))
                : (span ? DateOnly.TryParseExact("20320417".AsSpan(), "yyyyMMdd".AsSpan(), CultureInfo.InvariantCulture, style, out _)
                        : DateOnly.TryParseExact("20320417", "yyyyMMdd", CultureInfo.InvariantCulture, style, out _)), invalidStyle);
            Observe($"TimeOnly {entry} exact multiple={multiple} invalid style", () => multiple
                ? (span ? TimeOnly.TryParseExact("1845".AsSpan(), timeSingle, CultureInfo.InvariantCulture, style, out _)
                        : TimeOnly.TryParseExact("1845", timeSingle, CultureInfo.InvariantCulture, style, out _))
                : (span ? TimeOnly.TryParseExact("1845".AsSpan(), "HHmm".AsSpan(), CultureInfo.InvariantCulture, style, out _)
                        : TimeOnly.TryParseExact("1845", "HHmm", CultureInfo.InvariantCulture, style, out _)), invalidStyle);
        }
    }
    // Unsupported standard format Q fails as configuration on these binaries.
    Observe($"DateOnly {entry} exact unsupported standard", () => span
        ? DateOnly.TryParseExact("20320417".AsSpan(), "Q".AsSpan(), CultureInfo.InvariantCulture, DateTimeStyles.None, out _)
        : DateOnly.TryParseExact("20320417", "Q", CultureInfo.InvariantCulture, DateTimeStyles.None, out _), "FormatException");
    Observe($"TimeOnly {entry} exact unsupported standard", () => span
        ? TimeOnly.TryParseExact("1845".AsSpan(), "Q".AsSpan(), CultureInfo.InvariantCulture, DateTimeStyles.None, out _)
        : TimeOnly.TryParseExact("1845", "Q", CultureInfo.InvariantCulture, DateTimeStyles.None, out _), "FormatException");
    foreach (string?[]? formats in new string?[]?[] { null, [""], [null] })
    {
        Observe($"DateOnly {entry} exact invalid array", () => span
            ? DateOnly.TryParseExact("20320417".AsSpan(), formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out _)
            : DateOnly.TryParseExact("20320417", formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out _), formats is null ? nullFormats : invalidFormat);
        Observe($"TimeOnly {entry} exact invalid array", () => span
            ? TimeOnly.TryParseExact("1845".AsSpan(), formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out _)
            : TimeOnly.TryParseExact("1845", formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out _), formats is null ? nullFormats : invalidFormat);
    }
    Observe($"DateOnly {entry} exact malformed custom", () => span
        ? DateOnly.TryParseExact("20320417".AsSpan(), "yyyyMMdd\\".AsSpan(), CultureInfo.InvariantCulture, DateTimeStyles.None, out _)
        : DateOnly.TryParseExact("20320417", "yyyyMMdd\\", CultureInfo.InvariantCulture, DateTimeStyles.None, out _), "false");
    Observe($"TimeOnly {entry} exact malformed custom", () => span
        ? TimeOnly.TryParseExact("1845".AsSpan(), "HHmm\\".AsSpan(), CultureInfo.InvariantCulture, DateTimeStyles.None, out _)
        : TimeOnly.TryParseExact("1845", "HHmm\\", CultureInfo.InvariantCulture, DateTimeStyles.None, out _), "false");
    string[] dateUnchecked = ["yyyyMMdd", "Q"];
    string[] timeUnchecked = ["HHmm", "Q"];
    Observe($"DateOnly {entry} matching first format skips Q", () => span
        ? DateOnly.TryParseExact("20320417".AsSpan(), dateUnchecked, CultureInfo.InvariantCulture, DateTimeStyles.None, out _)
        : DateOnly.TryParseExact("20320417", dateUnchecked, CultureInfo.InvariantCulture, DateTimeStyles.None, out _), "true");
    Observe($"TimeOnly {entry} matching first format skips Q", () => span
        ? TimeOnly.TryParseExact("1845".AsSpan(), timeUnchecked, CultureInfo.InvariantCulture, DateTimeStyles.None, out _)
        : TimeOnly.TryParseExact("1845", timeUnchecked, CultureInfo.InvariantCulture, DateTimeStyles.None, out _), "true");
    foreach (string input in new[] { "17/04/2032", "not-a-day", "31/02/2032" })
    {
        bool valid = input == "17/04/2032";
        Observe($"DateOnly {entry} single {input}", () => span
            ? DateOnly.TryParseExact(input.AsSpan(), "dd'/'MM'/'yyyy".AsSpan(), CultureInfo.InvariantCulture, DateTimeStyles.None, out _)
            : DateOnly.TryParseExact(input, "dd'/'MM'/'yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out _), valid ? "true" : "false");
        Observe($"DateOnly {entry} multiple {input}", () => span
            ? DateOnly.TryParseExact(input.AsSpan(), dateMultiple, CultureInfo.InvariantCulture, DateTimeStyles.None, out _)
            : DateOnly.TryParseExact(input, dateMultiple, CultureInfo.InvariantCulture, DateTimeStyles.None, out _), valid ? "true" : "false");
    }
    foreach (string input in new[] { "18:45", "not-a-time", "25:61" })
    {
        bool valid = input == "18:45";
        Observe($"TimeOnly {entry} single {input}", () => span
            ? TimeOnly.TryParseExact(input.AsSpan(), "HH':'mm".AsSpan(), CultureInfo.InvariantCulture, DateTimeStyles.None, out _)
            : TimeOnly.TryParseExact(input, "HH':'mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out _), valid ? "true" : "false");
        Observe($"TimeOnly {entry} multiple {input}", () => span
            ? TimeOnly.TryParseExact(input.AsSpan(), timeMultiple, CultureInfo.InvariantCulture, DateTimeStyles.None, out _)
            : TimeOnly.TryParseExact(input, timeMultiple, CultureInfo.InvariantCulture, DateTimeStyles.None, out _), valid ? "true" : "false");
    }
    Observe($"DateOnly {entry} TryParse valid", () => span ? DateOnly.TryParse("2032-04-17".AsSpan(), CultureInfo.InvariantCulture, DateTimeStyles.None, out _) : DateOnly.TryParse("2032-04-17", CultureInfo.InvariantCulture, DateTimeStyles.None, out _), "true");
    Observe($"TimeOnly {entry} TryParse valid", () => span ? TimeOnly.TryParse("18:45".AsSpan(), CultureInfo.InvariantCulture, DateTimeStyles.None, out _) : TimeOnly.TryParse("18:45", CultureInfo.InvariantCulture, DateTimeStyles.None, out _), "true");
    Observe($"DateOnly {entry} TryParse bad data", () => span ? DateOnly.TryParse("bad".AsSpan(), CultureInfo.InvariantCulture, DateTimeStyles.None, out _) : DateOnly.TryParse("bad", CultureInfo.InvariantCulture, DateTimeStyles.None, out _), "false");
    Observe($"TimeOnly {entry} TryParse bad data", () => span ? TimeOnly.TryParse("bad".AsSpan(), CultureInfo.InvariantCulture, DateTimeStyles.None, out _) : TimeOnly.TryParse("bad", CultureInfo.InvariantCulture, DateTimeStyles.None, out _), "false");
}
Observe("DateOnly null string short-circuits invalid style", () => DateOnly.TryParse(null, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out _), "false");
Observe("TimeOnly null string short-circuits invalid style", () => TimeOnly.TryParse(null, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out _), "false");

// snippet:usage
string[] days = ["yyyyMMdd", "dd'/'MM'/'yyyy"];
BookingReader reader = new(days, ["HHmm", "HH':'mm"], DateTimeStyles.AllowWhiteSpaces);
bool accepted = reader.TryRead("20320417", "1845", out DateOnly bookingDate, out TimeOnly bookingTime);
// endsnippet:usage
Require(accepted && bookingDate == new DateOnly(2032, 4, 17) && bookingTime == new TimeOnly(18, 45), "configured booking caller");
days[0] = "broken"; // Configured readers own their format arrays.
Require(reader.TryRead(" 20320417 ", "1845", out DateOnly date, out TimeOnly time) && date == new DateOnly(2032, 4, 17) && time == new TimeOnly(18, 45), "booking values and whitespace");
Require(reader.TryRead("17/04/2032".AsSpan(), "18:45".AsSpan(), out _, out _), "alternate booking format");
Require(!reader.TryRead("31/02/2032", "18:45", out _, out _), "bad day is row rejection");
Require(!reader.TryRead("17/04/2032", "25:61", out _, out _), "bad time is row rejection");
Require(!reader.TryRead(null, null, out _, out _), "missing fields reject");
Observe("configuration invalid style", () => { _ = new BookingReader(["yyyyMMdd"], ["HHmm"], DateTimeStyles.AssumeLocal); return true; }, "ArgumentException");
Observe("configuration invalid format", () => { _ = new BookingReader(["yyyyMMdd\\"], ["HHmm"], DateTimeStyles.None); return true; }, "ArgumentException");
Observe("configuration empty formats", () => { _ = new BookingReader([], ["HHmm"], DateTimeStyles.None); return true; }, "ArgumentException");
foreach (Type type in new[] { typeof(DateOnly), typeof(TimeOnly) })
    Require(!type.GetCustomAttributesData().Any(attribute => attribute.AttributeType.Name == "ExperimentalAttribute"), $"no experimental type opt-in {type}");
Assembly core = typeof(DateOnly).Assembly;
Console.WriteLine($"ARTIFACT {core.GetName()} sha256={Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(core.Location)))}");
Console.WriteLine($"PASS {checks} booking checks; runtime={RuntimeInformation.FrameworkDescription}; OS={RuntimeInformation.OSDescription}; arch={RuntimeInformation.ProcessArchitecture}; invariant={Environment.GetEnvironmentVariable("DOTNET_SYSTEM_GLOBALIZATION_INVARIANT") ?? "unset"}; culture={CultureInfo.CurrentCulture.Name}; invariant provider explicitly used.");
