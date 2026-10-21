using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Playbook.Samples;

// snippet:generic
[JsonPolymorphic]
[JsonDerivedType(typeof(Dispatch<int>), "dispatch")]
public abstract class Notice<T> { public T? Value { get; set; } }
public sealed class Dispatch<T> : Notice<T> { public string Dock { get; set; } = ""; }

[JsonConverter(typeof(BadgeConverterFactory))]
public sealed record Badge<T>(T Value);

public sealed class BadgeConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert) => typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(Badge<>);
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) =>
        (JsonConverter)Activator.CreateInstance(typeof(BadgeConverter<>).MakeGenericType(typeToConvert.GetGenericArguments()))!;
}

public sealed class BadgeConverter<T> : JsonConverter<Badge<T>>
{
    public override Badge<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => new(JsonSerializer.Deserialize<T>(ref reader, options)!);
    public override void Write(Utf8JsonWriter writer, Badge<T> value, JsonSerializerOptions options) => JsonSerializer.Serialize(writer, value.Value, options);
}
[JsonSerializable(typeof(Notice<int>))]
public partial class NoticeContext : JsonSerializerContext;
// endsnippet:generic

// snippet:metadata
public static class ManifestMetadata
{
    public static JsonTypeInfo<Manifest> Required(JsonSerializerOptions options) =>
        (JsonTypeInfo<Manifest>)options.GetTypeInfo(typeof(Manifest));

    public static bool TryResolve<T>(JsonSerializerOptions options, out JsonTypeInfo<T>? info)
    {
        // Resolver misses are null; other resolver exceptions deliberately propagate.
        info = options.TypeInfoResolver!.GetTypeInfo(typeof(T), options) as JsonTypeInfo<T>;
        return info is not null;
    }
}
public sealed record Manifest(int Crates);
[JsonSerializable(typeof(Manifest))]
public partial class ManifestContext : JsonSerializerContext;
// endsnippet:metadata
