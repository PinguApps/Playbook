using System.Diagnostics;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Reqnroll;
using Xunit;

namespace PinguApps.Playbook.Tests.Steps;

[Binding]
public sealed class InstalledToolSteps : IDisposable
{
    private readonly IReqnrollOutputHelper _output;
    private readonly string _directory = Path.Combine(Path.GetTempPath(), $"playbook-consumer-{Guid.NewGuid():N}");
    private Process? _server;
    private Task<string>? _diagnostics;
    private string? _executable;
    private int _requestId;
    private string _packageVariable = "PLAYBOOK_TEST_PACKAGE";

    public InstalledToolSteps(IReqnrollOutputHelper output)
    {
        _output = output;
    }

    [Then("SDK workflow guidance retains complete bodies editions and honest proof boundaries")]
    public async Task CheckSdkWorkflowsAsync()
    {
        string[] ids = ["sdk.run-file-utility", "sdk.compose-file-apps", "sdk.filter-solutions", "sdk.format-targeted-code", "sdk.pass-launch-settings", "sdk.keep-build-configuration-explicit", "sdk.install-preview-templates", "sdk.watch-app-hosts-and-devices"];
        foreach (string target in new[] { "10.0", "11.0" })
        {
            JsonElement catalogue = AssertMatchingRepresentations(await CallAsync("list_recipes", new { targetVersion = target, query = "sdk-workflows", limit = 50 }).ConfigureAwait(false));
            Assert.Equal(target == "10.0" ? 2 : 8, catalogue.GetProperty("total").GetInt32());
            foreach (string id in ids)
            {
                if (target == "10.0" && id is not "sdk.run-file-utility" and not "sdk.compose-file-apps")
                {
                    AssertToolError(await CallAsync("get_recipe", new { id, targetVersion = target }).ConfigureAwait(false), "no_applicable_edition");
                    continue;
                }

                string edition = id == "sdk.run-file-utility" ? "10.0" : "11.0";
                string path = $"content/recipes/{id}/{edition}";
                using JsonDocument metadata = JsonDocument.Parse(await ReadSdkInputAsync($"{path}/recipe.json").ConfigureAwait(false));
                string expected = await ReadSdkInputAsync($"{path}/recipe.md").ConfigureAwait(false);
                foreach (JsonProperty snippet in metadata.RootElement.GetProperty("snippets").EnumerateObject())
                {
                    string source = await ReadSdkInputAsync(snippet.Value.GetProperty("path").GetString()!).ConfigureAwait(false);
                    string region = System.Text.RegularExpressions.Regex.Escape(snippet.Value.GetProperty("region").GetString()!);
                    System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(source, $"(?ms)^[ \\t]*(?:// snippet:{region}|<!-- snippet:{region} -->)\\n(.*?)^[ \\t]*(?:// endsnippet:{region}|<!-- endsnippet:{region} -->)$");
                    Assert.True(match.Success, $"Missing canonical region {id}/{snippet.Name}");
                    string language = snippet.Value.GetProperty("language").GetString()!;
                    expected = expected.Replace($"{{{{snippet:{snippet.Name}}}}}", $"```{language}\n{match.Groups[1].Value.TrimEnd()}\n```", StringComparison.Ordinal);
                }

                JsonElement recipe = AssertMatchingRepresentations(await CallAsync("get_recipe", new { id, targetVersion = target }).ConfigureAwait(false)).GetProperty("recipe");
                JsonElement summary = Assert.Single(catalogue.GetProperty("recipes").EnumerateArray(), entry => entry.GetProperty("id").GetString() == id);
                Assert.Equal(metadata.RootElement.GetProperty("goal").GetString(), summary.GetProperty("goal").GetString());
                Assert.Equal(edition, recipe.GetProperty("edition").GetString());
                Assert.Equal(expected, recipe.GetProperty("markdown").GetString());
                AssertSelection(summary, recipe);
                JsonElement route = Assert.Single(recipe.GetProperty("applicability").GetProperty("routes").EnumerateArray());
                Assert.Equal(target, route.GetProperty("target").GetString());
                Assert.Equal(id == "sdk.compose-file-apps" ? "11.0" : target, route.GetProperty("requirements").GetProperty("sdk").GetProperty("version").GetString());
                Assert.Equal(id == "sdk.compose-file-apps" && target == "10.0", route.GetProperty("conditional").GetBoolean());
                Assert.All(recipe.GetProperty("evidence").EnumerateArray(), evidence => Assert.Equal("current", evidence.GetProperty("freshness").GetString()));
                bool pending = id == "sdk.watch-app-hosts-and-devices";
                Assert.Equal(pending ? "pending" : "not-required", recipe.GetProperty("humanProof").GetProperty("status").GetString());
                if (pending)
                {
                    Assert.Contains("blocks publication", summary.ToString(), StringComparison.Ordinal);
                    Assert.Contains("no compiled mobile sample", recipe.GetProperty("markdown").GetString(), StringComparison.Ordinal);
                }
            }
        }
    }

    [Then("SDK delivery recipes retain complete canonical bodies applicability and target editions")]
    public async Task CheckSdkDeliveryAsync()
    {
        string[] ids = ["sdk.configure-test-runs", "sdk.create-test-runner-projects", "sdk.package-tools-correctly", "sdk.migrate-single-file-output", "sdk.declare-static-asset-groups"];
        JsonElement catalogue = AssertMatchingRepresentations(await CallAsync("list_recipes", new { targetVersion = "11.0", query = "sdk-delivery", limit = 50 }).ConfigureAwait(false));
        Assert.Equal(ids.Length, catalogue.GetProperty("total").GetInt32());
        foreach (string id in ids)
        {
            string path = $"content/recipes/{id}/11.0";
            using JsonDocument metadata = JsonDocument.Parse(await ReadSdkInputAsync($"{path}/recipe.json").ConfigureAwait(false));
            string expected = await ReadSdkInputAsync($"{path}/recipe.md").ConfigureAwait(false);
            foreach (JsonProperty snippet in metadata.RootElement.GetProperty("snippets").EnumerateObject())
            {
                string source = await ReadSdkInputAsync(snippet.Value.GetProperty("path").GetString()!).ConfigureAwait(false);
                string region = System.Text.RegularExpressions.Regex.Escape(snippet.Value.GetProperty("region").GetString()!);
                System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(source, $"(?ms)^[ \\t]*(?:// snippet:{region}|<!-- snippet:{region} -->)\\n(.*?)^[ \\t]*(?:// endsnippet:{region}|<!-- endsnippet:{region} -->)$");
                Assert.True(match.Success, $"Missing canonical region {id}/{snippet.Name}");
                string language = snippet.Value.GetProperty("language").GetString()!;
                expected = expected.Replace($"{{{{snippet:{snippet.Name}}}}}", $"```{language}\n{match.Groups[1].Value.TrimEnd()}\n```", StringComparison.Ordinal);
            }

            JsonElement recipe = AssertMatchingRepresentations(await CallAsync("get_recipe", new { id, targetVersion = "11.0" }).ConfigureAwait(false)).GetProperty("recipe");
            JsonElement summary = Assert.Single(catalogue.GetProperty("recipes").EnumerateArray(), item => item.GetProperty("id").GetString() == id);
            Assert.Equal(metadata.RootElement.GetProperty("goal").GetString(), summary.GetProperty("goal").GetString());
            Assert.Equal("11.0", recipe.GetProperty("edition").GetString());
            Assert.Equal(expected, recipe.GetProperty("markdown").GetString());
            AssertSelection(summary, recipe);
            Assert.All(recipe.GetProperty("applicability").GetProperty("routes").EnumerateArray(), route =>
            {
                Assert.Equal("11.0", route.GetProperty("target").GetString());
                Assert.Equal("11.0", route.GetProperty("requirements").GetProperty("sdk").GetProperty("version").GetString());
                Assert.False(route.GetProperty("conditional").GetBoolean());
            });
            Assert.All(recipe.GetProperty("evidence").EnumerateArray(), evidence => Assert.Equal("current", evidence.GetProperty("freshness").GetString()));
            Assert.Equal("not-required", recipe.GetProperty("humanProof").GetProperty("status").GetString());
            AssertToolError(await CallAsync("get_recipe", new { id, targetVersion = "10.0" }).ConfigureAwait(false), "no_applicable_edition");
        }

        JsonElement older = AssertMatchingRepresentations(await CallAsync("list_recipes", new { targetVersion = "10.0", query = "sdk-delivery" }).ConfigureAwait(false));
        Assert.Equal(0, older.GetProperty("total").GetInt32());
    }

    private static async Task<string> ReadSdkInputAsync(string path)
    {
        System.Reflection.Assembly assembly = typeof(InstalledToolSteps).Assembly;
        string name = Assert.Single(assembly.GetManifestResourceNames(), resource => resource.StartsWith("SdkInput.", StringComparison.Ordinal) && resource.Replace('\\', '/').EndsWith(path, StringComparison.Ordinal));
        await using Stream stream = assembly.GetManifestResourceStream(name)!;
        using StreamReader reader = new(stream);
        return (await reader.ReadToEndAsync().ConfigureAwait(false)).Replace("\r\n", "\n", StringComparison.Ordinal);
    }

    [Then("theoretical SDK and build recipes retain complete bodies and source-only proof")]
    public async Task CheckTheoreticalBuildGuidanceAsync()
    {
        string[] ids = ["sdk.select-local-container-engine", "sdk.publish-reproducible-containers", "sdk.publish-multiarch-with-podman", "sdk.configure-cli-telemetry", "msbuild.archive-build-artifacts", "msbuild.inspect-item-globs"];
        JsonElement catalogue = AssertMatchingRepresentations(await CallAsync("list_recipes", new { targetVersion = "11.0", query = "sdk-build-artifacts", limit = 50 }).ConfigureAwait(false));
        Assert.Equal(ids.Length, catalogue.GetProperty("total").GetInt32());
        foreach (string id in ids)
        {
            string path = $"content/recipes/{id}/11.0";
            using JsonDocument metadata = JsonDocument.Parse(await ReadSdkInputAsync($"{path}/recipe.json").ConfigureAwait(false));
            string expected = await ReadSdkInputAsync($"{path}/recipe.md").ConfigureAwait(false);
            foreach (JsonProperty snippet in metadata.RootElement.GetProperty("snippets").EnumerateObject())
            {
                string source = await ReadSdkInputAsync(snippet.Value.GetProperty("path").GetString()!).ConfigureAwait(false);
                string region = System.Text.RegularExpressions.Regex.Escape(snippet.Value.GetProperty("region").GetString()!);
                System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(source, $"(?ms)^[ \\t]*(?:// snippet:{region}|<!-- snippet:{region} -->)\\n(.*?)^[ \\t]*(?:// endsnippet:{region}|<!-- endsnippet:{region} -->)$");
                Assert.True(match.Success, $"Missing canonical region {id}/{snippet.Name}");
                string language = snippet.Value.GetProperty("language").GetString()!;
                expected = expected.Replace($"{{{{snippet:{snippet.Name}}}}}", $"```{language}\n{match.Groups[1].Value.TrimEnd()}\n```", StringComparison.Ordinal);
            }

            JsonElement recipe = AssertMatchingRepresentations(await CallAsync("get_recipe", new { id, targetVersion = "11.0" }).ConfigureAwait(false)).GetProperty("recipe");
            JsonElement summary = Assert.Single(catalogue.GetProperty("recipes").EnumerateArray(), item => item.GetProperty("id").GetString() == id);
            Assert.Equal(metadata.RootElement.GetProperty("goal").GetString(), summary.GetProperty("goal").GetString());
            Assert.Equal("11.0", recipe.GetProperty("edition").GetString());
            Assert.Equal(expected, recipe.GetProperty("markdown").GetString());
            AssertSelection(summary, recipe);
            Assert.Equal("pending", recipe.GetProperty("humanProof").GetProperty("status").GetString());
            Assert.Contains("Theoretical guidance", summary.ToString(), StringComparison.Ordinal);
            Assert.All(recipe.GetProperty("evidence").EnumerateArray(), evidence =>
            {
                Assert.Equal("official-review", evidence.GetProperty("kind").GetString());
                Assert.Equal("source-review", evidence.GetProperty("role").GetString());
            });
            JsonElement route = Assert.Single(recipe.GetProperty("applicability").GetProperty("routes").EnumerateArray());
            Assert.Equal("11.0", route.GetProperty("target").GetString());
            Assert.False(route.GetProperty("conditional").GetBoolean());
            AssertToolError(await CallAsync("get_recipe", new { id, targetVersion = "10.0" }).ConfigureAwait(false), "no_applicable_edition");
        }

        JsonElement older = AssertMatchingRepresentations(await CallAsync("list_recipes", new { targetVersion = "10.0", query = "sdk-build-artifacts" }).ConfigureAwait(false));
        Assert.Equal(0, older.GetProperty("total").GetInt32());
        string[] neutralLanguages = ["fsharp"];
        JsonElement neutral = AssertMatchingRepresentations(await CallAsync("list_recipes", new { targetVersion = "11.0", query = "sdk-build-artifacts", languages = neutralLanguages }).ConfigureAwait(false));
        Assert.Equal("msbuild.inspect-item-globs", Assert.Single(neutral.GetProperty("recipes").EnumerateArray()).GetProperty("id").GetString());
    }

