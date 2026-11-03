// snippet:complete
using System.Text.Json;
using Microsoft.AspNetCore.Components.AI;

namespace HarbourDispatch;

public sealed class BerthActivity : ActivityContentBlock;

public sealed class BerthActivityHandler : ActivityHandler<BerthActivity>
{
    protected override bool TryCreateBlock(BlockMappingContext context, BerthActivity state) => Apply(context, state, out _);
    protected override bool TryUpdateBlock(BlockMappingContext context, BerthActivity state, out bool isCompleted) => Apply(context, state, out isCompleted);
    private static bool Apply(BlockMappingContext context, BerthActivity state, out bool complete)
    {
        complete = false;
        if (context.Update.RawRepresentation is not BerthProgress progress) { return false; }
        state.ActivityType = "berth-check";
        state.Content = JsonSerializer.SerializeToElement(progress);
        complete = progress.Complete;
        context.MarkUpdateHandled();
        return true;
    }
}
// endsnippet:complete
