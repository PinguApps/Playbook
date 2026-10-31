using Microsoft.Extensions.FileProviders.Physical;
using System.Runtime.InteropServices;

internal static class WatcherChecks
{
    internal static async Task Run()
    {
        FileChecks.Contract(typeof(PhysicalFilesWatcher));
        bool current = Environment.Version.Major == 11;
        Console.WriteLine($"runtime={RuntimeInformation.FrameworkDescription}; os={RuntimeInformation.OSDescription}; arch={RuntimeInformation.ProcessArchitecture}");
        string parent = Path.Combine(Path.GetTempPath(), "garden-" + Guid.NewGuid().ToString("N"));
        string root = Path.Combine(parent, "root");
        Directory.CreateDirectory(Path.Combine(root, "beds"));
        Directory.CreateDirectory(Path.Combine(parent, "root-other"));
        Directory.CreateDirectory(Path.Combine(parent, "sibling"));
        try
        {
            foreach (bool filters in new[] { false, true })
            {
                foreach (string? invalid in new[] { null, "bad\0path" })
                {
                    using var fsw = new FileSystemWatcher(root);
                    var error = FileChecks.Catch(() => { using var p = Make(invalid!, fsw, filters); });
                    FileChecks.Check(current ? error is ArgumentException : error is null, "root validation");
                    Console.WriteLine($"constructor filters={filters}; root={(invalid is null ? "null" : "NUL")}; result={error?.GetType().Name ?? "accepted (not monitored)"}");
                }
                foreach (string sibling in new[] { "sibling", "root-other" })
                {
                    using var fsw = new FileSystemWatcher(Path.Combine(parent, sibling));
                    var error = FileChecks.Catch(() => { using var p = Make(root, fsw, filters); });
                    FileChecks.Check(current ? error is ArgumentException : error is null, "unrelated path validation");
                    Console.WriteLine($"constructor filters={filters}; unrelated={sibling}; result={error?.GetType().Name ?? "accepted (not functional evidence)"}");
                }
                foreach (string layout in new[] { "equal", "ancestor", "descendant", "empty", "relative", "deferred" })
                {
                    string selectedRoot = layout == "deferred" ? Path.Combine(parent, "later" + filters) : root;
                    string watcherPath = layout switch { "ancestor" => parent, "descendant" => Path.Combine(root, "beds"), "empty" or "deferred" => "", _ => root };
                    using var fsw = new FileSystemWatcher { Path = watcherPath };
                    string argument = layout == "relative" ? Path.GetRelativePath(Environment.CurrentDirectory, root) : Path.TrimEndingDirectorySeparator(selectedRoot) + Path.DirectorySeparatorChar;
                    using var watcher = Make(argument, fsw, filters);
                    string relative = layout == "descendant" ? "beds/observation.json" : "observation.json";
                    if (!current && layout is "relative" or "ancestor")
                    {
                        Console.WriteLine($"old {layout} root construction accepted; no monitoring contract claimed; older factory uses normalized equal paths");
                        continue;
                    }
                    Microsoft.Extensions.Primitives.IChangeToken? token = null;
                    var tokenError = FileChecks.Catch(() => token = watcher.CreateFileChangeToken(relative));
                    if (!current && layout is "empty" or "deferred")
                    {
                        FileChecks.Check(tokenError is FileNotFoundException, "older empty path failure");
                        Console.WriteLine($"old layout={layout}; token creation={tokenError!.GetType().Name}");
                        continue;
                    }
                    if (tokenError is not null) throw tokenError;
                    if (layout == "deferred")
                    {
                        Directory.CreateDirectory(selectedRoot);
                        await FileChecks.Until(() => fsw.EnableRaisingEvents, "deferred watcher armed");
                    }
                    await File.WriteAllTextAsync(Path.Combine(selectedRoot, relative), Guid.NewGuid().ToString());
                    await FileChecks.Until(() => token!.HasChanged, "watch " + layout);
                    if (layout == "ancestor") FileChecks.Check(fsw.IncludeSubdirectories, "ancestor recursive");
                    Console.WriteLine($"PASS filters={filters}; layout={layout}; token={token!.HasChanged}; fswPath={fsw.Path}; recursive={fsw.IncludeSubdirectories}");
                    File.Delete(Path.Combine(selectedRoot, relative));
                }

            }
            using (var canonical = GardenWatch.Open(root))
            {
                var token = canonical.CreateFileChangeToken("canonical.json");
                await File.WriteAllTextAsync(Path.Combine(root, "canonical.json"), "{}");
                await FileChecks.Until(() => token.HasChanged, "canonical garden watcher");
            }
        }
        finally { Directory.Delete(parent, true); Console.WriteLine($"CLEANUP watchers disposed; tempRemoved={!Directory.Exists(parent)}"); }
    }
    private static PhysicalFilesWatcher Make(string root, FileSystemWatcher watcher, bool filters) => filters
        ? new(root, watcher, false, ExclusionFilters.Sensitive) : new(root, watcher, false);
}
