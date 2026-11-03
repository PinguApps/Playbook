// snippet:complete
using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;

namespace HarbourDispatch;

public sealed record BerthProgress(string Stage, bool Complete);

// Deterministic local source. It checks the decision before changing the ledger.
public sealed class DispatchClient : IChatClient
{
    private string? _pending;
    public int Dispatches { get; private set; }
    public bool DeclarationReceived { get; private set; }

    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();
        ChatMessage latest = messages.Last();
        AIContent? continuation = latest.Contents.FirstOrDefault();
        if (continuation is ToolApprovalResponseContent decision)
        {
            if (decision.RequestId != _pending) { throw new InvalidOperationException("Unknown decision."); }
            _pending = null;
            if (decision.Approved) { Dispatches++; }
            yield return new(ChatRole.Assistant, decision.Approved ? "Dispatch recorded." : "Dispatch denied; ledger unchanged.");
            yield break;
        }
        if (continuation is FunctionResultContent result)
        {
            yield return new(ChatRole.Assistant, $"Frontend result received: {result.Result}");
            yield break;
        }
        string callId = Guid.NewGuid().ToString("N");
        if (latest.Text.Contains("dispatch", StringComparison.OrdinalIgnoreCase))
        {
            _pending = callId;
            yield return new(ChatRole.Assistant, [new ToolApprovalRequestContent(callId,
                new FunctionCallContent(callId, "dispatch_crate", new Dictionary<string, object?> { ["crate"] = "H-17" }))]);
        }
        else
        {
            DeclarationReceived = options?.Tools?.Any(tool => tool.Name == "select_berth" && tool is not AIFunction) == true;
            if (!DeclarationReceived) { throw new InvalidOperationException("Frontend declaration missing or executable."); }
            yield return new(ChatRole.Assistant, []) { RawRepresentation = new BerthProgress("Checking berth", false) };
            yield return new(ChatRole.Assistant, []) { RawRepresentation = new BerthProgress("Berth ready", true) };
            yield return new(ChatRole.Assistant, [new FunctionCallContent(callId, "select_berth",
                new Dictionary<string, object?> { ["berth"] = "West-4" })]);
        }
    }

    public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null,
        CancellationToken cancellationToken = default) => throw new NotSupportedException("Streaming sample.");
    public object? GetService(Type serviceType, object? serviceKey = null) =>
        serviceKey is null && serviceType.IsInstanceOfType(this) ? this : null;
    public void Dispose() { }
}
// endsnippet:complete
