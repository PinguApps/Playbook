using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.EnvironmentVariables;

namespace Playbook.Samples;

internal static class VariableChecks
{
    public static void Run(string[] args)
    {
        if (args.Length != 0)
        {
            CheckChild(args[0], args[1]);
            return;
        }
        string prefix = "PLAYBOOK_PIN1112_" + Guid.NewGuid().ToString("N") + "___";
        Dictionary<string, string> inputs = new()
        {
            [prefix + "Dome__Camera___Gain"] = "23",
            [prefix.ToLowerInvariant() + "MiXeD"] = "case",
            [prefix + "Literal_under"] = "single",
            [prefix + "dotted.name"] = "literal-dot",
            [prefix + "Collision__Key"] = "pair",
            [prefix + "Collision:Key"] = "colon"
        };
        for (int count = 1; count <= 8; count++) inputs[prefix + "Run" + new string('_', count) + "End"] = count.ToString();
        inputs[prefix + "Custom_SECTION_Value"] = "custom";
        inputs["UNRELATED_PIN1112_" + Guid.NewGuid().ToString("N")] = "excluded";
        foreach (string mode in new[] { "dots", "default", "null", "custom" })
        {
            ProcessStartInfo start = new(Environment.ProcessPath!) { UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true };
            start.ArgumentList.Add(Assembly.GetExecutingAssembly().Location);
            start.ArgumentList.Add(prefix);
            start.ArgumentList.Add(mode);
            foreach ((string key, string value) in inputs) start.Environment[key] = value;
            start.Environment["DOTNET_ADD_GLOBAL_TOOLS_TO_PATH"] = "0";
            using Process child = Process.Start(start)!;
            Task<string> stdout = child.StandardOutput.ReadToEndAsync();
            Task<string> stderr = child.StandardError.ReadToEndAsync();
            if (!child.WaitForExit(30_000)) { child.Kill(entireProcessTree: true); throw new Exception("Environment child timed out."); }
            Task.WaitAll(stdout, stderr);
            Console.Write(stdout.Result);
            if (child.ExitCode != 0) throw new Exception($"Environment child {mode} failed: {stderr.Result}");
        }
        foreach (string key in inputs.Keys)
            if (Environment.GetEnvironmentVariable(key) is not null) throw new Exception("Child environment leaked to parent.");
        Console.WriteLine("INPUTS " + System.Text.Json.JsonSerializer.Serialize(inputs));
    }

    private static void CheckChild(string prefix, string mode)
    {
        using ConfigurationRoot configuration = (ConfigurationRoot)Load(prefix, mode);
        Dictionary<string, string?> expected = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Dome:Camera.Gain"] = "23", ["mixed"] = "case", ["Literal_under"] = "single",
            ["dotted.name"] = "literal-dot", ["Collision:Key"] = null,
            [mode == "custom" ? "Custom:Value" : "Custom_SECTION_Value"] = "custom"
        };
        string[] dots = ["_", ":", ".", "._", ".:", "..", ".._", "..:"];
        string[] defaults = ["_", ":", ":_", "::", "::_", ":::", ":::_", "::::"];
        bool useDots = mode == "dots";
        if (useDots)
        {
            using ConfigurationRoot rawPrefix = (ConfigurationRoot)ObservatorySettings.Load(prefix);
            if (rawPrefix["Dome:Camera.Gain"] != "23") throw new Exception("Raw transformed-prefix route failed.");
        }
        if (!useDots) { expected.Remove("Dome:Camera.Gain"); expected["Dome:Camera:_Gain"] = "23"; }
        for (int count = 1; count <= 8; count++) expected["Run" + (useDots ? dots : defaults)[count - 1] + "End"] = count.ToString();
        IConfigurationProvider provider = configuration.Providers.Single();
        foreach ((string key, string? value) in expected)
        {
            if (!provider.TryGet(key, out string? actual)) throw new Exception($"Missing {mode} key {key}.");
            if (value is not null && actual != value) throw new Exception($"Wrong {mode} value at {key}: {actual}.");
            if (key == "Collision:Key" && actual is not ("pair" or "colon")) throw new Exception("Unexpected collision winner.");
        }
        // AsEnumerable includes structural ancestors. Only leaf values may be present.
        string[] actualKeys = configuration.AsEnumerable().Where(x => x.Value is not null).Select(x => x.Key).Order(StringComparer.OrdinalIgnoreCase).ToArray();
        if (!actualKeys.SequenceEqual(expected.Keys.Order(StringComparer.OrdinalIgnoreCase), StringComparer.OrdinalIgnoreCase))
            throw new Exception($"Unintended keys in {mode}: {string.Join(',', actualKeys)}");
        if (configuration["MIXED"] != "case") throw new Exception("Case-insensitive lookup failed.");

