// snippet:complete
using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;

namespace QuayChat;

// A deterministic stream source, not a model or remote provider.
public sealed class SurveyChatClient : IChatClient
{
    private TaskCompletionSource _release = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private int _turn;

    public void Release() => _release.TrySetResult();

    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        int turn = ++_turn;
        int historyCount = messages.Count();
        _release = new(TaskCreationOptions.RunContinuationsAsynchronously);
        string messageId = $"survey-{turn}";
        yield return new(ChatRole.Assistant, $"Survey {turn}: ")
        {
            MessageId = messageId,
            ResponseId = messageId
        };
        await _release.Task.WaitAsync(cancellationToken);
        yield return new(ChatRole.Assistant, $"North quay is sheltered. Input messages={historyCount}. <script>untrusted</script>")
        {
            MessageId = messageId,
            ResponseId = messageId,
            FinishReason = ChatFinishReason.Stop
        };
    }

    public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages,
        ChatOptions? options = null, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("This demonstration exposes streaming only.");

    public object? GetService(Type serviceType, object? serviceKey = null) =>
        serviceKey is null && serviceType.IsInstanceOfType(this) ? this : null;

    public void Dispose() => _release.TrySetCanceled();
}
// endsnippet:complete
