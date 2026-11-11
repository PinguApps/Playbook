// snippet:complete
using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;
namespace ObservatoryLog;
public sealed class VisitorClient : IChatClient
{
    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages,
        ChatOptions? options = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await Task.Delay(500, cancellationToken);
        yield return new(ChatRole.Assistant, $"Observation noted: {messages.Last().Text}") { MessageId = Guid.NewGuid().ToString("N") };
    }
    public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default) => throw new NotSupportedException("Streaming only.");
    public object? GetService(Type serviceType, object? serviceKey = null) => serviceKey is null && serviceType.IsInstanceOfType(this) ? this : null;
    public void Dispose() { }
}
// endsnippet:complete
