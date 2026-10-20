using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Playbook.Samples;

// snippet:generic
[JsonPolymorphic]
[JsonDerivedType(typeof(Dispatch<>), "dispatch")]
public abstract class Notice<T> { public T? Value { get; set; } }
public sealed class Dispatch<T> : Notice<T> { public string Dock { get; set; } = ""; }

[JsonConverter(typeof(BadgeConverter<>))]
public sealed record Badge<T>(T Value);

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
    public static JsonTypeInfo<Manifest> Required(JsonSerializerOptions options) => options.GetTypeInfo<Manifest>();
    public static bool TryResolve<T>(JsonSerializerOptions options, out JsonTypeInfo<T>? info) => options.TryGetTypeInfo<T>(out info);
}
public sealed record Manifest(int Crates);
[JsonSerializable(typeof(Manifest))]
public partial class ManifestContext : JsonSerializerContext;
// endsnippet:metadata

// This registration cannot satisfy the closed root's int argument.
[JsonDerivedType(typeof(TextNotice<>), "text")]
public class InvalidNotice<T>;
public sealed class TextNotice<T> : InvalidNotice<string>;
