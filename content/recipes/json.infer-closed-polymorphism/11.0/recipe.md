# Infer JSON contracts for a closed state family

## Goal

Round-trip every owned state subtype without maintaining a second derived-type list. Use inference for a closed family whose wire discriminators may follow its type names. Use explicit registrations when persisted names must remain independent of code changes. The museum's Waiting and Catalogued states preserve different payloads across storage.

## Older approach

.NET 10 can round-trip an abstract base through explicit JsonDerivedType registrations. The chosen strings are durable application contract names. A generated context supplies the known metadata; the compiler still requires a fallback when matching the open base. This is a valid long-term design, especially for versioned storage or public JSON.

In a net10.0 / C# 14 console project use `using System.Text.Json;`, `using System.Text.Json.Serialization;` and namespace `Playbook.Samples`:

{{snippet:older}}
{{snippet:older-context}}

## Newer approach

.NET 11 can infer descendants of a C# closed root. Opt in on the root and enable the generated context to create inference metadata at compile time. Pass that context's JsonTypeInfo to serialization and deserialization; avoid an unnoticed reflection fallback.

Target net11.0 / C# 15 and retain the same imports and namespace:

{{snippet:newer}}
{{snippet:newer-context}}

`StateWorkflow.Serialize(new Waiting(17))` writes a `$type` value of `Waiting` on the verified baseline; Catalogued uses `Catalogued`. Deserialize returns the original leaf and payload. Null remains null and Describe returns `absent`. The older sample uses `waiting` and `catalogued`, so switching the setting is an intentional wire-format migration, not a compatibility-preserving cleanup.

## Migration

1. Decide who owns the wire contract. Keep explicit derived registrations if names must survive type renames or old stored data must remain readable unchanged.
2. If inference fits, make the root closed, keep terminal descendants sealed and apply JsonPolymorphic with InferClosedTypePolymorphism enabled.
3. Enable InferClosedTypePolymorphism in JsonSourceGenerationOptions before compiling the context. For reflection-based serialization the corresponding JsonSerializerOptions switch enables global inference; a runtime-only switch cannot add contracts missing from generated metadata.
4. Round-trip every leaf through the base type. Review the emitted discriminator names and plan a data migration before changing an established contract.
5. Recheck serializer registration and consumer switch expressions whenever the family changes; inference does not eliminate versioning decisions.

## Constraints

Inference can be configured globally, on a generated context, or per type. A per-type false setting opts that hierarchy out of a global setting. Explicit JsonDerivedType registrations replace the inferred list rather than adding to it. Combining explicit registrations with a per-type opt-in produces a source-generation warning; don't suppress it and assume both lists are included.

An opt-in on a non-closed type is invalid: reflection throws InvalidOperationException and source generation reports an error. Enabling only the runtime option fails when generated metadata lacks the required contracts. Use metadata generation for polymorphism; serialization-only fast-path generation does not provide the required polymorphic contracts. See `csharp.closed-hierarchies` for assembly boundaries, non-transitive closure and null matching. Closed intermediate nodes can contribute their descendants, but this sample deliberately tests two sealed leaves only.

The route uses the .NET 11 framework System.Text.Json with SDK 11 / C# 15, no extra package assets, workloads, providers, rendering mode or experimental opt-in. It claims no older-target route and no Native AOT publish result. RC2 execution is not GA verification. Type-driven discriminators make type renames a wire-contract concern.

## Evidence

Actual .NET 10 and .NET 11 sample builds and executions check both leaf payloads and null. The RC2 runtime also checks that explicit registrations replace inference and that runtime-only inference cannot repair an incomplete generated context. The consumer compiler probe requires SYSLIB1243 for an opt-in on an open type. Exact sample inputs, compiler/runtime baselines and output are retained separately from source-reviewed general restrictions. No mocks, UI matrix or targeted Human proof are used.
