using System.Diagnostics;
using System.Text.Json;
using Reqnroll;
using Xunit;

namespace PinguApps.Playbook.Tests.Steps;

[Binding]
public sealed class InstalledToolSteps : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), $"playbook-consumer-{Guid.NewGuid():N}");
    private Process? _server;
    private Task<string>? _diagnostics;
    private string? _executable;
    private int _requestId;
    private string _packageVariable = "PLAYBOOK_TEST_PACKAGE";

    [Given("the release contract fixture package is installed from an isolated local feed and cache")]
    public async Task InstallFixtureAsync()
    {
        _packageVariable = "PLAYBOOK_TEST_RELEASE_FIXTURE";
        await InstallPackageAsync().ConfigureAwait(false);
    }

    [Given("the actual package is installed from an isolated local feed and cache")]
    public async Task InstallPackageAsync()
    {
        string package = Environment.GetEnvironmentVariable(_packageVariable)
            ?? throw new InvalidOperationException("Run eng/Verify.ps1 to pack and provide PLAYBOOK_TEST_PACKAGE.");
        Assert.True(File.Exists(package), package);
        Directory.CreateDirectory(_directory);
        string feed = Path.Combine(_directory, "feed");
        Directory.CreateDirectory(feed);
        File.Copy(package, Path.Combine(feed, Path.GetFileName(package)));
        string config = Path.Combine(_directory, "NuGet.Config");
        await File.WriteAllTextAsync(config, "<configuration><packageSources><clear/><add key=\"local\" value=\"feed\"/></packageSources></configuration>").ConfigureAwait(false);
        string version = Environment.GetEnvironmentVariable("PLAYBOOK_TEST_VERSION")
            ?? throw new InvalidOperationException("Missing PLAYBOOK_TEST_VERSION.");
        ProcessStartInfo install = CreateStartInfo("dotnet", "tool", "install", "PinguApps.Playbook", "--version", version,
            "--tool-path", Path.Combine(_directory, "tool"), "--configfile", config, "--no-cache");
        (int exitCode, string stdout, string stderr) = await RunAsync(install).ConfigureAwait(false);
        Assert.True(exitCode == 0, stdout + stderr);
        _executable = Path.Combine(_directory, "tool", OperatingSystem.IsWindows() ? "playbook.exe" : "playbook");
        Assert.True(File.Exists(_executable));
        Assert.True(Directory.Exists(Path.Combine(_directory, "tool", ".store", "pinguapps.playbook", version)), "Installation must use this scenario's fresh tool store.");
        string runtimeConfig = Directory.GetFiles(Path.Combine(_directory, "tool", ".store"), "PinguApps.Playbook.runtimeconfig.json", SearchOption.AllDirectories).Single();
        using JsonDocument configuration = JsonDocument.Parse(await File.ReadAllTextAsync(runtimeConfig).ConfigureAwait(false));
        JsonElement runtime = configuration.RootElement.GetProperty("runtimeOptions");
        Assert.Equal("net10.0", runtime.GetProperty("tfm").GetString());
        Assert.Equal("Major", runtime.GetProperty("rollForward").GetString());
        Assert.Equal("Microsoft.NETCore.App", runtime.GetProperty("framework").GetProperty("name").GetString());
    }

    [Then("bare invocation help and version identify the installed command")]
    public async Task CheckCliAsync()
    {
        foreach (string[] args in new string[][] { [], ["--help"], ["--version"] })
        {
            (int exitCode, string stdout, string stderr) = await RunAsync(CreateStartInfo(_executable!, args)).ConfigureAwait(false);
            Assert.Equal(0, exitCode);
            Assert.Empty(stderr);
            Assert.Contains(args is ["--version"] ? Environment.GetEnvironmentVariable("PLAYBOOK_TEST_VERSION")! : "playbook mcp", stdout, StringComparison.Ordinal);
        }
    }

    [When("a client initializes the installed stdio server outside the checkout")]
    public async Task InitializeAsync()
    {
        ProcessStartInfo start = CreateStartInfo(_executable!, "mcp");
        start.RedirectStandardInput = true;
        _server = Process.Start(start) ?? throw new InvalidOperationException("Could not start installed tool.");
        _diagnostics = _server.StandardError.ReadToEndAsync();
        JsonElement initialized = await RequestAsync("initialize", new
        {
            protocolVersion = "2025-11-25",
            capabilities = new { },
            clientInfo = new { name = "Playbook packed consumer", version = "1.0.0" },
        }).ConfigureAwait(false);
        Assert.True(initialized.GetProperty("capabilities").TryGetProperty("tools", out _));
        await SendAsync(new { jsonrpc = "2.0", method = "notifications/initialized" }).ConfigureAwait(false);
    }

    [Then("exactly the two read-only tools advertise output schemas")]
    public async Task CheckToolsAsync()
    {
        JsonElement response = await RequestAsync("tools/list", new { }).ConfigureAwait(false);
        JsonElement[] tools = [.. response.GetProperty("tools").EnumerateArray()];
        Assert.Equal(["get_recipe", "list_recipes"], tools.Select(tool => tool.GetProperty("name").GetString()).Order(StringComparer.Ordinal));
        foreach (JsonElement tool in tools)
        {
            JsonElement annotations = tool.GetProperty("annotations");
            Assert.True(annotations.GetProperty("readOnlyHint").GetBoolean());
            Assert.False(annotations.GetProperty("destructiveHint").GetBoolean());
            Assert.False(annotations.GetProperty("openWorldHint").GetBoolean());
            Assert.Equal("object", tool.GetProperty("outputSchema").GetProperty("type").GetString());
        }
    }

    [Then("discovery and detail preserve complete structured and text results")]
    public async Task CheckContentAsync()
    {
        JsonElement list = AssertMatchingRepresentations(await CallAsync("list_recipes", new { targetVersion = "10.0" }).ConfigureAwait(false));
        Assert.Equal("10.0", list.GetProperty("resolvedTarget").GetString());
        Assert.False(list.GetProperty("targetAssumed").GetBoolean());
        JsonElement summary = Assert.Single(list.GetProperty("recipes").EnumerateArray());
        Assert.False(summary.TryGetProperty("markdown", out _));
        JsonElement route = summary.GetProperty("applicability").GetProperty("routes")[0];
        Assert.Equal("C# 14", route.GetProperty("language").GetString());
        Assert.Equal("10.0", route.GetProperty("target").GetString());
        string id = summary.GetProperty("id").GetString()!;
        JsonElement detail = AssertMatchingRepresentations(await CallAsync("get_recipe", new { id, targetVersion = "10.0" }).ConfigureAwait(false));
        Assert.False(detail.GetProperty("targetAssumed").GetBoolean());
        JsonElement recipe = detail.GetProperty("recipe");
        Assert.Equal(id, recipe.GetProperty("id").GetString());
        Assert.Equal("10.0", recipe.GetProperty("edition").GetString());
        Assert.Equal("stable", recipe.GetProperty("stability").GetString());
        Assert.Equal(summary.GetProperty("title").GetString(), recipe.GetProperty("title").GetString());
        Assert.Equal(summary.GetProperty("goal").GetString(), recipe.GetProperty("goal").GetString());
        Assert.True(JsonElement.DeepEquals(summary.GetProperty("applicability"), recipe.GetProperty("applicability")));
        string markdown = recipe.GetProperty("markdown").GetString()!;
        Assert.Contains("field = normalized;", markdown, StringComparison.Ordinal);
        Assert.Contains("_code = normalized;", markdown, StringComparison.Ordinal);
        Assert.Contains("does **not** run the setter", markdown, StringComparison.Ordinal);
        Assert.NotEmpty(recipe.GetProperty("applicability").GetProperty("routes")[0].GetProperty("constraints").EnumerateArray());
        Assert.NotEmpty(recipe.GetProperty("applicability").GetProperty("routes")[0].GetProperty("prerequisites").EnumerateArray());
        Assert.NotEmpty(route.GetProperty("sourceUrls").EnumerateArray());
        Assert.NotEmpty(route.GetProperty("evidenceSamples").EnumerateArray());
        Assert.NotEmpty(recipe.GetProperty("sources").EnumerateArray());
        Assert.Equal(["net9.0", "net10.0", "net10.0"], recipe.GetProperty("evidence").EnumerateArray().Select(evidence => evidence.GetProperty("target").GetString()));
    }

    [Then("the served examples match the canonical compiled sample sources")]
    public async Task CheckSnippetsAsync()
    {
        JsonElement detail = AssertMatchingRepresentations(await CallAsync("get_recipe", new { id = "csharp.validate-property-assignment" }).ConfigureAwait(false));
        string markdown = detail.GetProperty("recipe").GetProperty("markdown").GetString()!;
        foreach (string target in new[] { "net9.0", "net10.0" })
        {
            using Stream stream = typeof(InstalledToolSteps).Assembly.GetManifestResourceStream($"Canonical.{target}.StorageBay.cs")!;
            using StreamReader reader = new(stream);
            string source = await reader.ReadToEndAsync().ConfigureAwait(false);
            string snippet = source.Split("// snippet:storage-bay", StringSplitOptions.None)[1]
                .Split("// endsnippet:storage-bay", StringSplitOptions.None)[0].Trim().Replace("\r\n", "\n", StringComparison.Ordinal);
            Assert.Contains(snippet, markdown, StringComparison.Ordinal);
        }
    }

    [Then("an explicit undeclared target fails instead of returning newer guidance")]
    public async Task RejectTargetAsync()
    {
        JsonElement result = await CallAsync("get_recipe", new { id = "csharp.validate-property-assignment", targetVersion = "9.0" }).ConfigureAwait(false);
        AssertToolError(result, "unsupported_target");
    }

    [Then("release aliases select compatible retained editions with complete requirements")]
    public async Task CheckReleasesAsync()
    {
        foreach (string release in new[] { "10", "10.0", "net10.0", "11", "11.0", "net11.0" })
        {
            string target = release.Contains("10", StringComparison.Ordinal) ? "10.0" : "11.0";
            JsonElement list = AssertMatchingRepresentations(await CallAsync("list_recipes", new { targetVersion = release }).ConfigureAwait(false));
            Assert.Equal(target, list.GetProperty("resolvedTarget").GetString());
            Assert.False(list.GetProperty("targetAssumed").GetBoolean());
            Assert.Contains("csharp", list.GetProperty("supportedAreas").EnumerateArray().Select(item => item.GetString()));
            Assert.Contains("csharp", list.GetProperty("supportedExampleLanguages").EnumerateArray().Select(item => item.GetString()));
            JsonElement detail = AssertMatchingRepresentations(await CallAsync("get_recipe", new { id = "csharp.validate-property-assignment", targetVersion = release }).ConfigureAwait(false));
            JsonElement recipe = detail.GetProperty("recipe");
            Assert.Equal(target, recipe.GetProperty("edition").GetString());
            Assert.Equal("10.0", recipe.GetProperty("introducedIn").GetString());
            JsonElement routes = recipe.GetProperty("applicability").GetProperty("routes");
            Assert.All(routes.EnumerateArray(), route => Assert.Equal(target, route.GetProperty("target").GetString()));
            JsonElement route = routes[0];
            foreach (string field in new[] { "runtime", "sdk", "language", "frameworks", "packages", "packageAssets", "providers", "platforms", "renderingModes", "prerequisites", "constraints", "sourceUrls", "evidenceSamples", "optIns", "stabilityLimitations" })
            {
                Assert.True(route.TryGetProperty(field, out _), field);
            }

            Assert.True(JsonElement.DeepEquals(routes, list.GetProperty("recipes")[0].GetProperty("applicability").GetProperty("routes")));
            if (target == "10.0")
            {
                JsonElement conditional = Assert.Single(routes.EnumerateArray(), item => item.GetProperty("conditional").GetBoolean());
                Assert.Contains("11.0.100-rc.2.26504.105", conditional.GetProperty("sdk").GetString(), StringComparison.Ordinal);
                Assert.Contains("C# 14", conditional.GetProperty("language").GetString(), StringComparison.Ordinal);
                Assert.Contains(".NET 11 SDK", recipe.GetProperty("markdown").GetString(), StringComparison.Ordinal);
            }
        }
    }

    [Then("omitted targets disclose the newest installed release and coverage limits")]
    public async Task CheckDefaultsAsync()
    {
        foreach (string tool in new[] { "list_recipes", "get_recipe" })
        {
            JsonElement result = AssertMatchingRepresentations(await CallAsync(tool, new { id = "csharp.validate-property-assignment" }).ConfigureAwait(false));
            Assert.Equal("11.0", result.GetProperty("resolvedTarget").GetString());
            Assert.True(result.GetProperty("targetAssumed").GetBoolean());
            Assert.Equal(["10.0", "11.0"], result.GetProperty("supportedTargets").EnumerateArray().Select(item => item.GetString()));
            JsonElement[] coverage = [.. result.GetProperty("coverage").EnumerateArray()];
            Assert.Equal(["curated", "comprehensive"], coverage.Select(item => item.GetProperty("scope").GetString()));
            Assert.All(coverage, item => Assert.False(item.GetProperty("complete").GetBoolean()));
        }
    }

    [Then("invalid requests return actionable machine-readable tool failures")]
    public async Task CheckFailuresAsync()
    {
        foreach (string tool in new[] { "list_recipes", "get_recipe" })
        {
            foreach (string target in new[] { "9.0", "12.0", "10.0.1", "11.0.0-rc.2", "net10.0-windows", "", " " })
            {
                JsonElement error = AssertToolError(await CallAsync(tool, new { id = "csharp.validate-property-assignment", targetVersion = target }).ConfigureAwait(false), "unsupported_target");
                Assert.Equal(target, error.GetProperty("requestedTarget").GetString());
                Assert.Equal(["10.0", "11.0"], error.GetProperty("supportedTargets").EnumerateArray().Select(item => item.GetString()));
                Assert.Contains("platform", error.GetProperty("message").GetString(), StringComparison.Ordinal);
            }
        }

        JsonElement unknown = AssertToolError(await CallAsync("get_recipe", new { id = "missing.goal", targetVersion = "10" }).ConfigureAwait(false), "unknown_recipe");
        Assert.Equal("missing.goal", unknown.GetProperty("recipeId").GetString());
    }

    private static JsonElement AssertToolError(JsonElement result, string code)
    {
        Assert.True(result.GetProperty("isError").GetBoolean());
        using JsonDocument document = JsonDocument.Parse(Assert.Single(result.GetProperty("content").EnumerateArray()).GetProperty("text").GetString()!);
        JsonElement error = document.RootElement;
        Assert.Equal(code, error.GetProperty("code").GetString());
        Assert.False(string.IsNullOrWhiteSpace(error.GetProperty("message").GetString()));
        Assert.False(result.TryGetProperty("structuredContent", out _));
        return error.Clone();
    }

    [Then("a known incompatible recipe fails and a conditional older-target route stays explicit")]
    public async Task CheckConditionalFixtureAsync()
    {
        JsonElement error = AssertToolError(await CallAsync("get_recipe", new { id = "fixture.only-eleven", targetVersion = "net10.0" }).ConfigureAwait(false), "no_applicable_edition");
        Assert.Equal("net10.0", error.GetProperty("requestedTarget").GetString());
        Assert.Equal(["11.0"], error.GetProperty("availableEditions").EnumerateArray().Select(item => item.GetString()));
        JsonElement list = AssertMatchingRepresentations(await CallAsync("list_recipes", new { targetVersion = "10" }).ConfigureAwait(false));
        Assert.DoesNotContain(list.GetProperty("recipes").EnumerateArray(), recipe => recipe.GetProperty("id").GetString() == "fixture.only-eleven");
        JsonElement detail = AssertMatchingRepresentations(await CallAsync("get_recipe", new { id = "fixture.conditional-route", targetVersion = "10" }).ConfigureAwait(false));
        JsonElement recipe = detail.GetProperty("recipe");
        Assert.Equal("11.0", recipe.GetProperty("introducedIn").GetString());
        Assert.Equal("10.0", detail.GetProperty("resolvedTarget").GetString());
        JsonElement route = Assert.Single(recipe.GetProperty("applicability").GetProperty("routes").EnumerateArray());
        Assert.True(route.GetProperty("conditional").GetBoolean());
        Assert.Contains("11.0.100-rc.2.26504.105", route.GetProperty("sdk").GetString(), StringComparison.Ordinal);
        Assert.NotEmpty(route.GetProperty("sourceUrls").EnumerateArray());
        Assert.NotEmpty(route.GetProperty("evidenceSamples").EnumerateArray());
        JsonElement newest = AssertMatchingRepresentations(await CallAsync("get_recipe", new { id = "csharp.validate-property-assignment", targetVersion = "11" }).ConfigureAwait(false));
        Assert.Equal("11.0", newest.GetProperty("recipe").GetProperty("edition").GetString());
    }

    [When("the client closes its input")]
    public void CloseInput()
    {
        _server!.StandardInput.Close();
    }

    [Then("the server exits cleanly with diagnostics on stderr")]
    public async Task CheckShutdownAsync()
    {
        using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(15));
        await _server!.WaitForExitAsync(timeout.Token).ConfigureAwait(false);
        Assert.Equal(0, _server.ExitCode);
        Assert.Empty(await _server.StandardOutput.ReadToEndAsync(timeout.Token).ConfigureAwait(false));
        Assert.NotEmpty(await _diagnostics!.ConfigureAwait(false));
    }

    public void Dispose()
    {
        if (_server is { HasExited: false })
        {
            _server.Kill(entireProcessTree: true);
            _server.WaitForExit();
        }

        _server?.Dispose();
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }

    private static JsonElement AssertMatchingRepresentations(JsonElement result)
    {
        Assert.False(result.TryGetProperty("isError", out JsonElement error) && error.GetBoolean());
        JsonElement structured = result.GetProperty("structuredContent");
        using JsonDocument fallback = JsonDocument.Parse(Assert.Single(result.GetProperty("content").EnumerateArray()).GetProperty("text").GetString()!);
        Assert.True(JsonElement.DeepEquals(structured, fallback.RootElement), "The JSON text fallback must retain the entire structured object.");
        return structured;
    }

    private ProcessStartInfo CreateStartInfo(string file, params string[] args)
    {
        ProcessStartInfo info = new(file)
        {
            WorkingDirectory = _directory,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        };
        foreach (string arg in args)
        {
            info.ArgumentList.Add(arg);
        }

        info.Environment["NUGET_PACKAGES"] = Path.Combine(_directory, "packages");
        info.Environment["NUGET_HTTP_CACHE_PATH"] = Path.Combine(_directory, "http-cache");
        info.Environment["DOTNET_CLI_HOME"] = _directory;
        info.Environment["DOTNET_NOLOGO"] = "1";
        info.Environment["DOTNET_SKIP_FIRST_TIME_EXPERIENCE"] = "1";
        info.Environment["DOTNET_CLI_TELEMETRY_OPTOUT"] = "1";
        info.Environment["HTTP_PROXY"] = "http://127.0.0.1:1";
        info.Environment["HTTPS_PROXY"] = "http://127.0.0.1:1";
        info.Environment["ALL_PROXY"] = "http://127.0.0.1:1";
        return info;
    }

    private async Task<JsonElement> CallAsync(string name, object arguments)
    {
        return await RequestAsync("tools/call", new { name, arguments }).ConfigureAwait(false);
    }

    private async Task<JsonElement> RequestAsync(string method, object parameters)
    {
        int id = ++_requestId;
        await SendAsync(new { jsonrpc = "2.0", id, method, @params = parameters }).ConfigureAwait(false);
        using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(20));
        while (true)
        {
            string line = await _server!.StandardOutput.ReadLineAsync(timeout.Token).ConfigureAwait(false)
                ?? throw new InvalidOperationException("Server closed stdout before replying.");
            using JsonDocument document = JsonDocument.Parse(line);
            JsonElement response = document.RootElement;
            Assert.Equal("2.0", response.GetProperty("jsonrpc").GetString());
            if (response.TryGetProperty("id", out JsonElement responseId) && responseId.GetInt32() == id)
            {
                Assert.False(response.TryGetProperty("error", out _), line);
                return response.GetProperty("result").Clone();
            }
        }
    }

    private async Task SendAsync(object message)
    {
        await _server!.StandardInput.WriteLineAsync(JsonSerializer.Serialize(message)).ConfigureAwait(false);
        await _server.StandardInput.FlushAsync().ConfigureAwait(false);
    }

    private static async Task<(int ExitCode, string Stdout, string Stderr)> RunAsync(ProcessStartInfo info)
    {
        using Process process = Process.Start(info) ?? throw new InvalidOperationException("Could not start process.");
        using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(60));
        Task<string> stdout = process.StandardOutput.ReadToEndAsync(timeout.Token);
        Task<string> stderr = process.StandardError.ReadToEndAsync(timeout.Token);
        try
        {
            await process.WaitForExitAsync(timeout.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            process.Kill(entireProcessTree: true);
            throw;
        }

        return (process.ExitCode, await stdout.ConfigureAwait(false), await stderr.ConfigureAwait(false));
    }
}
