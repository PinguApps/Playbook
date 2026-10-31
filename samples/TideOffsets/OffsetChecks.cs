using System.Text.Json;

namespace TideOffsets;

public static class OffsetChecks
{
    public static void Run()
    {
        foreach (bool reversed in new[] { false, true })
        foreach (int offset in new[] { 17, -9 })
        {
            string json = reversed ? $$"""{"Offset":{{offset}},"Station":"Mallow Quay"}""" : $$"""{"Station":"Mallow Quay","Offset":{{offset}}}""";
            TideOffset value = OffsetCodec.Read(json)!;
            if (value.Station != "Mallow Quay" || value.Offset != offset || value.GetType() != (offset > 0 ? typeof(AboveDatum) : typeof(BelowDatum))) throw new InvalidOperationException("Lost type or payload.");
            Console.WriteLine($"ACCEPT {value.GetType().Name} {value.Station} {value.Offset} reversed={reversed}");
        }
        foreach (string json in new[] { "{}", "[]", "{\"Offset\":0}", "{\"Offset\":\"17\"}", "{\"Offset\":2147483648}", "{\"Offset\":1.5}", "{\"Offset\":null}", "{\"Offset\":17,\"Offset\":-9}", "{\"Offset\":17", "{\"Offset\":17} {}" })
        {
            try { OffsetCodec.Read(json); throw new InvalidOperationException($"Accepted {json}"); }
            catch (JsonException) { Console.WriteLine($"REJECT {json}"); }
        }
        if (OffsetCodec.Read("null") is not null) throw new InvalidOperationException("Expected reference null.");
        if (JsonSerializer.IsReflectionEnabledByDefault) throw new InvalidOperationException("Reflection fallback enabled.");
        foreach (Type type in new[] { typeof(TideOffset), typeof(AboveDatum), typeof(BelowDatum) })
            if (OffsetJson.Default.GetTypeInfo(type) is null) throw new InvalidOperationException("Missing generated candidate.");
        Console.WriteLine("PASS: reference null and source-generated candidates without reflection fallback.");
    }
}
