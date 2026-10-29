// snippet:bridge
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;

namespace CwmLab;

public static class TypedBridge
{
    public const string Completion = "CwmLab.BatchCompleted";

    // This bridge accepts only its negotiated protocol versions and message kind.
    public static LabRun Read(string wire)
    {
        var serializer = JsonDataSerializer.Instance;
        var message = serializer.DeserializeMessage(wire);
#if LEGACY_PLATFORM
        var version = (message as VersionedMessage)?.Version ?? 0;
#else
        var version = message.Version;
#endif
        if (message.MessageType != Completion || version is not (0 or 1 or 2))
        {
            throw new InvalidDataException("Unexpected message kind or unnegotiated protocol version.");
        }

        var run = serializer.DeserializePayload<LabRun>(message)
            ?? throw new InvalidDataException("A completion payload is required.");
        run.Check();
        return run;
    }
}
// endsnippet:bridge
