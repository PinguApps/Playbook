using System.IO.Compression;
using System.Security.Cryptography;

if (args.Length != 0 || !string.Equals(Path.GetFileNameWithoutExtension(Environment.ProcessPath), "dotnet", StringComparison.OrdinalIgnoreCase))
{
    throw new InvalidOperationException("Run this verification DLL with the dotnet host, without arguments.");
}

string directory = Path.Combine(Path.GetTempPath(), $"playbook-timestamps-{Guid.NewGuid():N}");
Directory.CreateDirectory(directory);
File.Copy(Path.Combine(AppContext.BaseDirectory, "global.json"), Path.Combine(directory, "global.json"));
File.Copy(Path.Combine(AppContext.BaseDirectory, "TimestampPolicy.props"), Path.Combine(directory, "TimestampPolicy.props"));
File.WriteAllText(Path.Combine(directory, "NuGet.Config"), "<configuration><packageSources><clear/></packageSources></configuration>");
File.WriteAllText(Path.Combine(directory, "Depot.csproj"), $"<Project Sdk=\"Microsoft.NET.Sdk\"><Import Project=\"TimestampPolicy.props\"/><PropertyGroup><TargetFramework>net{Environment.Version.Major}.0</TargetFramework><PackageId>Depot.Manifest</PackageId><Version>1.0.0</Version><Authors>Playbook sample</Authors><Description>Original local manifest</Description></PropertyGroup><ItemGroup><None Include=\"manifest.txt\" Pack=\"true\" PackagePath=\"content/\"/></ItemGroup></Project>");
string payload = Path.Combine(directory, "manifest.txt");
File.WriteAllText(payload, "berth=14\n");

string Pack(string name, string? policy = null)
{
    File.SetLastWriteTimeUtc(payload, new DateTime(2026, 1, name[^1] == 'a' ? 2 : 4, 0, 0, 0, DateTimeKind.Utc));
    string output = Path.Combine(directory, name);
    List<string> arguments = ["pack", "Depot.csproj", "-c", "Release", "-o", output];
    if (policy is not null) { arguments.Add($"-p:DeterministicTimestamp={policy}"); }
    PolicyCommand.Run(directory, arguments.ToArray());
    return Path.Combine(output, "Depot.Manifest.1.0.0.nupkg");
}

DateTime[] Stamps(string package)
{
    using ZipArchive archive = ZipFile.OpenRead(package);
    DateTime[] stamps = archive.Entries.Select(entry => entry.LastWriteTime.DateTime).ToArray();
    Console.WriteLine($"{package}: {string.Join(",", stamps.Select(stamp => stamp.ToString("O")))}; sha256={Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(package)))}");
    return stamps;
}

void EqualStamps(string first, string second)
{
    DateTime[] a = Stamps(first);
    DateTime[] b = Stamps(second);
    if (a.Length == 0 || a.Distinct().Count() != 1 || b.Length != a.Length || !a.SequenceEqual(b)) { throw new InvalidOperationException("Package entry timestamps differ"); }
}

string first = Pack("repeat-a");
string second = Pack("repeat-b");
#if NET10_0
Normalize.UnsignedArchive(first);
Normalize.UnsignedArchive(second);
EqualStamps(first, second);
Console.WriteLine("PASS older unsigned archive normalization; no full-byte reproducibility claim");
#else
EqualStamps(first, second);
if (!File.ReadAllBytes(first).SequenceEqual(File.ReadAllBytes(second))) { throw new InvalidOperationException("Controlled deterministic packages differ"); }
string buildTime = Pack("true-a", "true");
if (Stamps(buildTime).Distinct().Count() != 1 || Stamps(buildTime)[0].Year != DateTime.UtcNow.Year) { throw new InvalidOperationException("True did not select the build timestamp"); }
string fixedA = Pack("fixed-a", "946684800");
string fixedB = Pack("fixed-b", "946684800");
EqualStamps(fixedA, fixedB);
if (Stamps(fixedA).Any(stamp => stamp != new DateTime(2000, 1, 1))) { throw new InvalidOperationException("Explicit epoch was not honored"); }
foreach (string name in new[] { "default-a", "false-b" })
{
    File.WriteAllText(Path.Combine(directory, "TimestampPolicy.props"), "<Project><PropertyGroup><Deterministic>true</Deterministic></PropertyGroup></Project>");
    string package = Pack(name, name == "false-b" ? "false" : null);
    using ZipArchive archive = ZipFile.OpenRead(package);
    DateTime expected = File.GetLastWriteTime(payload);
    if (archive.GetEntry("content/manifest.txt")!.LastWriteTime.DateTime != expected) { throw new InvalidOperationException("Default/false did not preserve file timestamp"); }
}
Console.WriteLine("PASS fixed RFC3339 repeated package bytes, Unix epoch, true build time, default and false timestamps");
#endif
Console.WriteLine($"PASS package timestamps; runtime={Environment.Version}; fixture={directory}");
