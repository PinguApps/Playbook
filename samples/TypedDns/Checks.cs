using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace Playbook.Samples;

internal static class Program
{
    internal static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    public static async Task Main()
    {
        AssertRuntimeBaseline();
        using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        await using var first = new AuthoritativeFixture(IPAddress.Loopback, "west", deadline.Token);
        await using var second = new AuthoritativeFixture(IPAddress.Parse("127.0.0.2"), "east", deadline.Token);
        using var inspector = new ZoneInspector(first.Endpoint);
        string suffix = Guid.NewGuid().ToString("N") + ".reedbank.test";
        foreach (RecordKind kind in Enum.GetValues<RecordKind>())
        {
            string name = kind == RecordKind.ReverseName ? "23.2.0.192.in-addr.arpa" : kind.ToString().ToLowerInvariant() + "." + suffix;
            ZoneAnswer answer = await inspector.QueryAsync(name, kind, deadline.Token);
            Console.WriteLine($"RECORD {kind} {string.Join(';', answer.Records.Select(r => string.Join('|', r.Values) + " ttl=" + r.Ttl.TotalSeconds))}");
            string[][] expected = kind switch
            {
                RecordKind.A => [["192.0.2.23"], ["192.0.2.24"]],
                RecordKind.Aaaa => [["2001:db8::23"], ["2001:db8::24"]],
                RecordKind.Mx => [["mail.reedbank.test", "17"]],
                RecordKind.Txt => [["station=west", "batch=41"]],
                RecordKind.CName => [["archive.reedbank.test"]],
                RecordKind.ReverseName => [["sensor.reedbank.test"]],
                RecordKind.Ns => [["ns.reedbank.test"]],
                _ => throw new InvalidOperationException(),
            };
            Require(answer.Status == "NoError" && answer.Records.Count == expected.Length, "Positive envelope/count: " + kind);
            int seconds = kind switch
            {
                RecordKind.A => 91, RecordKind.Aaaa => 118, RecordKind.Mx => 105, RecordKind.Txt => 106,
                RecordKind.CName => 95, RecordKind.ReverseName => 102, RecordKind.Ns => 92,
                _ => throw new InvalidOperationException(),
            };
            for (int i = 0; i < expected.Length; i++)
            {
                Require(answer.Records.Any(r => r.Values.SequenceEqual(expected[i]) && r.Ttl == TimeSpan.FromSeconds(seconds)), "Fields/TTL: " + kind);
            }
            Require(answer.NegativeTtl == TimeSpan.Zero, "Positive negative TTL.");
        }
        foreach (string prefix in new[] { "empty", "absent", "nosoa" })
        {
            ZoneAnswer answer = await inspector.QueryAsync(prefix + "." + suffix, RecordKind.Txt, deadline.Token);
            Require(answer.Status == (prefix == "absent" ? "NxDomain" : "NoError") && answer.Records.Count == 0, "Negative envelope.");
            int seconds = prefix == "nosoa" ? 0 : 45;
#if NET11_0_OR_GREATER
            if (OperatingSystem.IsWindows()) seconds = 0; // DnsQueryEx drops this authority SOA.
#endif
            Require(answer.NegativeTtl == TimeSpan.FromSeconds(seconds), "Negative TTL did not match observed platform contract.");
            string soa = prefix == "nosoa" ? "wire-soa-ttl=none minimum=none" : "wire-soa-ttl=120 minimum=45";
            Console.WriteLine($"NEGATIVE {prefix} {answer.Status} ttl={answer.NegativeTtl.TotalSeconds} {soa}");
        }
        using (var other = new ZoneInspector(second.Endpoint))
        {
            ZoneAnswer west = await inspector.QueryAsync("override." + suffix, RecordKind.Txt, deadline.Token);
            ZoneAnswer east = await other.QueryAsync("override." + suffix, RecordKind.Txt, deadline.Token);
            Require(west.Records.Single().Values[0] == "station=west" && east.Records.Single().Values[0] == "station=east", "Server override ignored.");
        }
        using (var canceled = new CancellationTokenSource())
        {
            await canceled.CancelAsync();
            try { await inspector.QueryAsync("precancel." + suffix, RecordKind.Txt, canceled.Token); throw new InvalidOperationException("Pre-cancel ignored."); }
            catch (OperationCanceledException) { }
        }
        using (var pending = new CancellationTokenSource())
        {
            Task<ZoneAnswer> query = inspector.QueryAsync("drop." + suffix, RecordKind.Txt, pending.Token);
            await first.Dropped.Task.WaitAsync(deadline.Token); // Cancel only after a real wire query arrives.
            await pending.CancelAsync();
            try { await query.WaitAsync(TimeSpan.FromSeconds(5)); throw new InvalidOperationException("In-flight cancel ignored."); }
            catch (OperationCanceledException) { }
        }
#if NET11_0_OR_GREATER
        if (OperatingSystem.IsWindows())
        {
            try { using var invalid = new ZoneInspector(new(IPAddress.Loopback, 5353)); throw new InvalidOperationException("Custom port accepted."); }
            catch (PlatformNotSupportedException) { }
            try { using var invalid = new DnsResolver(new DnsResolverOptions { Servers = [first.Endpoint, new(IPAddress.IPv6Loopback, 53)] }); throw new InvalidOperationException("Mixed family accepted."); }
            catch (ArgumentException) { }
        }
#endif
        Console.WriteLine($"PASS {Environment.Version} {System.Runtime.InteropServices.RuntimeInformation.OSDescription}; authoritative UDP endpoints={first.Endpoint},{second.Endpoint}; A/AAAA/MX/TXT/CNAME/PTR/NS values and TTLs; NODATA/NXDOMAIN/SOA absent; two-server override; pre/in-flight cancellation; no public DNS or machine configuration edits.");
    }

    private static void AssertRuntimeBaseline()
    {
        string coreLib = typeof(object).Assembly.Location;
        Console.WriteLine("RUNTIME " + JsonSerializer.Serialize(new
        {
            framework = RuntimeInformation.FrameworkDescription,
            coreLibPath = coreLib,
            coreLibVersion = FileVersionInfo.GetVersionInfo(coreLib).ProductVersion,
            coreLibSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(coreLib))).ToLowerInvariant(),
        }));
#if NET11_0_OR_GREATER
        const string expected = ".NET 11.0.0-rc.2.26504.105";
#else
        const string expected = ".NET 10.0.13";
#endif
        Require(RuntimeInformation.FrameworkDescription == expected, "Runtime baseline drift: " + RuntimeInformation.FrameworkDescription + "; expected " + expected);
    }
}
