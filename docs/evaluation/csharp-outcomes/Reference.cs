using System.Text.Json;
namespace TideSync;
public sealed record Applied(string Revision);
public sealed record Unchanged(string Revision);
public sealed record Rejected(string Reason);
public abstract record Result
{
    private Result() { }
    public sealed record Success(Applied Value) : Result;
    public sealed record NoChange(Unchanged Value) : Result;
    public sealed record Failure(Rejected Value) : Result;
}
public static class SyncWorkflow
{
    public static Result Apply(string revision) => new Result.Success(new(revision));
    public static Result Skip(string revision) => new Result.NoChange(new(revision));
    public static Result Reject(string reason) => new Result.Failure(new(reason));
    public static string Describe(Result? result) => result switch
    {
        Result.Success x => "applied:" + x.Value.Revision,
        Result.NoChange x => "unchanged:" + x.Value.Revision,
        Result.Failure x => "rejected:" + x.Value.Reason,
        null => "absent",
        _ => throw new ArgumentException("Unknown result")
    };
    public static string Serialize(Result? result) => result switch
    {
        Result.Success x => JsonSerializer.Serialize(new { applied = x.Value.Revision }),
        Result.NoChange x => JsonSerializer.Serialize(new { unchanged = x.Value.Revision }),
        Result.Failure x => JsonSerializer.Serialize(new { rejected = x.Value.Reason }),
        null => "null",
        _ => throw new ArgumentException("Unknown result")
    };
    public static Result? Deserialize(string json)
    {
        using var doc = JsonDocument.Parse(json);
        if (doc.RootElement.ValueKind == JsonValueKind.Null) return null;
        if (doc.RootElement.ValueKind != JsonValueKind.Object) throw new JsonException();
        var entries = doc.RootElement.EnumerateObject().ToArray();
        if (entries.Length != 1 || entries[0].Value.ValueKind != JsonValueKind.String) throw new JsonException();
        string payload = entries[0].Value.GetString()!;
        return entries[0].Name switch
        {
            "applied" => Apply(payload), "unchanged" => Skip(payload), "rejected" => Reject(payload),
            _ => throw new JsonException()
        };
    }
}
