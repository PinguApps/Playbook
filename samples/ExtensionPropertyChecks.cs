using System.Runtime.InteropServices;
using Playbook.Samples;

List<string> pending = [];
ShelvesWithStatusCollection ownMember = new();
if (ownMember.HasPending) { throw new InvalidOperationException("The extension replaced an instance property."); }
if (!ShelfWorkflow.HasPending(ownMember)) { throw new InvalidOperationException("Interface receiver did not use the extension computation."); }
if (ShelfWorkflow.HasPending(pending)) { throw new InvalidOperationException("Empty shelves reported pending work."); }
pending.Add("shelf-r");
if (!ShelfWorkflow.HasPending(pending)) { throw new InvalidOperationException("Computed value did not observe mutation."); }
pending.Clear();
if (ShelfWorkflow.HasPending(pending)) { throw new InvalidOperationException("Computed value was cached."); }
try { ShelfWorkflow.HasPending(null!); }
catch (ArgumentNullException)
{
    Console.WriteLine($"Runtime: {RuntimeInformation.FrameworkDescription}");
    Console.WriteLine("PASS: extension property empty, mutation, clear, instance-member precedence and null rejection");
    return;
}
throw new InvalidOperationException("Null receiver was accepted.");

internal sealed class ShelvesWithStatusCollection : IReadOnlyCollection<string>
{
    public int Count => 1;
    public bool HasPending => false;
    public IEnumerator<string> GetEnumerator() => Enumerable.Repeat("shelf-r", 1).GetEnumerator();
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}
