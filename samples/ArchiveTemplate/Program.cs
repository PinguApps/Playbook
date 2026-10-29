// snippet:driver
using CwmArchive.Hosting;

string workspace = args.Length == 1 ? Path.GetFullPath(args[0])
    : Path.Combine(Path.GetTempPath(), "cwm-archive-" + Guid.NewGuid().ToString("N"));
try
{
    string output = await ArchiveTemplate.CreateAsync(workspace);
    string project = await File.ReadAllTextAsync(Path.Combine(output, "CwmArchive.csproj"));
    string program = await File.ReadAllTextAsync(Path.Combine(output, "Program.cs"));
    if (!project.Contains("<TargetFramework>net10.0</TargetFramework>", StringComparison.Ordinal)
        || !program.Contains("archive=CwmArchive;collection=MAPS-1932", StringComparison.Ordinal))
        throw new InvalidOperationException("Generated archive inputs differ.");
    Console.WriteLine($"host={System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription};generated=net10.0;collection=MAPS-1932");
    Console.WriteLine("engine=" + typeof(Microsoft.TemplateEngine.Edge.DefaultTemplateEngineHost).Assembly
        .GetCustomAttributes(typeof(System.Reflection.AssemblyInformationalVersionAttribute), false)
        .Cast<System.Reflection.AssemblyInformationalVersionAttribute>().Single().InformationalVersion);
}
finally
{
    if (args.Length == 0 && Directory.Exists(workspace)) Directory.Delete(workspace, recursive: true);
}
// endsnippet:driver
