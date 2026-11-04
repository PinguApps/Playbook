#if PLATFORM_PROBE
using System.Runtime.Versioning;

internal static class PlatformProbe
{
    [SupportedOSPlatform("android")]
    internal static HttpResponseMessage Synchronous(HttpClient client, HttpRequestMessage request) => client.Send(request);

    [SupportedOSPlatform("android")]
    internal static Task<string> Asynchronous(HttpClient client, Uri endpoint, CancellationToken token) =>
        MoorlandReports.ReadAsync(client, endpoint, token);
}
#endif
