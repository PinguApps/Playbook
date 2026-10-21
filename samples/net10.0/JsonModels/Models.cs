using System.Text.Json;
using System.Text.Json.Serialization;

namespace Playbook.Samples;

// snippet:private
// An explicit DTO keeps private state behind a deliberate mapping boundary.
public sealed class Locker
{
    private readonly int _seal;
    private Locker(int seal) { _seal = seal; }
    public static Locker Create(int seal) => new(seal);
    public int InspectSeal() => _seal;
    public string Save() => JsonSerializer.Serialize(new LockerData(_seal), LockerContext.Default.LockerData);
    public static Locker Load(string json) => new(JsonSerializer.Deserialize(json, LockerContext.Default.LockerData)!.Seal);
}
public sealed record LockerData(int Seal);
[JsonSerializable(typeof(LockerData))]
public partial class LockerContext : JsonSerializerContext;
// endsnippet:private

// snippet:initializers
// A constructor default is a valid .NET 10 workaround for this generated path.
public sealed class StoragePlan
{
    [JsonConstructor]
    public StoragePlan(string zone = "North") { Zone = zone; }
    public string Zone { get; init; }
}
[JsonSerializable(typeof(StoragePlan))]
public partial class PlanContext : JsonSerializerContext;
// endsnippet:initializers

// snippet:constructors
// Use ordinary value parameters for the older generated JSON boundary.
public sealed class BinCount
{
    [JsonConstructor]
    public BinCount(int count) { Count = count; }
    public int Count { get; }
}
[JsonSerializable(typeof(BinCount))]
public partial class CountContext : JsonSerializerContext;
// endsnippet:constructors
