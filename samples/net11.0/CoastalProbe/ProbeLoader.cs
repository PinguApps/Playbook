// snippet:complete
namespace CoastalProbe;

public static class ProbeLoader
{
    // Call on the initialization path after the library's prerequisite is ready,
    // before invoking any of its JNI methods. LoadLibrary takes no lib/.so affixes.
    public static void Load() => Java.Lang.JavaSystem.LoadLibrary("coastalprobe");
}
// endsnippet:complete
