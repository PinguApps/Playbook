// snippet:program
string manifest = Path.Combine(AppContext.BaseDirectory, "berth.txt");
if (!File.Exists(manifest))
{
    Console.Error.WriteLine("ESTUARY_MISSING_MANIFEST");
    return 23;
}
string cargo = File.ReadAllText(manifest).Trim();
if (args.Length != 1 || cargo != args[0])
{
    Console.Error.WriteLine("ESTUARY_WRONG_CARGO");
    return 24;
}
Console.WriteLine($"ESTUARY_NATIVE: {cargo}; AOT={!System.Runtime.CompilerServices.RuntimeFeature.IsDynamicCodeSupported}");
return 0;
// endsnippet:program
