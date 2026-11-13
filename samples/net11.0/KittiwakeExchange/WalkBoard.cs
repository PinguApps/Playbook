// snippet:complete
using Microsoft.Maui.Controls;

namespace KittiwakeExchange;

// An in-app drop owns the transaction. Never delete on DragOver or DropCompleted.
public sealed class WalkBoard
{
    public List<string> Planned { get; } = ["north-ridge"];
    public List<string> Assigned { get; } = [];

    public void DragOver(object? sender, DragEventArgs args)
    {
        if (args.Data is not { } data || !data.Properties.TryGetValue("kittiwake-owner", out var owner)
            || !ReferenceEquals(owner, this) || !Planned.Contains(data.Text))
        {
            args.AcceptedOperation = DataPackageOperation.None;
            return;
        }
#if NET11_0_OR_GREATER && !ANDROID
        args.AcceptedOperation = DataPackageOperation.Move;
#else
        args.AcceptedOperation = DataPackageOperation.Copy;
#endif
    }

    public bool CommitDrop(string id, bool destinationAccepted, bool move)
    {
        if (!destinationAccepted || !Planned.Contains(id) || Assigned.Contains(id))
            return false;
        Assigned.Add(id); // Destination validation/write succeeded first.
        if (move) Planned.Remove(id);
        return true;
    }

    public void Attach(Label source, Label destination)
    {
        var drag = new DragGestureRecognizer { CanDrag = true };
        drag.DragStarting += (_, args) =>
        {
            args.Data.Text = "north-ridge";
            args.Data.Properties["kittiwake-owner"] = this;
#if WINDOWS && NET11_0_OR_GREATER
            if (args.PlatformArgs?.DragStartingEventArgs is { } native)
            {
                native.AllowedOperations = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Move;
                native.Data.SetText("north-ridge");
                args.PlatformArgs.Handled = true;
#pragma warning disable CS0618 // GA Windows handler checks both flags with OR.
                args.Handled = true;
#pragma warning restore CS0618
            }
#endif
        };
        var drop = new DropGestureRecognizer { AllowDrop = true };
        drop.DragOver += DragOver;
        drop.Drop += async (_, args) =>
        {
            // Only our own validated local payload is accepted. No cross-app deletion protocol.
            if (!args.Data.Properties.TryGetValue("kittiwake-owner", out var owner) || !ReferenceEquals(owner, this)) return;
            var id = await args.Data.GetTextAsync();
#if NET11_0_OR_GREATER && !ANDROID
            CommitDrop(id, destinationAccepted: true, move: true);
#else
            CommitDrop(id, destinationAccepted: true, move: false);
#endif
        };
        source.GestureRecognizers.Add(drag);
        destination.GestureRecognizers.Add(drop);
    }
}
// endsnippet:complete


