// snippet:complete
using Microsoft.AspNetCore.Components.AI;
using Microsoft.Extensions.AI;

namespace HarbourDispatch;

internal static class BehaviorChecks
{
    internal static async Task RunAsync()
    {
        using DispatchClient source = new();
        int selections = 0;
        using UIAgent agent = new(source, options =>
        {
            options.AddBlockHandler(new BerthActivityHandler());
            options.RegisterUIAction(AIFunctionFactory.Create((string berth) => { selections++; return berth; }, name: "select_berth"));
        });
        using AgentContext context = new(agent);
        var observed = new List<ContentBlock>();
        using IDisposable subscription = context.RegisterOnBlockAdded((_, block) => observed.Add(block));
        Task turn = context.SendMessageAsync("berth");
        await WaitAsync(() => context.Status == ConversationStatus.AwaitingInput);
        UIActionBlock action = observed.OfType<UIActionBlock>().Single();
        Require(observed.OfType<BerthActivity>().Single().Content.GetProperty("Stage").GetString() == "Berth ready", "Progress must update one activity in place.");
        Require(source.DeclarationReceived && selections == 0, "Declaration must arrive without executing the tool.");
        await Task.WhenAll(action.InvokeAsync(), action.InvokeAsync());
        await turn;
        Require(selections == 1 && action.IsComplete && context.Status == ConversationStatus.Idle, "Frontend invocation and continuation failed.");
        foreach (bool approved in new[] { false, true })
        {
            turn = context.SendMessageAsync("dispatch");
            await WaitAsync(() => context.Status == ConversationStatus.AwaitingInput);
            FunctionApprovalBlock approval = observed.OfType<FunctionApprovalBlock>().Last();
            Require(source.Dispatches == 0, "Pending operation already executed.");
            if (approved) { approval.Approve(); approval.Reject(); }
            else { approval.Reject("Operator declined"); approval.Approve(); }
            await turn;
            Require(source.Dispatches == (approved ? 1 : 0), "Decision did not control the actual ledger mutation.");
            Require(context.Status == ConversationStatus.Idle, "Continuation did not finish.");
        }
        Console.WriteLine("PASS actual Components.AI declarations, single invocation, denial=0, approval=1, continuation; scripted local provider only.");
    }
    private static async Task WaitAsync(Func<bool> ready)
    {
        using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(10));
        while (!ready()) { await Task.Delay(10, timeout.Token); }
    }
    private static void Require(bool condition, string message)
    {
        if (!condition) { throw new InvalidOperationException(message); }
    }
}
// endsnippet:complete
