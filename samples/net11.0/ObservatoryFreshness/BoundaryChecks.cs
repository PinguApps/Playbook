// snippet:complete
using System.Globalization;
using System.Text.Json;

static class BoundaryChecks
{
    public static void Run() {
        var now = DateTimeOffset.FromUnixTimeSeconds(2_000_000_000);
        foreach (var (seconds, expected) in new[] { (1_999_999_935L, true), (1_999_999_934L, false), (2_000_000_005L, true), (2_000_000_006L, false) }) {
            using var json = JsonDocument.Parse("{\"auth_time\":" + seconds.ToString(CultureInfo.InvariantCulture) + "}");
            if (Freshness.IsValid(json.RootElement, "60", now, TimeSpan.FromSeconds(5)) != expected)
                throw new InvalidOperationException("Inclusive clock-skew boundary");
        }
        foreach (var value in new[] { "-1", "1.5", " 60", "+60", "1,000", "9223372036854775808" })
            if (Freshness.TryMaxAge(value, out _)) throw new InvalidOperationException("Invalid max_age accepted");
        if (!Freshness.TryMaxAge("0", out var zero) || zero != 0) throw new InvalidOperationException("max_age=0");
        Console.WriteLine("PASS invariant parsing and inclusive past/future skew boundaries (deterministic clock)");
    }
}
// endsnippet:complete
