string directory = AppContext.BaseDirectory;
bool current = Environment.Version.Major == 11;
string xml = PolicyCommand.Run(directory, ["msbuild", "BuildInputs.proj", "-t:ReadTrusted", "-nologo"]);
PolicyCommand.Contains(xml, "destination=QUAY-7");
PolicyCommand.Contains(File.ReadAllText(Path.Combine(directory, "working.xml")), "QUAY-8");
PolicyCommand.Contains(File.ReadAllText(Path.Combine(directory, "route.xml")), "QUAY-7");
string flat = PolicyCommand.Run(directory, ["msbuild", "BuildInputs.proj", "-t:ReadFlat", "-nologo"]);
PolicyCommand.Contains(flat, "destination=QUAY-7");
if (current)
{
    foreach (string task in new[] { "PeekDefault", "PokeDefault", "TransformDefault" })
    {
        PolicyCommand.Contains(PolicyCommand.Run(directory, ["msbuild", "BuildInputs.proj", $"-t:{task}", "-nologo"], 1), "DTD");
    }
}

foreach (string wave in new[] { "17.10", "17.12", "17.14", "18.3" })
{
    string output = PolicyCommand.Run(directory, ["msbuild", "BuildInputs.proj", "-t:WaveProbe", "-nologo"], wave: wave);
    if (current) { PolicyCommand.Contains(output, "MSB4272"); PolicyCommand.Contains(output, "18.4"); }
    Console.WriteLine($"PASS wave={wave}, retired-warning={output.Contains("MSB4272", StringComparison.Ordinal)}");
}

PolicyCommand.Contains(PolicyCommand.Run(directory, ["msbuild", "BuildInputs.proj", "-t:RunDispatch", $"-p:DotnetExecutable={Environment.ProcessPath}", "-nologo"]), "dispatch=QUAY-7;QUAY-8");

string clean = PolicyCommand.Run(directory, ["msbuild", "BuildInputs.proj", "-t:WaveProbe", "-nologo"]);
if (clean.Contains("MSB4272", StringComparison.Ordinal)) { throw new InvalidOperationException("Unexpected wave warning without opt-out"); }
Console.WriteLine($"PASS XML trusted/DTD-free migration and change-wave diagnostics; runtime={Environment.Version}");
