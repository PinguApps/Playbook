using System.Text.Json;
using System.Text.Json.Nodes;
#if NET11_0_OR_GREATER
using System.Numerics;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
#endif

namespace Playbook.Samples;

internal static class Program
{
    private static void Main()
    {
        JsonObject conventions = JsonNode.Parse(FerryJson.Write())!.AsObject();
        Check(conventions["deck_code"]!.GetValue<string>() == "A3", "explicit name wins");
        Check(conventions.ContainsKey("loadCount") && conventions["loadCount"]!.GetValue<int>() == 0, "member naming and type ignore override options");
        Check(conventions["sailing_name"]!.GetValue<string>() == "Dawn", "type naming wins");
        Check(!conventions.ContainsKey("Notes") && !conventions.ContainsKey("notes"), "null omitted");
        Check(conventions.ContainsKey("audit") && conventions["audit"] is null, "member Never wins");
        Check(!conventions.ContainsKey("attempts"), "member default override");
        JsonNode schema = ImageSchema.Export();
        foreach (string name in new[] { "Bytes", "Scratch", "Original" })
            Check(schema["properties"]![name]!["contentEncoding"]!.GetValue<string>() == "base64", "binary schema " + name);
        SealImage image = new([2, 4], new byte[] { 6, 8 }, new byte[] { 10, 12 });
        SealImage copy = JsonSerializer.Deserialize<SealImage>(JsonSerializer.Serialize(image))!;
        Check(copy.Bytes.SequenceEqual(image.Bytes) && copy.Scratch.Span.SequenceEqual(image.Scratch.Span) && copy.Original.Span.SequenceEqual(image.Original.Span), "binary buffers round trip");
#if NET11_0_OR_GREATER
        CargoMeasure measure = new((Decimal32)2, (Decimal64)3, (Decimal128)4, (BFloat16)5);
        CargoMeasure roundTrip = JsonSerializer.Deserialize<CargoMeasure>(JsonSerializer.Serialize(measure))!;
        Check(measure == roundTrip, "four numeric converters");
        Check(JsonMetadataServices.Decimal32Converter is not null && JsonMetadataServices.Decimal64Converter is not null && JsonMetadataServices.Decimal128Converter is not null && JsonMetadataServices.BFloat16Converter is not null, "net11 converter metadata");
        JsonSerializerOptions strings = new() { NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString };
        Check(JsonSerializer.Deserialize<Decimal64>(JsonSerializer.Serialize((Decimal64)3, strings), strings) == (Decimal64)3, "number handling");
        Dictionary<Decimal64, int> keys = new() { [(Decimal64)2] = 7 };
        Check(JsonSerializer.Deserialize<Dictionary<Decimal64, int>>(JsonSerializer.Serialize(keys))![(Decimal64)2] == 7, "numeric dictionary key");
        CargoManifest manifest = JsonSerializer.Deserialize<CargoManifest>("{\"Bays\":[3,3,5],\"revision\":2,\"added\":9}")!;
        Check(manifest.Bays is HashSet<int> && manifest.Bays.SetEquals([3, 5]), "read only set contract");
        Check(manifest.Extra["retained"].GetInt32() == 8 && manifest.Extra["revision"].GetInt32() == 2, "extension initial data copied with incoming precedence");
        CargoManifest generated = JsonSerializer.Deserialize("{\"Bays\":[4,4,6]}", CargoContext.Default.CargoManifest)!;
        Check(generated.Bays.SetEquals([4, 6]), "generated read only set contract");
        CargoNode node = new() { Id = 4, Extra = new JsonObject { ["quay"] = true } };
        Check(JsonNode.Parse(JsonSerializer.Serialize(node))!["quay"]!.GetValue<bool>(), "JsonObject overflow flattened");
        try { JsonSerializer.Serialize(new InvalidIgnore()); throw new Exception("invalid type ignore accepted"); }
        catch (InvalidOperationException) { }
#else
        CargoManifest manifest = JsonSerializer.Deserialize<CargoManifest>("{\"Bays\":[3,3,5],\"revision\":2}")!;
        Check(manifest.Bays.SetEquals([3, 5]) && manifest.Extra["revision"].GetInt32() == 2, "concrete collection contract");
        CargoMeasure measure = new(3m, 5f);
        Check(JsonSerializer.Deserialize<CargoMeasure>(JsonSerializer.Serialize(measure)) == measure, "previous numeric representations");
#endif
        Console.WriteLine($"PASS naming/ignore precedence, numeric/collection contracts, binary schema and round trips; runtime {Environment.Version}");
    }
    private static void Check(bool condition, string claim) { if (!condition) throw new Exception(claim); }
#if NET11_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
    private sealed class InvalidIgnore { public int Count { get; set; } }
#endif
}
