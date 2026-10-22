using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;

string directory = Path.Combine(Path.GetTempPath(), $"playbook-package-policy-{Guid.NewGuid():N}");
Directory.CreateDirectory(directory);
File.Copy(Path.Combine(AppContext.BaseDirectory, "global.json"), Path.Combine(directory, "global.json"));
bool current = Environment.Version.Major == 11;
using PackageFeed feed = new();
string config = Path.Combine(directory, "NuGet.Config");
File.WriteAllText(config, $"<configuration><packageSources><clear/><add key=\"quay\" value=\"{feed.Url}index.json\" allowInsecureConnections=\"true\" /></packageSources><auditSources><clear/><add key=\"quay\" value=\"{feed.Url}index.json\" allowInsecureConnections=\"true\" /></auditSources></configuration>");

void Project(string version, bool audit = false, bool exception = false, bool missing = false)
{
    string policy = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "AuditPolicy.props"));
    string warningPolicy = exception ? XElement.Parse(policy).Element("PropertyGroup")!.ToString() : "<PropertyGroup><TreatWarningsAsErrors>true</TreatWarningsAsErrors></PropertyGroup>";
    File.WriteAllText(Path.Combine(directory, "Quay.csproj"), $"<Project Sdk=\"Microsoft.NET.Sdk\">{warningPolicy}<PropertyGroup><TargetFramework>net{Environment.Version.Major}.0</TargetFramework><NuGetAudit>{audit.ToString().ToLowerInvariant()}</NuGetAudit></PropertyGroup><ItemGroup><PackageReference Include=\"Quay.Manifest\" Version=\"{version}\" />{(missing ? "<PackageReference Include=\"Quay.Missing\" Version=\"1.0.0\" />" : "")}</ItemGroup></Project>");
}

void Version(string expected)
{
    XElement project = XElement.Load(Path.Combine(directory, "Quay.csproj"));
    string actual = project.Descendants("PackageReference").First(p => (string?)p.Attribute("Include") == "Quay.Manifest").Attribute("Version")!.Value;
    PolicyCommand.Contains(actual, expected);
    Console.WriteLine($"PASS selected Quay.Manifest {actual}");
}

try
{
    Project("1.0.0");
    if (current)
    {
        PolicyCommand.Run(directory, ["nuget", "remove", "source", "quay", "--configfile", config]);
        PolicyCommand.Run(directory, ["nuget", "add", "source", feed.Url + "index.json", "--name", "quay", "--allow-insecure-connections", "--min-publish-age-hours", "24", "--configfile", config]);
        PolicyCommand.Run(directory, ["nuget", "update", "source", "quay", "--allow-insecure-connections", "--min-publish-age-hours", "48", "--configfile", config]);
        string expectedAge = XElement.Load(Path.Combine(AppContext.BaseDirectory, "Policy.config")).Descendants("add").Single().Attribute("minPublishAgeHours")!.Value;
        string actualAge = XElement.Load(config).Element("packageSources")!.Elements("add").Single().Attribute("minPublishAgeHours")!.Value;
        if (actualAge != expectedAge) { throw new InvalidOperationException("CLI cooldown differs from the canonical source configuration"); }
        PolicyCommand.Run(directory, ["package", "update"]);
        Version("1.1.0");
        Project("1.0.0");
        XDocument empty = XDocument.Load(Path.Combine(directory, "Quay.csproj"));
        empty.Descendants("PackageReference").Remove();
        empty.Save(Path.Combine(directory, "Quay.csproj"));
        PolicyCommand.Run(directory, ["package", "add", "Quay.Manifest"]);
        Version("1.1.0");
        // Explicit versions still restore; cooldown is not a universal ban.
        Project("1.2.0");
        PolicyCommand.Run(directory, ["restore", "--force", "--no-http-cache"]);
        Project("1.2.*");
        PolicyCommand.Contains(PolicyCommand.Run(directory, ["restore", "--force", "--no-http-cache"], 1), "NU1020");
        Project("1.*");
        PolicyCommand.Contains(PolicyCommand.Run(directory, ["restore", "--force", "--no-http-cache"], 1), "NU1020");
        PolicyCommand.Run(directory, ["nuget", "update", "source", "quay", "--allow-insecure-connections", "--min-publish-age-hours", "0", "--configfile", config]);
    }
    else
    {
        // The older workflow deliberately chooses an already reviewed version.
        PolicyCommand.Run(directory, ["package", "update", "Quay.Manifest@1.1.0"]);
        Version("1.1.0");
    }

    Project("1.0.0", audit: true);
    PolicyCommand.Contains(PolicyCommand.Run(directory, ["restore", "--force", "--no-http-cache"], 1), "NU1903");
    if (!current)
    {
        PolicyCommand.Run(directory, ["package", "update", "--vulnerable"], 1);
        Project("1.0.0", audit: true, exception: true);
    }
    PolicyCommand.Run(directory, ["package", "update", "--vulnerable"]);
    Version("1.1.0");
    Project("1.1.0", audit: true);
    PolicyCommand.Run(directory, ["restore", "--force", "--no-http-cache"]);
    Project("1.0.0", audit: true, exception: !current, missing: true);
    PolicyCommand.Contains(PolicyCommand.Run(directory, ["restore", "--force", "--no-http-cache"], 1), "NU1101");
    PolicyCommand.Contains(PolicyCommand.Run(directory, ["package", "update", "--vulnerable"], 1), "Restore failed");
    Version("1.0.0");
    Console.WriteLine($"PASS controlled V3 metadata, cooldown/explicit restore/floating conflict and vulnerability updates; runtime={Environment.Version}; fixture={directory}");
}
finally
{
    Console.WriteLine($"Fixture retained for inspection: {directory}");
}

