using Playbook.Samples;

try
{
    if (args.Length == 0)
    {
        RuntimeIdentity.Assert();
        System.Reflection.Assembly assembly = typeof(Microsoft.Extensions.Configuration.EnvironmentVariables.EnvironmentVariablesConfigurationProvider).Assembly;
        string path = assembly.Location;
        Console.WriteLine("EXTENSION " + System.Text.Json.JsonSerializer.Serialize(new { path, version = System.Diagnostics.FileVersionInfo.GetVersionInfo(path).ProductVersion,
            sha256 = Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(path))) }));
    }
    VariableChecks.Run(args);
    Console.WriteLine("PASS ObservatoryVariables: actual net10.0 consumer checks complete");
    return 0;
}
catch (Exception error) { Console.Error.WriteLine(error); return 1; }
