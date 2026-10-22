using System.IO.Compression;
using NuGet.Packaging;

// snippet:host
string package = Path.Combine(Path.GetTempPath(), $"depot-manifest-{Guid.NewGuid():N}.nupkg");
try
{
    using (ZipArchive archive = ZipFile.Open(package, ZipArchiveMode.Create))
    {
        using StreamWriter writer = new(archive.CreateEntry("Depot.Manifest.nuspec").Open());
        writer.Write("<package><metadata><id>Depot.Manifest</id><version>1.2.3</version><authors>Playbook sample</authors><description>Local manifest inspection</description></metadata></package>");
    }

    using PackageArchiveReader reader = new(package);
    NuGet.Packaging.Core.PackageIdentity identity = reader.GetIdentity();
    if (identity.Id != "Depot.Manifest" || identity.Version.ToNormalizedString() != "1.2.3")
    {
        throw new InvalidOperationException("Unexpected manifest identity");
    }

    Console.WriteLine($"PASS {identity.Id}/{identity.Version}; host={Environment.Version}; library={typeof(PackageArchiveReader).Assembly.GetName().Version}");
}
finally
{
    File.Delete(package);
}
// endsnippet:host