// Controlled metadata, not a real security advisory or a production feed.
internal sealed class PackageFeed : IDisposable
{
    private static readonly string[] Versions = ["1.0.0", "1.1.0", "1.2.0"];
    private readonly HttpListener _listener = new();
    private readonly Task _worker;
    private readonly DateTimeOffset _now = DateTimeOffset.UtcNow;
    internal string Url { get; }

    internal PackageFeed()
    {
        using TcpListener port = new(IPAddress.Loopback, 0);
        port.Start();
        int number = ((IPEndPoint)port.LocalEndpoint).Port;
        port.Stop();
        Url = $"http://127.0.0.1:{number}/";
        _listener.Prefixes.Add(Url);
        _listener.Start();
        _worker = Task.Run(ServeAsync);
    }

    private async Task ServeAsync()
    {
        while (_listener.IsListening)
        {
            HttpListenerContext context;
            try { context = await _listener.GetContextAsync(); }
            catch (HttpListenerException) { break; }
            catch (ObjectDisposedException) { break; }
            string path = context.Request.Url!.AbsolutePath;
            byte[] body;
            if (path.EndsWith(".nupkg", StringComparison.Ordinal))
            {
                string version = path.Split('/')[3];
                using MemoryStream buffer = new();
                using (ZipArchive archive = new(buffer, ZipArchiveMode.Create, true))
                {
                    await using (StreamWriter writer = new(await archive.CreateEntry("Quay.Manifest.nuspec").OpenAsync()))
                    {
                        await writer.WriteAsync($"<package><metadata><id>Quay.Manifest</id><version>{version}</version><authors>Playbook</authors><description>Original metadata fixture</description></metadata></package>");
                    }
                    archive.CreateEntry("lib/netstandard2.0/_._");
                }
                body = buffer.ToArray();
                context.Response.ContentType = "application/octet-stream";
            }
            else
            {
                object? value = path switch
                {
                    "/index.json" => new { version = "3.0.0", resources = new[] { Resource("flat/", "PackageBaseAddress/3.0.0"), Resource("registration/", "RegistrationsBaseUrl/3.6.0"), Resource("vulnerability/index.json", "VulnerabilityInfo/6.7.0") } },
                    "/flat/quay.manifest/index.json" => new { versions = new[] { "1.0.0", "1.1.0", "1.2.0" } },
                    "/registration/quay.manifest/index.json" => Registration(),
                    "/vulnerability/index.json" => new[] { new Dictionary<string, object> { ["@name"] = "base", ["@id"] = Url + "vulnerability/base.json", ["@updated"] = _now.ToString("O") } },
                    "/vulnerability/base.json" => new Dictionary<string, object> { ["quay.manifest"] = new[] { new { url = "https://example.invalid/advisories/quay-fixture", severity = 2, versions = "[1.0.0, 1.1.0)" } } },
                    _ => null
                };
                if (value is null) { context.Response.StatusCode = 404; }
                context.Response.ContentType = "application/json";
                body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value));
            }
            context.Response.ContentLength64 = body.Length;
            await context.Response.OutputStream.WriteAsync(body);
            context.Response.Close();
        }
    }

    private Dictionary<string, string> Resource(string path, string type) => new() { ["@id"] = Url + path, ["@type"] = type };

    private object Registration()
    {
        object[] leaves = Versions.Select(version => (object)new Dictionary<string, object>
        {
            ["@id"] = Url + $"registration/quay.manifest/{version}.json",
            ["packageContent"] = Url + $"flat/quay.manifest/{version}/quay.manifest.{version}.nupkg",
            ["catalogEntry"] = new Dictionary<string, object>
            {
                ["@id"] = Url + $"catalog/{version}.json", ["id"] = "Quay.Manifest", ["version"] = version,
                ["listed"] = true, ["published"] = _now.AddDays(version == "1.2.0" ? 0 : -30).ToString("O"),
                ["dependencyGroups"] = Array.Empty<object>()
            }
        }).ToArray();
        return new { count = 1, items = new[] { new Dictionary<string, object> { ["@id"] = Url + "registration/quay.manifest/page.json", ["count"] = 3, ["lower"] = "1.0.0", ["upper"] = "1.2.0", ["items"] = leaves } } };
    }

    public void Dispose()
    {
        _listener.Close();
        _worker.GetAwaiter().GetResult();
    }
}
