// snippet:complete
using Microsoft.AspNetCore.Components.AI;
using Microsoft.Extensions.AI;
namespace ObservatoryLog;
// Bounded text-only storage. Completed plain text is the durable format, not arbitrary AIContent.
public sealed class LocalThread(SavedLog saved) : IConversationThread
{
    private string? _question;
    private readonly System.Text.StringBuilder _answer = new();
    public string ThreadId => saved.ThreadId;
    public bool IsStateful => false;
    public string? ConversationId => null;
    public SavedLog Snapshot => new(ThreadId, [.. saved.Turns]);
    public void AppendUserMessage(ChatMessage message) { _question = message.Text; _answer.Clear(); }
    public void AppendUpdate(ChatResponseUpdate update)
    {
        if (update.Contents.Any(c => c is not TextContent)) { throw new NotSupportedException("This store retains plain text only."); }
        foreach (var text in update.Contents.OfType<TextContent>()) { _answer.Append(text.Text); }
    }
    public void CompleteTurn()
    {
        if (_question is null) { throw new InvalidOperationException("No staged turn."); }
        saved.Turns.Add(new(_question, _answer.ToString())); _question = null; _answer.Clear();
    }
    public IReadOnlyList<ChatResponseUpdate> GetUpdates() => saved.Turns.SelectMany((turn, index) => new[]
    {
        new ChatResponseUpdate(ChatRole.User, turn.Question) { MessageId = $"{ThreadId}-{index}-user" },
        new ChatResponseUpdate(ChatRole.Assistant, turn.Answer) { MessageId = $"{ThreadId}-{index}-assistant" }
    }).ToArray();
}
// endsnippet:complete
