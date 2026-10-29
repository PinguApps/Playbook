using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Xml.Linq;

// Real MSBuild subprocess proof; no synthetic binding or CreateWithProjectDirectoryAndEnvironment.
string dotnet = Environment.ProcessPath!;
#if TYPED_TASKS
const string sdk = "11.0.100-rc.2.26504.105";
bool typed = true;
#else
const string sdk = "10.0.402";
bool typed = false;
#endif
string assembly = Assembly.GetExecutingAssembly().Location;
string root = Path.Combine(Path.GetTempPath(), "playbook-observatory-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(root);
File.WriteAllText(Path.Combine(root, "global.json"), JsonSerializer.Serialize(new
{
    sdk = new { version = sdk, rollForward = "disable", allowPrerelease = typed }
}));
try
{
    VerifyArtifacts();
    RunPair("ordinary", false);
    RunPair("default", false, environmentDirectory: null);
    RunPair("threaded", true);
#if TYPED_TASKS
    RunPair("override", true, "selected-reports");
    RunDirect();
#endif
    RunMissing();
    RunConstructor();
    Console.WriteLine($"PASS: SDK {sdk}; real task binding, project-relative inputs, content/metadata round-trip, missing-file failure, constructor selection and process state checks.");
}
finally
{
    Directory.Delete(root, true);
}

void VerifyArtifacts()
{
    string sdkRoot = Path.Combine(Path.GetDirectoryName(dotnet)!, "sdk", sdk);
    using var pins = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "artifact-pins.json")));
    foreach (var pin in pins.RootElement.GetProperty(sdk).EnumerateArray())
    {
        string path = Path.Combine(sdkRoot, pin.GetProperty("path").GetString()!);
        string actual = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
        Require(actual == pin.GetProperty("sha256").GetString(), "Artifact hash mismatch: " + path);
        Console.WriteLine($"ARTIFACT {pin.GetProperty("path").GetString()} sha256={actual} product={FileVersionInfo.GetVersionInfo(path).ProductVersion}");
    }
}

