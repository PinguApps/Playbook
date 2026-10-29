// snippet:obsolete-call
namespace Gannet.Legacy;
public static class OldDiagnostic
{
    public static Exception Missing(string configured, string attempted) =>
        NuGet.Credentials.PluginException.CreatePathNotFoundMessage(configured, attempted);
}
// endsnippet:obsolete-call