    [Then("process and data recipes retain full bodies applicability and target editions")]
    public async Task CheckProcessAndDataGuidanceAsync()
    {
        string[] areas = ["libraries"];
        string[] languages = ["csharp"];
        JsonElement list = AssertMatchingRepresentations(await CallAsync("list_recipes", new
        {
            targetVersion = "net11.0",
            areas,
            languages,
            limit = 50,
        }).ConfigureAwait(false));
        foreach ((string id, string file, string region) in new[]
        {
            ("process.capture-output", "Commands", "capture"),
            ("process.control-lifetime", "Commands", "lifetime"),
            ("console.color-piped-output", "Commands", "color"),
            ("process.control-native-handles", "Commands", "handles"),
            ("process.prepare-before-start", "Commands", "prepare"),
            ("process.inspect-termination", "Commands", "termination"),
            ("io.adapt-existing-buffers", "Adapters", "streams"),
            ("text.transfer-builder-content", "Adapters", "transfer"),
            ("text.handle-unicode-scalars", "TextRules", "scalars"),
            ("text.compare-characters", "TextRules", "compare"),
            ("text.locate-invalid-unicode", "TextRules", "invalid"),
            ("regex.handle-unicode-newlines", "TextRules", "newlines"),
            ("text.encode-base64", "Telemetry", "base64"),
            ("cbor.read-chunks", "Telemetry", "cbor"),
        })
        {
            JsonElement summary = Assert.Single(list.GetProperty("recipes").EnumerateArray(), value => value.GetProperty("id").GetString() == id);
            Assert.NotEmpty(summary.GetProperty("goal").GetString()!);
            JsonElement result = AssertMatchingRepresentations(await CallAsync("get_recipe", new { id, targetVersion = "11.0" }).ConfigureAwait(false));
            JsonElement recipe = result.GetProperty("recipe");
            Assert.Equal("11.0", result.GetProperty("resolvedTarget").GetString());
            Assert.Equal("11.0", recipe.GetProperty("edition").GetString());
            Assert.Equal("11.0", recipe.GetProperty("introducedIn").GetString());
            AssertSelection(summary, recipe);
            string expected = await ReadCanonicalAsync($"{id}.md").ConfigureAwait(false);
            foreach ((string token, string target) in new[] { ("older", "net10.0"), ("newer", "net11.0") })
            {
                string source = await ReadCanonicalAsync($"{target}.{file}.cs").ConfigureAwait(false);
                string snippet = source.Split($"// snippet:{region}\n", StringSplitOptions.None)[1]
                    .Split($"// endsnippet:{region}", StringSplitOptions.None)[0].TrimEnd();
                expected = expected.Replace($"{{{{snippet:{token}}}}}", $"```csharp\n{snippet}\n```", StringComparison.Ordinal);
            }

            Assert.Equal(expected, recipe.GetProperty("markdown").GetString());
            Assert.DoesNotContain("{{snippet:", expected, StringComparison.Ordinal);
            JsonElement requirements = Assert.Single(recipe.GetProperty("applicability").GetProperty("routes").EnumerateArray()).GetProperty("requirements");
            Assert.Equal("11.0", requirements.GetProperty("runtime").GetProperty("version").GetString());
            Assert.NotEmpty(requirements.GetProperty("platforms").EnumerateArray());
            Assert.NotEmpty(requirements.GetProperty("constraints").EnumerateArray());
            Assert.All(recipe.GetProperty("evidence").EnumerateArray(), evidence =>
            {
                Assert.Equal("current", evidence.GetProperty("freshness").GetString());
                Assert.Equal("compile-and-run", evidence.GetProperty("kind").GetString());
                Assert.Contains("PASS:", evidence.GetProperty("output").GetString(), StringComparison.Ordinal);
            });
            if (file == "Commands")
            {
                Assert.Equal(3, recipe.GetProperty("evidence").GetArrayLength());
                Assert.Equal("pending", recipe.GetProperty("humanProof").GetProperty("status").GetString());
                Assert.Contains("macOS", requirements.GetProperty("stabilityLimitations").ToString(), StringComparison.Ordinal);
                Assert.Contains(recipe.GetProperty("evidence").EnumerateArray(), evidence => evidence.GetProperty("host").GetString()!.Contains("Ubuntu", StringComparison.Ordinal));
            }

            if (id == "cbor.read-chunks")
            {
                Assert.Contains("Lax", expected, StringComparison.Ordinal);
                Assert.Contains("lib/net11.0", requirements.GetProperty("packageAssets").ToString(), StringComparison.Ordinal);
            }

            AssertToolError(await CallAsync("get_recipe", new { id, targetVersion = "10.0" }).ConfigureAwait(false), "no_applicable_edition");
        }
    }

    [Then("F sharp guidance retains complete bodies constraints and editions")]
    public async Task CheckFSharpGuidanceAsync()
    {
        string[] languages = ["fsharp"];
        JsonElement list = AssertMatchingRepresentations(await CallAsync("list_recipes", new { targetVersion = "net11.0", languages }).ConfigureAwait(false));
        foreach ((string id, string sample, string file, string region) in new[]
        {
            ("fsharp.compose-records", "FSharpModels", "Models", "compose"),
            ("fsharp.construct-records", "FSharpConstructors", "Constructors", "construct"),
            ("json.serialize-fsharp-unions", "FSharpModels", "Models", "json"),
            ("fsharp.preserve-interpolation-culture", "FSharpDiagnostics", "Diagnostics", "culture"),
            ("fsharp.format-without-reflection", "FSharpDiagnostics", "Diagnostics", "reflectionfree"),
            ("fsharp.pass-methods-as-delegates", "FSharpApi", "Api", "delegates"),
            ("fsharp.reuse-api-documentation", "FSharpApi", "Api", "documentation"),
            ("fsharp.adapt-generic-operations", "FSharpApiPreview", "ApiPreview", "generic"),
            ("fsharp.require-named-arguments", "FSharpApiPreview", "ApiPreview", "named"),
            ("fsharp.start-async-on-caller", "FSharpAsync", "AsyncWork", "start"),
            ("fsharp.bound-async-work", "FSharpAsync", "AsyncWork", "bounded"),
            ("fsharp.rethrow-in-computations", "FSharpRethrow", "Rethrow", "rethrow"),
            ("fsharp.migrate-enum-flags", "FSharpFlags", "Flags", "flags")
        })
        {
            JsonElement summary = Assert.Single(list.GetProperty("recipes").EnumerateArray(), value => value.GetProperty("id").GetString() == id);
            Assert.NotEmpty(summary.GetProperty("goal").GetString()!);
            JsonElement detail = AssertMatchingRepresentations(await CallAsync("get_recipe", new { id, targetVersion = "11.0" }).ConfigureAwait(false));
            Assert.Equal("11.0", detail.GetProperty("resolvedTarget").GetString());
            JsonElement recipe = detail.GetProperty("recipe");
            Assert.Equal("11.0", recipe.GetProperty("edition").GetString());
            Assert.Equal("11.0", recipe.GetProperty("introducedIn").GetString());
            AssertSelection(summary, recipe);
            string expected = await ReadCanonicalAsync($"{id}.md").ConfigureAwait(false);
            foreach ((string token, string target) in new[] { ("older", "net10.0"), ("newer", "net11.0") })
            {
                string source = await ReadCanonicalAsync($"{target}.{file}.fs").ConfigureAwait(false);
                string snippet = source.Split($"// snippet:{region}\n", StringSplitOptions.None)[1]
                    .Split($"// endsnippet:{region}", StringSplitOptions.None)[0].TrimEnd();
                expected = expected.Replace($"{{{{snippet:{token}}}}}", $"```fsharp\n{snippet}\n```", StringComparison.Ordinal);
            }

            if (id == "fsharp.require-named-arguments")
            {
                string source = await ReadCanonicalAsync("RequireNamedArguments.fs").ConfigureAwait(false);
                string attribute = source.Split("// snippet:attribute\n", StringSplitOptions.None)[1]
                    .Split("// endsnippet:attribute", StringSplitOptions.None)[0].TrimEnd();
                expected = expected.Replace("{{snippet:attribute}}", $"```fsharp\n{attribute}\n```", StringComparison.Ordinal);
                Assert.Contains("RC2 BCL does not ship", expected, StringComparison.Ordinal);
                Assert.Contains("FS3923", expected, StringComparison.Ordinal);
            }

            if (id == "fsharp.reuse-api-documentation")
            {
                string source = await ReadCanonicalAsync("ApiDocs.xml").ConfigureAwait(false);
                string xml = source.Split("<!-- snippet:external-doc -->\n", StringSplitOptions.None)[1]
                    .Split("<!-- endsnippet:external-doc -->", StringSplitOptions.None)[0].TrimEnd();
                expected = expected.Replace("{{snippet:external-doc}}", $"```xml\n{xml}\n```", StringComparison.Ordinal);
                Assert.Contains("GenerateDocumentationFile=true", expected, StringComparison.Ordinal);
                Assert.Contains("leaves inheritdoc unexpanded", expected, StringComparison.Ordinal);
            }

            Assert.Equal(expected, recipe.GetProperty("markdown").GetString());
            Assert.DoesNotContain("{{snippet:", expected, StringComparison.Ordinal);
            JsonElement requirements = Assert.Single(recipe.GetProperty("applicability").GetProperty("routes").EnumerateArray()).GetProperty("requirements");
            Assert.Equal("11.0", requirements.GetProperty("runtime").GetProperty("version").GetString());
            Assert.Equal("11.0", requirements.GetProperty("sdk").GetProperty("version").GetString());
            foreach (string field in new[] { "packages", "packageAssets", "platforms", "constraints" })
            {
                Assert.NotEmpty(requirements.GetProperty(field).EnumerateArray());
            }

            JsonElement core = Assert.Single(requirements.GetProperty("packages").EnumerateArray());
            Assert.Equal("11.0", core.GetProperty("version").GetString());
            Assert.Equal("release-family", core.GetProperty("match").GetString());
            Assert.Equal(2, recipe.GetProperty("evidence").GetArrayLength());
            Assert.All(recipe.GetProperty("evidence").EnumerateArray(), evidence =>
            {
                Assert.Equal("current", evidence.GetProperty("freshness").GetString());
                Assert.Equal("compile-and-run", evidence.GetProperty("kind").GetString());
                Assert.Contains("PASS:", evidence.GetProperty("output").GetString(), StringComparison.Ordinal);
                Assert.Contains(sample, evidence.GetProperty("sample").GetString(), StringComparison.Ordinal);
            });
            if (id is "fsharp.construct-records" or "fsharp.adapt-generic-operations" or "fsharp.require-named-arguments" or "fsharp.rethrow-in-computations")
            {
                Assert.Equal("experimental", summary.GetProperty("stability").GetString());
                Assert.Equal("preview", requirements.GetProperty("language").GetProperty("version").GetString());
                Assert.Contains("LangVersion=preview", requirements.GetProperty("optIns").ToString(), StringComparison.Ordinal);
            }

            if (id == "fsharp.start-async-on-caller")
            {
                Assert.Contains("source/artifact mismatch", requirements.GetProperty("constraints").ToString(), StringComparison.Ordinal);
                Assert.Contains("SynchronizationContext.Current", expected, StringComparison.Ordinal);
            }

            if (id == "json.serialize-fsharp-unions")
            {
                Assert.Contains("reflection-only", requirements.GetProperty("constraints").ToString(), StringComparison.Ordinal);
                Assert.Contains("Native AOT", expected, StringComparison.Ordinal);
                Assert.Contains("source generation", expected, StringComparison.Ordinal);
            }

            AssertToolError(await CallAsync("get_recipe", new { id, targetVersion = "10.0" }).ConfigureAwait(false), "no_applicable_edition");
        }
    }

    [Then("unsafe boundary guidance retains its full body applicability and edition")]
    public async Task CheckUnsafeBoundariesAsync()
    {
        const string id = "csharp.audit-memory-boundaries";
        JsonElement list = AssertMatchingRepresentations(await CallAsync("list_recipes", new { targetVersion = "net11.0", query = "unsafe memory boundaries" }).ConfigureAwait(false));
        JsonElement summary = Assert.Single(list.GetProperty("recipes").EnumerateArray());
        Assert.Equal(id, summary.GetProperty("id").GetString());
        Assert.Contains("experimental", summary.GetProperty("title").GetString(), StringComparison.Ordinal);
        Assert.Contains("caller", summary.GetProperty("goal").GetString(), StringComparison.Ordinal);
        JsonElement detail = AssertMatchingRepresentations(await CallAsync("get_recipe", new { id, targetVersion = "11.0" }).ConfigureAwait(false));
        Assert.Equal("11.0", detail.GetProperty("resolvedTarget").GetString());
        JsonElement recipe = detail.GetProperty("recipe");
        Assert.Equal("11.0", recipe.GetProperty("edition").GetString());
        Assert.Equal("11.0", recipe.GetProperty("introducedIn").GetString());
        Assert.Equal("experimental", recipe.GetProperty("stability").GetString());
        Assert.Equal(["11.0"], recipe.GetProperty("supportedTargets").EnumerateArray().Select(value => value.GetString()));
        AssertSelection(summary, recipe);
        string expected = await ReadCanonicalAsync($"{id}.md").ConfigureAwait(false);
        foreach ((string token, string file, string region, string language, bool xml) in new[]
        {
            ("older", "net10.0.Frames.cs", "frames", "csharp", false),
            ("newer", "net11.0.Frames.cs", "frames", "csharp", false),
            ("layout", "net11.0.Frames.cs", "layout", "csharp", false),
            ("program", "UnsafeProgram.cs", "program", "csharp", false),
            ("newer-project", "UnsafeBoundaries.csproj", "project", "xml", true)
        })
        {
            string source = await ReadCanonicalAsync(file).ConfigureAwait(false);
            string start = xml ? $"<!-- snippet:{region} -->\n" : $"// snippet:{region}\n";
            string end = xml ? $"<!-- endsnippet:{region} -->" : $"// endsnippet:{region}";
            string snippet = source.Split(start, StringSplitOptions.None)[1].Split(end, StringSplitOptions.None)[0].TrimEnd();
            expected = expected.Replace($"{{{{snippet:{token}}}}}", $"```{language}\n{snippet}\n```", StringComparison.Ordinal);
        }

        Assert.Equal(expected, recipe.GetProperty("markdown").GetString());
        Assert.DoesNotContain("{{snippet:", expected, StringComparison.Ordinal);
        JsonElement requirements = Assert.Single(recipe.GetProperty("applicability").GetProperty("routes").EnumerateArray()).GetProperty("requirements");
        Assert.Equal("11.0", requirements.GetProperty("runtime").GetProperty("version").GetString());
        Assert.Equal("11.0", requirements.GetProperty("sdk").GetProperty("version").GetString());
        Assert.Equal("preview", requirements.GetProperty("language").GetProperty("version").GetString());
        foreach (string field in new[] { "frameworks", "packages", "packageAssets", "providers", "renderingModes", "workloads" })
        {
            Assert.Empty(requirements.GetProperty(field).EnumerateArray());
        }

        foreach (string term in new[] { "LangVersion=preview", "updated-memory-safety-rules", "AllowUnsafeBlocks=true" })
        {
            Assert.Contains(term, requirements.GetProperty("optIns").ToString(), StringComparison.Ordinal);
            Assert.Contains(term, summary.GetProperty("applicability").ToString(), StringComparison.Ordinal);
        }

        foreach (string term in new[] { "private", "caller", "RC2" })
        {
            Assert.Contains(term, summary.GetProperty("applicability").ToString(), StringComparison.Ordinal);
            Assert.Contains(term, expected, StringComparison.Ordinal);
        }

        Assert.NotEmpty(requirements.GetProperty("platforms").EnumerateArray());
        Assert.NotEmpty(recipe.GetProperty("sources").EnumerateArray());
        Assert.Equal("not-required", recipe.GetProperty("humanProof").GetProperty("status").GetString());
        Assert.Equal(["net10.0", "net11.0"], recipe.GetProperty("evidence").EnumerateArray().Select(value => value.GetProperty("target").GetString()));
        Assert.All(recipe.GetProperty("evidence").EnumerateArray(), evidence =>
        {
            Assert.Equal("current", evidence.GetProperty("freshness").GetString());
            Assert.Equal("compile-and-run", evidence.GetProperty("kind").GetString());
            Assert.Contains("PASS: unsafe boundary", evidence.GetProperty("output").GetString(), StringComparison.Ordinal);
        });
        JsonElement older = AssertMatchingRepresentations(await CallAsync("list_recipes", new { targetVersion = "10.0", query = "unsafe memory boundaries" }).ConfigureAwait(false));
        Assert.Empty(RecipeIds(older));
        AssertToolError(await CallAsync("get_recipe", new { id, targetVersion = "10.0" }).ConfigureAwait(false), "no_applicable_edition");
    }

