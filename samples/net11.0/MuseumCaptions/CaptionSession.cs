// snippet:complete
using System.Text.Json;
using AGUI.Abstractions;
using AGUI.Client;
using Microsoft.AspNetCore.Components.AI;
using Microsoft.Extensions.AI;
namespace MuseumCaptions;
public sealed class CaptionState
{
    public string Caption { get; set; } = "Tide clock";
    public int Revision { get; set; }
}
// One session per component; all calls run on its Blazor dispatcher.
public sealed class CaptionSession : IDisposable
{
    private readonly HttpClient _http = new();
    private readonly AGUIChatClient _client;
    private readonly string _threadId = Guid.NewGuid().ToString("N");
    public UIAgent<CaptionState> Agent { get; }
    public string Baseline { get; private set; } = "";
    public CaptionSession(string endpoint)
    {
        _client = new(new(_http, endpoint));
        Agent = new(_client, options =>
        {
            options.ChatOptions = new ChatOptions
            {
                RawRepresentationFactory = _ => new RunAgentInput
                {
                    ThreadId = _threadId,
                    State = JsonSerializer.SerializeToElement(Agent!.State.Value)
                }
            };
            options.StateMapper = Map;
            options.RegisterUIAction(AIFunctionFactory.Create((string caption, bool accepted) =>
            {
                if (!Agent!.State.HasPendingPredictiveState) { throw new InvalidOperationException("No proposal."); }
                if (accepted) { Agent.State.AcceptPredictiveState(); }
                else { Agent.State.RejectPredictiveState(); }
                return accepted ? "Caption accepted" : "Caption rejected";
            }, name: "propose_caption"));
        });
    }
    private void Map(StateMapperContext context)
    {
        if (context.Update.RawRepresentation is StateDeltaEvent)
        {
            throw new InvalidDataException("This editor accepts full snapshots only; request a fresh snapshot.");
        }
        if (context.Update.RawRepresentation is StateSnapshotEvent snapshot)
        {
            CaptionState next = snapshot.Snapshot.Deserialize<CaptionState>() ?? throw new InvalidDataException("Missing caption state.");
            Validate(next);
            context.SetState(next);
        }
        foreach (var call in context.Update.Contents.OfType<FunctionCallContent>().Where(c => c.Name == "propose_caption"))
        {
            if (call.Arguments is null || !call.Arguments.TryGetValue("caption", out object? raw) ||
                raw is not JsonElement { ValueKind: JsonValueKind.String } text)
            { throw new InvalidDataException("Expected a caption string."); }
            var proposal = new CaptionState { Caption = text.GetString()!, Revision = Agent.State.Value.Revision + 1 };
            Validate(proposal);
            Baseline = Agent.State.Value.Caption;
            context.SetPredictiveState(proposal);
        }
    }
    private void Validate(CaptionState value)
    {
        if (string.IsNullOrWhiteSpace(value.Caption) || value.Caption.Length > 240 ||
            value.Revision != Agent.State.Value.Revision + 1 || Agent.State.HasPendingPredictiveState)
        { throw new InvalidDataException("Invalid, stale or overlapping caption update."); }
    }
    public void Edit(string value)
    {
        if (Agent.State.HasPendingPredictiveState) { throw new InvalidOperationException("Decide the proposal first."); }
        if (string.IsNullOrWhiteSpace(value) || value.Length > 240) { throw new InvalidDataException("Caption must contain 1–240 characters."); }
        Agent.State.Value = new() { Caption = value, Revision = Agent.State.Value.Revision + 1 };
    }
    public void Dispose() { Agent.Dispose(); _client.Dispose(); _http.Dispose(); }
}
// endsnippet:complete
