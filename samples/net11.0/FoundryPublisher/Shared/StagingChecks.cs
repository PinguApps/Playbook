using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http.Headers;
using System.Reflection;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;

namespace Foundry.Staging;

internal static class StagingChecks
{
    public static async Task Run(string sdkRoot)
    {
        CheckArtifacts(sdkRoot);
        string root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../.."));
        string fixture = Path.Combine(root, "artifacts", "foundry-staging", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(fixture);
        try
        {
            // Pack real original library/PDB inputs under this consumer's actual SDK.
            string project = Path.Combine(root, "samples", "net11.0", "FoundryPublisher", "Shared", "Alloy", "Alloy.csproj");
            var start = new ProcessStartInfo(Environment.ProcessPath!) { WorkingDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../..")), RedirectStandardOutput = true, RedirectStandardError = true };
            foreach (string arg in new[] { "pack", project, "-c", "Release", "-o", fixture, "-p:FoundryTarget=" + (Environment.Version.Major == 11 ? "net11.0" : "net10.0"), "-p:UseSharedCompilation=false", "-p:NuGetAudit=false" }) start.ArgumentList.Add(arg);
            start.Environment["DOTNET_ADD_GLOBAL_TOOLS_TO_PATH"] = "0";
            start.Environment["MSBUILDUSESERVER"] = "0";
            using var child = Process.Start(start)!;
            Task<string> stdout = child.StandardOutput.ReadToEndAsync(), stderr = child.StandardError.ReadToEndAsync();
            await child.WaitForExitAsync();
            Require(child.ExitCode == 0, "Actual pack failed: " + await stdout + await stderr);
            Console.WriteLine("COMMAND dotnet pack samples/net11.0/FoundryPublisher/Shared/Alloy/Alloy.csproj -c Release -o <fixture> -p:FoundryTarget=net" + Environment.Version.Major + ".0 -p:UseSharedCompilation=false -p:NuGetAudit=false");
            string package = Directory.GetFiles(fixture, "*.nupkg").Single();
            string symbols = Directory.GetFiles(fixture, "*.snupkg").Single();
            using (var zip = await ZipFile.OpenReadAsync(package)) Require(zip.Entries.Any(e => e.FullName.EndsWith("Alloy.dll", StringComparison.Ordinal)), "No real library");
            using (var zip = await ZipFile.OpenReadAsync(symbols))
            {
                var pdb = zip.Entries.Single(e => e.FullName.EndsWith(".pdb", StringComparison.Ordinal));
                using var stream = await pdb.OpenAsync();
                using var memory = new MemoryStream();
                await stream.CopyToAsync(memory);
                memory.Position = 0;
                using var provider = MetadataReaderProvider.FromPortablePdbStream(memory);
                Require(provider.GetMetadataReader().Documents.Count > 0, "No original portable PDB documents");
            }
            Console.WriteLine("PASS actual SDK pack produced original library nupkg and document-bearing portable-PDB snupkg.");
            await using var server = new WireServer();
            // A generated fixture-only marker checks header transport. Never log it.
            string marker = Convert.ToHexString(RandomNumberGenerator.GetBytes(24));
            await Reject<InvalidOperationException>(() => Task.FromResult(Publisher.Open("http://example.invalid/index.json", true)));
            await Reject<InvalidOperationException>(() => Task.FromResult(Publisher.Open("https://example.invalid/index.json", true)));
            var repository = Publisher.Open(server.Origin + "/index.json", allowLoopbackHttp: true);
#if NET11_0
            var resource = await repository.GetResourceAsync<PackageStagingResourceV3>(CancellationToken.None);
            Require(resource is not null && resource.SourceUri.OriginalString == server.Endpoint, "Advertised endpoint was changed");
            await Publisher.Stage(repository, package, symbols, marker, "bronze-42", true, CancellationToken.None);
            Check(server.Uploads.ToArray()[0], package, "/foundry/batches/package?lane=bronze", "package", marker, "bronze-42", chunked: true);
            Check(server.Uploads.ToArray()[1], symbols, "/foundry/batches/symbols?lane=bronze", "symbols", marker, "bronze-42", chunked: true);
            await Publisher.Stage(repository, package, symbols, null, null, true, CancellationToken.None);
            Check(server.Uploads.ToArray()[2], package, "/foundry/batches/package?lane=bronze", "package", null, null, chunked: true);
            Check(server.Uploads.ToArray()[3], symbols, "/foundry/batches/symbols?lane=bronze", "symbols", null, null, chunked: true);
            // Empty is non-null: the protocol sends an empty groupId part.
            await resource!.PushPackageAsync(package, null, "", TimeSpan.FromSeconds(5), true, NullLogger.Instance, CancellationToken.None);
            Check(server.Uploads.ToArray()[4], package, "/foundry/batches/package?lane=bronze", "package", null, "", chunked: true);
            int before = server.Uploads.Count;
            await Reject<FatalProtocolException>(() => resource.PushPackageAsync(package, marker, null, TimeSpan.FromSeconds(5), false, NullLogger.Instance, CancellationToken.None));
            Require(server.Uploads.Count == before, "Default HTTP refusal transmitted a package");
            Console.WriteLine("PASS discovered exact SourceUri; real PUT chunked multipart package/symbols bytes, filename, content type, query retained; group present/null/empty and API-key present/absent; default HTTP refused before upload.");
            var missing = await Publisher.Open(server.Origin + "/missing.json", true).GetResourceAsync<PackageStagingResourceV3>(CancellationToken.None);
            Require(missing is null, "Unsupported staging version accepted");
            await Reject<InvalidOperationException>(() => Publisher.Stage(Publisher.Open(server.Origin + "/missing-publisher.json", true), package, symbols, null, null, true, CancellationToken.None));
            Require(server.Uploads.Count == before, "Missing resource sent data");
            await Reject<InvalidOperationException>(() => Publisher.Stage(Publisher.Open(server.Origin + "/alien.json", true), package, symbols, marker, null, true, CancellationToken.None));
            Require(server.Uploads.Count == before, "Non-fixture discovered endpoint sent data");
            server.Status = 400;
            await Reject<HttpRequestException>(() => resource.PushPackageAsync(package, null, null, TimeSpan.FromSeconds(5), true, NullLogger.Instance, CancellationToken.None));
            server.Status = 201;
            server.Hold = true;
            server.ResetSignal();
            using (var cancel = new CancellationTokenSource())
            {
                Task pending = resource.PushSymbolsAsync(symbols, null, null, TimeSpan.FromSeconds(30), true, NullLogger.Instance, cancel.Token);
                await server.Arrived.Task.WaitAsync(TimeSpan.FromSeconds(10));
                await cancel.CancelAsync();
                await Reject<OperationCanceledException>(() => pending.WaitAsync(TimeSpan.FromSeconds(10)));
            }
            // Cancellation interrupts a request the real fixture has fully received.
            server.ResetSignal();
            using (var bound = new CancellationTokenSource(TimeSpan.FromSeconds(25)))
            {
                Task pending = resource.PushPackageAsync(package, null, null, TimeSpan.FromMilliseconds(150), true, NullLogger.Instance, bound.Token);
                await server.Arrived.Task.WaitAsync(TimeSpan.FromSeconds(10));
                await Reject<TimeoutException>(() => pending);
                Require(!bound.IsCancellationRequested, "Outer bound masked request timeout");
            }
            Console.WriteLine("PASS unsupported staging version/missing resource and non-fixture advertised endpoint prevent upload; HTTP400 rejected; received symbols request cancelled; received package request timed out independently of outer cancellation.");
#else
            Require(typeof(SourceRepository).Assembly.GetType("NuGet.Protocol.PackageStagingResourceV3") is null, "Unexpected older staging API");
            await Publisher.Publish(repository, package, marker, CancellationToken.None, allowLoopbackHttp: true);
            var receipt = server.Uploads.ToArray().Single();
            Check(receipt, package, "/published/", "package", marker, null, chunked: true, filename: "package.nupkg");
            server.Status = 400;
            await Reject<HttpRequestException>(() => Publisher.Publish(repository, package, marker, CancellationToken.None, allowLoopbackHttp: true));
            Console.WriteLine("PASS actual SDK10 lacks staging API; ordinary PackagePublish/2.0.0 discovery and real chunked multipart publication work; HTTP400 rejected. Fixed package.nupkg filename, no group metadata or staging equivalence.");
#endif
            // Identify every SDK assembly actually loaded; compare its exact file to pins.
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic && a.Location.StartsWith(sdkRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)).OrderBy(a => a.GetName().Name, StringComparer.Ordinal))
                Console.WriteLine($"LOADED {assembly.GetName().FullName} tfm={assembly.GetCustomAttribute<System.Runtime.Versioning.TargetFrameworkAttribute>()?.FrameworkName} product={FileVersionInfo.GetVersionInfo(assembly.Location).ProductVersion} sha256={Hash(assembly.Location)}");
        }
        finally { Directory.Delete(fixture, recursive: true); }
    }

