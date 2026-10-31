// snippet:update
using System.Security.Cryptography.X509Certificates;

internal static class ExtensionUpdate
{
    // Historical setter: the caller may already have decoded this instance.
    internal static void Assign(X509BasicConstraintsExtension current, byte[] encoded)
        => current.RawData = encoded;
}
// endsnippet:update
