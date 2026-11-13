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
Console.WriteLine("PASS actual MAUI operation API; local copy/move, rejected and cancelled-drop controls. No native gesture execution.");

