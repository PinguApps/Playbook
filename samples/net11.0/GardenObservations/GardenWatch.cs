// snippet:watch
using Microsoft.Extensions.FileProviders.Physical;

internal static class GardenWatch
{
    internal static PhysicalFilesWatcher Open(string configuredDirectory)
    {
        // The root may be created later. An empty native path permits deferred watching.
        var native = new FileSystemWatcher();
        try
        {
            return new PhysicalFilesWatcher(configuredDirectory, native,
                pollForChanges: false, ExclusionFilters.Sensitive);
        }
        catch { native.Dispose(); throw; }
    }
}
// endsnippet:watch
