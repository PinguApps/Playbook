using System.Text.Json;
using System.Text.Json.Serialization;

namespace Playbook.Samples;

// snippet:private
public sealed class Locker
{
    [JsonInclude]
    private int Seal { get; set; }
    [JsonConstructor]
    private Locker(int seal) { Seal = seal; }
    public static Locker Create(int seal) => new(seal);
    public int InspectSeal() => Seal;
    public string Save() => JsonSerializer.Serialize(this, LockerContext.Default.Locker);
    public static Locker Load(string json) => JsonSerializer.Deserialize(json, LockerContext.Default.Locker)!;
}
[JsonSerializable(typeof(Locker))]
public partial class LockerContext : JsonSerializerContext;
// endsnippet:private

// snippet:initializers
public sealed class StoragePlan
{
    public string Zone { get; init; } = "North";
}
[JsonSerializable(typeof(StoragePlan))]
public partial class PlanContext : JsonSerializerContext;
// endsnippet:initializers

// snippet:constructors
public sealed class BinCount
{
    [JsonConstructor]
    public BinCount(in int count) { Count = count; }
    public int Count { get; }
}
public sealed class RefCount
{
    [JsonConstructor]
    public RefCount(ref int count) { Count = count; }
    public int Count { get; }
}
public sealed class ReadonlyCount
{
    [JsonConstructor]
    public ReadonlyCount(ref readonly int count) { Count = count; }
    public int Count { get; }
}
public sealed class OutCount
{
    [JsonConstructor]
    public OutCount(out int count) { count = 4; Count = count; }
    public int Count { get; set; }
}
[JsonSerializable(typeof(BinCount))]
[JsonSerializable(typeof(RefCount))]
[JsonSerializable(typeof(ReadonlyCount))]
[JsonSerializable(typeof(OutCount))]
public partial class CountContext : JsonSerializerContext;
// endsnippet:constructors
