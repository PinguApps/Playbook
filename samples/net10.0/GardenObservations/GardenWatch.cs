// snippet:watch
using Microsoft.Extensions.FileProviders.Physical;

internal static class GardenWatch
{
    internal static PhysicalFilesWatcher Open(string configuredDirectory)
    {
        string root = Path.GetFullPath(configuredDirectory);
        Directory.CreateDirectory(root);
        var native = new FileSystemWatcher(root);
        try
        {
            return new PhysicalFilesWatcher(
                Path.TrimEndingDirectorySeparator(root) + Path.DirectorySeparatorChar,
                native, pollForChanges: false, ExclusionFilters.Sensitive);
        }
        catch { native.Dispose(); throw; }
    }
}
// endsnippet:watch
