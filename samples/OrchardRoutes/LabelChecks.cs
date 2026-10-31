using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using OrchardRoutes;

Console.WriteLine($"RUNTIME {RuntimeInformation.FrameworkDescription} / {RuntimeInformation.OSDescription}");
// The projects explicitly select non-invariant Windows ICU. Evidence records the
// loaded native DLLs/data version rather than equating CurrentCulture with IDNA policy.
Check(OperatingSystem.IsWindows(), "declared Windows ICU route");
Check(AppContext.TryGetSwitch("System.Globalization.Invariant", out bool invariant) && !invariant, "non-invariant configured");
Check(AppContext.TryGetSwitch("System.Globalization.UseNls", out bool nls) && !nls, "ICU configured");
Console.WriteLine($"GLOBALIZATION invariant={invariant}; useNls={nls}; culture={CultureInfo.CurrentCulture.Name}");
var mapping = new IdnMapping { UseStd3AsciiRules = true, AllowUnassigned = false };
foreach ((string unicode, string ascii) in new[] { ("mélange", "xn--mlange-bva"), ("bücher", "xn--bcher-kva"), ("東京", "xn--1lqs71d") })
{
    char[] encoded = new char[ascii.Length];
    Check(DomainLabels.Convert(mapping, unicode, encoded, true, out int count) && count == ascii.Length && new string(encoded) == ascii, "ASCII round trip input");
    char[] decoded = new char[unicode.Length];
    Check(DomainLabels.Convert(mapping, ascii, decoded, false, out count) && count == unicode.Length && new string(decoded) == unicode, "Unicode round trip output");
    foreach (bool toAscii in new[] { true, false })
    {
        string input = toAscii ? unicode : ascii;
        int size = toAscii ? ascii.Length : unicode.Length;
        foreach (int capacity in new[] { 0, size - 1 })
        {
            char[] small = new char[capacity];
            Check(!DomainLabels.Convert(mapping, input, small, toAscii, out count) && count == 0, "short buffer false/zero");
        }
    }
    Console.WriteLine($"ROUNDTRIP {unicode} -> {ascii} -> {unicode}; exact/short/empty buffers");
}
foreach (string invalid in new[] { "", "a..b", "bad_label", "-orchard", "a\0b", "\uD800", new string('x', 64) })
    ExpectArgument(() => DomainLabels.Convert(mapping, invalid, new char[256], true, out _));
foreach (string invalid in new[] { "xn--", "a..b", "bad_label", "a\0" })
    ExpectArgument(() => DomainLabels.Convert(mapping, invalid, new char[256], false, out _));

var relaxed = new IdnMapping { UseStd3AsciiRules = false, AllowUnassigned = false };
Check(DomainLabels.Convert(relaxed, "orchard_route", new char[64], true, out int relaxedCount) && relaxedCount == 13, "relaxed underscore policy");
foreach (bool allow in new[] { false, true })
{
    var policy = new IdnMapping { UseStd3AsciiRules = true, AllowUnassigned = allow };
    // ICU's IDNA implementation may ignore this legacy option. Record the real
    // U+0378 outcome for each choice; do not invent an acceptance guarantee.
    try
    {
        char[] output = new char[128];
        bool success = DomainLabels.Convert(policy, "sprout\u0378", output, true, out int count);
        Console.WriteLine($"UNASSIGNED U+0378 allow={allow}: success={success} result={new string(output, 0, count)}");
    }
    catch (ArgumentException) { Console.WriteLine($"UNASSIGNED U+0378 allow={allow}: ArgumentException"); }
    Check(DomainLabels.Convert(policy, "bücher", new char[64], true, out _), "assigned label under both options");
}
#if NET11_0_OR_GREATER
char[] overlapping = "bücher................".ToCharArray();
ExpectArgument(() => DomainLabels.Convert(mapping, overlapping.AsSpan(0, 6), overlapping, true, out _));
ExpectArgument(() => DomainLabels.Convert(mapping, overlapping.AsSpan(0, 6), overlapping, false, out _));
#endif
var sort = CultureInfo.GetCultureInfo("en-GB").CompareInfo.Version;
Console.WriteLine($"GLOBALIZATION sortVersion={sort.FullVersion}; sortId={sort.SortId}");
var native = Process.GetCurrentProcess().Modules.Cast<ProcessModule>().Where(m => m.ModuleName.Contains("icu", StringComparison.OrdinalIgnoreCase)).ToArray();
Check(native.Length > 0, "actual ICU module loaded");
foreach (var module in native)
    Console.WriteLine($"NATIVE {module.FileName} version={module.FileVersionInfo.FileVersion} sha256={Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(module.FileName))).ToLowerInvariant()}");
Console.WriteLine("PASS IDN: Unicode/ASCII vectors, exact/short buffers, invalid labels, STD3, both AllowUnassigned choices, actual Windows ICU");

static void Check(bool value, string name) { if (!value) throw new InvalidOperationException(name); }
static void ExpectArgument(Action action)
{
    try { action(); }
    catch (ArgumentException) { return; }
    throw new InvalidOperationException("Expected ArgumentException");
}
