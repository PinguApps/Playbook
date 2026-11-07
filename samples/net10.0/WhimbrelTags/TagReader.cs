// snippet:complete
using CoreNFC;
using Foundation;

namespace WhimbrelTags;

public static class TagReader
{
    // Older annotations do not make a missing native result safe to dereference.
    public static void Read(INFCNdefTag tag, Action<string> report)
    {
        tag.ReadNdef((message, error) =>
        {
            if (error is not null)
            {
                report($"Trail marker could not be read: {error.LocalizedDescription}");
                return;
            }
            if (message is null)
            {
                report("No trail marker was returned. Retry within the active session.");
                return;
            }
            report($"Trail marker contains {message.Records.Length} records.");
        });
    }
}
// endsnippet:complete
