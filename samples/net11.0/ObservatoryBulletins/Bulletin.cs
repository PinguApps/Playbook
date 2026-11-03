using System.Security.Cryptography;
namespace Observatory;

// snippet:contract
public static class Bulletin
{
    // The caller supplies the protocol's agreed set; never silently substitute one.
    public static CompositeMLDsa Create(CompositeMLDsaAlgorithm agreed)
    {
        if (!CompositeMLDsa.IsAlgorithmSupported(agreed))
            throw new PlatformNotSupportedException($"Required bulletin algorithm {agreed.Name} is unavailable.");
        return CompositeMLDsa.GenerateKey(agreed);
    }
}
// endsnippet:contract
