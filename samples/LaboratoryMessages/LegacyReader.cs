// snippet:legacy
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.Serialization;
using Newtonsoft.Json;

namespace CwmLab;

public static class LegacyReader
{
    // Existing 18.7 code may own Newtonsoft settings for the known platform graph.
    // These concrete public classes are NOT extension contracts in 18.11.
    public static LabRun Read(string wire, bool versionOne)
    {
        // DeserializeMessage's fast path did not populate Payload even in 18.7.
        var message = JsonDataSerializer.Instance.Deserialize<VersionedMessage>(wire)!;
        var settings = new JsonSerializerSettings
        {
            ContractResolver = versionOne
                ? new TestPlatformContractResolver1()
                : new DefaultTestPlatformContractResolver()
        };
        // The resolver chooses its converters. Adding every converter globally
        // is invalid: TestObjectConverter.CanConvert throws NotImplementedException.
        var run = JsonConvert.DeserializeObject<LabRun>(message.Payload!.ToString(), settings)
            ?? throw new InvalidDataException("A completion payload is required.");
        run.Check();
        return run;
    }
}
// endsnippet:legacy
