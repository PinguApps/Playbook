using System.IO.Compression;
using System.Net.Http.Headers;
using System.Text;

namespace Playbook.Samples;

internal static class ResponseChecks
{
    private static readonly string[] ExpectedCodings = ["br", "deflate", "gzip", "zstd"];
    private static readonly byte[] Payload = Encoding.UTF8.GetBytes(string.Concat(Enumerable.Repeat("Cairn survey 7: depth=42; report=accepted.\n", 100)));

    private static byte[] Encode(byte[] value, string format)
    {
        using MemoryStream output = new();
        using (Stream encoder = format switch
        {
#if NET11_0_OR_GREATER
            "zstd" => new ZstandardStream(output, new ZstandardCompressionOptions { AppendChecksum = true }, leaveOpen: true),
#else
            "zstd" => new ZstdSharp.CompressionStream(output, leaveOpen: true),
#endif
            "gzip" => new GZipStream(output, CompressionLevel.Optimal, leaveOpen: true),
            "deflate" => new ZLibStream(output, CompressionLevel.Optimal, leaveOpen: true),
            "br" => new BrotliStream(output, CompressionLevel.Optimal, leaveOpen: true),
            _ => throw new InvalidOperationException(format)
        }) encoder.Write(value);
        return output.ToArray();
    }

    public static async Task RunAsync()
    {
        using HttpClient client = CairnReports.Create();
        foreach (string? format in new string?[] { null, "zstd", "gzip", "deflate", "br" })
        {
            byte[] wire = format is null ? Payload : Encode(Payload, format);
            if (format == "zstd") Wire.Check(wire.Length < Payload.Length && Convert.ToHexString(wire.AsSpan(0, 4)) == "28B52FFD", "Not an actual compressed zstd frame.");
            await using HttpFixture fixture = new(wire, format);
            using HttpRequestMessage request = new(HttpMethod.Get, fixture.Address);
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("zstd"));
            using HttpResponseMessage response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            Wire.Check(response.Content.Headers.ContentEncoding.Count == 0, "Single encoding not removed.");
            Wire.Check(format is null ? response.Content.Headers.ContentLength == wire.Length : response.Content.Headers.ContentLength is null, "Wire length leaked after decoding.");
            Wire.Check(response.Content.Headers.ContentType?.MediaType == "application/octet-stream" && response.Headers.GetValues("X-Cairn").Single() == "survey-7", "Unrelated headers changed.");
            byte[] decoded = await response.Content.ReadAsByteArrayAsync();
            Wire.Check(decoded.SequenceEqual(Payload), "Decoded body mismatch.");
            string sent = await fixture.Request.Task;
            string accept = sent.Split("\r\n").Single(l => l.StartsWith("Accept-Encoding:", StringComparison.OrdinalIgnoreCase));
            string[] encodings = accept.Split(':', 2)[1].Split(',').Select(s => s.Trim()).ToArray();
            Wire.Check(encodings.Order().SequenceEqual(ExpectedCodings), "Missing or duplicate negotiated coding.");
            Console.WriteLine($"HTTP {format ?? "identity"}: {accept}; wire={wire.Length}, decoded={decoded.Length}; body and headers correct");
        }

