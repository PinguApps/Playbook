// snippet:diagnostic
namespace SeedImports;

public static class ImportDiagnostic
{
    public static string Read(string path)
    {
        string attempted = Path.GetFullPath(path);
        try { return File.ReadAllText(attempted); }
        catch (DirectoryNotFoundException error)
        {
            throw new DirectoryNotFoundException("Seed import directory unavailable.",
                directoryPath: error.DirectoryPath ?? attempted, innerException: error);
        }
    }

    public static string SelectPath(DirectoryNotFoundException error, string? attempted = null) =>
        error.DirectoryPath ?? attempted ?? "(path unavailable)";
}
// endsnippet:diagnostic