    [Then("enclosing loop guidance retains its full body applicability and edition")]
    public async Task CheckEnclosingLoopsAsync()
    {
        const string id = "csharp.control-nested-loops";
        JsonElement list = AssertMatchingRepresentations(await CallAsync("list_recipes", new { targetVersion = "net11.0", query = "enclosing loop" }).ConfigureAwait(false));
        JsonElement summary = Assert.Single(list.GetProperty("recipes").EnumerateArray());
        Assert.Equal(id, summary.GetProperty("id").GetString());
        Assert.NotEmpty(summary.GetProperty("goal").GetString()!);
        JsonElement detail = AssertMatchingRepresentations(await CallAsync("get_recipe", new { id, targetVersion = "11.0" }).ConfigureAwait(false));
        Assert.Equal("11.0", detail.GetProperty("resolvedTarget").GetString());
        JsonElement recipe = detail.GetProperty("recipe");
        Assert.Equal("11.0", recipe.GetProperty("edition").GetString());
        Assert.Equal("11.0", recipe.GetProperty("introducedIn").GetString());
        Assert.Equal(["11.0"], recipe.GetProperty("supportedTargets").EnumerateArray().Select(value => value.GetString()));
        Assert.Equal("stable", recipe.GetProperty("stability").GetString());
        AssertSelection(summary, recipe);
        string expected = await ReadCanonicalAsync($"{id}.md").ConfigureAwait(false);
        foreach ((string token, string target) in new[] { ("older", "net10.0"), ("newer", "net11.0") })
        {
            string source = await ReadCanonicalAsync($"{target}.Traversal.cs").ConfigureAwait(false);
            string snippet = source.Split("// snippet:traversal\n", StringSplitOptions.None)[1]
                .Split("// endsnippet:traversal", StringSplitOptions.None)[0].TrimEnd();
            expected = expected.Replace($"{{{{snippet:{token}}}}}", $"```csharp\n{snippet}\n```", StringComparison.Ordinal);
        }

        Assert.Equal(expected, recipe.GetProperty("markdown").GetString());
        Assert.DoesNotContain("{{snippet:", expected, StringComparison.Ordinal);
        JsonElement requirements = Assert.Single(recipe.GetProperty("applicability").GetProperty("routes").EnumerateArray()).GetProperty("requirements");
        Assert.Equal("11.0", requirements.GetProperty("runtime").GetProperty("version").GetString());
        Assert.Equal("11.0", requirements.GetProperty("sdk").GetProperty("version").GetString());
        Assert.Equal("15.0", requirements.GetProperty("language").GetProperty("version").GetString());
        foreach (string field in new[] { "frameworks", "packages", "packageAssets", "providers", "renderingModes", "workloads", "optIns" })
        {
            Assert.Empty(requirements.GetProperty(field).EnumerateArray());
        }

        Assert.NotEmpty(requirements.GetProperty("platforms").EnumerateArray());
        Assert.Contains("RC2", Assert.Single(requirements.GetProperty("stabilityLimitations").EnumerateArray()).GetString(), StringComparison.Ordinal);
        Assert.NotEmpty(recipe.GetProperty("sources").EnumerateArray());
        Assert.Equal("not-required", recipe.GetProperty("humanProof").GetProperty("status").GetString());
        Assert.Equal(["net10.0", "net11.0"], recipe.GetProperty("evidence").EnumerateArray().Select(value => value.GetProperty("target").GetString()));
        Assert.All(recipe.GetProperty("evidence").EnumerateArray(), evidence =>
        {
            Assert.Equal("current", evidence.GetProperty("freshness").GetString());
            Assert.Equal("compile-and-run", evidence.GetProperty("kind").GetString());
            Assert.Contains("PASS: nested traversal", evidence.GetProperty("output").GetString(), StringComparison.Ordinal);
        });
        JsonElement older = AssertMatchingRepresentations(await CallAsync("list_recipes", new { targetVersion = "10.0", query = "enclosing loop" }).ConfigureAwait(false));
        Assert.Empty(RecipeIds(older));
        AssertToolError(await CallAsync("get_recipe", new { id, targetVersion = "10.0" }).ConfigureAwait(false), "no_applicable_edition");
    }

    [Then("static interface helper guidance retains its full body applicability and edition")]
    public async Task CheckStaticInterfaceHelpersAsync()
    {
        const string id = "csharp.static-interface-helpers";
        JsonElement list = AssertMatchingRepresentations(await CallAsync("list_recipes", new { targetVersion = "net11.0", query = "static interface helpers" }).ConfigureAwait(false));
        JsonElement summary = Assert.Single(list.GetProperty("recipes").EnumerateArray());
        Assert.Equal(id, summary.GetProperty("id").GetString());
        Assert.Contains("without adding implementation requirements", summary.GetProperty("title").GetString(), StringComparison.Ordinal);
        Assert.Contains("non-virtual", summary.GetProperty("goal").GetString(), StringComparison.Ordinal);
        JsonElement detail = AssertMatchingRepresentations(await CallAsync("get_recipe", new { id, targetVersion = "11" }).ConfigureAwait(false));
        Assert.Equal("11.0", detail.GetProperty("resolvedTarget").GetString());
        JsonElement recipe = detail.GetProperty("recipe");
        Assert.Equal("11.0", recipe.GetProperty("edition").GetString());
        Assert.Equal("11.0", recipe.GetProperty("introducedIn").GetString());
        Assert.Equal("stable", recipe.GetProperty("stability").GetString());
        Assert.Equal(["11.0"], recipe.GetProperty("supportedTargets").EnumerateArray().Select(value => value.GetString()));
        AssertSelection(summary, recipe);

        string expected = await ReadCanonicalAsync($"{id}.md").ConfigureAwait(false);
        foreach ((string token, string target, string region) in new[]
        {
            ("older", "net10.0", "helpers"), ("newer", "net11.0", "helpers"),
            ("older-consumer", "net10.0", "consumer"), ("newer-consumer", "net11.0", "consumer")
        })
        {
            string source = await ReadCanonicalAsync($"{target}.Observations.cs").ConfigureAwait(false);
            string snippet = source.Split($"// snippet:{region}\n", StringSplitOptions.None)[1]
                .Split($"// endsnippet:{region}", StringSplitOptions.None)[0].TrimEnd();
            expected = expected.Replace($"{{{{snippet:{token}}}}}", $"```csharp\n{snippet}\n```", StringComparison.Ordinal);
        }

        Assert.Equal(expected, recipe.GetProperty("markdown").GetString());
        Assert.DoesNotContain("{{snippet:", expected, StringComparison.Ordinal);
        Assert.Contains("also legal on .NET 10", expected, StringComparison.Ordinal);
        JsonElement route = Assert.Single(recipe.GetProperty("applicability").GetProperty("routes").EnumerateArray());
        Assert.Equal("11.0", route.GetProperty("target").GetString());
        Assert.False(route.GetProperty("conditional").GetBoolean());
        JsonElement requirements = route.GetProperty("requirements");
        Assert.Equal("11.0", requirements.GetProperty("runtime").GetProperty("version").GetString());
        Assert.Equal("11.0", requirements.GetProperty("sdk").GetProperty("version").GetString());
        Assert.Equal("15.0", requirements.GetProperty("language").GetProperty("version").GetString());
        foreach (string field in new[] { "packages", "packageAssets", "workloads", "providers", "renderingModes", "optIns" })
        {
            Assert.Empty(requirements.GetProperty(field).EnumerateArray());
        }

        Assert.Contains("RC2", Assert.Single(requirements.GetProperty("stabilityLimitations").EnumerateArray()).GetString(), StringComparison.Ordinal);
        Assert.Equal("not-required", recipe.GetProperty("humanProof").GetProperty("status").GetString());
        Assert.Equal(2, recipe.GetProperty("evidence").GetArrayLength());
        Assert.All(recipe.GetProperty("evidence").EnumerateArray(), evidence =>
        {
            Assert.Equal("current", evidence.GetProperty("freshness").GetString());
            Assert.Equal("compile-and-run", evidence.GetProperty("kind").GetString());
            Assert.Contains("PASS: static interface helper", evidence.GetProperty("output").GetString(), StringComparison.Ordinal);
            Assert.NotEmpty(evidence.GetProperty("inputs").EnumerateObject());
        });

        JsonElement older = AssertMatchingRepresentations(await CallAsync("list_recipes", new { targetVersion = "10.0" }).ConfigureAwait(false));
        Assert.DoesNotContain(id, RecipeIds(older));
        AssertToolError(await CallAsync("get_recipe", new { id, targetVersion = "10.0" }).ConfigureAwait(false), "no_applicable_edition");
    }

    [Then("all four closed outcome recipes retain their full body applicability and edition")]
    public async Task CheckClosedOutcomesAsync()
    {
        foreach ((string id, string region) in new[]
        {
            ("csharp.closed-results", "outcomes"), ("csharp.closed-hierarchies", "hierarchy"),
            ("json.serialize-closed-results", "outcomes"), ("json.infer-closed-polymorphism", "hierarchy")
        })
        {
            JsonElement list = AssertMatchingRepresentations(await CallAsync("list_recipes", new { targetVersion = "net11.0", languages = (string[])["csharp"], query = "closed" }).ConfigureAwait(false));
            JsonElement summary = Assert.Single(list.GetProperty("recipes").EnumerateArray(), item => item.GetProperty("id").GetString() == id);
            Assert.Equal(id, summary.GetProperty("id").GetString());
            Assert.NotEmpty(summary.GetProperty("goal").GetString()!);
            Assert.DoesNotContain("museum", summary.GetProperty("title").GetString()!, StringComparison.OrdinalIgnoreCase);
            JsonElement detail = AssertMatchingRepresentations(await CallAsync("get_recipe", new { id, targetVersion = "11" }).ConfigureAwait(false));
            Assert.Equal("11.0", detail.GetProperty("resolvedTarget").GetString());
            JsonElement recipe = detail.GetProperty("recipe");
            Assert.Equal("11.0", recipe.GetProperty("edition").GetString());
            Assert.Equal("11.0", recipe.GetProperty("introducedIn").GetString());
            Assert.Equal(["11.0"], recipe.GetProperty("supportedTargets").EnumerateArray().Select(value => value.GetString()));
            AssertSelection(summary, recipe);

            string expected = await ReadCanonicalAsync($"{id}.md").ConfigureAwait(false);
            string context = region == "outcomes" ? "outcome-context" : "state-context";
            foreach ((string token, string target, string sourceRegion) in new[]
            {
                ("older", "net10.0", region), ("newer", "net11.0", region),
                ("older-context", "net10.0", context), ("newer-context", "net11.0", context),
                ("converter", "net10.0", "converter")
            })
            {
                string source = await ReadCanonicalAsync($"{target}.Outcomes.cs").ConfigureAwait(false);
                string snippet = source.Split($"// snippet:{sourceRegion}\n", StringSplitOptions.None)[1]
                    .Split($"// endsnippet:{sourceRegion}", StringSplitOptions.None)[0].TrimEnd();
                expected = expected.Replace($"{{{{snippet:{token}}}}}", $"```csharp\n{snippet}\n```", StringComparison.Ordinal);
            }

            Assert.Equal(expected, recipe.GetProperty("markdown").GetString());
            Assert.DoesNotContain("{{snippet:", expected, StringComparison.Ordinal);
            JsonElement requirements = Assert.Single(recipe.GetProperty("applicability").GetProperty("routes").EnumerateArray()).GetProperty("requirements");
            Assert.Equal("11.0", requirements.GetProperty("runtime").GetProperty("version").GetString());
            Assert.Equal("11.0", requirements.GetProperty("sdk").GetProperty("version").GetString());
            Assert.Equal("15.0", requirements.GetProperty("language").GetProperty("version").GetString());
            foreach (string field in new[] { "packages", "packageAssets", "workloads", "providers", "renderingModes", "optIns" })
            {
                Assert.Empty(requirements.GetProperty(field).EnumerateArray());
            }

            Assert.Contains("RC2", Assert.Single(requirements.GetProperty("stabilityLimitations").EnumerateArray()).GetString(), StringComparison.Ordinal);
            Assert.Equal("not-required", recipe.GetProperty("humanProof").GetProperty("status").GetString());
            Assert.Equal(2, recipe.GetProperty("evidence").GetArrayLength());
            Assert.All(recipe.GetProperty("evidence").EnumerateArray(), evidence =>
            {
                Assert.Equal("current", evidence.GetProperty("freshness").GetString());
                Assert.Equal("compile-and-run", evidence.GetProperty("kind").GetString());
                Assert.Contains("PASS: outcome matching", evidence.GetProperty("output").GetString(), StringComparison.Ordinal);
                Assert.NotEmpty(evidence.GetProperty("inputs").EnumerateObject());
            });

            JsonElement older = AssertMatchingRepresentations(await CallAsync("list_recipes", new { targetVersion = "10.0" }).ConfigureAwait(false));
            Assert.DoesNotContain(id, RecipeIds(older));
            AssertToolError(await CallAsync("get_recipe", new { id, targetVersion = "10.0" }).ConfigureAwait(false), "no_applicable_edition");
        }
    }

    [Then("collection recipes retain complete canonical bodies and independent target requirements")]
    public async Task CheckCollectionGuidanceAsync()
    {
        foreach ((string id, string region) in new[]
        {
            ("csharp.configure-collection-construction", "construction"),
            ("csharp.index-existing-types", "indexer"),
            ("csharp.extend-uneditable-types", "property"),
        })
        {
            bool foundation = region == "property";
            string edition = foundation ? "10.0" : "11.0";
            string expected = await ReadCanonicalAsync($"{id}.md").ConfigureAwait(false);
            foreach (string token in new[] { "older", "newer", "builder" })
            {
                if (!expected.Contains($"{{{{snippet:{token}}}}}", StringComparison.Ordinal))
                {
                    continue;
                }

                string olderTarget = foundation ? "net9.0" : "net10.0";
                string newerTarget = foundation ? "net10.0" : "net11.0";
                string target = token == "older" ? olderTarget : newerTarget;
                string sourceFile = foundation ? "ShelfExtensions" : "Collections";
                string name = token == "builder" ? "CrateLabels.cs" : $"{target}.{sourceFile}.cs";
                string source = await ReadCanonicalAsync(name).ConfigureAwait(false);
                string marker = token == "builder" ? "builder" : region;
                string snippet = source.Split($"// snippet:{marker}\n", StringSplitOptions.None)[1]
                    .Split($"// endsnippet:{marker}", StringSplitOptions.None)[0].TrimEnd();
                expected = expected.Replace($"{{{{snippet:{token}}}}}", $"```csharp\n{snippet}\n```", StringComparison.Ordinal);
            }

            foreach (string target in new[] { "10.0", "11.0" })
            {
                JsonElement list = AssertMatchingRepresentations(await CallAsync("list_recipes", new { targetVersion = target }).ConfigureAwait(false));
                if (!foundation && target == "10.0")
                {
                    Assert.DoesNotContain(id, RecipeIds(list));
                    AssertToolError(await CallAsync("get_recipe", new { id, targetVersion = target }).ConfigureAwait(false), "no_applicable_edition");
                    continue;
                }

                JsonElement summary = Assert.Single(list.GetProperty("recipes").EnumerateArray(), item => item.GetProperty("id").GetString() == id);
                Assert.NotEmpty(summary.GetProperty("goal").GetString()!);
                JsonElement detail = AssertMatchingRepresentations(await CallAsync("get_recipe", new { id, targetVersion = target }).ConfigureAwait(false));
                Assert.Equal(target, detail.GetProperty("resolvedTarget").GetString());
                JsonElement recipe = detail.GetProperty("recipe");
                AssertSelection(summary, recipe);
                Assert.Equal(edition, recipe.GetProperty("edition").GetString());
                Assert.Equal(edition, recipe.GetProperty("introducedIn").GetString());
                Assert.Equal("stable", recipe.GetProperty("stability").GetString());
                Assert.Equal(expected, recipe.GetProperty("markdown").GetString());
                Assert.DoesNotContain("{{snippet:", expected, StringComparison.Ordinal);
                JsonElement route = Assert.Single(recipe.GetProperty("applicability").GetProperty("routes").EnumerateArray());
                Assert.Equal(target, route.GetProperty("target").GetString());
                Assert.False(route.GetProperty("conditional").GetBoolean());
                JsonElement requirements = route.GetProperty("requirements");
                Assert.Equal(target, requirements.GetProperty("runtime").GetProperty("version").GetString());
                Assert.Equal(target, requirements.GetProperty("sdk").GetProperty("version").GetString());
                Assert.Equal(foundation ? "14.0" : "15.0", requirements.GetProperty("language").GetProperty("version").GetString());
                foreach (string field in new[] { "packages", "packageAssets", "workloads", "providers", "renderingModes", "optIns" })
                {
                    Assert.Empty(requirements.GetProperty(field).EnumerateArray());
                }

                if (target == "11.0")
                {
                    Assert.Contains("RC2", Assert.Single(requirements.GetProperty("stabilityLimitations").EnumerateArray()).GetString(), StringComparison.Ordinal);
                }

                Assert.Equal(foundation ? 3 : 2, recipe.GetProperty("evidence").GetArrayLength());
                Assert.All(recipe.GetProperty("evidence").EnumerateArray(), evidence =>
                {
                    Assert.Equal("current", evidence.GetProperty("freshness").GetString());
                    Assert.Equal("compile-and-run", evidence.GetProperty("kind").GetString());
                    Assert.Contains(foundation ? "PASS: extension property" : "PASS: collection order", evidence.GetProperty("output").GetString(), StringComparison.Ordinal);
                    Assert.NotEmpty(evidence.GetProperty("inputs").EnumerateObject());
                });
                Assert.Equal("not-required", recipe.GetProperty("humanProof").GetProperty("status").GetString());
            }
        }
    }

