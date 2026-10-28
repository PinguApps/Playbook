// snippet:complete
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace CedarPlanetarium;

[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Metadata)]
[JsonSerializable(typeof(ShowTicket))]
internal sealed partial class TicketJson : JsonSerializerContext;

// Diagnostics are bounded and identify options instances, never hold circuit payloads.
public sealed class MetadataTrace
{
    private readonly ConditionalWeakTable<JsonSerializerOptions, Identity> identities = new();
    private readonly ConcurrentQueue<MetadataEvent> events = new();
    private int nextIdentity;
    public void Record(JsonSerializerOptions options, string resolver, Type type)
    {
        var identity = identities.GetValue(options, _ => new Identity(Interlocked.Increment(ref nextIdentity)));
        events.Enqueue(new(identity.Value, resolver, type.Name));
        while (events.Count > 64) events.TryDequeue(out _);
    }
    public MetadataEvent[] Snapshot() => events.ToArray();
    private sealed record Identity(int Value);
}
public sealed record MetadataEvent(int Options, string Resolver, string Type);

public sealed class TicketResolver(string name, bool ownsTicket, MetadataTrace trace) : IJsonTypeInfoResolver
{
    public JsonTypeInfo? GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        trace.Record(options, name, type);
        if (type != typeof(ShowTicket)) return null; // Preserve reflection fallback and framework converters.
        if (!ownsTicket) throw new InvalidOperationException("Second resolver unexpectedly won.");
        // Use the resolver overload: the framework's options are already read-only.
        return ((IJsonTypeInfoResolver)TicketJson.Default).GetTypeInfo(type, options);
    }
}
// endsnippet:complete
