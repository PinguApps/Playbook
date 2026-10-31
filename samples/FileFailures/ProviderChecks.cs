using static FileChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;

internal static class ProviderChecks
{
    private static readonly bool Current = Environment.Version.Major == 11;
    internal static async Task Run()
    {
        Contract(typeof(FileConfigurationProvider)); Contract(typeof(JsonConfigurationProvider));
        Console.WriteLine($"runtime={RuntimeInformation.FrameworkDescription}; os={RuntimeInformation.OSDescription}; arch={RuntimeInformation.ProcessArchitecture}");
        string directory = Path.Combine(Path.GetTempPath(), "planetarium-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            foreach (bool ignore in new[] { true, false })
            {
                using var files = new PhysicalFileProvider(directory);
                var missingErrors = new List<Exception>();
                using (var missing = new JsonConfigurationProvider(new JsonConfigurationSource { FileProvider = files, Path = "never.json",
                    OnLoadException = c => { missingErrors.Add(c.Exception); c.Ignore = ignore; } }))
                {
                    var missingToken = missing.GetReloadToken(); var missingThrown = Catch(missing.Load);
                    Check(missingErrors.Single() is FileNotFoundException && (missingThrown is null) == ignore && Value(missing) is null,
                        "initial required missing");
                    Check(missingToken.HasChanged == (ignore && !Current), "initial missing token");
                    Report("initial required missing", ignore, missingThrown, missingErrors, missing, missingToken);
                }
                string path = Path.Combine(directory, "display.json");
                await File.WriteAllTextAsync(path, "{broken");
                var initialErrors = new List<Exception>();
                using (var initial = new JsonConfigurationProvider(new JsonConfigurationSource { FileProvider = files, Path = "display.json",
                    OnLoadException = c => { initialErrors.Add(c.Exception); c.Ignore = ignore; } }))
                {
                    var initialToken = initial.GetReloadToken();
                    var initialThrown = Catch(initial.Load);
                    Check(initialErrors.Single() is InvalidDataException && (initialThrown is null) == ignore && Value(initial) is null,
                        "initial malformed policy");
                    Check(initialToken.HasChanged == (ignore && !Current), "initial malformed token");
                    Report("initial malformed", ignore, initialThrown, initialErrors, initial, initialToken);
                }
                await File.WriteAllTextAsync(path, "{\"Display\":\"Orion\"}");
                var errors = new List<Exception>();
                var source = new JsonConfigurationSource { FileProvider = files, Path = "display.json", OnLoadException = c => { errors.Add(c.Exception); c.Ignore = ignore; } };
                using var provider = new JsonConfigurationProvider(source);
                var token = provider.GetReloadToken();
                provider.Load();
                Check(token.HasChanged && Value(provider) == "Orion", "initial success");
                token = provider.GetReloadToken();
                await File.WriteAllTextAsync(path, "{broken");
                Exception? thrown = Catch(provider.Load);
                Check(errors.Last() is InvalidDataException && (thrown is null) == ignore, "parse policy");
                // JsonConfigurationFileParser publishes its dictionary only after parsing succeeds.
                Check(Value(provider) == "Orion" && token.HasChanged == (ignore && !Current), "explicit parse state/token");
                Report("explicit malformed", ignore, thrown, errors, provider, token);
                File.Delete(path);
                errors.Clear(); token = provider.GetReloadToken(); thrown = Catch(provider.Load);
                Check(errors.Single() is FileNotFoundException && (thrown is null) == ignore, "required missing policy");
                Check(token.HasChanged == (ignore && !Current), "required missing token");
                Report("required missing", ignore, thrown, errors, provider, token);

                source.Optional = true; errors.Clear(); token = provider.GetReloadToken(); provider.Load();
                Check(Value(provider) is null && token.HasChanged && errors.Count == 0, "optional missing clears/notifies");
                await File.WriteAllTextAsync(path, "{\"Display\":\"Lyra\"}"); provider.Load();
                if (OperatingSystem.IsWindows())
                {
                    errors.Clear(); token = provider.GetReloadToken();
                    using (var locked = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                    {
                        thrown = Catch(provider.Load);
                        Check(errors.Count == (Current ? 1 : 0) && (!Current || errors[0] is IOException), "physical open callback");
                        Check((thrown is null) == (Current && ignore) && !token.HasChanged && Value(provider) == "Lyra", "physical open state");
                        Report("Windows exclusive FileShare.None open", ignore, thrown, errors, provider, token);
                    }
                    token = provider.GetReloadToken(); provider.Load();
                    Check(token.HasChanged && Value(provider) == "Lyra", "physical IO recovery");
                }
            }
            foreach (bool ignore in new[] { true, false })
            {
                using var physical = new PhysicalFileProvider(directory);
                var files = new ObservedFiles(physical);
                var errors = new ConcurrentQueue<Exception>();
                string path = Path.Combine(directory, "reload.json");
                await File.WriteAllTextAsync(path, "{\"Display\":\"Vega\"}");
                var source = new JsonConfigurationSource { FileProvider = files, Path = "reload.json", ReloadOnChange = true, ReloadDelay = 40,
                    OnLoadException = c => { c.Ignore = ignore; errors.Enqueue(c.Exception); } };
                using var provider = new JsonConfigurationProvider(source);
                provider.Load(); Check(Value(provider) == "Vega", "baseline before reload failure");
                int registrations = files.Registrations;
                var token = provider.GetReloadToken();
                using var signal = new ManualResetEventSlim();
                using var registration = token.RegisterChangeCallback(_ => signal.Set(), null);
                await File.WriteAllTextAsync(path, "{\"Display\":\"Deneb\"}");
                await Until(() => token.HasChanged && Value(provider) == "Deneb", "real successful reload");
                await Until(() => files.Registrations > registrations, "success rearmed");
                Check(signal.IsSet, "registered notification");
                registrations = files.Registrations;
                token = provider.GetReloadToken(); await File.WriteAllTextAsync(path, "{broken");
                await Until(() => !errors.IsEmpty && Value(provider) is null, "real failed reload clears");
                if (ignore) await Until(() => token.HasChanged, "ignored reload parse notifies");
                else Check(!token.HasChanged, "unignored parse throws before notify");
                await Until(() => files.Registrations > registrations, "failure rearmed");
                Check(errors.All(e => e is InvalidDataException), "reload callback types");
                Console.WriteLine($"reload malformed ignore={ignore}; callback=InvalidDataException; data=<empty>; token={token.HasChanged}");
                // A missing file is optional on reload even for a required source.
                token = provider.GetReloadToken();
                registrations = files.Registrations;
                File.Delete(path);
                await Until(() => token.HasChanged, "deletion notification");
                await Until(() => files.Registrations > registrations, "deletion rearmed");
                token = provider.GetReloadToken(); await File.WriteAllTextAsync(path, "{\"Display\":\"Altair\"}");
                await Until(() => token.HasChanged && Value(provider) == "Altair", "recreation recovery");
            }
            using (var files = new PhysicalFileProvider(directory))
            {
                await File.WriteAllTextAsync(Path.Combine(directory, "display.json"), "{\"Display\":\"Capella\"}");
                var reported = new ConcurrentQueue<Exception>();
                using var canonical = SchedulePolicy.Create(files, reported.Enqueue);
                SchedulePolicy.Load(canonical, reported.Enqueue);
                Check(Value(canonical) == "Capella" && reported.IsEmpty, "canonical policy initial value");
            }
            await CheckUnignoredReload(directory);
            foreach (bool ignore in new[] { true, false })
            {
                var fault = new ApplicationException("synthetic stream fault");
                var files = new FaultFiles(fault);
                var errors = new List<Exception>();
                using var provider = new JsonConfigurationProvider(new JsonConfigurationSource { FileProvider = files, Path = "display.json",
                    OnLoadException = c => { errors.Add(c.Exception); c.Ignore = ignore; } });
                var token = provider.GetReloadToken(); var thrown = Catch(provider.Load);
                Check(errors.Count == (Current ? 1 : 0) && (!Current || ReferenceEquals(errors[0], fault)), "arbitrary stream callback identity");
                Check((thrown is null) == (Current && ignore) && !token.HasChanged, "synthetic open policy");
                Report("SYNTHETIC CreateReadStream", ignore, thrown, errors, provider, token);
                files.FailInfo = true; errors.Clear(); thrown = Catch(provider.Load);
                Check(ReferenceEquals(thrown, fault) && errors.Count == 0, "GetFileInfo outside callback");
                Console.WriteLine($"SYNTHETIC GetFileInfo; Ignore={ignore}; thrown={thrown?.GetType().Name}; callback=none; token={token.HasChanged}");
            }
            Console.WriteLine("PASS provider matrix; registrations/providers disposed; no global task event used");
        }
        finally { Directory.Delete(directory, true); Console.WriteLine($"CLEANUP tempRemoved={!Directory.Exists(directory)}"); }
    }
    // Observe registration completion while retaining the actual physical provider and file info.
    private sealed class ObservedFiles(IFileProvider inner) : IFileProvider
    {
        private int registrations;
        internal int Registrations => Volatile.Read(ref registrations);
        public IFileInfo GetFileInfo(string subpath) => inner.GetFileInfo(subpath);
        public IDirectoryContents GetDirectoryContents(string subpath) => inner.GetDirectoryContents(subpath);
        public IChangeToken Watch(string filter) => new ObservedToken(inner.Watch(filter), () => Interlocked.Increment(ref registrations));
        private sealed class ObservedToken(IChangeToken inner, Action registered) : IChangeToken
        {
            public bool HasChanged => inner.HasChanged;
            public bool ActiveChangeCallbacks => inner.ActiveChangeCallbacks;
            public IDisposable RegisterChangeCallback(Action<object?> callback, object? state)
            {
                var registration = inner.RegisterChangeCallback(callback, state);
                registered(); return registration;
            }
        }
    }
    internal static async Task UnignoredChild(string directory)
    {
        string path = Path.Combine(directory, "unignored.json");
        await File.WriteAllTextAsync(path, "{\"Display\":\"Arcturus\"}");
        using var files = new PhysicalFileProvider(directory);
        using var provider = new JsonConfigurationProvider(new JsonConfigurationSource
        {
            FileProvider = files, Path = "unignored.json", ReloadOnChange = true, ReloadDelay = 40,
            OnLoadException = c => { Console.WriteLine($"CHILD callback={c.Exception.GetType().Name}; Ignore=False"); c.Ignore = false; }
        });
        provider.Load();
        await File.WriteAllTextAsync(path, "{broken");
        await Until(() => Value(provider) is null, "child reload clears");
        // The current provider catches escaped automatic reload failures; observe it alive after reporting.
        await Task.Delay(250);
        Console.WriteLine("CHILD survived reload");
    }
    private static async Task CheckUnignoredReload(string directory)
    {
        var start = new System.Diagnostics.ProcessStartInfo(Environment.ProcessPath!)
        {
            UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true
        };
        if (string.Equals(Path.GetFileNameWithoutExtension(Environment.ProcessPath), "dotnet", StringComparison.OrdinalIgnoreCase))
            start.ArgumentList.Add(typeof(ProviderChecks).Assembly.Location);
        start.ArgumentList.Add("--unignored-reload"); start.ArgumentList.Add(directory);
        start.Environment["DOTNET_ADD_GLOBAL_TOOLS_TO_PATH"] = "0";
        using var child = System.Diagnostics.Process.Start(start)!;
        var stdout = child.StandardOutput.ReadToEndAsync(); var stderr = child.StandardError.ReadToEndAsync();
        try
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            await child.WaitForExitAsync(timeout.Token);
            string output = await stdout; string error = await stderr;
            Check(output.Contains("CHILD callback=InvalidDataException; Ignore=False", StringComparison.Ordinal), "unignored child callback");
            Check(child.ExitCode == 0 && output.Contains("CHILD survived", StringComparison.Ordinal), "unignored automatic reload boundary");
            Console.WriteLine($"PASS unignored real automatic reload child; callback=InvalidDataException; exit={child.ExitCode}; survived=True; stderrLength={error.Length}");
        }
        finally
        {
            if (!child.HasExited) child.Kill(true);
            await child.WaitForExitAsync();
        }
    }
    private static string? Value(JsonConfigurationProvider p) => p.TryGet("Display", out var value) ? value : null;
    private static void Report(string name, bool ignore, Exception? thrown, IEnumerable<Exception> errors, JsonConfigurationProvider p, IChangeToken token)
        => Console.WriteLine($"{name}; Ignore={ignore}; thrown={thrown?.GetType().Name ?? "none"}; callback={string.Join(',', errors.Select(e => e.GetType().Name))}; data={Value(p) ?? "<empty>"}; token={token.HasChanged}");
    private sealed class FaultFiles(Exception fault) : IFileProvider, IFileInfo
    {
        internal bool FailInfo { get; set; }
        public IFileInfo GetFileInfo(string subpath) => FailInfo ? throw fault : this;
        public IDirectoryContents GetDirectoryContents(string subpath) => NotFoundDirectoryContents.Singleton;
        public IChangeToken Watch(string filter) => NullChangeToken.Singleton;
        public bool Exists => true;
        public long Length => 1;
        public string? PhysicalPath => null;
        public string Name => "display.json";
        public DateTimeOffset LastModified => DateTimeOffset.UtcNow;
        public bool IsDirectory => false;
        public Stream CreateReadStream() => throw fault;
    }
}
