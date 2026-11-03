// snippet:complete
using System.Text.Json;
using AGUI.Client;
using Microsoft.AspNetCore.Components.AI;
using Microsoft.Extensions.AI;

namespace HarbourDispatch;

internal static class LoopbackChecks
{
    internal static async Task RunAsync()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        await using WebApplication app = builder.Build();
        int ledger = 0;
        bool declaration = false;
        string? pending = null;
        app.MapPost("/agent", async (HttpContext context) =>
        {
            using JsonDocument input = await JsonDocument.ParseAsync(context.Request.Body);
            JsonElement root = input.RootElement;
            string thread = root.GetProperty("threadId").GetString()!;
            string run = root.GetProperty("runId").GetString()!;
            context.Response.ContentType = "text/event-stream";
            async Task EmitAsync(object value) => await context.Response.WriteAsync($"data: {JsonSerializer.Serialize(value)}\n\n");
            await EmitAsync(new { type = "RUN_STARTED", threadId = thread, runId = run });
            if (root.TryGetProperty("resume", out JsonElement resumes) && resumes.GetArrayLength() > 0)
            {
                JsonElement resume = resumes[0];
                if (resume.GetProperty("interruptId").GetString() != pending) { throw new InvalidOperationException("Uncorrelated approval."); }
                pending = null;
                if (resume.GetProperty("payload").GetProperty("approved").GetBoolean()) { ledger++; }
            }
            else if (root.GetProperty("messages").EnumerateArray().Last().GetProperty("role").GetString() != "tool")
            {
                declaration |= root.TryGetProperty("tools", out JsonElement tools) &&
                    tools.EnumerateArray().Any(tool => tool.GetProperty("name").GetString() == "select_berth");
                bool approval = root.GetProperty("messages").EnumerateArray().Last().GetProperty("content").ToString().Contains("dispatch", StringComparison.Ordinal);
                string call = Guid.NewGuid().ToString("N");
                await EmitAsync(new { type = "TOOL_CALL_START", toolCallId = call, toolCallName = approval ? "dispatch_crate" : "select_berth" });
                await EmitAsync(new { type = "TOOL_CALL_ARGS", toolCallId = call, delta = approval ? "{\"crate\":\"H-17\"}" : "{\"berth\":\"West-4\"}" });
                await EmitAsync(new { type = "TOOL_CALL_END", toolCallId = call });
                if (approval)
                {
                    pending = call;
                    await EmitAsync(new { type = "RUN_FINISHED", threadId = thread, runId = run,
                        outcome = new { type = "interrupt", interrupts = new[] { new { id = call, reason = "tool_call", toolCallId = call } } } });
                    return;
                }
            }
            await EmitAsync(new { type = "RUN_FINISHED", threadId = thread, runId = run });
        });
        await app.StartAsync();
        try
        {
            using HttpClient http = new();
            using AGUIChatClient client = new(new(http, app.Urls.Single() + "/agent"));
            int calls = 0;
            using UIAgent agent = new(client, options => options.RegisterUIAction(
                AIFunctionFactory.Create((string berth) => { calls++; return berth; }, name: "select_berth")));
            using AgentContext conversation = new(agent);
            var blocks = new List<ContentBlock>();
            using IDisposable subscription = conversation.RegisterOnBlockAdded((_, block) => blocks.Add(block));
            Task turn = conversation.SendMessageAsync("berth");
            await AwaitInputAsync(conversation, turn);
            await blocks.OfType<UIActionBlock>().Single().InvokeAsync();
            await turn;
            Require(declaration && calls == 1 && conversation.Error is null, "AG-UI declaration/action failed.");
            foreach (bool approved in new[] { false, true })
            {
                using AGUIChatClient approvalClient = new(new(http, app.Urls.Single() + "/agent"));
                using UIAgent approvalAgent = new(approvalClient);
                using AgentContext approvalContext = new(approvalAgent);
                var approvalBlocks = new List<FunctionApprovalBlock>();
                using IDisposable approvalSubscription = approvalContext.RegisterOnBlockAdded((_, block) =>
                {
                    if (block is FunctionApprovalBlock approval) { approvalBlocks.Add(approval); }
                });
                turn = approvalContext.SendMessageAsync("dispatch");
                await AwaitInputAsync(approvalContext, turn);
                FunctionApprovalBlock block = approvalBlocks.Single();
                Require(ledger == 0, "AG-UI operation ran before approval.");
                if (approved) { block.Approve(); } else { block.Reject(); }
                await turn;
                Require(ledger == (approved ? 1 : 0) && approvalContext.Error is null, "AG-UI decision did not control continuation.");
            }
            Console.WriteLine("PASS real loopback HTTP/SSE AGUI.Client declarations, frontend result, approval interrupt/resume: denial=0 approval=1. Scripted endpoint, no remote model.");
        }
        finally { await app.StopAsync(); }
    }
    private static async Task AwaitInputAsync(AgentContext context, Task turn)
    {
        using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(10));
        while (context.Status != ConversationStatus.AwaitingInput)
        {
            if (turn.IsCompleted) { throw new InvalidOperationException("Turn ended before input.", context.Error); }
            await Task.Delay(10, timeout.Token);
        }
    }
    private static void Require(bool condition, string message)
    {
        if (!condition) { throw new InvalidOperationException(message); }
    }
}
// endsnippet:complete
