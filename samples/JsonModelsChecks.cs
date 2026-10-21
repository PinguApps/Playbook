using System.Text.Json;

namespace Playbook.Samples;

internal static class Program
{
    private static void Main()
    {
        Locker locker = Locker.Create(73);
        string json = locker.Save();
        Check(json == "{\"Seal\":73}" && Locker.Load(json).InspectSeal() == 73, "private state round trip");
        Check(JsonSerializer.Deserialize("{}", PlanContext.Default.StoragePlan)!.Zone == "North", "omitted initializer");
        Check(JsonSerializer.Deserialize("{\"Zone\":\"South\"}", PlanContext.Default.StoragePlan)!.Zone == "South", "explicit initializer override");
        Check(JsonSerializer.Deserialize("{\"Zone\":null}", PlanContext.Default.StoragePlan)!.Zone is null, "explicit null is not omission");
        Check(JsonSerializer.Deserialize("{\"Count\":19}", CountContext.Default.BinCount)!.Count == 19, "constructor binding");
#if NET11_0_OR_GREATER
        Check(JsonSerializer.Deserialize("{\"Count\":21}", CountContext.Default.RefCount)!.Count == 21, "ref binding");
        Check(JsonSerializer.Deserialize("{\"Count\":22}", CountContext.Default.ReadonlyCount)!.Count == 22, "ref readonly binding");
        Check(JsonSerializer.Deserialize("{}", CountContext.Default.OutCount)!.Count == 4, "out constructor default");
        Check(JsonSerializer.Deserialize("{\"Count\":23}", CountContext.Default.OutCount)!.Count == 23, "out writable property post assignment");
        Check(JsonSerializer.Deserialize<BinCount>("{\"Count\":19}")!.Count == 19, "reflection in binding");
        Check(JsonSerializer.Deserialize<RefCount>("{\"Count\":21}")!.Count == 21, "reflection ref binding");
        Check(JsonSerializer.Deserialize<ReadonlyCount>("{\"Count\":22}")!.Count == 22, "reflection ref readonly binding");
        Check(JsonSerializer.Deserialize<OutCount>("{\"Count\":23}")!.Count == 23, "reflection out post assignment");
#endif
        Console.WriteLine($"PASS generated private state, omission/explicit/null, constructor binding; runtime {Environment.Version}");
    }
    private static void Check(bool condition, string claim) { if (!condition) throw new Exception(claim); }
}
