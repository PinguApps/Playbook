// snippet:rookery-provider
using System.Security.Cryptography;
using System.Runtime.Versioning;
[SupportedOSPlatform("linux")]
internal static class RookerySigner
{
    public static SafeEvpPKeyHandle Open(string keyUri) =>
        SafeEvpPKeyHandle.OpenKeyFromProvider("default", keyUri);
}
// endsnippet:rookery-provider