    private static async Task<string> ReadCanonicalAsync(string name)
    {
        using Stream stream = typeof(InstalledToolSteps).Assembly.GetManifestResourceStream($"Canonical.{name}")!;
        using StreamReader reader = new(stream);
        return (await reader.ReadToEndAsync().ConfigureAwait(false)).Replace("\r\n", "\n", StringComparison.Ordinal);
    }

    [Then("ordinary discovery fits the documented byte budgets")]
    public async Task CheckOrdinarySizeAsync()
    {
        List<string> measurements = [];
        bool withinBudget = true;
        foreach (string target in new[] { "10.0", "11.0" })
        {
            JsonElement result = await CallAsync("list_recipes", new { targetVersion = target }).ConfigureAwait(false);
            JsonElement list = AssertMatchingRepresentations(result);
            JsonElement[] summaries = [.. list.GetProperty("recipes").EnumerateArray()];
            measurements.Add($"{target}: count={summaries.Length}, largest summary={summaries.Max(Bytes)}, structured={Bytes(list)}, result={Bytes(result)} UTF-8 bytes");
            withinBudget &= summaries.All(summary => Bytes(summary) <= 3000) && Bytes(result) <= 80000;
        }

        string report = string.Join(Environment.NewLine, measurements);
        _output.WriteLine(report);
        Assert.True(withinBudget, report);
    }

    [Then("discovery pages fit the documented byte budgets")]
    public async Task CheckPageSizeAsync()
    {
        List<string> measurements = [];
        bool withinBudget = true;
        foreach (string? query in new string?[] { "traversal fixture", null })
        {
            foreach (int limit in new[] { 20, 50 })
            {
                object arguments = limit == 20 ? new { targetVersion = "latest", query } : new { targetVersion = "latest", query, limit };
                JsonElement result = await CallAsync("list_recipes", arguments).ConfigureAwait(false);
                JsonElement list = AssertMatchingRepresentations(result);
                Assert.Equal(limit, list.GetProperty("recipes").GetArrayLength());
                measurements.Add($"{query ?? "all"} {limit}: summary={Bytes(list.GetProperty("recipes")[0])}, structured={Bytes(list)}, result={Bytes(result)} UTF-8 bytes");
                int budget = query is null ? 80000 : 65000;
                if (limit == 50)
                {
                    budget = 160000;
                }

                withinBudget &= Bytes(result) <= budget;
            }
        }

        string report = string.Join(Environment.NewLine, measurements);
        _output.WriteLine(report);
        Assert.True(withinBudget, report);
    }

    private static int Bytes(JsonElement value) => Encoding.UTF8.GetByteCount(value.GetRawText());

    [Then("empty keyword searches preserve constraints in a useful suggested request")]
    public async Task CheckKeywordRecoveryAsync()
    {
        foreach (string target in new[] { "net10.0", "latest" })
        {
            object arguments = new { targetVersion = target, areas = (string[])["csharp"], languages = (string[])["csharp"], query = " field\tno-such-term ", includeShared = false, offset = 7, limit = 1 };
            JsonElement result = await CallAsync("list_recipes", arguments).ConfigureAwait(false);
            JsonElement empty = AssertMatchingRepresentations(result);
            Assert.Equal(0, empty.GetProperty("total").GetInt32());
            Assert.Empty(RecipeIds(empty));
            Assert.Contains(empty.GetProperty("coverage").EnumerateArray(), coverage => !coverage.GetProperty("complete").GetBoolean());
            Assert.Equal(target == "latest" ? "11.0" : "10.0", empty.GetProperty("resolvedTarget").GetString());
            JsonElement recovery = empty.GetProperty("recovery");
            Assert.Equal("no_matches", recovery.GetProperty("reason").GetString());
            Assert.Contains("not proof", recovery.GetProperty("message").GetString(), StringComparison.Ordinal);
            JsonElement filters = recovery.GetProperty("filters");
            Assert.Equal("field no-such-term", filters.GetProperty("query").GetString());
            Assert.Equal("csharp", Assert.Single(filters.GetProperty("areas").EnumerateArray()).GetString());
            Assert.Equal("csharp", Assert.Single(filters.GetProperty("languages").EnumerateArray()).GetString());
            Assert.False(filters.GetProperty("includeShared").GetBoolean());
            JsonElement retry = recovery.GetProperty("suggestedRequest");
            Assert.Equal(target, retry.GetProperty("targetVersion").GetString());
            foreach (string field in new[] { "areas", "languages", "includeShared" })
            {
                Assert.True(JsonElement.DeepEquals(filters.GetProperty(field), retry.GetProperty(field)), field);
            }

            Assert.Equal(JsonValueKind.Null, retry.GetProperty("query").ValueKind);
            Assert.Equal(0, retry.GetProperty("offset").GetInt32());
            Assert.Equal(1, retry.GetProperty("limit").GetInt32());
            Assert.True(Bytes(recovery) <= 1200, $"Recovery: {Bytes(recovery)} UTF-8 bytes");
            JsonElement repeated = AssertMatchingRepresentations(await CallAsync("list_recipes", arguments).ConfigureAwait(false));
            Assert.True(JsonElement.DeepEquals(empty, repeated));
            JsonElement matched = AssertMatchingRepresentations(await CallAsync("list_recipes", retry).ConfigureAwait(false));
            Assert.Single(RecipeIds(matched));
            Assert.False(matched.TryGetProperty("recovery", out _));
            JsonElement recipe = AssertMatchingRepresentations(await CallAsync("get_recipe", new { id = RecipeIds(matched)[0], targetVersion = target }).ConfigureAwait(false)).GetProperty("recipe");
            Assert.NotEmpty(recipe.GetProperty("markdown").GetString()!);
            AssertSelection(matched.GetProperty("recipes")[0], recipe);
        }

        JsonElement defaults = AssertMatchingRepresentations(await CallAsync("list_recipes", new { targetVersion = "10", query = "no-such-recovery-term" }).ConfigureAwait(false));
        JsonElement defaultRetry = defaults.GetProperty("recovery").GetProperty("suggestedRequest");
        Assert.Empty(defaultRetry.GetProperty("areas").EnumerateArray());
        Assert.Empty(defaultRetry.GetProperty("languages").EnumerateArray());
        Assert.True(defaultRetry.GetProperty("includeShared").GetBoolean());
        Assert.Equal(20, defaultRetry.GetProperty("limit").GetInt32());
    }