void RunPair(string mode, bool threaded, string? reportOverride = null, string? environmentDirectory = "night-reports")
{
    string directory = Path.Combine(root, mode);
    Directory.CreateDirectory(directory);
    foreach (string project in new[] { "north", "south" })
    {
        string site = Path.Combine(directory, project);
        Directory.CreateDirectory(Path.Combine(site, "frames"));
        File.WriteAllText(Path.Combine(site, "frames", "meridian.txt"), project + "-42");
        var task = new XElement("Observatory.CheckedExposures",
            new XAttribute("Sources", "@(Exposure)"), new XAttribute("ProofFile", Path.Combine(site, "proof.json")),
            new XAttribute("Concurrent", threaded && typed),
            new XElement("Output", new XAttribute("TaskParameter", "Reports"), new XAttribute("ItemName", "Measured")));
        if (!typed)
        {
            task.Add(new XAttribute("ProjectDirectory", "$(MSBuildProjectDirectory)"));
            task.Add(new XAttribute("ReportDirectory", environmentDirectory ?? "reports"));
        }
        else if (reportOverride != null)
            task.Add(new XAttribute("ReportDirectory", reportOverride));
        var projectXml = new XElement("Project",
            new XElement("UsingTask", new XAttribute("TaskName", "Observatory.CheckedExposures"), new XAttribute("AssemblyFile", assembly)),
            new XElement("ItemGroup", new XElement("Exposure", new XAttribute("Include", "frames/meridian.txt"), new XElement("Filter", project + " hydrogen"))),
            new XElement("Target", new XAttribute("Name", "Measure"), task,
                new XElement("WriteLinesToFile", new XAttribute("File", Path.Combine(site, "bound.txt")),
                    new XAttribute("Lines", "@(Measured->'%(FullPath)|%(Filter)')"), new XAttribute("Overwrite", "true"))));
        projectXml.Save(Path.Combine(site, "site.proj"));
    }
    new XElement("Project", new XElement("Target", new XAttribute("Name", "Measure"),
        new XElement("MSBuild", new XAttribute("Projects", "north/site.proj;south/site.proj"),
            new XAttribute("Targets", "Measure"), new XAttribute("BuildInParallel", "true"))))
        .Save(Path.Combine(directory, "pair.proj"));
    RunBuild(Path.Combine(directory, "pair.proj"), threaded, environmentDirectory: environmentDirectory);
    var observations = new List<JsonElement>();
    foreach (string project in new[] { "north", "south" })
    {
        string site = Path.Combine(directory, project);
        var observation = JsonDocument.Parse(File.ReadAllText(Path.Combine(site, "proof.json"))).RootElement.Clone();
        observations.Add(observation);
        string report = Path.Combine(site, reportOverride ?? environmentDirectory ?? "reports", "meridian.report");
        Require(File.ReadAllText(report) == project + " hydrogen|" + project + "-42", "Wrong file content/project");
        Require(observation.GetProperty("inputs")[0].GetString() == Path.Combine(site, "frames", "meridian.txt"), "Wrong bound input path");
        Require(Path.GetFullPath(observation.GetProperty("project").GetString()!) == site, "Wrong task project environment");
        Require(File.ReadAllText(Path.Combine(site, "bound.txt")).Trim() == report + "|" + project + " hydrogen", "Output path/metadata did not round-trip through MSBuild");
        Require(observation.GetProperty("unchanged").GetBoolean(), "Task mutated process state");
        string sdkRoot = Path.Combine(Path.GetDirectoryName(dotnet)!, "sdk", sdk);
        Require(string.Equals(observation.GetProperty("frameworkPath").GetString(), Path.Combine(sdkRoot, "Microsoft.Build.Framework.dll"), StringComparison.OrdinalIgnoreCase), "Unexpected loaded Framework assembly");
        Require(string.Equals(observation.GetProperty("utilitiesPath").GetString(), Path.Combine(sdkRoot, "Microsoft.Build.Utilities.Core.dll"), StringComparison.OrdinalIgnoreCase), "Unexpected loaded Utilities assembly");
        string expectedRuntime = typed ? "11.0.0-rc.2.26504.105" : "10.0.13";
        Require(string.Equals(observation.GetProperty("runtimeDirectory").GetString()!.TrimEnd(Path.DirectorySeparatorChar), Path.Combine(Path.GetDirectoryName(dotnet)!, "shared", "Microsoft.NETCore.App", expectedRuntime), StringComparison.OrdinalIgnoreCase), "Unexpected loaded runtime");
        if (typed) Require(observation.GetProperty("constructor").GetString() == "injected", "Missing constructor injection");
        Console.WriteLine($"OBSERVATION {mode}/{project} {observation.GetRawText()}");
    }
    if (threaded && typed)
    {
        var a = observations[0]; var b = observations[1];
        Require(a.GetProperty("pid").GetInt32() == b.GetProperty("pid").GetInt32(), "Not in the same MSBuild process");
        Require(a.GetProperty("thread").GetInt32() != b.GetProperty("thread").GetInt32(), "Not on different threads");
        Require(a.GetProperty("start").GetInt64() < b.GetProperty("end").GetInt64() && b.GetProperty("start").GetInt64() < a.GetProperty("end").GetInt64(), "Tasks did not overlap");
        Console.WriteLine("PASS: actual -mt tasks rendezvoused in one process on different threads with overlapping execution.");
    }
}

void RunMissing()
{
    string file = Path.Combine(root, "missing.proj");
    var task = new XElement("Observatory.SummarizeExposures", new XAttribute("Sources", "not-found.txt"));
    if (!typed)
    {
        task.Add(new XAttribute("ProjectDirectory", root));
        task.Add(new XAttribute("ReportDirectory", "reports"));
    }
    new XElement("Project", new XElement("UsingTask", new XAttribute("TaskName", "Observatory.SummarizeExposures"), new XAttribute("AssemblyFile", assembly)),
        new XElement("Target", new XAttribute("Name", "Measure"), task)).Save(file);
    string output = RunBuild(file, typed, false);
    Require(output.Contains("Missing exposure:", StringComparison.Ordinal) && output.Contains("not-found.txt", StringComparison.Ordinal), "Missing-file failure was not the task's bounded diagnostic");
    Console.WriteLine("PASS: typed/path binding does not establish file existence; real missing input rejected.");
}

#if TYPED_TASKS
void RunDirect()
{
    string directory = Path.Combine(root, "canonical");
    Directory.CreateDirectory(Path.Combine(directory, "frames"));
    File.WriteAllText(Path.Combine(directory, "frames", "meridian.txt"), "direct-42");
    string file = Path.Combine(directory, "Measure.proj");
    var project = XElement.Load(Path.Combine(AppContext.BaseDirectory, "Measure.proj"));
    project.AddFirst(new XElement("PropertyGroup", new XElement("ObservatoryTaskAssembly", assembly)));
    project.Save(file);
    RunBuild(file, true);
    string report = Path.Combine(directory, "night-reports", "meridian.report");
    Require(File.ReadAllText(report) == "hydrogen alpha|direct-42", "Canonical invocation changed file content");
    Require(File.ReadAllText(Path.Combine(directory, "bound.txt")).Trim() == report + "|hydrogen alpha", "Canonical invocation lost output metadata");
    Console.WriteLine("PASS: canonical production task and Measure.proj executed directly under -mt.");
}
#endif

