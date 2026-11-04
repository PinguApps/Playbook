// snippet:rookery-provider
using System.Security.Cryptography;
using System.Runtime.Versioning;
[SupportedOSPlatform("linux")]
internal static class RookerySigner
{
    public static SafeEvpPKeyHandle Open(string keyUri) =>
        SafeEvpPKeyHandle.OpenKeyFromProvider(["default", "base"], keyUri, "provider=default");
}
// endsnippet:rookery-provider
