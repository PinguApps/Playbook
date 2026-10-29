using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Xml;
using Microsoft.Build.Construction;
using Microsoft.Build.Definition;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;

internal static class MuseumChecks
{
    private static readonly string[] ExpectedObjects = ["bowl-17|Harbour", "tile-42|Harbour"];
    public static void Run(string dotnet, string sdkDirectory, string sdkVersion)
    {
        VerifyPins(sdkDirectory, sdkVersion);
        string directory = Path.Combine(Path.GetTempPath(), "playbook-museum-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            foreach (string name in new[] { "Museum.csproj", "Collection.props" })
                File.Copy(Path.Combine(AppContext.BaseDirectory, name), Path.Combine(directory, name));
            File.WriteAllText(Path.Combine(directory, "global.json"), JsonSerializer.Serialize(new
            {
                sdk = new { version = sdkVersion, rollForward = "disable", allowPrerelease = true }
            }));
            string projectPath = Path.Combine(directory, "Museum.csproj");
            CheckEvaluation(projectPath, sdkDirectory);
            CheckCli(dotnet, directory, projectPath, sdkVersion);
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic).OrderBy(a => a.FullName, StringComparer.Ordinal))
            {
                string location = assembly.Location;
                if (assembly.GetName().Name != "Microsoft.Build.Locator" &&
                    (assembly.GetName().Name!.StartsWith("Microsoft.Build", StringComparison.Ordinal) ||
                     assembly.GetName().Name!.Contains("SdkResolver", StringComparison.Ordinal)))
                    Require(Path.GetFullPath(location).StartsWith(Path.GetFullPath(sdkDirectory) + Path.DirectorySeparatorChar,
                        StringComparison.OrdinalIgnoreCase), "Foreign MSBuild/resolver assembly: " + location);
                Console.WriteLine($"LOADED {assembly.FullName} path={location} sha256={Hash(location)}");
            }
            Console.WriteLine($"PASS: SDK {sdkVersion}; real imported SDK project, full model, CLI property/item and target-execution controls.");
        }
        finally { Directory.Delete(directory, true); }
    }

    private static void CheckEvaluation(string projectPath, string sdkDirectory)
    {
        using var projects = new ProjectCollection();
#if NET11_0
        // snippet:partial
        var properties = ProjectInstance.FromFile(projectPath, new ProjectOptions
        {
            ProjectCollection = projects, EvaluationStage = ProjectEvaluationStage.Properties
        });
        Console.WriteLine($"{properties.EvaluationStage}: {properties.GetPropertyValue("CollectionCode")}");
        // Items need a new instance evaluated through the later pass.
        var items = ProjectInstance.FromFile(projectPath, new ProjectOptions
        {
            ProjectCollection = projects, EvaluationStage = ProjectEvaluationStage.Items
        });
        foreach (var item in items.GetItems("MuseumObject"))
            Console.WriteLine($"{item.EvaluatedInclude}: {item.GetMetadataValue("Gallery")}");
        // endsnippet:partial
        var full = ProjectInstance.FromFile(projectPath, new ProjectOptions
        {
            ProjectCollection = projects, EvaluationStage = ProjectEvaluationStage.Full
        });
        var defaultFull = ProjectInstance.FromFile(projectPath, new ProjectOptions { ProjectCollection = projects });
        Require(defaultFull.EvaluationStage == ProjectEvaluationStage.Full, "Default stage must be Full");
        Require(full.EvaluationStage == ProjectEvaluationStage.Full, "Explicit Full stage");
        Require(defaultFull.Targets.Count == full.Targets.Count && defaultFull.Items.Count == full.Items.Count,
            "Default/full state mismatch");
        Require(properties.EvaluationStage == ProjectEvaluationStage.Properties && items.EvaluationStage == ProjectEvaluationStage.Items,
            "Requested stage not retained");
        Require(properties.ImportPaths.SequenceEqual(full.ImportPaths) && items.ImportPaths.SequenceEqual(full.ImportPaths), "Partial/full imports differ");
        var expectedProperties = full.Properties.OrderBy(p => p.Name, StringComparer.Ordinal).Select(p => p.Name + "=" + p.EvaluatedValue).ToArray();
        Require(properties.Properties.OrderBy(p => p.Name, StringComparer.Ordinal).Select(p => p.Name + "=" + p.EvaluatedValue).SequenceEqual(expectedProperties), "Partial/full property set differs");
        Throws<InvalidOperationException>(() => _ = properties.Items.Count, "Properties.Items");
        Throws<InvalidOperationException>(() => _ = properties.GetItems("MuseumObject"), "Properties.GetItems");
        Throws<InvalidOperationException>(() => _ = properties.Targets.Count, "Properties.Targets.Count");
        Throws<InvalidOperationException>(() => _ = items.Targets.Count, "Items.Targets.Count");
        Require(Objects(items).SequenceEqual(Objects(full)), "Partial/full item identities and metadata differ");
        Require(items.Items.Count == full.Items.Count, "Partial/full total item counts differ");
        foreach (var stage in new[] { ProjectEvaluationStage.Properties, ProjectEvaluationStage.ItemDefinitions,
                     ProjectEvaluationStage.Items, ProjectEvaluationStage.UsingTasks })
        {
            var options = new ProjectOptions { ProjectCollection = projects, EvaluationStage = stage };
            var instance = ProjectInstance.FromFile(projectPath, options);
            Require(instance.EvaluationStage == stage, "Stage mismatch");
            Require(instance.GetPropertyValue("CollectionCode") == full.GetPropertyValue("CollectionCode"), "Property mismatch");
            if (stage >= ProjectEvaluationStage.ItemDefinitions)
                Require(instance.ItemDefinitions["MuseumObject"].GetMetadata("Gallery").EvaluatedValue == "Harbour", "Item definition missing");
            Throws<InvalidOperationException>(() => _ = instance.Targets.Count, stage + ".Targets.Count");
            Throws<ArgumentException>(() => _ = Project.FromFile(projectPath, options), "Project.FromFile " + stage);
            var xml = ProjectRootElement.Open(projectPath, projects)!;
            Throws<ArgumentException>(() => _ = Project.FromProjectRootElement(xml, options), "Project.FromProjectRootElement " + stage);
            using var reader = XmlReader.Create(projectPath);
            Throws<ArgumentException>(() => _ = Project.FromXmlReader(reader, options), "Project.FromXmlReader " + stage);
        }
        Throws<ArgumentOutOfRangeException>(() => _ = new ProjectOptions { EvaluationStage = (ProjectEvaluationStage)5 }, "Undefined stage");
        using (var mutableProjects = new ProjectCollection())
        {
            var mutable = Project.FromFile(projectPath, new ProjectOptions { ProjectCollection = mutableProjects });
            Require(mutable.Targets.Count == full.Targets.Count, "Mutable Project full control");
        }
        Console.WriteLine($"STAGES Properties={properties.EvaluationStage}, Items={items.EvaluationStage}, explicit/default={full.EvaluationStage}; partial target counts unavailable (throw), full targets={full.Targets.Count}, items={full.Items.Count}");
#else
        // snippet:legacy
        var full = ProjectInstance.FromFile(projectPath, new ProjectOptions { ProjectCollection = projects });
        Console.WriteLine(full.GetPropertyValue("CollectionCode"));
        foreach (var item in full.GetItems("MuseumObject"))
            Console.WriteLine($"{item.EvaluatedInclude}: {item.GetMetadataValue("Gallery")}");
        Console.WriteLine($"Registered targets: {full.Targets.Count}");
        // endsnippet:legacy
        Require(typeof(ProjectOptions).GetProperty("EvaluationStage") is null, "Unexpected older partial API");
        Require(typeof(ProjectInstance).Assembly.GetType("Microsoft.Build.Evaluation.ProjectEvaluationStage") is null,
            "Unexpected older stage enum");
#endif
        Require(full.GetPropertyValue("MuseumName") == "Bracken Quay Museum", "Imported property missing");
        Require(full.GetPropertyValue("CollectionCode") == "BQ-ceramics", "Imported code missing");
        Require(Objects(full).SequenceEqual(ExpectedObjects), "Imported item definitions/items missing");
        Require(full.Targets.ContainsKey("CountInventory") && full.Targets.ContainsKey("ValidateCollection") && full.Targets.ContainsKey("Build"),
            "Local/import/SDK targets missing");
        Require(full.GetPropertyValue("InventoryStatus") == "not-counted", "Evaluation executed a target");
        string expectedSdk = Path.GetFullPath(Path.Combine(sdkDirectory, "Sdks", "Microsoft.NET.Sdk"));
        Require(Path.GetFullPath(full.GetPropertyValue("MSBuildSDKsPath")) == Path.GetFullPath(Path.Combine(sdkDirectory, "Sdks")), "Wrong SDK search path");
        Require(full.ImportPaths.Any(p => Path.GetFullPath(p).StartsWith(expectedSdk + Path.DirectorySeparatorChar,
            StringComparison.OrdinalIgnoreCase)), "Actual Microsoft.NET.Sdk imports missing");
        Require(full.ImportPaths.Any(p => Path.GetFileName(p) == "Collection.props"), "Original import missing");
        Console.WriteLine($"MODEL code={full.GetPropertyValue("CollectionCode")} objects={string.Join(';', Objects(full))} items={full.Items.Count} targets={full.Targets.Count} imports={full.ImportPaths.Count}");
        foreach (string import in full.ImportPaths)
            Console.WriteLine($"IMPORT {import} sha256={Hash(import)}");
    }

    private static string[] Objects(ProjectInstance instance) => instance.GetItems("MuseumObject")
        .Select(i => i.EvaluatedInclude + "|" + i.GetMetadataValue("Gallery")).Order(StringComparer.Ordinal).ToArray();

    private static void CheckCli(string dotnet, string directory, string projectPath, string sdkVersion)
    {
        string properties = Cli("-getProperty:MuseumName,CollectionCode,InventoryStatus,MSBuildSDKsPath");
        using var propertyJson = JsonDocument.Parse(properties);
        Require(propertyJson.RootElement.GetProperty("Properties").GetProperty("CollectionCode").GetString() == "BQ-ceramics", "CLI property mismatch");
        Require(propertyJson.RootElement.GetProperty("Properties").GetProperty("InventoryStatus").GetString() == "not-counted", "CLI query executed target");
        string items = Cli("-getItem:MuseumObject");
        using var itemJson = JsonDocument.Parse(items);
        var values = itemJson.RootElement.GetProperty("Items").GetProperty("MuseumObject").EnumerateArray()
            .Select(i => i.GetProperty("Identity").GetString() + "|" + i.GetProperty("Gallery").GetString()).Order(StringComparer.Ordinal);
        Require(values.SequenceEqual(ExpectedObjects), "CLI items mismatch");
        string executed = Cli("-target:CountInventory", "-getProperty:InventoryStatus");
        Require(executed.Trim() == "counted", "Explicit target did not execute");
        Console.WriteLine($"CLI SDK={sdkVersion} property={properties} items={items} target-control={executed}");

        string Cli(params string[] arguments)
        {
            var start = new ProcessStartInfo(dotnet) { WorkingDirectory = directory, RedirectStandardOutput = true, RedirectStandardError = true };
            foreach (string arg in new[] { "msbuild", projectPath, "-nologo", "-nr:false" }.Concat(arguments)) start.ArgumentList.Add(arg);
            start.Environment["DOTNET_ADD_GLOBAL_TOOLS_TO_PATH"] = "0";
            start.Environment["MSBUILDUSESERVER"] = "0";
            using var child = Process.Start(start)!;
            var output = child.StandardOutput.ReadToEndAsync();
            var error = child.StandardError.ReadToEndAsync();
            if (!child.WaitForExit(60000)) { child.Kill(true); throw new TimeoutException("Museum CLI child"); }
            Require(child.ExitCode == 0, "CLI failed: " + error.Result + output.Result);
            Console.WriteLine("COMMAND " + dotnet + " " + string.Join(' ', start.ArgumentList));
            return output.Result.Trim();
        }
    }

    private static void VerifyPins(string sdkDirectory, string sdkVersion)
    {
        using var pins = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "artifact-pins.json")));
        foreach (var pin in pins.RootElement.GetProperty(sdkVersion).EnumerateArray())
        {
            string path = Path.Combine(sdkDirectory, pin.GetProperty("path").GetString()!);
            Require(Hash(path) == pin.GetProperty("sha256").GetString(), "Artifact hash mismatch: " + path);
            Console.WriteLine($"ARTIFACT {path} sha256={Hash(path)} product={FileVersionInfo.GetVersionInfo(path).ProductVersion}");
        }
    }

    private static string Hash(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
    private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    private static void Throws<T>(Action action, string label) where T : Exception
    {
        try { action(); }
        catch (T error) { Console.WriteLine($"GUARD {label}: {error.GetType().Name}: {error.Message}"); return; }
        throw new InvalidOperationException("Expected " + typeof(T).Name + ": " + label);
    }
}