    [Then("unmatched combinations have no unsafe retry and empty pages retain their query")]
    public async Task CheckCombinationAndPageRecoveryAsync()
    {
        foreach (string? query in new string?[] { null, " \t ", "unmatched" })
        {
            JsonElement empty = AssertMatchingRepresentations(await CallAsync("list_recipes", new { targetVersion = "net10.0", areas = (string[])["wpf"], languages = (string[])["fsharp"], query, includeShared = false }).ConfigureAwait(false));
            Assert.Equal(0, empty.GetProperty("total").GetInt32());
            JsonElement recovery = empty.GetProperty("recovery");
            Assert.Equal("no_matches", recovery.GetProperty("reason").GetString());
            Assert.Equal(JsonValueKind.Null, recovery.GetProperty("suggestedRequest").ValueKind);
            Assert.Contains("no compatible", recovery.GetProperty("message").GetString(), StringComparison.OrdinalIgnoreCase);
            Assert.Contains("not proof", recovery.GetProperty("message").GetString(), StringComparison.Ordinal);
        }

        foreach (int offset in new[] { 53, int.MaxValue })
        {
            JsonElement beyond = AssertMatchingRepresentations(await CallAsync("list_recipes", new { targetVersion = "latest", areas = (string[])["csharp"], languages = (string[])["csharp"], query = "traversal fixture", includeShared = false, offset, limit = 50 }).ConfigureAwait(false));
            Assert.Equal(53, beyond.GetProperty("total").GetInt32());
            Assert.Empty(RecipeIds(beyond));
            JsonElement recovery = beyond.GetProperty("recovery");
            Assert.Equal("offset_out_of_range", recovery.GetProperty("reason").GetString());
            JsonElement retry = recovery.GetProperty("suggestedRequest");
            Assert.Equal("latest", retry.GetProperty("targetVersion").GetString());
            Assert.Equal("traversal fixture", retry.GetProperty("query").GetString());
            Assert.Equal(0, retry.GetProperty("offset").GetInt32());
            Assert.Equal(50, retry.GetProperty("limit").GetInt32());
            Assert.False(retry.GetProperty("includeShared").GetBoolean());
            Assert.Equal("csharp", Assert.Single(retry.GetProperty("areas").EnumerateArray()).GetString());
            Assert.Equal("csharp", Assert.Single(retry.GetProperty("languages").EnumerateArray()).GetString());
            JsonElement page = AssertMatchingRepresentations(await CallAsync("list_recipes", retry).ConfigureAwait(false));
            Assert.Equal(50, page.GetProperty("recipes").GetArrayLength());
            Assert.Equal(50, page.GetProperty("nextOffset").GetInt32());
        }
    }

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
        string? processPath = Environment.GetEnvironmentVariable("PATH");
        string? userPath = OperatingSystem.IsWindows() ? Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User) : null;
        string? machinePath = OperatingSystem.IsWindows() ? Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine) : null;
        (int exitCode, string stdout, string stderr) = await RunAsync(install).ConfigureAwait(false);
        Assert.Equal(processPath, Environment.GetEnvironmentVariable("PATH"));
        if (OperatingSystem.IsWindows())
        {
            Assert.Equal(userPath, Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User));
            Assert.Equal(machinePath, Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine));
        }

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
        await using ZipArchive archive = await ZipFile.OpenReadAsync(package).ConfigureAwait(false);
        await using Stream packedAssembly = await archive.GetEntry("tools/net10.0/any/PinguApps.Playbook.dll")!.OpenAsync().ConfigureAwait(false);
        using FileStream installedAssembly = File.OpenRead(runtimeConfig.Replace(".runtimeconfig.json", ".dll", StringComparison.Ordinal));
        Assert.Equal(await SHA256.HashDataAsync(packedAssembly).ConfigureAwait(false), await SHA256.HashDataAsync(installedAssembly).ConfigureAwait(false));
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
        // Trace only the installed server, not the test runner or SDK used for installation.
        start.Environment["DOTNET_HOST_TRACE"] = "1";
        start.Environment["DOTNET_HOST_TRACEFILE"] = Path.Combine(_directory, "host.log");
        start.Environment.Remove("DOTNET_ROLL_FORWARD");
        _server = Process.Start(start) ?? throw new InvalidOperationException("Could not start installed tool.");
        _diagnostics = _server.StandardError.ReadToEndAsync();
        JsonElement initialized = await RequestAsync("initialize", new
        {
            protocolVersion = "2025-11-25",
            capabilities = new { },
            clientInfo = new { name = "Playbook packed consumer", version = "1.0.0" },
        }).ConfigureAwait(false);
        Assert.True(initialized.GetProperty("capabilities").TryGetProperty("tools", out _));
        string instructions = initialized.GetProperty("instructions").GetString()!;
        Assert.Contains("TargetFramework", instructions, StringComparison.Ordinal);
        Assert.Contains("global.json", instructions, StringComparison.Ordinal);
        Assert.Contains("targetVersion", instructions, StringComparison.Ordinal);
        Assert.Contains("latest does not establish consumer compatibility", instructions, StringComparison.Ordinal);
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
            if (tool.GetProperty("name").GetString() == "list_recipes")
            {
                Assert.Contains("Before applying any recipe, retrieve get_recipe", tool.GetProperty("description").GetString(), StringComparison.Ordinal);
                string schema = tool.GetProperty("outputSchema").GetRawText();
                Assert.Contains("selectionNotes", schema, StringComparison.Ordinal);
                Assert.DoesNotContain("evidenceSamples", schema, StringComparison.Ordinal);
                Assert.DoesNotContain("prerequisites", schema, StringComparison.Ordinal);
            }
        }
    }

    [Then("discovery and detail preserve complete structured and text results")]
    public async Task CheckContentAsync()
    {
        JsonElement list = AssertMatchingRepresentations(await CallAsync("list_recipes", new { targetVersion = "10.0" }).ConfigureAwait(false));
        Assert.Equal("10.0", list.GetProperty("resolvedTarget").GetString());
        Assert.False(list.GetProperty("targetAssumed").GetBoolean());
        JsonElement summary = Assert.Single(list.GetProperty("recipes").EnumerateArray(), item => item.GetProperty("id").GetString() == "csharp.validate-property-assignment");
        Assert.False(summary.TryGetProperty("markdown", out _));
        JsonElement selection = summary.GetProperty("applicability").GetProperty("routes")[0];
        Assert.Equal("14.0", selection.GetProperty("requirements").GetProperty("language").GetProperty("version").GetString());
        Assert.Equal("10.0", selection.GetProperty("target").GetString());
        string id = summary.GetProperty("id").GetString()!;
        JsonElement detail = AssertMatchingRepresentations(await CallAsync("get_recipe", new { id, targetVersion = "10.0" }).ConfigureAwait(false));
        Assert.False(detail.GetProperty("targetAssumed").GetBoolean());
        JsonElement recipe = detail.GetProperty("recipe");
        Assert.Equal(id, recipe.GetProperty("id").GetString());
        Assert.Equal("10.0", recipe.GetProperty("edition").GetString());
        Assert.Equal("stable", recipe.GetProperty("stability").GetString());
        Assert.Equal(summary.GetProperty("title").GetString(), recipe.GetProperty("title").GetString());
        Assert.Equal(summary.GetProperty("goal").GetString(), recipe.GetProperty("goal").GetString());
        AssertSelection(summary, recipe);
        JsonElement route = recipe.GetProperty("applicability").GetProperty("routes")[0];
        string markdown = recipe.GetProperty("markdown").GetString()!;
        Assert.Contains("field = normalized;", markdown, StringComparison.Ordinal);
        Assert.Contains("_code = normalized;", markdown, StringComparison.Ordinal);
        Assert.Contains("does **not** run the setter", markdown, StringComparison.Ordinal);
        Assert.NotEmpty(route.GetProperty("requirements").GetProperty("constraints").EnumerateArray());
        Assert.NotEmpty(route.GetProperty("requirements").GetProperty("prerequisites").EnumerateArray());
        Assert.NotEmpty(route.GetProperty("sourceUrls").EnumerateArray());
        Assert.NotEmpty(route.GetProperty("evidenceSamples").EnumerateArray());
        Assert.NotEmpty(recipe.GetProperty("sources").EnumerateArray());
        Assert.Equal(["net9.0", "net10.0", "net10.0", "net11.0"], recipe.GetProperty("evidence").EnumerateArray().Select(evidence => evidence.GetProperty("target").GetString()));
        Assert.Equal("not-required", recipe.GetProperty("humanProof").GetProperty("status").GetString());
        foreach (JsonElement evidence in recipe.GetProperty("evidence").EnumerateArray())
        {
            Assert.Equal("current", evidence.GetProperty("freshness").GetString());
            Assert.Equal("compile-and-run", evidence.GetProperty("kind").GetString());
            Assert.Equal(40, evidence.GetProperty("sourceCommit").GetString()!.Length);
            Assert.NotEmpty(evidence.GetProperty("recordedAt").GetString()!);
            Assert.Contains("build PropertyAssignment.csproj --configuration Release", evidence.GetProperty("command").GetString(), StringComparison.Ordinal);
            Assert.Contains("PASS: constructor", evidence.GetProperty("output").GetString(), StringComparison.Ordinal);
            Assert.Contains("Runtime: .NET", evidence.GetProperty("output").GetString(), StringComparison.Ordinal);
            Assert.Equal(64, evidence.GetProperty("inputs").GetProperty("samples/PropertyAssignmentChecks.cs").GetString()!.Length);
            Assert.Empty(evidence.GetProperty("dependencies").EnumerateArray());
            Assert.Empty(evidence.GetProperty("workloads").EnumerateArray());
            Assert.Empty(evidence.GetProperty("nativeToolchains").EnumerateArray());
        }
    }

    [Then("property requirements keep normal and optional routes separate from exact test baselines")]
    public async Task CheckRequirementsAndBaselinesAsync()
    {
        string? canonicalMarkdown = null;
        foreach (string target in new[] { "10.0", "11.0" })
        {
            JsonElement list = AssertMatchingRepresentations(await CallAsync("list_recipes", new { targetVersion = target }).ConfigureAwait(false));
            JsonElement summary = Assert.Single(list.GetProperty("recipes").EnumerateArray(), item => item.GetProperty("id").GetString() == "csharp.validate-property-assignment");
            JsonElement detail = AssertMatchingRepresentations(await CallAsync("get_recipe", new { id = "csharp.validate-property-assignment", targetVersion = target }).ConfigureAwait(false));
            JsonElement recipe = detail.GetProperty("recipe");
            Assert.Equal("10.0", recipe.GetProperty("edition").GetString());
            Assert.Equal("10.0", summary.GetProperty("edition").GetString());
            Assert.Equal(["10.0", "11.0"], recipe.GetProperty("supportedTargets").EnumerateArray().Select(value => value.GetString()));
            string markdown = recipe.GetProperty("markdown").GetString()!;
            canonicalMarkdown ??= markdown;
            Assert.Equal(canonicalMarkdown, markdown);
            Assert.Equal("10.0", recipe.GetProperty("introducedIn").GetString());
            Assert.Equal(target, detail.GetProperty("resolvedTarget").GetString());
            AssertSelection(summary, recipe);
            JsonElement routes = recipe.GetProperty("applicability").GetProperty("routes");
            Assert.Equal(target == "10.0" ? 2 : 1, routes.GetArrayLength());
            Assert.All(routes.EnumerateArray(), route => Assert.Equal(target, route.GetProperty("target").GetString()));
            JsonElement primary = routes[0];
            Assert.Equal("primary", primary.GetProperty("usage").GetString());
            JsonElement requirements = primary.GetProperty("requirements");
            Assert.Equal(target, requirements.GetProperty("runtime").GetProperty("version").GetString());
            Assert.Equal(target, requirements.GetProperty("sdk").GetProperty("version").GetString());
            Assert.Equal("release-family", requirements.GetProperty("sdk").GetProperty("match").GetString());
            Assert.Equal("14.0", requirements.GetProperty("language").GetProperty("version").GetString());
            Assert.Empty(requirements.GetProperty("packages").EnumerateArray());
            Assert.Empty(requirements.GetProperty("workloads").EnumerateArray());
            Assert.DoesNotContain("verification baseline", requirements.GetRawText(), StringComparison.OrdinalIgnoreCase);
            Assert.False(primary.TryGetProperty("sdk", out _));
            foreach (JsonElement route in routes.EnumerateArray())
            {
                foreach (JsonElement sample in route.GetProperty("evidenceSamples").EnumerateArray())
                {
                    JsonElement evidence = Assert.Single(recipe.GetProperty("evidence").EnumerateArray(), item => item.GetProperty("sample").GetString() == sample.GetString());
                    Assert.Equal("verification-baseline", evidence.GetProperty("role").GetString());
                    Assert.Equal("current", evidence.GetProperty("freshness").GetString());
                    Assert.Equal($"net{target}", evidence.GetProperty("target").GetString());
                    Assert.Contains("Runtime: .NET", evidence.GetProperty("output").GetString(), StringComparison.Ordinal);
                    Assert.NotEmpty(evidence.GetProperty("command").GetString()!);
                    Assert.NotEmpty(evidence.GetProperty("inputs").EnumerateObject());
                }
            }

            if (target == "10.0")
            {
                JsonElement alternative = routes[1];
                Assert.Equal("optional-alternative", alternative.GetProperty("usage").GetString());
                Assert.Equal("11.0", alternative.GetProperty("requirements").GetProperty("sdk").GetProperty("version").GetString());
                Assert.Equal("10.0", alternative.GetProperty("requirements").GetProperty("runtime").GetProperty("version").GetString());
                Assert.DoesNotContain("11", requirements.GetRawText(), StringComparison.Ordinal);
                JsonElement baseline = Assert.Single(recipe.GetProperty("evidence").EnumerateArray(), item => item.GetProperty("sample").GetString()!.Contains("Sdk11", StringComparison.Ordinal));
                Assert.Equal("11.0.100-rc.2.26504.105", baseline.GetProperty("sdk").GetString());
                Assert.Equal("net10.0", baseline.GetProperty("target").GetString());
                Assert.Contains("Runtime: .NET 10.", baseline.GetProperty("output").GetString(), StringComparison.Ordinal);
            }
            else
            {
                Assert.Contains("RC", primary.GetProperty("requirements").GetProperty("stabilityLimitations")[0].GetString(), StringComparison.Ordinal);
                Assert.Contains("not GA verification", recipe.GetProperty("markdown").GetString(), StringComparison.Ordinal);
            }
        }
    }

    [Then("the served examples match the canonical compiled sample sources")]
    public async Task CheckSnippetsAsync()
    {
        JsonElement detail = AssertMatchingRepresentations(await CallAsync("get_recipe", new { id = "csharp.validate-property-assignment", targetVersion = "10.0" }).ConfigureAwait(false));
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
            Assert.Equal("10.0", recipe.GetProperty("edition").GetString());
            Assert.Equal("10.0", recipe.GetProperty("introducedIn").GetString());
            JsonElement routes = recipe.GetProperty("applicability").GetProperty("routes");
            Assert.All(routes.EnumerateArray(), route => Assert.Equal(target, route.GetProperty("target").GetString()));
            JsonElement route = routes[0];
            foreach (string field in new[] { "runtime", "sdk", "language", "frameworks", "packages", "workloads", "packageAssets", "providers", "platforms", "renderingModes", "prerequisites", "constraints", "optIns", "stabilityLimitations" })
            {
                Assert.True(route.GetProperty("requirements").TryGetProperty(field, out _), field);
            }

            AssertSelection(Assert.Single(list.GetProperty("recipes").EnumerateArray(), item => item.GetProperty("id").GetString() == "csharp.validate-property-assignment"), recipe);
            if (target == "10.0")
            {
                JsonElement conditional = Assert.Single(routes.EnumerateArray(), item => item.GetProperty("conditional").GetBoolean());
                Assert.Equal("11.0", conditional.GetProperty("requirements").GetProperty("sdk").GetProperty("version").GetString());
                Assert.Equal("14.0", conditional.GetProperty("requirements").GetProperty("language").GetProperty("version").GetString());
                Assert.Contains(".NET 11 SDK", recipe.GetProperty("markdown").GetString(), StringComparison.Ordinal);
            }
        }
    }

    [Then("both tools require a nonnullable target string")]
    public async Task CheckRequiredSchemasAsync()
    {
        JsonElement response = await RequestAsync("tools/list", new { }).ConfigureAwait(false);
        foreach (JsonElement tool in response.GetProperty("tools").EnumerateArray())
        {
            JsonElement schema = tool.GetProperty("inputSchema");
            Assert.Contains("targetVersion", schema.GetProperty("required").EnumerateArray().Select(value => value.GetString()));
            JsonElement target = schema.GetProperty("properties").GetProperty("targetVersion");
            Assert.Equal("string", target.GetProperty("type").GetString());
            Assert.False(target.TryGetProperty("default", out _));
        }
    }

    [Then("omitted null and blank targets fail helpfully on both tools")]
    public async Task CheckRequiredTargetsAsync()
    {
        foreach (string tool in new[] { "list_recipes", "get_recipe" })
        {
            foreach (object arguments in new object[]
            {
                new { id = "csharp.validate-property-assignment" },
                new { id = "csharp.validate-property-assignment", targetVersion = (string?)null },
                new { id = "csharp.validate-property-assignment", targetVersion = "" },
                new { id = "csharp.validate-property-assignment", targetVersion = " \t\n" },
            })
            {
                JsonElement result = await CallAsync(tool, arguments).ConfigureAwait(false);
                JsonElement error = AssertToolError(result, "required_target");
                Assert.Contains("targetVersion", error.GetProperty("message").GetString(), StringComparison.Ordinal);
                Assert.Contains("latest", error.GetProperty("message").GetString(), StringComparison.Ordinal);
            }
        }
    }

    [Then("explicit latest resolves from the installed catalogue independently on every call")]
    public async Task CheckLatestAsync()
    {
        // A concrete discovery must not supply the target of a later detail request.
        AssertMatchingRepresentations(await CallAsync("list_recipes", new { targetVersion = "10" }).ConfigureAwait(false));
        AssertToolError(await CallAsync("get_recipe", new { id = "csharp.validate-property-assignment" }).ConfigureAwait(false), "required_target");
        foreach (string tool in new[] { "list_recipes", "get_recipe" })
        {
            JsonElement result = AssertMatchingRepresentations(await CallAsync(tool, new { id = "csharp.validate-property-assignment", targetVersion = "latest" }).ConfigureAwait(false));
            string newest = result.GetProperty("supportedTargets").EnumerateArray().Select(item => item.GetString()!).MaxBy(Version.Parse)!;
            Assert.Equal(newest, result.GetProperty("resolvedTarget").GetString());
            Assert.Equal("latest", result.GetProperty("requestedTarget").GetString());
            Assert.False(result.GetProperty("targetAssumed").GetBoolean());
            Assert.Equal(["10.0", "11.0"], result.GetProperty("supportedTargets").EnumerateArray().Select(item => item.GetString()));
            JsonElement[] coverage = [.. result.GetProperty("coverage").EnumerateArray()];
            Assert.Equal(["curated", "comprehensive"], coverage.Select(item => item.GetProperty("scope").GetString()));
            Assert.All(coverage, item => Assert.False(item.GetProperty("complete").GetBoolean()));
        }

        JsonElement older = AssertMatchingRepresentations(await CallAsync("get_recipe", new { id = "csharp.validate-property-assignment", targetVersion = "net10.0" }).ConfigureAwait(false));
        Assert.Equal("net10.0", older.GetProperty("requestedTarget").GetString());
        Assert.Equal("10.0", older.GetProperty("resolvedTarget").GetString());
        Assert.Equal("10.0", older.GetProperty("recipe").GetProperty("edition").GetString());
        JsonElement latest = AssertMatchingRepresentations(await CallAsync("list_recipes", new { targetVersion = "latest" }).ConfigureAwait(false));
        Assert.Equal("11.0", latest.GetProperty("resolvedTarget").GetString());
        JsonElement explicitDetail = AssertMatchingRepresentations(await CallAsync("get_recipe", new { id = "csharp.validate-property-assignment", targetVersion = "10.0" }).ConfigureAwait(false));
        Assert.Equal("10.0", explicitDetail.GetProperty("resolvedTarget").GetString());
    }

    [Then("invalid requests return actionable machine-readable tool failures")]
    public async Task CheckFailuresAsync()
    {
        foreach (string tool in new[] { "list_recipes", "get_recipe" })
        {
            foreach (string target in new[] { "9.0", "12.0", "10.0.1", "11.0.0-rc.2", "net10.0-windows" })
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
        Assert.Equal("pending", recipe.GetProperty("humanProof").GetProperty("status").GetString());
        Assert.Equal("10.0", detail.GetProperty("resolvedTarget").GetString());
        JsonElement route = Assert.Single(recipe.GetProperty("applicability").GetProperty("routes").EnumerateArray());
        Assert.True(route.GetProperty("conditional").GetBoolean());
        JsonElement conditionalSummary = Assert.Single(list.GetProperty("recipes").EnumerateArray(), item => item.GetProperty("id").GetString() == "fixture.conditional-route");
        AssertSelection(conditionalSummary, recipe);
        Assert.Equal("11.0", route.GetProperty("requirements").GetProperty("sdk").GetProperty("version").GetString());
        Assert.NotEmpty(route.GetProperty("sourceUrls").EnumerateArray());
        Assert.NotEmpty(route.GetProperty("evidenceSamples").EnumerateArray());
        JsonElement newest = AssertMatchingRepresentations(await CallAsync("get_recipe", new { id = "csharp.validate-property-assignment", targetVersion = "11" }).ConfigureAwait(false));
        Assert.Equal("10.0", newest.GetProperty("recipe").GetProperty("edition").GetString());
        // latest is catalogue-wide, not the newest target available for this particular recipe.
        JsonElement latestFailure = AssertToolError(await CallAsync("get_recipe", new { id = "fixture.conditional-route", targetVersion = "latest" }).ConfigureAwait(false), "no_applicable_edition");
        Assert.Equal("latest", latestFailure.GetProperty("requestedTarget").GetString());
    }

    [Then("changed guidance prefers a newer compatible edition while retaining the older guidance")]
    public async Task CheckChangedEditionsAsync()
    {
        string? olderMarkdown = null;
        foreach (string target in new[] { "10.0", "11.0" })
        {
            JsonElement list = AssertMatchingRepresentations(await CallAsync("list_recipes", new { targetVersion = target, query = "fixture.changed-guidance" }).ConfigureAwait(false));
            JsonElement summary = Assert.Single(list.GetProperty("recipes").EnumerateArray());
            JsonElement detail = AssertMatchingRepresentations(await CallAsync("get_recipe", new { id = "fixture.changed-guidance", targetVersion = target }).ConfigureAwait(false));
            JsonElement recipe = detail.GetProperty("recipe");
            Assert.Equal(target, recipe.GetProperty("edition").GetString());
            Assert.Equal(target, summary.GetProperty("edition").GetString());
            Assert.Equal("10.0", recipe.GetProperty("introducedIn").GetString());
            AssertSelection(summary, recipe);
            string markdown = recipe.GetProperty("markdown").GetString()!;
            if (target == "10.0")
            {
                olderMarkdown = markdown;
                // The retained edition also declares .NET 11: selection must prefer the changed edition there.
                Assert.Contains("11.0", recipe.GetProperty("supportedTargets").EnumerateArray().Select(value => value.GetString()));
                Assert.DoesNotContain("every partial declaration", markdown, StringComparison.Ordinal);
            }
            else
            {
                Assert.NotEqual(olderMarkdown, markdown);
                Assert.Contains("every partial declaration", markdown, StringComparison.Ordinal);
            }
        }
    }

    [Then("combined filters respect shared areas demonstrated languages and neutral configuration")]
    public async Task CheckCombinedDiscoveryAsync()
    {
        JsonElement combined = AssertMatchingRepresentations(await CallAsync("list_recipes", new
        {
            targetVersion = "11",
            areas = (string[])["blazor", "wpf"],
            languages = (string[])["fsharp", "csharp"],
            query = "  DISCOVERY\treservation\nBeaconApi catalogue  ",
        }).ConfigureAwait(false));
        Assert.Equal(["fixture.direct", "fixture.directive", "fixture.neutral", "fixture.shared"], RecipeIds(combined));
        Assert.Equal(4, combined.GetProperty("total").GetInt32());
        Assert.Contains("blazor", combined.GetProperty("supportedAreas").EnumerateArray().Select(value => value.GetString()));
        Assert.Contains("fsharp", combined.GetProperty("supportedExampleLanguages").EnumerateArray().Select(value => value.GetString()));
        JsonElement neutral = combined.GetProperty("recipes")[2];
        Assert.True(neutral.GetProperty("languageNeutral").GetBoolean());
        Assert.Empty(neutral.GetProperty("exampleLanguages").EnumerateArray());
        Assert.Equal(["csharp", "fsharp", "vb"], neutral.GetProperty("applicability").GetProperty("routes")[0].GetProperty("neutralLanguages").EnumerateArray().Select(value => value.GetString()));
        JsonElement neutralDetail = AssertMatchingRepresentations(await CallAsync("get_recipe", new { id = "fixture.neutral", targetVersion = "11.0" }).ConfigureAwait(false));
        AssertSelection(neutral, neutralDetail.GetProperty("recipe"));
        JsonElement directDetail = AssertMatchingRepresentations(await CallAsync("get_recipe", new { id = "fixture.direct", targetVersion = "11.0" }).ConfigureAwait(false));
        JsonElement directSummary = combined.GetProperty("recipes")[0];
        AssertSelection(directSummary, directDetail.GetProperty("recipe"));
        Assert.Equal("Keep this complete selection warning: " + new string('x', 600) + " END", Assert.Single(directSummary.GetProperty("applicability").GetProperty("routes")[0].GetProperty("selectionNotes").EnumerateArray()).GetString());
        Assert.Equal("experimental", combined.GetProperty("recipes")[3].GetProperty("stability").GetString());
        Assert.NotEmpty(combined.GetProperty("recipes")[3].GetProperty("applicability").GetProperty("routes")[0].GetProperty("requirements").GetProperty("optIns").EnumerateArray());
        JsonElement pinned = combined.GetProperty("recipes")[3].GetProperty("applicability").GetProperty("routes")[0].GetProperty("requirements");
        Assert.Equal("exact", pinned.GetProperty("sdk").GetProperty("match").GetString());
        Assert.Equal("11.0.100-rc.2.26504.105", pinned.GetProperty("sdk").GetProperty("version").GetString());
        Assert.Equal("1.2.3", Assert.Single(pinned.GetProperty("packages").EnumerateArray()).GetProperty("version").GetString());
        Assert.Equal("exact", pinned.GetProperty("packages")[0].GetProperty("match").GetString());
        Assert.Equal("1.0.0", Assert.Single(pinned.GetProperty("workloads").EnumerateArray()).GetProperty("version").GetString());
        Assert.Equal("lib/net11.0/Fixture.Contract.dll", Assert.Single(pinned.GetProperty("packageAssets").EnumerateArray()).GetString());
        Assert.NotEmpty(pinned.GetProperty("providers").EnumerateArray());
        Assert.NotEmpty(pinned.GetProperty("platforms").EnumerateArray());
        Assert.NotEmpty(pinned.GetProperty("renderingModes").EnumerateArray());
        Assert.NotEmpty(pinned.GetProperty("stabilityLimitations").EnumerateArray());
        JsonElement sharedDetail = AssertMatchingRepresentations(await CallAsync("get_recipe", new { id = "fixture.shared", targetVersion = "11.0" }).ConfigureAwait(false));
        AssertSelection(combined.GetProperty("recipes")[3], sharedDetail.GetProperty("recipe"));
        Assert.All(combined.GetProperty("recipes").EnumerateArray(), recipe => Assert.False(recipe.TryGetProperty("markdown", out _)));

        JsonElement direct = AssertMatchingRepresentations(await CallAsync("list_recipes", new
        {
            targetVersion = "latest",
            areas = (string[])["blazor", "wpf"],
            languages = (string[])["csharp", "fsharp"],
            query = "discovery reservation BeaconApi",
            includeShared = false,
        }).ConfigureAwait(false));
        Assert.Equal(["fixture.direct"], RecipeIds(direct));
        JsonElement fsharp = AssertMatchingRepresentations(await CallAsync("list_recipes", new
        {
            targetVersion = "latest",
            areas = (string[])["blazor"],
            languages = (string[])["fsharp"],
            query = "discovery reservation BeaconApi",
        }).ConfigureAwait(false));
        Assert.Equal(["fixture.neutral"], RecipeIds(fsharp));
        JsonElement older = AssertMatchingRepresentations(await CallAsync("list_recipes", new
        {
            targetVersion = "10",
            areas = (string[])["blazor"],
            query = "discovery reservation BeaconApi",
        }).ConfigureAwait(false));
        Assert.Empty(RecipeIds(older));
        JsonElement allAreas = AssertMatchingRepresentations(await CallAsync("list_recipes", new
        {
            targetVersion = "latest",
            areas = Array.Empty<string>(),
            languages = Array.Empty<string>(),
            includeShared = false,
            query = "discovery reservation BeaconApi",
        }).ConfigureAwait(false));
        Assert.Equal(["fixture.direct", "fixture.directive", "fixture.neutral", "fixture.shared", "fixture.windows"], RecipeIds(allAreas));
        JsonElement windows = allAreas.GetProperty("recipes")[4];
        Assert.Equal("Windows only; unrelated to the requested web area.", Assert.Single(windows.GetProperty("applicability").GetProperty("routes")[0].GetProperty("requirements").GetProperty("platforms").EnumerateArray()).GetString());
        JsonElement forbidden = AssertMatchingRepresentations(await CallAsync("list_recipes", new
        {
            targetVersion = "latest",
            areas = (string[])["blazor"],
            query = "discovery reservation missing",
        }).ConfigureAwait(false));
        Assert.Empty(RecipeIds(forbidden));
        JsonElement splitConditions = AssertMatchingRepresentations(await CallAsync("list_recipes", new
        {
            targetVersion = "latest",
            areas = (string[])["blazor"],
            languages = (string[])["fsharp"],
            query = "routecoherence",
        }).ConfigureAwait(false));
        Assert.Empty(RecipeIds(splitConditions));
        JsonElement sameRoute = AssertMatchingRepresentations(await CallAsync("list_recipes", new
        {
            targetVersion = "latest",
            areas = (string[])["wpf"],
            languages = (string[])["fsharp"],
            query = "routecoherence",
        }).ConfigureAwait(false));
        Assert.Equal(["fixture.route-conjunction"], RecipeIds(sameRoute));
        JsonElement matchedRoute = Assert.Single(sameRoute.GetProperty("recipes")[0].GetProperty("applicability").GetProperty("routes").EnumerateArray());
        Assert.Equal(["wpf"], matchedRoute.GetProperty("sharedAreas").EnumerateArray().Select(value => value.GetString()));
    }

    [Then("discovery pages preserve ordinal order totals and explicit boundaries")]
    public async Task CheckDiscoveryPagingAsync()
    {
        JsonElement first = AssertMatchingRepresentations(await CallAsync("list_recipes", new { targetVersion = "latest", query = "traversal fixture" }).ConfigureAwait(false));
        Assert.Equal(20, first.GetProperty("recipes").GetArrayLength());
        Assert.Equal(0, first.GetProperty("offset").GetInt32());
        Assert.Equal(20, first.GetProperty("limit").GetInt32());
        Assert.Equal(53, first.GetProperty("total").GetInt32());
        List<string> ids = [.. RecipeIds(first)];
        int offset = first.GetProperty("nextOffset").GetInt32();
        while (true)
        {
            JsonElement page = AssertMatchingRepresentations(await CallAsync("list_recipes", new { targetVersion = "latest", query = "traversal fixture", offset, limit = 20 }).ConfigureAwait(false));
            Assert.Equal(53, page.GetProperty("total").GetInt32());
            ids.AddRange(RecipeIds(page));
            if (page.GetProperty("nextOffset").ValueKind == JsonValueKind.Null)
            {
                break;
            }

            int next = page.GetProperty("nextOffset").GetInt32();
            Assert.True(next > offset);
            offset = next;
            Assert.True(ids.Count <= 53, "Paging must terminate without repeating results.");
        }

        string[] expected = [.. Enumerable.Range(0, 53).Select(index => $"fixture.page-{index:D2}")];
        Assert.Equal(expected, ids);
        JsonElement repeated = AssertMatchingRepresentations(await CallAsync("list_recipes", new { targetVersion = "latest", query = "traversal fixture", offset = 0 }).ConfigureAwait(false));
        Assert.True(JsonElement.DeepEquals(first, repeated));
        JsonElement maximum = AssertMatchingRepresentations(await CallAsync("list_recipes", new { targetVersion = "latest", query = "traversal fixture", limit = 50 }).ConfigureAwait(false));
        Assert.Equal(50, maximum.GetProperty("recipes").GetArrayLength());
        Assert.Equal(50, maximum.GetProperty("nextOffset").GetInt32());
        JsonElement beyond = AssertMatchingRepresentations(await CallAsync("list_recipes", new { targetVersion = "latest", query = "traversal fixture", offset = int.MaxValue }).ConfigureAwait(false));
        Assert.Empty(RecipeIds(beyond));
        Assert.Equal(53, beyond.GetProperty("total").GetInt32());
        Assert.Equal(JsonValueKind.Null, beyond.GetProperty("nextOffset").ValueKind);
    }

    [Then("invalid filters and ranges explain corrections while zero matches succeed")]
    public async Task CheckDiscoveryFailuresAsync()
    {
        JsonElement area = AssertToolError(await CallAsync("list_recipes", new { targetVersion = "10.0", areas = (string[])["csharp", "blazro"] }).ConfigureAwait(false), "unknown_area");
        Assert.Equal("areas", area.GetProperty("filter").GetString());
        Assert.Equal(["blazro"], area.GetProperty("invalidValues").EnumerateArray().Select(value => value.GetString()));
        Assert.Contains("csharp", area.GetProperty("supportedValues").EnumerateArray().Select(value => value.GetString()));
        JsonElement language = AssertToolError(await CallAsync("list_recipes", new { targetVersion = "10.0", languages = (string[])["cshrap"] }).ConfigureAwait(false), "unknown_language");
        Assert.Equal("languages", language.GetProperty("filter").GetString());
        Assert.Contains("csharp", language.GetProperty("supportedValues").EnumerateArray().Select(value => value.GetString()));
        foreach (object arguments in new object[] { new { targetVersion = "10.0", offset = -1 }, new { targetVersion = "10.0", limit = 0 }, new { targetVersion = "10.0", limit = 51 } })
        {
            JsonElement error = AssertToolError(await CallAsync("list_recipes", arguments).ConfigureAwait(false), "invalid_range");
            Assert.Contains("offset", error.GetProperty("message").GetString(), StringComparison.Ordinal);
            Assert.Contains("50", error.GetProperty("message").GetString(), StringComparison.Ordinal);
        }

        JsonElement empty = AssertMatchingRepresentations(await CallAsync("list_recipes", new { targetVersion = "10.0", areas = (string[])["csharp"], languages = (string[])["csharp"], query = "unmatched term" }).ConfigureAwait(false));
        Assert.Empty(RecipeIds(empty));
        Assert.Equal(0, empty.GetProperty("total").GetInt32());
        Assert.Equal(JsonValueKind.Null, empty.GetProperty("nextOffset").ValueKind);
        JsonElement blank = AssertMatchingRepresentations(await CallAsync("list_recipes", new { targetVersion = "10.0", query = " \t\n " }).ConfigureAwait(false));
        JsonElement omitted = AssertMatchingRepresentations(await CallAsync("list_recipes", new { targetVersion = "10.0" }).ConfigureAwait(false));
        Assert.True(JsonElement.DeepEquals(blank, omitted));
    }

    [Then("JSON workflows retain complete canonical bodies applicability and editions")]
    public async Task CheckJsonWorkflowsAsync()
    {
        foreach ((string goal, string sample, string file, string region) in new[]
        {
            ("stream-json-lines", "JsonStreaming", "Streaming", "streaming"),
            ("reuse-writer", "JsonStreaming", "Streaming", "writer"),
            ("configure-generic-contracts", "JsonResolution", "Contracts", "generic"),
            ("retrieve-typed-metadata", "JsonResolution", "Contracts", "metadata"),
            ("generate-private-contracts", "JsonModels", "Models", "private"),
            ("preserve-omitted-initializers", "JsonModels", "Models", "initializers"),
            ("bind-reference-constructors", "JsonModels", "Models", "constructors"),
            ("configure-member-conventions", "JsonContracts", "Contracts", "conventions"),
            ("use-numeric-collection-contracts", "JsonContracts", "Contracts", "values"),
            ("describe-binary-schema", "JsonContracts", "Contracts", "schema"),
            ("reject-malformed-contracts", "JsonImport", "Import", "import")
        })
        {
            string id = $"json.{goal}";
            bool foundation = sample == "JsonImport";
            string edition = foundation ? "10.0" : "11.0";
            foreach (string target in new[] { "10.0", "11.0" })
            {
                JsonElement list = AssertMatchingRepresentations(await CallAsync("list_recipes", new { targetVersion = target, query = goal }).ConfigureAwait(false));
                if (!foundation && target == "10.0")
                {
                    Assert.Empty(RecipeIds(list));
                    JsonElement failure = await CallAsync("get_recipe", new { id, targetVersion = target }).ConfigureAwait(false);
                    Assert.True(failure.GetProperty("isError").GetBoolean());
                    continue;
                }

                JsonElement summary = Assert.Single(list.GetProperty("recipes").EnumerateArray());
                Assert.Equal(id, summary.GetProperty("id").GetString());
                Assert.NotEmpty(summary.GetProperty("goal").GetString()!);
                JsonElement detail = AssertMatchingRepresentations(await CallAsync("get_recipe", new { id, targetVersion = target }).ConfigureAwait(false));
                JsonElement recipe = detail.GetProperty("recipe");
                Assert.Equal(edition, recipe.GetProperty("edition").GetString());
                Assert.Equal(edition, recipe.GetProperty("introducedIn").GetString());
                AssertSelection(summary, recipe);
                string expected = await ReadCanonicalAsync($"{id}.md").ConfigureAwait(false);
                foreach ((string token, string release) in new[] { ("older", foundation ? "net9.0" : "net10.0"), ("newer", foundation ? "net10.0" : "net11.0") })
                {
                    string source = await ReadCanonicalAsync($"{release}.{sample}.{file}.cs").ConfigureAwait(false);
                    string snippet = source.Split($"// snippet:{region}\n", StringSplitOptions.None)[1]
                        .Split($"// endsnippet:{region}", StringSplitOptions.None)[0].TrimEnd();
                    expected = expected.Replace($"{{{{snippet:{token}}}}}", $"```csharp\n{snippet}\n```", StringComparison.Ordinal);
                }

                Assert.Equal(expected, recipe.GetProperty("markdown").GetString());
                Assert.DoesNotContain("{{snippet:", expected, StringComparison.Ordinal);
                JsonElement route = Assert.Single(recipe.GetProperty("applicability").GetProperty("routes").EnumerateArray());
                Assert.Equal(target, route.GetProperty("target").GetString());
                Assert.Equal(target, route.GetProperty("requirements").GetProperty("runtime").GetProperty("version").GetString());
                Assert.NotEmpty(route.GetProperty("requirements").GetProperty("constraints").EnumerateArray());
                Assert.NotEmpty(recipe.GetProperty("sources").EnumerateArray());
                Assert.Equal("not-required", recipe.GetProperty("humanProof").GetProperty("status").GetString());
                Assert.All(recipe.GetProperty("evidence").EnumerateArray(), evidence =>
                {
                    Assert.Equal("current", evidence.GetProperty("freshness").GetString());
                    Assert.Equal("compile-and-run", evidence.GetProperty("kind").GetString());
                    Assert.Contains("PASS", evidence.GetProperty("output").GetString(), StringComparison.Ordinal);
                });
                if (goal == "use-numeric-collection-contracts")
                {
                    Assert.Contains("net11.0", route.GetProperty("requirements").GetProperty("constraints").ToString(), StringComparison.Ordinal);
                }
            }
        }
    }

    [Then("LINQ guidance retains complete canonical bodies applicability and editions")]
    public async Task CheckLinqGuidanceAsync()
    {
        foreach ((string goal, string sample, string file, string region, bool foundation) in new[]
        {
            ("retain-unmatched-rows", "Reconciliation", "Manifests", "outer", true),
            ("reconcile-all-rows", "Reconciliation", "Manifests", "full", false),
            ("transform-async-stream", "AsyncStream", "Readings", "stream", true),
        })
        {
            string id = $"linq.{goal}";
            string edition = foundation ? "10.0" : "11.0";
            foreach (string target in new[] { "10.0", "11.0" })
            {
                JsonElement list = AssertMatchingRepresentations(await CallAsync("list_recipes", new { targetVersion = target, query = goal }).ConfigureAwait(false));
                if (!foundation && target == "10.0")
                {
                    Assert.Empty(RecipeIds(list));
                    AssertToolError(await CallAsync("get_recipe", new { id, targetVersion = target }).ConfigureAwait(false), "no_applicable_edition");
                    continue;
                }

                JsonElement summary = Assert.Single(list.GetProperty("recipes").EnumerateArray());
                Assert.Equal(id, summary.GetProperty("id").GetString());
                Assert.NotEmpty(summary.GetProperty("goal").GetString()!);
                JsonElement recipe = AssertMatchingRepresentations(await CallAsync("get_recipe", new { id, targetVersion = target }).ConfigureAwait(false)).GetProperty("recipe");
                Assert.Equal(edition, recipe.GetProperty("edition").GetString());
                Assert.Equal(edition, recipe.GetProperty("introducedIn").GetString());
                AssertSelection(summary, recipe);
                string expected = await ReadCanonicalAsync($"{id}.md").ConfigureAwait(false);
                List<(string Token, string Release, string Region)> snippets =
                [
                    ("older", foundation ? "net9.0" : "net10.0", region),
                    ("newer", foundation ? "net10.0" : "net11.0", region),
                ];
                if (!foundation)
                {
                    snippets.Add(("support", "net10.0", "outer"));
                    snippets.Add(("pairs", "net11.0", "pairs"));
                }

                foreach ((string token, string release, string marker) in snippets)
                {
                    string source = await ReadCanonicalAsync($"{release}.{sample}.{file}.cs").ConfigureAwait(false);
                    string snippet = source.Split($"// snippet:{marker}\n", StringSplitOptions.None)[1]
                        .Split($"// endsnippet:{marker}", StringSplitOptions.None)[0].TrimEnd();
                    expected = expected.Replace($"{{{{snippet:{token}}}}}", $"```csharp\n{snippet}\n```", StringComparison.Ordinal);
                }

                Assert.Equal(expected, recipe.GetProperty("markdown").GetString());
                Assert.DoesNotContain("{{snippet:", expected, StringComparison.Ordinal);
                JsonElement route = Assert.Single(recipe.GetProperty("applicability").GetProperty("routes").EnumerateArray());
                Assert.Equal(target, route.GetProperty("target").GetString());
                Assert.Equal(target, route.GetProperty("requirements").GetProperty("runtime").GetProperty("version").GetString());
                Assert.Empty(route.GetProperty("requirements").GetProperty("packages").EnumerateArray());
                Assert.Empty(route.GetProperty("requirements").GetProperty("providers").EnumerateArray());
                Assert.NotEmpty(recipe.GetProperty("sources").EnumerateArray());
                Assert.Equal("not-required", recipe.GetProperty("humanProof").GetProperty("status").GetString());
                Assert.All(recipe.GetProperty("evidence").EnumerateArray(), evidence =>
                {
                    Assert.Equal("current", evidence.GetProperty("freshness").GetString());
                    Assert.Equal("compile-and-run", evidence.GetProperty("kind").GetString());
                    Assert.Contains("PASS", evidence.GetProperty("output").GetString(), StringComparison.Ordinal);
                });
            }
        }
    }

    [Then("assembly annotation and media guidance retains canonical bodies applicability and editions")]
    public async Task CheckAssemblyAnnotationMediaGuidanceAsync()
    {
        foreach ((string id, string family, string file, string region) in new[]
        {
            ("reflection.inspect-loaded-metadata", "MetadataInspection", "Inspection", "metadata"),
            ("hosting.report-assembly-location", "MetadataInspection", "Inspection", "location"),
            ("tooling.annotate-code-strings", "ContentDescriptions", "Descriptions", "syntax"),
            ("uri.identify-data-uris", "ContentDescriptions", "Descriptions", "media"),
        })
        {
            string goal = id.Split('.')[1];
            JsonElement olderList = AssertMatchingRepresentations(await CallAsync("list_recipes", new { targetVersion = "10.0", query = goal }).ConfigureAwait(false));
            Assert.Empty(RecipeIds(olderList));
            AssertToolError(await CallAsync("get_recipe", new { id, targetVersion = "10.0" }).ConfigureAwait(false), "no_applicable_edition");
            JsonElement list = AssertMatchingRepresentations(await CallAsync("list_recipes", new { targetVersion = "11.0", query = goal }).ConfigureAwait(false));
            JsonElement summary = Assert.Single(list.GetProperty("recipes").EnumerateArray());
            Assert.Equal(id, summary.GetProperty("id").GetString());
            Assert.NotEmpty(summary.GetProperty("goal").GetString()!);
            Assert.Equal("stable", summary.GetProperty("stability").GetString());
            JsonElement recipe = AssertMatchingRepresentations(await CallAsync("get_recipe", new { id, targetVersion = "11.0" }).ConfigureAwait(false)).GetProperty("recipe");
            Assert.Equal("11.0", recipe.GetProperty("edition").GetString());
            Assert.Equal("11.0", recipe.GetProperty("introducedIn").GetString());
            AssertSelection(summary, recipe);
            string expected = await ReadCanonicalAsync($"{id}.md").ConfigureAwait(false);
            bool deployment = family == "MetadataInspection";
            foreach ((string token, string release) in new[] { ("older", "net10.0"), ("newer", "net11.0") })
            {
                string source = await ReadCanonicalAsync($"{release}.{family}.{file}.cs").ConfigureAwait(false);
                string snippet = source.Split($"// snippet:{region}\n", StringSplitOptions.None)[1]
                    .Split($"// endsnippet:{region}", StringSplitOptions.None)[0].TrimEnd();
                expected = expected.Replace($"{{{{snippet:{token}}}}}", $"```csharp\n{snippet}\n```", StringComparison.Ordinal);
                if (deployment)
                {
                    string callerSource = await ReadCanonicalAsync($"{release}.DeploymentInspection.Inspection.cs").ConfigureAwait(false);
                    string caller = callerSource.Split("// snippet:caller\n", StringSplitOptions.None)[1]
                        .Split("// endsnippet:caller", StringSplitOptions.None)[0].TrimEnd();
                    expected = expected.Replace($"{{{{snippet:caller-{token}}}}}", $"```csharp\n{caller}\n```", StringComparison.Ordinal);
                }
            }

            if (id.StartsWith("reflection.", StringComparison.Ordinal))
            {
                string customBase = await ReadCanonicalAsync("MetadataInspection.HarbourTokenType.cs").ConfigureAwait(false);
                string baseSnippet = customBase.Split("// snippet:custom-base\n", StringSplitOptions.None)[1]
                    .Split("// endsnippet:custom-base", StringSplitOptions.None)[0].TrimEnd();
                expected = expected.Replace("{{snippet:custom-base}}", $"```csharp\n{baseSnippet}\n```", StringComparison.Ordinal);
                foreach ((string token, string release) in new[] { ("custom-older", "net10.0"), ("custom-newer", "net11.0") })
                {
                    string custom = await ReadCanonicalAsync($"{release}.MetadataInspection.Inspection.cs").ConfigureAwait(false);
                    string customSnippet = custom.Split("// snippet:custom-type\n", StringSplitOptions.None)[1]
                        .Split("// endsnippet:custom-type", StringSplitOptions.None)[0].TrimEnd();
                    expected = expected.Replace($"{{{{snippet:{token}}}}}", $"```csharp\n{customSnippet}\n```", StringComparison.Ordinal);
                }
            }

            Assert.Equal(expected, recipe.GetProperty("markdown").GetString());
            Assert.DoesNotContain("{{snippet:", expected, StringComparison.Ordinal);
            JsonElement route = Assert.Single(recipe.GetProperty("applicability").GetProperty("routes").EnumerateArray());
            Assert.Equal("11.0", route.GetProperty("target").GetString());
            JsonElement requirements = route.GetProperty("requirements");
            Assert.Equal("11.0", requirements.GetProperty("runtime").GetProperty("version").GetString());
            Assert.Equal("11.0", requirements.GetProperty("sdk").GetProperty("version").GetString());
            Assert.Equal("15.0", requirements.GetProperty("language").GetProperty("version").GetString());
            Assert.Equal(id.StartsWith("reflection.", StringComparison.Ordinal) ? 1 : 0, requirements.GetProperty("packages").GetArrayLength());
            foreach (string field in new[] { "providers", "workloads", "renderingModes", "optIns" })
            {
                Assert.Empty(requirements.GetProperty(field).EnumerateArray());
            }

            Assert.Equal("not-required", recipe.GetProperty("humanProof").GetProperty("status").GetString());
            Assert.Equal(deployment ? 4 : 2, recipe.GetProperty("evidence").GetArrayLength());
            Assert.All(recipe.GetProperty("evidence").EnumerateArray(), evidence =>
            {
                Assert.Equal("current", evidence.GetProperty("freshness").GetString());
                Assert.Equal("compile-and-run", evidence.GetProperty("kind").GetString());
                Assert.Contains("PASS", evidence.GetProperty("output").GetString(), StringComparison.Ordinal);
                if (evidence.GetProperty("sample").GetString()!.Contains("DeploymentInspection", StringComparison.Ordinal))
                {
                    Assert.Contains("native-enabled", evidence.GetProperty("output").GetString(), StringComparison.Ordinal);
                    Assert.Contains("native-disabled", evidence.GetProperty("output").GetString(), StringComparison.Ordinal);
                    Assert.Contains("location=(unavailable)", evidence.GetProperty("output").GetString(), StringComparison.Ordinal);
                }
            });
        }
    }

    [Then("bit collection and vector guidance retains canonical bodies applicability editions and pending proof")]
    public async Task CheckBitCollectionAndVectorGuidanceAsync()
    {
        foreach ((string id, string family, string file, string region) in new[]
        {
            ("collections.count-and-build-bits", "BitCollections", "Collections", "bits"),
            ("collections.compare-by-key", "BitCollections", "Collections", "keys"),
            ("simd.arrange-lanes", "VectorLanes", "Lanes", "lanes"),
            ("simd.select-supported-instructions", "SupportedInstructions", "Instructions", "dispatch"),
        })
        {
            string goal = id.Split('.')[1];
            JsonElement olderList = AssertMatchingRepresentations(await CallAsync("list_recipes", new { targetVersion = "10.0", query = goal }).ConfigureAwait(false));
            Assert.Empty(RecipeIds(olderList));
            AssertToolError(await CallAsync("get_recipe", new { id, targetVersion = "10.0" }).ConfigureAwait(false), "no_applicable_edition");
            JsonElement list = AssertMatchingRepresentations(await CallAsync("list_recipes", new { targetVersion = "11.0", query = goal }).ConfigureAwait(false));
            JsonElement summary = Assert.Single(list.GetProperty("recipes").EnumerateArray());
            Assert.Equal(id, summary.GetProperty("id").GetString());
            Assert.NotEmpty(summary.GetProperty("goal").GetString()!);
            JsonElement recipe = AssertMatchingRepresentations(await CallAsync("get_recipe", new { id, targetVersion = "11.0" }).ConfigureAwait(false)).GetProperty("recipe");
            Assert.Equal("11.0", recipe.GetProperty("edition").GetString());
            Assert.Equal("11.0", recipe.GetProperty("introducedIn").GetString());
            AssertSelection(summary, recipe);
            string expected = await ReadCanonicalAsync($"{id}.md").ConfigureAwait(false);
            foreach ((string token, string release) in new[] { ("older", "net10.0"), ("newer", "net11.0") })
            {
                string source = await ReadCanonicalAsync($"{release}.{family}.{file}.cs").ConfigureAwait(false);
                string snippet = source.Split($"// snippet:{region}\n", StringSplitOptions.None)[1]
                    .Split($"// endsnippet:{region}", StringSplitOptions.None)[0].TrimEnd();
                expected = expected.Replace($"{{{{snippet:{token}}}}}", $"```csharp\n{snippet}\n```", StringComparison.Ordinal);
            }

            bool advancedInstructions = family == "SupportedInstructions";
            if (advancedInstructions)
            {
                string fallback = await ReadCanonicalAsync("net11.0.SupportedInstructions.Fallbacks.cs").ConfigureAwait(false);
                Assert.Equal(await ReadCanonicalAsync("net10.0.SupportedInstructions.Fallbacks.cs").ConfigureAwait(false), fallback);
                string snippet = fallback.Split("// snippet:fallback\n", StringSplitOptions.None)[1]
                    .Split("// endsnippet:fallback", StringSplitOptions.None)[0].TrimEnd();
                expected = expected.Replace("{{snippet:fallback}}", $"```csharp\n{snippet}\n```", StringComparison.Ordinal);
            }

            Assert.Equal(expected, recipe.GetProperty("markdown").GetString());
            Assert.DoesNotContain("{{snippet:", expected, StringComparison.Ordinal);
            JsonElement route = Assert.Single(recipe.GetProperty("applicability").GetProperty("routes").EnumerateArray());
            Assert.Equal("11.0", route.GetProperty("target").GetString());
            JsonElement requirements = route.GetProperty("requirements");
            Assert.Equal("11.0", requirements.GetProperty("runtime").GetProperty("version").GetString());
            Assert.Equal("11.0", requirements.GetProperty("sdk").GetProperty("version").GetString());
            Assert.Equal("15.0", requirements.GetProperty("language").GetProperty("version").GetString());
            foreach (string field in new[] { "packages", "packageAssets", "providers", "workloads", "renderingModes" })
            {
                Assert.Empty(requirements.GetProperty(field).EnumerateArray());
            }

            Assert.Equal(advancedInstructions ? "experimental" : "stable", summary.GetProperty("stability").GetString());
            Assert.Equal(advancedInstructions ? "pending" : "not-required", recipe.GetProperty("humanProof").GetProperty("status").GetString());
            if (advancedInstructions)
            {
                Assert.NotEmpty(requirements.GetProperty("optIns").EnumerateArray());
                Assert.Contains("SYSLIB5003", requirements.GetProperty("optIns").ToString(), StringComparison.Ordinal);
                Assert.Contains("pending", summary.GetProperty("goal").GetString(), StringComparison.Ordinal);
                Assert.Contains("Pending Human proof", expected, StringComparison.Ordinal);
            }

            Assert.Equal(2, recipe.GetProperty("evidence").GetArrayLength());
            Assert.All(recipe.GetProperty("evidence").EnumerateArray(), evidence =>
            {
                Assert.Equal("current", evidence.GetProperty("freshness").GetString());
                Assert.Equal("compile-and-run", evidence.GetProperty("kind").GetString());
                Assert.Contains("PASS", evidence.GetProperty("output").GetString(), StringComparison.Ordinal);
            });
        }
    }

    [Then("numeric guidance retains complete canonical bodies applicability and editions")]
    public async Task CheckNumericGuidanceAsync()
    {
        foreach ((string goal, string region) in new[]
        {
            ("choose-ieee-decimal", "decimal"),
            ("select-complex-precision", "complex"),
            ("preserve-conversion-intent", "conversion"),
            ("parse-delimited-values", "parsing"),
            ("roundtrip-float-bits", "bits"),
            ("generate-generic-random", "random"),
        })
        {
            string id = $"numerics.{goal}";
            JsonElement olderList = AssertMatchingRepresentations(await CallAsync("list_recipes", new { targetVersion = "10.0", query = goal }).ConfigureAwait(false));
            Assert.Empty(RecipeIds(olderList));
            AssertToolError(await CallAsync("get_recipe", new { id, targetVersion = "10.0" }).ConfigureAwait(false), "no_applicable_edition");
            JsonElement list = AssertMatchingRepresentations(await CallAsync("list_recipes", new { targetVersion = "11.0", query = goal }).ConfigureAwait(false));
            JsonElement summary = Assert.Single(list.GetProperty("recipes").EnumerateArray());
            Assert.Equal(id, summary.GetProperty("id").GetString());
            Assert.NotEmpty(summary.GetProperty("goal").GetString()!);
            JsonElement recipe = AssertMatchingRepresentations(await CallAsync("get_recipe", new { id, targetVersion = "11.0" }).ConfigureAwait(false)).GetProperty("recipe");
            Assert.Equal("11.0", recipe.GetProperty("edition").GetString());
            Assert.Equal("11.0", recipe.GetProperty("introducedIn").GetString());
            AssertSelection(summary, recipe);
            string expected = await ReadCanonicalAsync($"{id}.md").ConfigureAwait(false);
            foreach ((string token, string release) in new[] { ("older", "net10.0"), ("newer", "net11.0") })
            {
                string source = await ReadCanonicalAsync($"{release}.NumericGuidance.Numbers.cs").ConfigureAwait(false);
                string snippet = source.Split($"// snippet:{region}\n", StringSplitOptions.None)[1]
                    .Split($"// endsnippet:{region}", StringSplitOptions.None)[0].TrimEnd();
                expected = expected.Replace($"{{{{snippet:{token}}}}}", $"```csharp\n{snippet}\n```", StringComparison.Ordinal);
            }

            Assert.Equal(expected, recipe.GetProperty("markdown").GetString());
            Assert.DoesNotContain("{{snippet:", expected, StringComparison.Ordinal);
            JsonElement route = Assert.Single(recipe.GetProperty("applicability").GetProperty("routes").EnumerateArray());
            Assert.Equal("11.0", route.GetProperty("target").GetString());
            JsonElement requirements = route.GetProperty("requirements");
            Assert.Equal("11.0", requirements.GetProperty("runtime").GetProperty("version").GetString());
            Assert.Equal("11.0", requirements.GetProperty("sdk").GetProperty("version").GetString());
            Assert.Equal("15.0", requirements.GetProperty("language").GetProperty("version").GetString());
            foreach (string field in new[] { "packages", "packageAssets", "providers", "workloads", "renderingModes", "optIns" })
            {
                Assert.Empty(requirements.GetProperty(field).EnumerateArray());
            }
            Assert.NotEmpty(recipe.GetProperty("sources").EnumerateArray());
            Assert.Equal("not-required", recipe.GetProperty("humanProof").GetProperty("status").GetString());
            Assert.Equal(2, recipe.GetProperty("evidence").GetArrayLength());
            Assert.All(recipe.GetProperty("evidence").EnumerateArray(), evidence =>
            {
                Assert.Equal("current", evidence.GetProperty("freshness").GetString());
                Assert.Equal("compile-and-run", evidence.GetProperty("kind").GetString());
                Assert.Contains("PASS", evidence.GetProperty("output").GetString(), StringComparison.Ordinal);
            });
        }
    }

    [Then("archive and compression guidance retains complete canonical bodies applicability and editions")]
    public async Task CheckArchiveCompressionGuidanceAsync()
    {
        foreach ((string id, string family, string region) in new[]
        {
            ("archive.control-entry-access", "Archives", "access"),
            ("archive.protect-entries", "Archives", "protection"),
            ("archive.handle-corruption", "Archives", "corruption"),
            ("archive.select-tar-format", "Archives", "tar"),
            ("compression.handle-empty-payloads", "Compression", "empty"),
            ("compression.process-buffers", "Compression", "buffers"),
            ("compression.use-zstandard", "Compression", "zstandard"),
        })
        {
            JsonElement list = AssertMatchingRepresentations(await CallAsync("list_recipes", new { targetVersion = "11.0", query = id.Split('.')[1] }).ConfigureAwait(false));
            JsonElement summary = Assert.Single(list.GetProperty("recipes").EnumerateArray(), value => value.GetProperty("id").GetString() == id);
            Assert.NotEmpty(summary.GetProperty("goal").GetString()!);
            JsonElement recipe = AssertMatchingRepresentations(await CallAsync("get_recipe", new { id, targetVersion = "11.0" }).ConfigureAwait(false)).GetProperty("recipe");
            Assert.Equal("11.0", recipe.GetProperty("edition").GetString());
            Assert.Equal("11.0", recipe.GetProperty("introducedIn").GetString());
            AssertSelection(summary, recipe);
            string expected = await ReadCanonicalAsync($"{id}.md").ConfigureAwait(false);
            List<(string Token, string Target, string Region)> snippets = [("older", "net10.0", region), ("newer", "net11.0", region)];
            if (id == "archive.handle-corruption")
            {
                snippets.Add(("zip-older", "net10.0", "zipread"));
                snippets.Add(("zip-newer", "net11.0", "zipread"));
            }
            else if (id == "archive.select-tar-format")
            {
                snippets.Add(("reader", "net11.0", "corruption"));
            }
            else if (family == "Compression")
            {
                snippets.Add(("restore-older", "net10.0", "restore"));
                snippets.Add(("restore-newer", "net11.0", "restore"));
            }

            foreach ((string token, string target, string marker) in snippets)
            {
                string source = await ReadCanonicalAsync($"{target}.{family}.cs").ConfigureAwait(false);
                string snippet = source.Split($"// snippet:{marker}\n", StringSplitOptions.None)[1]
                    .Split($"// endsnippet:{marker}", StringSplitOptions.None)[0].TrimEnd();
                expected = expected.Replace($"{{{{snippet:{token}}}}}", $"```csharp\n{snippet}\n```", StringComparison.Ordinal);
            }

            Assert.Equal(expected, recipe.GetProperty("markdown").GetString());
            Assert.DoesNotContain("{{snippet:", expected, StringComparison.Ordinal);
            JsonElement requirements = Assert.Single(recipe.GetProperty("applicability").GetProperty("routes").EnumerateArray()).GetProperty("requirements");
            Assert.Equal("11.0", requirements.GetProperty("runtime").GetProperty("version").GetString());
            Assert.Empty(requirements.GetProperty("packages").EnumerateArray());
            Assert.Empty(requirements.GetProperty("providers").EnumerateArray());
            Assert.All(recipe.GetProperty("evidence").EnumerateArray(), evidence =>
            {
                Assert.Equal("current", evidence.GetProperty("freshness").GetString());
                Assert.Equal("compile-and-run", evidence.GetProperty("kind").GetString());
                Assert.Contains("PASS:", evidence.GetProperty("output").GetString(), StringComparison.Ordinal);
            });
            JsonElement old = AssertMatchingRepresentations(await CallAsync("list_recipes", new { targetVersion = "10.0", query = id.Split('.')[1] }).ConfigureAwait(false));
            Assert.DoesNotContain(id, RecipeIds(old));
            AssertToolError(await CallAsync("get_recipe", new { id, targetVersion = "10.0" }).ConfigureAwait(false), "no_applicable_edition");
        }
    }

    private static void AssertSelection(JsonElement summary, JsonElement recipe)
    {
        JsonElement selections = summary.GetProperty("applicability").GetProperty("routes");
        JsonElement details = recipe.GetProperty("applicability").GetProperty("routes");
        Assert.Equal(details.GetArrayLength(), selections.GetArrayLength());
        for (int index = 0; index < selections.GetArrayLength(); index++)
        {
            JsonElement selection = selections[index];
            JsonElement detail = details[index];
            foreach (string field in new[] { "target", "usage", "conditional", "sharedAreas", "neutralLanguages", "selectionNotes" })
            {
                Assert.True(JsonElement.DeepEquals(detail.GetProperty(field), selection.GetProperty(field)), field);
            }

            Assert.False(selection.TryGetProperty("sourceUrls", out _));
            Assert.False(selection.TryGetProperty("evidenceSamples", out _));
            JsonElement compact = selection.GetProperty("requirements");
            JsonElement full = detail.GetProperty("requirements");
            Assert.False(compact.TryGetProperty("prerequisites", out _));
            Assert.False(compact.TryGetProperty("constraints", out _));
            foreach (string field in new[] { "runtime", "sdk", "language", "packages", "workloads" })
            {
                JsonElement[] expected = field is "packages" or "workloads" ? [.. full.GetProperty(field).EnumerateArray()] : [full.GetProperty(field)];
                JsonElement[] actual = field is "packages" or "workloads" ? [.. compact.GetProperty(field).EnumerateArray()] : [compact.GetProperty(field)];
                Assert.Equal(expected.Length, actual.Length);
                for (int version = 0; version < expected.Length; version++)
                {
                    Assert.False(actual[version].TryGetProperty("reason", out _));
                    Assert.NotEmpty(expected[version].GetProperty("reason").GetString()!);
                    foreach (string part in new[] { "name", "version", "match" })
                    {
                        Assert.Equal(expected[version].GetProperty(part).GetString(), actual[version].GetProperty(part).GetString());
                    }
                }
            }

            foreach (string field in new[] { "frameworks", "packageAssets", "providers", "platforms", "renderingModes", "optIns", "stabilityLimitations" })
            {
                Assert.True(JsonElement.DeepEquals(full.GetProperty(field), compact.GetProperty(field)), field);
            }

            Assert.NotEmpty(full.GetProperty("prerequisites").EnumerateArray());
            Assert.NotEmpty(full.GetProperty("constraints").EnumerateArray());
            Assert.NotEmpty(detail.GetProperty("sourceUrls").EnumerateArray());
            Assert.NotEmpty(detail.GetProperty("evidenceSamples").EnumerateArray());
        }
    }

    private static string[] RecipeIds(JsonElement list)
    {
        return [.. list.GetProperty("recipes").EnumerateArray().Select(recipe => recipe.GetProperty("id").GetString()!)];
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
        string trace = await File.ReadAllTextAsync(Path.Combine(_directory, "host.log")).ConfigureAwait(false);
        string coreClr = Assert.Single(trace.Split('\n'), line => line.StartsWith("CoreCLR path =", StringComparison.Ordinal));
        _output.WriteLine(coreClr);
        string? expectedRuntime = Environment.GetEnvironmentVariable("PLAYBOOK_TEST_RUNTIME_MAJOR");
        if (expectedRuntime is not null)
        {
            Assert.Contains($"/shared/Microsoft.NETCore.App/{expectedRuntime}.", coreClr.Replace('\\', '/'), StringComparison.Ordinal);
        }

        string? evidenceDirectory = Environment.GetEnvironmentVariable("PLAYBOOK_TEST_EVIDENCE_DIRECTORY");
        if (evidenceDirectory is not null)
        {
            Directory.CreateDirectory(evidenceDirectory);
            string evidencePath = Path.Combine(evidenceDirectory, Path.GetFileName(_directory));
            File.Copy(Path.Combine(_directory, "host.log"), evidencePath + ".host.log");
            string package = Environment.GetEnvironmentVariable(_packageVariable)!;
            using FileStream packageStream = File.OpenRead(package);
            string packageHash = Convert.ToHexStringLower(await SHA256.HashDataAsync(packageStream).ConfigureAwait(false));
            string evidence = JsonSerializer.Serialize(new
            {
                package = Path.GetFileName(package),
                sha256 = packageHash,
                serverProcessId = _server.Id,
                workingDirectory = _directory,
                coreClr = coreClr.Trim(),
                expectedRuntime,
                shutdownExitCode = _server.ExitCode,
            });
            await File.WriteAllTextAsync(evidencePath + ".json", evidence).ConfigureAwait(false);
        }
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
        // Every fresh CLI home triggers first-run setup, even with an explicit --tool-path.
        info.Environment["DOTNET_ADD_GLOBAL_TOOLS_TO_PATH"] = "0";
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
