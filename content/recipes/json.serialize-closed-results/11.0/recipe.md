# Preserve a union's distinct cases through JSON

## Goal

Persist unrelated result types without losing which outcome occurred. Use this when JSON must retain both case identity and payload, and when its shape can select a case unambiguously. The archive receipt has a required ShelfCode; a refusal has a required ReasonCode. Neither is encoded as an arbitrary object with guessed runtime type.

## Older approach

In .NET 10, an explicit converter can read the object, choose exactly one case and delegate payload handling to source-generated contracts. This offers direct control of the wire contract and remains valid when selection depends on business rules. It also means the application owns that selection code.

For a net10.0 / C# 14 console project, add `using System.Text.Json;`, `using System.Text.Json.Serialization;` and namespace `Playbook.Samples`. These three canonical blocks form the complete model and serialization setup:

{{snippet:older}}
{{snippet:converter}}
{{snippet:older-context}}

## Newer approach

.NET 11's serializer writes the active union payload directly, without a case wrapper or discriminator. Token kinds distinguish primitive-shaped cases, but both of our cases are objects. JsonUnionTypeStructuralClassifier narrows candidates using root property names and required-property metadata. JsonRequired makes the distinction explicit. The generated metadata context registers the union and supplies its case contracts.

In net11.0 with LangVersion 15.0, use the same imports and namespace:

{{snippet:newer}}
{{snippet:newer-context}}

Serializing `OutcomeWorkflow.Receipt("C-24")` produces `{"ShelfCode":"C-24"}`; the refusal produces `{"ReasonCode":"humidity"}`. Deserialize through OutcomeWorkflow before matching. Both sample implementations preserve these two wire shapes. An empty object, an unknown-only object or an object carrying both required case properties is rejected as ambiguous or unmatched.

## Migration

1. Inventory persisted shapes and choose a stable way to select exactly one case. Do not assume two object cases are distinguished automatically by their CLR names.
2. Declare the union, annotate its structural classifier and mark the identifying properties required. Supply the metadata context and pass its JsonTypeInfo to both serialization calls.
3. Round-trip each case and check both its identity and payload. Test unmatched and ambiguous inputs, not only successful writes.
4. Decide how JSON null behaves. In this union no case accepts null, so it becomes default storage and serializes back to null; the matching helper returns `unassigned`.
5. Remove the old converter only after compatibility checks. If selection needs property values or nested data, retain a converter or implement a JsonTypeClassifierFactory; structural property-name matching cannot supply that policy.

## Constraints

The structural classifier does not examine property values or nested content. Zero or multiple candidates throw JsonException. It does not support nested unions, polymorphic cases, multiple non-object cases sharing one JSON token kind, or ReferenceHandler.Preserve. Don't treat required member presence as business validation or a guarantee that a string value is non-null.

Separate T and nullable T cases overlap for non-null JSON and require deliberate custom classification. When a case accepts null the null selection rules differ from this sample; default storage remains possible. Union default is not an application success or refusal. See `csharp.closed-results` for matching, boxing and declaration restrictions.

Use .NET 11 runtime/reference assemblies, SDK 11 and C# 15. System.Text.Json is supplied by the framework: no extra package assets, provider, platform-specific workload or rendering mode is needed. Metadata source generation is executed here; the reflection-only restriction of F# discriminated unions does not apply to this C# example. This does not establish a Native AOT publish or down-level package route. RC2 evidence is not GA verification; no experimental opt-in is used.

## Evidence

The declared .NET 10 and .NET 11 projects both compile and execute real serialization and deserialization. Shared checks compare distinct case/payload descriptions, null/default round trips and rejected unknown/ambiguous objects. They use generated JsonTypeInfo contracts, not mocked serializers. Exact SDKs, target assemblies, inputs and runtime output are saved in verification-evidence.json. No network resource, Native AOT artifact or targeted Human proof is needed for the bounded portable claims.
