using System.Diagnostics;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;

namespace Gannet;

internal static class AnalyzerChecks
{
    public static async Task Run(string repository, string sdkRoot)
    {
        string fixture = Path.Combine(repository, "artifacts", "gannet-assets", "net" + Environment.Version.Major, Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(fixture);
        string dotnet = Environment.ProcessPath!, oldDotnet = Environment.GetEnvironmentVariable("GANNET_NET10_DOTNET") ?? "C:/Program Files/dotnet/dotnet.exe";
        string feed = Path.Combine(fixture, "feed"), cache = Path.Combine(fixture, "cache");
        Directory.CreateDirectory(feed);
        string analyzer = Path.Combine(fixture, "analyzer-source");
        Directory.CreateDirectory(analyzer);
        foreach (string file in Directory.GetFiles(Path.Combine(repository, "samples", "net11.0", "GannetWorkshop", "Shared", "Analyzers")))
            File.Copy(file, Path.Combine(analyzer, Path.GetFileName(file)));
        foreach (string file in new[] { "Directory.Build.props", "Directory.Build.targets", "Directory.Packages.props" })
            File.Copy(Path.Combine(repository, "samples", file), Path.Combine(analyzer, file));
        var pack = await Command(oldDotnet, analyzer, ["pack", "HarbourRules.csproj", "-c", "Release", "-o", feed, "-p:UseSharedCompilation=false"], cache);
        Require(pack.exit == 0, pack.output);
        string nupkg = Directory.GetFiles(feed, "*.nupkg").Single();
        using (var zip = await ZipFile.OpenReadAsync(nupkg))
            Require(zip.Entries.Count(e => e.FullName.EndsWith("HarbourRules.dll", StringComparison.Ordinal)) == 4, "Missing language/version layout");
        Console.WriteLine($"PACKAGE Gannet.HarbourRules/1.0.0 sha256={Hash(nupkg)} analyzer built against actual SDK10 Roslyn5.9, net10 runtime; four cs/vb/compiler-version paths. No external package substitution.");

        async Task Case(string name, string[] settings, bool corrected = false, bool transitive = false)
        {
            string folder = Path.Combine(fixture, name);
            Directory.CreateDirectory(folder);
            string source = Path.Combine(repository, "samples", "net11.0", "GannetWorkshop", "Shared", "Consumer");
            foreach (string file in new[] { "Harbour.csproj", "Harbour.cs" }) File.Copy(Path.Combine(source, file), Path.Combine(folder, file));
            string sdk = Path.GetFileName(sdkRoot);
            await File.WriteAllTextAsync(Path.Combine(folder, "global.json"), JsonSerializer.Serialize(new { sdk = new { version = sdk, rollForward = "disable", allowPrerelease = true } }));
            await File.WriteAllTextAsync(Path.Combine(folder, "Directory.Build.props"), "<Project><PropertyGroup><TreatWarningsAsErrors>true</TreatWarningsAsErrors></PropertyGroup></Project>");
            await File.WriteAllTextAsync(Path.Combine(folder, "NuGet.Config"), $"<configuration><packageSources><clear/><add key=\"local\" value=\"{System.Security.SecurityElement.Escape(feed)}\"/></packageSources></configuration>");
            if (corrected) await File.WriteAllTextAsync(Path.Combine(folder, "Harbour.cs"), "namespace Gannet.Consumer; public class SafeHarbour { }");
            if (transitive)
            {
                string bridge = Path.Combine(folder, "Bridge"); Directory.CreateDirectory(bridge);
                File.Copy(Path.Combine(folder, "Harbour.csproj"), Path.Combine(bridge, "Harbour.csproj"));
                await File.WriteAllTextAsync(Path.Combine(bridge, "Bridge.cs"), "namespace Gannet.Consumer; public class SafeBridge { }");
                string text = await File.ReadAllTextAsync(Path.Combine(folder, "Harbour.csproj"));
                int start = text.IndexOf("    <PackageReference", StringComparison.Ordinal), end = text.IndexOf(" />", start, StringComparison.Ordinal) + 3;
                text = text[..start] + "    <ProjectReference Include=\"Bridge/Harbour.csproj\" />\n    <Compile Remove=\"Bridge/**/*.cs\" />" + text[end..];
                // Different assembly identities avoid a project reference cycle/name collision.
                text = text.Replace("<OutputType>Library</OutputType>", "<OutputType>Library</OutputType><AssemblyName>Gannet.Downstream</AssemblyName>", StringComparison.Ordinal);
                await File.WriteAllTextAsync(Path.Combine(folder, "Harbour.csproj"), text);
            }
            string major = Environment.Version.Major.ToString(System.Globalization.CultureInfo.InvariantCulture);
            string[] common = ["-p:HarbourTarget=net" + major + ".0", "-p:HarbourLanguage=" + (major == "10" ? "14.0" : "15.0"), "-p:HarbourNet10Packs=" + Path.Combine(Path.GetDirectoryName(oldDotnet)!, "packs"), "-p:UseSharedCompilation=false", ..settings];
            var restore = await Command(dotnet, folder, ["restore", "Harbour.csproj", ..common], cache);
            Require(restore.exit == 0, restore.output);
            // Exercise the actual generated closure in locked mode before any compiler claim.
            var locked = await Command(dotnet, folder, ["restore", "Harbour.csproj", "--locked-mode", ..common], cache);
            Require(locked.exit == 0, locked.output);
            var build = await Command(dotnet, folder, ["build", "Harbour.csproj", "--no-restore", "-c", "Release", "-v:normal", ..common], cache);
            bool diagnostic = build.output.Contains("error GANNET001", StringComparison.Ordinal);
            Require(build.exit == (diagnostic ? 1 : 0), "Unexpected compiler failure: " + build.output);
            if (name is "default" or "false" or "true" or "private-all" or "mixed-default" or "mixed-true" or "excluded" or "include-runtime" or "transitive-none")
                Require(diagnostic, "Known positive analyzer did not execute: " + name);
            Require(!corrected || !diagnostic, "Corrected source still triggers");
            Require(!build.output.Contains("CS8032", StringComparison.Ordinal) && !build.output.Contains("CS9057", StringComparison.Ordinal), "Analyzer incompatible with actual compiler");
            using var assets = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(folder, "obj", "project.assets.json")));
            foreach (var target in assets.RootElement.GetProperty("targets").EnumerateObject())
            {
                string metadata = "absent";
                if (target.Value.TryGetProperty("Gannet.HarbourRules/1.0.0", out var library) && library.TryGetProperty("analyzers", out var entries)) metadata = entries.GetRawText();
                string receipt = Path.Combine(folder, "obj", "Release", target.Name, "selected-analyzers.txt");
                Require(File.Exists(receipt), "Target never reached CoreCompile: " + target.Name);
                string[] paths = (await File.ReadAllLinesAsync(receipt)).Where(s => s.Contains("gannet.harbourrules", StringComparison.OrdinalIgnoreCase)).ToArray();
                string selected = string.Join(";", paths.Select(s => s[(s.IndexOf("analyzers", StringComparison.OrdinalIgnoreCase))..].Replace('\\', '/')));
                string expected = name == "transitive-all" ? "" : "analyzers/dotnet/roslyn" + (major == "10" ? "5.9" : "5.12") + "/cs/HarbourRules.dll";
                Require(selected == expected, "Actual compiler analyzer selection changed: " + selected);
                foreach (string path in paths) Require(build.output.Contains("/analyzer:" + path, StringComparison.Ordinal) || build.output.Contains("/analyzer:\"" + path, StringComparison.Ordinal), "No actual compiler invocation for " + selected);
                bool recordsAssets = major == "11" && name is not ("default" or "false" or "mixed-default" or "transitive-all");
                Require((metadata != "absent") == recordsAssets, "Unexpected restore metadata: " + name + "/" + target.Name);
                if (recordsAssets)
                {
                    bool placeholder = metadata.Contains("_._", StringComparison.Ordinal);
                    Require(placeholder == (name is "excluded" or "include-runtime"), "Incorrect include/exclude assets representation");
                    if (!placeholder) Require(metadata.Contains("\"codeLanguage\": \"vb\"", StringComparison.Ordinal) && metadata.Contains("roslyn99.0", StringComparison.Ordinal), "Metadata incorrectly preselects compiler/language");
                }
                if (name is "mixed-default" or "mixed-true")
                    Require(build.output.Split('\n').Any(line => line.Contains("error GANNET001", StringComparison.Ordinal) && line.Contains("TargetFramework=" + target.Name, StringComparison.Ordinal)), "No actual diagnostic execution in " + target.Name);
                Console.WriteLine($"ASSETS {name} target={target.Name} metadata={metadata} SELECTED={selected} DIAGNOSTIC={diagnostic} EXIT={build.exit} lock-sha256={Hash(Path.Combine(folder, "packages.lock.json"))}");
                // Every target must actually reach CoreCompile, even on an expected rule error.
                Require(name != "transitive-all" || !diagnostic, "PrivateAssets=all leaked analyzer transitively");
            }
            // Keep raw invocations, compiler command lines, actual locks and files disposable.
            await File.WriteAllTextAsync(Path.Combine(folder, "observed.log"), restore.output + locked.output + build.output);
            Console.WriteLine("COMMAND " + name + " restore; restore --locked-mode; build --no-restore -c Release -v:normal " + string.Join(' ', common));
        }

