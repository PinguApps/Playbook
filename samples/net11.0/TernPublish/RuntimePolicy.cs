// snippet:complete
using System.Runtime.CompilerServices;
namespace TernPublish;
public static class RuntimePolicy
{
    public static string Describe() => RuntimeFeature.IsDynamicCodeSupported
        ? "Tern catalogue: dynamic code available; prefer generated serializers for trimming."
        : "Tern catalogue: generated serializers required; no runtime code generation.";
}
// endsnippet:complete
