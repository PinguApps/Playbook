# Model distinct outcomes with a C# union

## Goal

Return either an archive receipt or a refusal without accepting arbitrary objects. Use a union when the alternatives already exist as unrelated types and callers should handle each case. The museum example keeps a shelf location separate from a refusal reason; both remain recoverable after persistence.

## Older approach

.NET 10 / C# 14 has no union declaration. A small wrapper with typed constructors can constrain assignment while retaining unrelated payload types. Matching its object-valued property needs a defensive fallback because the compiler cannot infer the complete set. This remains a valid choice for an older target or a deliberately custom representation.

In a console project targeting net10.0 with LangVersion 14.0, use `using System.Text.Json;`, `using System.Text.Json.Serialization;` and the file-scoped namespace `Playbook.Samples`. The following model, converter and context are complete together:

{{snippet:older}}
{{snippet:converter}}
{{snippet:older-context}}

## Newer approach

Target net11.0 with LangVersion 15.0. Keep the same imports and namespace. The compiler supplies a struct wrapper, constructors, implicit conversions and union-aware matching. The null arm is intentional: default storage has no active case. The JSON attribute selects between two object-shaped payloads by required root properties. The metadata context supplies serialization contracts without reflection-based fallback.

{{snippet:newer}}
{{snippet:newer-context}}

For example, `Outcome value = OutcomeWorkflow.Receipt("C-24");` followed by `OutcomeWorkflow.Describe(value)` returns `stored:C-24`. Serializing and deserializing through these helpers retains that result. Use `OutcomeWorkflow.Refusal("humidity")` to get the distinct `refused:humidity` result. See recipe `json.serialize-closed-results` for the classification and persistence tradeoffs.

## Migration

1. Keep each payload's meaning and validation rules. Replace the handwritten wrapper with the union declaration; remove only that wrapper's now-redundant constructors and storage.
2. Switch on the union itself instead of its Value property. Handle every payload plus null; treat an unassigned result deliberately instead of inventing a successful payload.
3. Retain the JSON classifier and generated context when persisting these object cases. Test existing payloads before removing the old converter.
4. Compile consumers with warnings treated as errors. Adding a third case must force review of incomplete switch expressions; don't mask it with a discard arm.
5. Escape existing type identifiers named `union` or `closed` as `@union` or `@closed` at declaration and affected uses when migrating to C# 15.

## Constraints

This route needs .NET 11 runtime/reference assemblies and the .NET 11 SDK with C# 15. No preview language flag, extra package, workload, provider, UI framework or rendering mode is used. No supported down-level C# 15 route is claimed. The verification baseline is RC2, not GA verification.

A declared union stores object-backed state; value-type payloads box. It supplies neither record equality nor a domain-specific empty case. `default(Outcome)` has null Value; JSON null maps to that default when no case accepts null. Nullable wrapper absence and empty underlying storage can both match null. Do not rely on a nullability annotation to make default impossible. Union declarations cannot add instance fields, auto-properties, field-like events or public single-parameter constructors. Custom union implementations must obey the compiler's soundness and access rules; this recipe uses the generated representation.

Exhaustiveness diagnoses omitted known cases; it does not validate payload properties or prove persisted data is trustworthy. Use a closed hierarchy when the alternatives need common inherited members (`csharp.closed-hierarchies`).

## Evidence

The original .NET 10/C# 14 and .NET 11 RC2/C# 15 projects compile and execute the same case/payload, null/default and JSON ambiguity checks. The .NET 11 consumer probe omits a union case and requires CS8509, promoted to an error. Evidence records retain exact SDKs, dependencies, inputs and actual output. No mocks, Native AOT publishing, GA execution or UI/client matrix is represented as proof. Portable execution needs no targeted Human proof.
