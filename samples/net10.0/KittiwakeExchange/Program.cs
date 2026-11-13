using KittiwakeExchange;
using Microsoft.Maui.Controls;

var board = new WalkBoard();
var over = new DragEventArgs(new DataPackage());
board.DragOver(null, over);
#if NET11_0_OR_GREATER
if (over.AcceptedOperation != DataPackageOperation.Move) throw new Exception("Move proposal missing");
#else
if (over.AcceptedOperation != DataPackageOperation.Copy) throw new Exception("Copy baseline changed");
#endif
if (board.CommitDrop("north-ridge", false, true) || board.Planned.Count != 1 || board.Assigned.Count != 0)
    throw new Exception("Failed drop lost data");
// A cancelled gesture never invokes CommitDrop.

if (!board.CommitDrop("north-ridge", true, false) || board.Planned.Count != 1)
    throw new Exception("Copy removed source");
board = new WalkBoard();
if (!board.CommitDrop("north-ridge", true, true) || board.Planned.Count != 0 || board.Assigned.Count != 1)
    throw new Exception("Successful move did not transfer data");
if (board.CommitDrop("north-ridge", true, true)) throw new Exception("Duplicate move accepted");
Console.WriteLine("PASS actual MAUI operation API; local copy/move, rejected-drop controls. No native gesture execution.");


// This spy checks application request/error handling only, never native sharing.
var share = new RecordingShare();
var image = Path.GetTempFileName();
try
{
    await TripShare.SendAsync(share, image, CancellationToken.None);
    if (share.Request?.Uri != "https://kittiwake.example/walks/north-ridge")
        throw new Exception("Share payload lost");
#if NET11_0_OR_GREATER
    if (share.Request.PreviewImage?.FullPath != image) throw new Exception("Preview missing");
#endif
    if (!File.Exists(image)) throw new Exception("Preview deleted after dispatch");
    var calls = share.Calls;
    using var cancelled = new CancellationTokenSource();
    await cancelled.CancelAsync();
    var status = await TripShare.SendAsync(share, image, cancelled.Token);
    if (share.Calls != calls || status != "Cancelled before dispatch.")
        throw new Exception("Cancelled request dispatched");
    share.Fail = true;
    status = await TripShare.SendAsync(share, image, CancellationToken.None);
    if (!status.StartsWith("Unable to open sharing:", StringComparison.Ordinal)) throw new Exception("Share error hidden");
    Console.WriteLine("PASS application share request, retained file, pre-dispatch cancellation and error controls using IShare spy; no native UI.");
}
finally { File.Delete(image); } // Safe only because the spy never dispatches a native share.

sealed class RecordingShare : Microsoft.Maui.ApplicationModel.DataTransfer.IShare
{
    public Microsoft.Maui.ApplicationModel.DataTransfer.ShareTextRequest? Request { get; private set; }
    public int Calls { get; private set; }
    public bool Fail { get; set; }
    public Task RequestAsync(Microsoft.Maui.ApplicationModel.DataTransfer.ShareTextRequest request)
    {
        Calls++;
        Request = request;
        return Fail ? Task.FromException(new IOException("Controlled dispatch failure")) : Task.CompletedTask;
    }
    public Task RequestAsync(Microsoft.Maui.ApplicationModel.DataTransfer.ShareFileRequest request) => throw new NotSupportedException();
    public Task RequestAsync(Microsoft.Maui.ApplicationModel.DataTransfer.ShareMultipleFilesRequest request) => throw new NotSupportedException();
}

