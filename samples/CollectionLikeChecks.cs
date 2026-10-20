using System.Runtime.InteropServices;
using Playbook.Samples;

string[] initial = ["shelf-c", "shelf-a", "shelf-c"];
string[] updated = ["shelf-c", "shelf-r", "shelf-b"];
int[] callResult = [21];
string[] chainLabels = ["shelf-c", "shelf-a", "shelf-b"];
List<string> queue = CollectionWorkflow.Queue();
Check(queue.SequenceEqual(initial), "List order or duplicates changed.");
Check(queue.Capacity >= 12, "Reserved capacity was lost.");
HashSet<string> labels = CollectionWorkflow.Labels();
Check(labels.Count == 2 && labels.Contains("BAY-Q"), "Set comparer was lost.");
Check(!labels.Add("bay-q"), "Equivalent label was added twice.");
CrateLabels built = CollectionWorkflow.BuiltLabels();
Check(built.Count == 2 && built.Contains("BAY-Q"), "Builder comparer was lost.");
CrateLabels defaults = ["bay-q", "BAY-Q"];
Check(defaults.Count == 2, "Default builder overload changed.");
Check(CollectionWorkflow.EscapedCall().SequenceEqual(callResult), "Escaped with call changed.");

LinkedList<string> chain = new(chainLabels);
Check(IndexWorkflow.Read(chain, 1) == "shelf-a", "Indexer read selected the wrong node.");
IndexWorkflow.Write(chain, 1, "shelf-r");
Check(chain.SequenceEqual(updated), "Indexer write changed order or length.");
Reject<ArgumentOutOfRangeException>(() => IndexWorkflow.Read(chain, -1));
Reject<ArgumentOutOfRangeException>(() => IndexWorkflow.Read(chain, chain.Count));
Reject<ArgumentOutOfRangeException>(() => IndexWorkflow.Write(chain, chain.Count, "lost"));
Reject<ArgumentNullException>(() => IndexWorkflow.Read(null!, 0));
Check(chain.SequenceEqual(updated), "Rejected index changed the collection.");
Console.WriteLine($"Runtime: {RuntimeInformation.FrameworkDescription}");
Console.WriteLine("PASS: collection order, capacity, comparer, builder overloads, escaped with call and indexer reads/writes/rejection");

static void Check(bool condition, string message)
{
    if (!condition) { throw new InvalidOperationException(message); }
}

static void Reject<TException>(Action action) where TException : Exception
{
    try { action(); }
    catch (TException) { return; }
    throw new InvalidOperationException($"Expected {typeof(TException).Name}.");
}