        await Case("default", []);
        await Case("false", ["-p:RestoreEnableAnalyzerAssets=false"]);
        await Case("true", ["-p:RestoreEnableAnalyzerAssets=true"]);
        await Case("excluded", ["-p:RestoreEnableAnalyzerAssets=true", "-p:HarbourExclude=analyzers"]);
        await Case("include-runtime", ["-p:RestoreEnableAnalyzerAssets=true", "-p:HarbourInclude=runtime"]);
        await Case("private-all", ["-p:RestoreEnableAnalyzerAssets=true", "-p:HarbourPrivate=all"]);
        await Case("corrected", ["-p:RestoreEnableAnalyzerAssets=true"], corrected: true);
        if (Environment.Version.Major == 11)
        {
            await Case("mixed-default", ["-p:Mixed=true"]);
            await Case("mixed-true", ["-p:Mixed=true", "-p:RestoreEnableAnalyzerAssets=true"]);
            await Case("transitive-none", ["-p:RestoreEnableAnalyzerAssets=true", "-p:HarbourPrivate=none"], transitive: true);
            await Case("transitive-all", ["-p:RestoreEnableAnalyzerAssets=true", "-p:HarbourPrivate=all"], transitive: true);
        }
        string warning = Path.Combine(fixture, "warning"); Directory.CreateDirectory(warning);
        foreach (var file in Directory.GetFiles(Path.Combine(repository, "samples", "net11.0", "GannetWorkshop", "Shared", "WarningProbe"))) File.Copy(file, Path.Combine(warning, Path.GetFileName(file)));
        File.Copy(Path.Combine(fixture, "default", "global.json"), Path.Combine(warning, "global.json"));
        File.Copy(Path.Combine(fixture, "default", "NuGet.Config"), Path.Combine(warning, "NuGet.Config"));
        File.Copy(Path.Combine(fixture, "default", "Directory.Build.props"), Path.Combine(warning, "Directory.Build.props"));
        var warningResult = await Command(dotnet, warning, ["build", "WarningProbe.csproj", "-p:HarbourTarget=net" + Environment.Version.Major + ".0", "-p:UseSharedCompilation=false"], cache);
        bool obsolete = warningResult.output.Contains("warning CS0618", StringComparison.Ordinal);
        Require(warningResult.exit == 0 && obsolete == (Environment.Version.Major == 11), warningResult.output);
        Console.WriteLine("OBSOLETE old/fixed comparison SDK" + Environment.Version.Major + " CS0618=" + obsolete + (obsolete ? " message=This method is unused and will be removed in a future release." : " actual old helper remains callable without that warning."));
        await File.WriteAllTextAsync(Path.Combine(warning, "observed.log"), warningResult.output);
        Console.WriteLine("PASS real local package, locked closure, actual old/current compiler paths and known positive/negative diagnostic controls; routine raw fixtures retained only in ignored artifacts/gannet-assets.");
    }

    private static async Task<(int exit, string output)> Command(string executable, string cwd, string[] args, string cache)
    {
        var start = new ProcessStartInfo(executable) { WorkingDirectory = cwd, UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true };
        foreach (string arg in args) start.ArgumentList.Add(arg);
        start.Environment["DOTNET_ADD_GLOBAL_TOOLS_TO_PATH"] = "0";
        start.Environment["MSBUILDUSESERVER"] = "0";
        start.Environment["MSBUILDDISABLENODEREUSE"] = "1";
        start.Environment["NUGET_PACKAGES"] = cache;
        using var process = Process.Start(start)!;
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(2));
        Task<string> stdout = process.StandardOutput.ReadToEndAsync(timeout.Token), stderr = process.StandardError.ReadToEndAsync(timeout.Token);
        try { await process.WaitForExitAsync(timeout.Token); return (process.ExitCode, await stdout + await stderr); }
        finally { if (!process.HasExited) { process.Kill(true); await process.WaitForExitAsync(CancellationToken.None); } }
    }
    private static string Hash(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
    private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
}
