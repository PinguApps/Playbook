namespace TideSync;

public sealed record Applied(string Revision);
public sealed record Unchanged(string Revision);
public sealed record Rejected(string Reason);

public static class SyncWorkflow
{
    // Replace the unbounded object result with a type-safe closed set.
    public static object Apply(string revision) => new Applied(revision);
    public static object Skip(string revision) => new Unchanged(revision);
    public static object Reject(string reason) => new Rejected(reason);
    public static string Describe(object? result) => throw new NotImplementedException();
    public static string Serialize(object? result) => throw new NotImplementedException();
    public static object? Deserialize(string json) => throw new NotImplementedException();
}