void RunConstructor()
{
#if TYPED_TASKS
    string legacy = Path.GetFullPath("../../../../../net10.0/ObservatoryTasks/bin/Release/net10.0/ObservatoryTasks.dll", AppContext.BaseDirectory);
    const string expected = "injected";
#else
    string legacy = assembly;
    const string expected = "parameterless";
#endif
    Require(File.Exists(legacy), "Build the paired net10.0 sample before running this sample.");
    string file = Path.Combine(root, "constructor.proj");
    new XElement("Project", new XElement("UsingTask", new XAttribute("TaskName", "Observatory.ConstructorProbe"), new XAttribute("AssemblyFile", legacy)),
        new XElement("Target", new XAttribute("Name", "Measure"),
            new XElement("Observatory.ConstructorProbe",
                new XElement("Output", new XAttribute("TaskParameter", "Construction"), new XAttribute("PropertyName", "Construction")),
                new XElement("Output", new XAttribute("TaskParameter", "DefaultDirectory"), new XAttribute("PropertyName", "DefaultDirectory"))),
            new XElement("Error", new XAttribute("Condition", $"'$(Construction)' != '{expected}'"), new XAttribute("Text", "Wrong constructor selected")),
            new XElement("Error", new XAttribute("Condition", "'$(DefaultDirectory)' != '$(MSBuildProjectDirectory)/night-reports'".Replace('/', Path.DirectorySeparatorChar)), new XAttribute("Text", "Wrong environment-dependent constructor default"))))
        .Save(file);
    RunBuild(file, false);
    Console.WriteLine($"PASS: same net10.0 compatibility binary sha256={Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(legacy))).ToLowerInvariant()} selected {expected} constructor in SDK {sdk}, default={Path.Combine(root, "night-reports")}");
}

string RunBuild(string project, bool threaded, bool success = true, string? environmentDirectory = "night-reports")
{
    var start = new ProcessStartInfo(dotnet) { WorkingDirectory = root, RedirectStandardOutput = true, RedirectStandardError = true };
    foreach (string arg in new[] { "msbuild", project, "-t:Measure", "-m:2", "-nr:false", "-v:minimal", "-nologo" }) start.ArgumentList.Add(arg);
    if (threaded) start.ArgumentList.Add("-mt");
    start.Environment["DOTNET_ADD_GLOBAL_TOOLS_TO_PATH"] = "0";
    start.Environment["DOTNET_CLI_TELEMETRY_OPTOUT"] = "1";
    // Exercise actual -mt thread nodes without starting/reusing another user's server.
    start.Environment["MSBUILDUSESERVER"] = "0";
    start.Environment.Remove("MSBUILDFORCEMULTITHREADED");
    start.Environment.Remove("MSBUILDENABLEMULTITHREADED");
    start.Environment.Remove("MSBUILDDISABLEFEATURESFROMVERSION");
    if (environmentDirectory == null) start.Environment.Remove("OBSERVATORY_REPORT_DIR");
    else start.Environment["OBSERVATORY_REPORT_DIR"] = environmentDirectory;
    string cwd = Environment.CurrentDirectory;
    string? value = Environment.GetEnvironmentVariable("OBSERVATORY_REPORT_DIR");
    Console.WriteLine($"COMMAND {dotnet} {string.Join(' ', start.ArgumentList)} [cwd={root}; child MSBUILDUSESERVER=0; OBSERVATORY_REPORT_DIR={environmentDirectory ?? "<unset>"}]");
    using var process = Process.Start(start)!;
    var stdout = process.StandardOutput.ReadToEndAsync();
    var stderr = process.StandardError.ReadToEndAsync();
    if (!process.WaitForExit(90000)) { process.Kill(true); throw new InvalidOperationException("MSBuild timed out."); }
    string output = stdout.GetAwaiter().GetResult() + stderr.GetAwaiter().GetResult();
    Console.WriteLine(output.Trim());
    Require((process.ExitCode == 0) == success, $"Unexpected MSBuild exit {process.ExitCode}");
    Require(Environment.CurrentDirectory == cwd && Environment.GetEnvironmentVariable("OBSERVATORY_REPORT_DIR") == value, "Child altered caller state");
    return output;
}

static void Require(bool value, string message)
{
    if (!value) throw new InvalidOperationException(message);
}
