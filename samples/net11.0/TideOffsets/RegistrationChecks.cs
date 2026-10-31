using System.Text.Json;
using System.Text.Json.Serialization;

namespace TideOffsets;

public static class RegistrationChecks
{
    public static void Run()
    {
        var conflict = new ForcedClassifier(typeof(BelowDatum));
        var options = new JsonSerializerOptions { TypeInfoResolver = OffsetJson.Default };
        options.TypeClassifiers.Add(conflict);
        if (JsonSerializer.Deserialize<TideOffset>("{\"Station\":\"Mallow Quay\",\"Offset\":17}", options) is not AboveDatum || conflict.Configurations != 0)
            throw new InvalidOperationException("Attribute did not win over global factory.");
        JsonSerializer.Deserialize<TideOffset>("{\"Offset\":-9,\"Station\":\"Mallow Quay\"}", options);

        // A separate hierarchy without a per-type factory exercises global registration.
        foreach (Type? forced in new Type?[] { null, typeof(string), typeof(GlobalBelow) })
        {
            var factory = new ForcedClassifier(forced);
            var global = new JsonSerializerOptions { TypeInfoResolver = RegistrationJson.Default };
            global.TypeClassifiers.Add(factory);
            try
            {
                var result = JsonSerializer.Deserialize<GlobalOffset>("{\"Station\":\"Mallow Quay\",\"Offset\":17}", global);
                if (forced != typeof(GlobalBelow) || result is not GlobalBelow { Station: "Mallow Quay", Offset: 17 }) throw new InvalidOperationException("Expected classifier rejection.");
                JsonSerializer.Deserialize<GlobalOffset>("{\"Offset\":-9,\"Station\":\"Mallow Quay\"}", global);
                if (JsonSerializer.Deserialize<GlobalOffset>("null", global) is not null) throw new InvalidOperationException("Expected null.");
                if (factory.Configurations != 1 || factory.Invocations != 2) throw new InvalidOperationException("Factory/delegate lifetime changed.");
            }
            catch (JsonException) when (forced is null) { }
            catch (NotSupportedException) when (forced == typeof(string)) { }
            Console.WriteLine($"GLOBAL forced={forced?.Name ?? "no-match"} configurations={factory.Configurations} invocations={factory.Invocations}");
        }
        Console.WriteLine("PASS: attribute precedence, global factory, no-match/unregistered failures and configuration/delegate lifetime.");
    }

    private sealed class ForcedClassifier(Type? result) : JsonTypeClassifierFactory
    {
        public override bool CanClassify(JsonTypeClassifierContext context) => context.DeclaringType == typeof(TideOffset) || context.DeclaringType == typeof(GlobalOffset);
        public int Configurations { get; private set; }
        public int Invocations { get; private set; }
        public override JsonTypeClassifier CreateJsonClassifier(JsonTypeClassifierContext context, JsonSerializerOptions options)
        {
            Configurations++;
            return (ref Utf8JsonReader reader) =>
            {
                Invocations++;
                while (reader.Read()) { } // Runtime's defensive copy may be consumed entirely.
                return result;
            };
        }
    }
}

[JsonPolymorphic]
[JsonDerivedType(typeof(GlobalBelow))]
public abstract record GlobalOffset(string Station, int Offset);
public sealed record GlobalBelow(string Station, int Offset) : GlobalOffset(Station, Offset);
[JsonSerializable(typeof(GlobalOffset))]
[JsonSerializable(typeof(GlobalBelow))]
public partial class RegistrationJson : JsonSerializerContext;
