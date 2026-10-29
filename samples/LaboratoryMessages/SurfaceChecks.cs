// snippet:surface
using System.Reflection;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
#if LEGACY_PLATFORM
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.Serialization;
#endif

namespace CwmLab;

public static class SurfaceChecks
{
    public static void Verify()
    {
        var assembly = typeof(Message).Assembly;
        var names = new[] { "DefaultTestPlatformContractResolver", "TestCaseConverter", "TestObjectConverter",
            "TestPlatformContractResolver1", "TestResultConverter", "TestRunStatisticsConverter", "VersionedMessage" };
        var exports = assembly.GetExportedTypes();
        foreach (var name in names)
        {
            var visible = exports.Any(t => t.Name == name);
#if LEGACY_PLATFORM
            if (!visible) throw new InvalidDataException($"Missing previous public type {name}.");
#else
            if (visible) throw new InvalidDataException($"Unexpected current public type {name}.");
#endif
            Console.WriteLine($"surface={name};public={visible}");
        }
#if LEGACY_PLATFORM
        // Compile every named previous public class against the actual old asset.
        Type[] oldContracts = [typeof(DefaultTestPlatformContractResolver), typeof(TestCaseConverter),
            typeof(TestObjectConverter), typeof(TestPlatformContractResolver1), typeof(TestResultConverter),
            typeof(TestRunStatisticsConverter), typeof(VersionedMessage)];
        if (oldContracts.Length != 7 || new Message().Payload is not null) throw new InvalidDataException();
#else
        if (typeof(Message).GetProperty("Payload", BindingFlags.Public | BindingFlags.Instance) is not null)
            throw new InvalidDataException("Payload unexpectedly remains public.");
        if (new Message().ToString() != "{}") throw new InvalidDataException("Empty-message semantics changed.");
#endif
        foreach (var method in typeof(JsonDataSerializer).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
            Console.WriteLine($"serializer-member={method}");
    }
}
// endsnippet:surface