        foreach ((string inner, string outer) in new[] { ("gzip", "zstd"), ("zstd", "gzip") })
        {
            byte[] innerBytes = Encode(Payload, inner);
            await using HttpFixture fixture = new(Encode(innerBytes, outer), $"{inner}, {outer}");
            using var response = await client.GetAsync(fixture.Address, HttpCompletionOption.ResponseHeadersRead);
            Wire.Check(response.Content.Headers.ContentEncoding.SequenceEqual(new[] { inner }) && response.Content.Headers.ContentLength is null, "Stacked header contract.");
            Wire.Check((await response.Content.ReadAsByteArrayAsync()).SequenceEqual(innerBytes), "Handler decoded more than the outer layer.");
            Console.WriteLine($"HTTP {inner}, {outer}: only outer layer removed; remaining {inner} bytes/header preserved");
        }
        foreach (string format in new[] { "cairn", "zstd, cairn" })
        {
            byte[] wire = Encode(Payload, "zstd");
            await using HttpFixture fixture = new(wire, format);
            using var response = await client.GetAsync(fixture.Address, HttpCompletionOption.ResponseHeadersRead);
            Wire.Check(response.Content.Headers.ContentEncoding.Count > 0 && response.Content.Headers.ContentLength == wire.Length, "Unknown encoding headers changed.");
            Wire.Check((await response.Content.ReadAsByteArrayAsync()).SequenceEqual(wire), "Unknown encoding decoded.");
            Console.WriteLine($"HTTP unsupported {format}: untouched encoded bytes and headers; caller must reject");
        }
        await using (HttpFixture fixture = new(Encoding.ASCII.GetBytes("invalid cairn frame"), "zstd"))
        {
            using var response = await client.GetAsync(fixture.Address, HttpCompletionOption.ResponseHeadersRead);
            try { await response.Content.ReadAsByteArrayAsync(); throw new InvalidOperationException("Malformed zstd accepted."); }
            catch (Exception exception) when (exception is HttpRequestException or InvalidDataException || exception.GetType().FullName == "ZstdSharp.ZstdException")
            { Console.WriteLine($"HTTP malformed zstd: rejected as {exception.GetType().FullName}"); }
        }
        await using (HttpFixture fixture = new(Encode(Payload, "zstd"), "zstd", stallBody: true))
        {
            using var response = await client.GetAsync(fixture.Address, HttpCompletionOption.ResponseHeadersRead);
            using CancellationTokenSource cancellation = new(TimeSpan.FromSeconds(5));
            Task<byte[]> read = response.Content.ReadAsByteArrayAsync(cancellation.Token);
            await cancellation.CancelAsync();
            try { await read; throw new InvalidOperationException("Cancelled body read completed."); }
            catch (OperationCanceledException) when (cancellation.IsCancellationRequested) { Console.WriteLine("HTTP stalled zstd body: cancellation observed; response disposed"); }
        }
        if (Environment.Version.Major == 10) Wire.Check(!Enum.GetNames<System.Net.DecompressionMethods>().Contains("Zstandard"), "Old framework unexpectedly includes Zstandard flag.");
        using CancellationTokenSource budget = new(TimeSpan.FromSeconds(10));
        await using (HttpFixture fixture = new(Encode(Payload, "zstd"), "zstd"))
            Wire.Check((await ReportReader.ReadAsync(client, fixture.Address, budget.Token)).SequenceEqual(Payload), "Canonical report reader mismatch.");
        await using (HttpFixture fixture = new(Encode(new byte[65537], "zstd"), "zstd"))
        {
            try { await ReportReader.ReadAsync(client, fixture.Address, budget.Token); throw new InvalidOperationException("Decoded budget ignored."); }
            catch (InvalidDataException) { }
        }
        await using (HttpFixture fixture = new(Payload, "cairn"))
        {
            try { await ReportReader.ReadAsync(client, fixture.Address, budget.Token); throw new InvalidOperationException("Unknown report accepted."); }
            catch (NotSupportedException) { }
        }
        Console.WriteLine("HTTP canonical reader: decoded report, 64 KiB budget and unknown-coding rejection passed");
        PrintArtifacts();
    }

    private static void PrintArtifacts()
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Where(a => a.GetName().Name is "ZstdSharp" or "System.IO.Compression" or "System.Net.Http"))
            Console.WriteLine($"LOADED {assembly.GetName().Name} {assembly.GetName().Version} {assembly.Location} sha256={Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(assembly.Location)))}");
        foreach (System.Diagnostics.ProcessModule module in System.Diagnostics.Process.GetCurrentProcess().Modules)
            if (module.ModuleName.Contains("Compression", StringComparison.OrdinalIgnoreCase))
                Console.WriteLine($"NATIVE {module.FileName} version={module.FileVersionInfo.ProductVersion} sha256={Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(module.FileName)))}");
    }
}
