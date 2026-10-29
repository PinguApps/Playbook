// snippet:host
using Microsoft.TemplateEngine.Edge;
using Microsoft.TemplateEngine.Edge.Settings;
using Microsoft.TemplateEngine.Edge.Template;
using Microsoft.TemplateEngine.Abstractions.Installer;

namespace CwmArchive.Hosting;

public static class ArchiveTemplate
{
    public static async Task<string> CreateAsync(string workspace)
    {
        string template = Path.Combine(workspace, "seed");
        Directory.CreateDirectory(Path.Combine(template, ".template.config"));
        var files = new Dictionary<string, string>
        {
            ["template.json"] = Path.Combine(".template.config", "template.json"),
            ["project.txt"] = "ArchiveSeed.csproj",
            ["program.txt"] = "Program.cs"
        };
        foreach (var file in files)
        {
            using var stream = typeof(ArchiveTemplate).Assembly.GetManifestResourceStream("ArchiveSeed." + file.Key)
                ?? throw new InvalidOperationException("Missing embedded seed: " + file.Key);
            using var reader = new StreamReader(stream);
            using var writer = new StreamWriter(Path.Combine(template, file.Value));
            await writer.WriteAsync(await reader.ReadToEndAsync());
        }

        var components = Microsoft.TemplateEngine.Edge.Components.AllComponents
            .Concat(Microsoft.TemplateEngine.Orchestrator.RunnableProjects.Components.AllComponents).ToArray();
        using var host = new DefaultTemplateEngineHost("cwm-archive", "1.0.0", builtIns: components);
        using var settings = new EngineEnvironmentSettings(host, virtualizeSettings: true,
            settingsLocation: Path.Combine(workspace, "settings"));
        using var manager = new TemplatePackageManager(settings);
        var installed = await manager.GetBuiltInManagedProvider().InstallAsync(
            new[] { new InstallRequest(template) }, CancellationToken.None);
        if (installed.Any(item => !item.Success))
            throw new InvalidOperationException("Could not register the embedded archive seed.");
        var templates = await manager.GetTemplatesAsync(CancellationToken.None);
        var seed = templates.Single(item => item.Identity == "CwmArchive.Collection.Index");
        string output = Path.Combine(workspace, "generated");
        var result = await new TemplateCreator(settings).InstantiateAsync(seed, "CwmArchive", "CwmArchive", output,
            new Dictionary<string, string?> { ["collection"] = "MAPS-1932" });
        if (result.Status != CreationResultStatus.Success)
            throw new InvalidOperationException("Template creation failed: " + result.Status);
        return output;
    }
}
// endsnippet:host