    private static void Check(Receipt receipt, string file, string route, string field, string? key, string? group, bool chunked, string? filename = null)
    {
        Require(receipt.Method == "PUT" && receipt.Route == route, "Method/endpoint path or query changed: " + receipt.Method + " " + receipt.Route);
        if (chunked) Require(receipt.Headers.GetValueOrDefault("Transfer-Encoding") == "chunked", "Not actual chunked transport");
        Require(key is null ? !receipt.Headers.ContainsKey("X-NuGet-ApiKey") : receipt.Headers.GetValueOrDefault("X-NuGet-ApiKey") == key, "API-key transport mismatch (redacted)");
        string type = receipt.Headers["Content-Type"];
        string boundary = type.Split("boundary=", 2)[1].Trim('"');
        string body = Encoding.Latin1.GetString(receipt.Body.Span);
        string[] parts = body.Split("--" + boundary, StringSplitOptions.None);
        Require(type.StartsWith("multipart/form-data;", StringComparison.Ordinal) && parts.Length == (group is null ? 3 : 4), "Multipart shape");
        string part = parts[1];
        int headerEnd = part.IndexOf("\r\n\r\n", StringComparison.Ordinal);
        string header = part[..headerEnd];
        filename ??= Path.GetFileName(file);
        string disposition = header.Split("\r\n", StringSplitOptions.RemoveEmptyEntries).Single(h => h.StartsWith("Content-Disposition:", StringComparison.OrdinalIgnoreCase));
        var parsed = ContentDispositionHeaderValue.Parse(disposition[(disposition.IndexOf(':', StringComparison.Ordinal) + 1)..].Trim());
        Require(parsed.DispositionType == "form-data" && parsed.Name?.Trim('"') == field, "Wrong file field");
        Require(parsed.FileName?.Trim('"') == filename, "Expected filename missing");
        Require(header.Contains("Content-Type: application/octet-stream", StringComparison.OrdinalIgnoreCase), "Wrong file media type");
        byte[] actual = Encoding.Latin1.GetBytes(part[(headerEnd + 4)..^2]);
        Require(actual.AsSpan().SequenceEqual(File.ReadAllBytes(file)), "Multipart bytes differ from actual packed artifact");
        if (group is not null)
        {
            string groupPart = parts[2];
            string groupDisposition = groupPart.Split("\r\n", StringSplitOptions.RemoveEmptyEntries).Single(h => h.StartsWith("Content-Disposition:", StringComparison.OrdinalIgnoreCase));
            var parsedGroup = ContentDispositionHeaderValue.Parse(groupDisposition[(groupDisposition.IndexOf(':', StringComparison.Ordinal) + 1)..].Trim());
            Require(parsedGroup.DispositionType == "form-data" && parsedGroup.Name?.Trim('"') == "groupId", "Wrong group field");
            Require(groupPart[(groupPart.IndexOf("\r\n\r\n", StringComparison.Ordinal) + 4)..^2] == group, "Group value changed");
        }
        Console.WriteLine($"WIRE PUT {route} field={field} group={(group is null ? "absent" : group.Length == 0 ? "empty" : "present")} key={(key is null ? "absent" : "present-redacted")} bytes={actual.Length} sha256={Hash(file)}");
    }

    private static async Task Reject<T>(Func<Task> operation) where T : Exception
    {
        try { await operation(); }
        catch (T) { return; }
        throw new InvalidOperationException("Expected " + typeof(T).Name);
    }

    private static void CheckArtifacts(string sdkRoot)
    {
        using var pins = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "artifact-pins.json")));
        var entries = pins.RootElement.GetProperty(Path.GetFileName(sdkRoot));
        foreach (var pin in entries.EnumerateArray())
            Require(Hash(Path.Combine(sdkRoot, pin.GetProperty("path").GetString()!)) == pin.GetProperty("sha256").GetString(), "SDK/compiler/ref/runtime/native-host hash changed");
        Console.WriteLine($"ARTIFACTS {entries.GetArrayLength()} exact SDK closure/compiler/reference/runtime/native-host inputs match audit/pins; no independent package equivalence claimed.");
    }

    private static string Hash(string file) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(file))).ToLowerInvariant();
    private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
}
