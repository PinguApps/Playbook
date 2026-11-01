using Playbook.Samples;

try
{
    
    {
        RuntimeIdentity.Assert();
        System.Reflection.Assembly assembly = typeof(Microsoft.Extensions.DependencyInjection.IServiceProviderIsService).Assembly;
        string path = assembly.Location;
        Console.WriteLine("EXTENSION " + System.Text.Json.JsonSerializer.Serialize(new { path, version = System.Diagnostics.FileVersionInfo.GetVersionInfo(path).ProductVersion,
            sha256 = Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(await File.ReadAllBytesAsync(path))) }));
    }
    await ServiceChecks.Run();
    Console.WriteLine("PASS MuseumServices: actual net11.0 consumer checks complete");
    return 0;
}
catch (Exception error) { await Console.Error.WriteLineAsync(error.ToString()); return 1; }