        IConfigurationSource environment = new ExistingRootSource(configuration);
        using ConfigurationRoot overrideLast = (ConfigurationRoot)new ConfigurationBuilder().Add(environment)
            .AddInMemoryCollection(new Dictionary<string,string?> { ["mixed"] = "override" }).Build();
        using ConfigurationRoot environmentLast = (ConfigurationRoot)new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string,string?> { ["mixed"] = "override" }).Add(environment).Build();
        if (overrideLast["mixed"] != "override" || environmentLast["mixed"] != "case") throw new Exception("Provider precedence changed.");
#if NET11_0_OR_GREATER
        // Exercise the constructor separately from source.Build and reject a broken delegate.
        EnvironmentVariablesConfigurationProvider direct = new(prefix, useDots ? EnvironmentVariablesConfigurationSource.ColonAndDotTransformation : null);
        direct.Load();
        if (!direct.TryGet("mixed", out string? result) || result != "case") throw new Exception("Direct provider load failed.");
        try { _ = new EnvironmentVariablesConfigurationProvider(prefix, _ => null!); }
        catch (InvalidOperationException) { Console.WriteLine($"PASS child {mode}: exact leaf map, collisions, transformed prefix and precedence"); return; }
        throw new Exception("Null transformation result was accepted.");
#else
        Console.WriteLine($"PASS child {mode}: exact leaf map, collisions, transformed prefix and precedence");
#endif
    }

    private static IConfigurationRoot Load(string prefix, string mode)
    {
        // Match and strip an already-transformed, differently cased prefix.
        if (mode == "dots") return ObservatorySettings.Load(prefix[..^3].ToLowerInvariant() + ".");
#if NET11_0_OR_GREATER
        return new ConfigurationBuilder().AddEnvironmentVariables(source =>
        {
            source.Prefix = prefix.Replace("__", ":", StringComparison.Ordinal).ToLowerInvariant();
            if (mode == "custom") source.VariableNameTransformation = name => name.Replace("_SECTION_", ":", StringComparison.Ordinal).Replace("__", ":", StringComparison.Ordinal);
            if (mode == "null") source.VariableNameTransformation = null;
        }).Build();
#else
        if (mode == "custom")
        {
            Dictionary<string, string?> mapped = new(StringComparer.OrdinalIgnoreCase);
            foreach (System.Collections.DictionaryEntry entry in Environment.GetEnvironmentVariables())
            {
                string name = ((string)entry.Key).Replace("_SECTION_", ":", StringComparison.Ordinal).Replace("__", ":", StringComparison.Ordinal);
                string mappedPrefix = prefix.Replace("__", ":", StringComparison.Ordinal);
                if (name.StartsWith(mappedPrefix, StringComparison.OrdinalIgnoreCase)) mapped[name[mappedPrefix.Length..]] = (string?)entry.Value;
            }
            return new ConfigurationBuilder().AddInMemoryCollection(mapped).Build();
        }
        return new ConfigurationBuilder().AddEnvironmentVariables(prefix.Replace("__", ":", StringComparison.Ordinal).ToLowerInvariant()).Build();
#endif
    }

    private sealed class ExistingRootSource(IConfigurationRoot root) : IConfigurationSource
    {
        public IConfigurationProvider Build(IConfigurationBuilder builder) => root.Providers.Single();
    }
}
