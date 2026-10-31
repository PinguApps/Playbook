// snippet:update
using System.Security.Cryptography.X509Certificates;

internal static class ExtensionUpdate
{
    // Replacement: choose the extension type and Critical value explicitly.
    internal static X509BasicConstraintsExtension Replace(X509BasicConstraintsExtension incoming)
        => new(incoming, incoming.Critical);

    // Mutation: this derived override invalidates decoded state. X509Extension
    // copies Critical as well as the OID and bytes from the incoming extension.
    internal static void Mutate(X509BasicConstraintsExtension current, X509BasicConstraintsExtension incoming)
        => current.CopyFrom(incoming);
}
// endsnippet:update
