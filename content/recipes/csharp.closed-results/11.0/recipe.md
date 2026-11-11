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

### Custom storage and providers

Use a custom representation when adapting an existing type or when typed storage matters. Apply public `[Union]` (`System.Runtime.CompilerServices`) and expose public single-argument constructors plus a public object-valued `Value` getter. Implement `IUnion` for common runtime access; it does not replace the required shape. A creation parameter can be by value or `in`.

{{snippet:tagged}}

The tag distinguishes empty, count and flag. The nullable count case accepts null, but its `TryGetValue(out int)` returns false for null; an underlying nonnullable out parameter is supported. `HasValue` agrees with `Value is not null`. Reading `Value` boxes numeric/Boolean payloads. Typed matching uses `TryGetValue` and `HasValue`; the instrumented consumer checks zero Value getter calls, not a claim that every union operation allocates nothing.

Alternatively, implement a nested public `IUnionMembers` interface with static single-argument `Create` factories returning the union type. Provider lookup **replaces** lookup on the union, so public constructors on the enclosing type no longer establish cases. The enclosing type implements the provider; Value is found on that interface.

{{snippet:provider}}

A custom class is supported. Seal it so derived implementations cannot change the trusted access behavior; class allocation remains part of its cost. Generated unions use one object reference and box value payloads on entry. Tagged structs can avoid that boxing during matching but grow in size and require consistent tag/payload storage.

The compiler trusts four behavioral contracts rather than proving them: Value is always null or a declared case (soundness); creation preserves the chosen case or null, including null input (stability); creation through different applicable cases has equivalent observable behavior (creation equivalence); Value, HasValue and every TryGetValue agree (access consistency). Default storage must obey all four. Overlapping creation paths must not invent different meanings for the same input. The consumer checks default/null, every case, direct versus implicit creation and access agreement. Do not execute deliberately inconsistent implementations: even exhaustive matching can behave incorrectly when the trusted contracts are false.

A multi-field struct reassigned concurrently can expose a tag and payload from different writes. Readonly fields do not make replacement of the whole struct atomic. Synchronize shared reads and writes; no unsynchronized local-copy mitigation is claimed or verified here.

### Patterns and conversions

{{snippet:patterns}}

Type/declaration patterns and typed property patterns inspect contents. Untyped property, positional and list patterns inspect the wrapper's own members. The consumer supplies wrapper Length/indexer/Deconstruct members to verify those distinctions. `{}` always succeeds on a nonnullable struct wrapper, including empty storage, so it is not a has-value check. `is not null` checks contents; a nullable wrapper can also be absent. `is ShelfReading` attempts to match contents against ShelfReading and reports CS8121 here; it is not a wrapper-presence test. Preserve an explicit null arm even when annotations suggest assigned storage.

Switch on the union and handle each case plus null. Switching on object-valued Value loses the compiler's closed-case knowledge; a discard arm masks omissions. The added-case probe requires CS8509 when a string case is introduced without an arm.

Overlapping interface cases can make creation ambiguous: converting int to `OverlappingShelf(IComparable, IFormattable)` reports CS0457. An applicable user-defined conversion takes precedence over union creation; PriorityShelf verifies that its operator runs. A union conversion cannot chain through another union conversion: int cannot implicitly become ShelfEnvelope through ShelfReading (CS0029). Explicitly construct the intermediate union. A conversion to a nullable union wrapper is supported when the source converts to a case. This does not lift arbitrary nullable inputs: int? cannot convert to a union whose count case is int (CS0029); declare an int? case deliberately, as ShelfReading does.

Nested unions retain the inner wrapper as a case; do not assume recursive flattening. ShelfEnvelope's Value is a ShelfReading. Generic case sets can overlap after substitution: GenericShelf<int> accepts int, while GenericShelf<string>'s two string creation paths produce CS0457. Choose distinct payload types or explicit domain factories rather than promising a universal generic conversion. A self-referential union declaration compiles in the focused probe; that proves declaration acceptance only. No recursive-union serialization or automatic recursive matching route is established by these probes.

## Migration

1. Keep each payload's meaning and validation rules. Replace the handwritten wrapper with the union declaration; remove only that wrapper's now-redundant constructors and storage.
2. Switch on the union itself instead of its Value property. Handle every payload plus null; treat an unassigned result deliberately instead of inventing a successful payload.
3. Retain the JSON classifier and generated context when persisting these object cases. Test existing payloads before removing the old converter.
4. Compile consumers with warnings treated as errors. Adding a third case must force review of incomplete switch expressions; don't mask it with a discard arm.
5. Escape existing type identifiers named `union` or `closed` as `@union` or `@closed` at declaration and affected uses when migrating to C# 15.

## Constraints

This route needs .NET 11 runtime/reference assemblies and the .NET 11 SDK with C# 15. No preview language flag, extra package, workload, provider, UI framework or rendering mode is used. No supported down-level C# 15 route is claimed. The verification baseline is GA SDK 11.0.100/runtime 11.0.0.

A declared union stores object-backed state; value-type payloads box. It supplies neither record equality nor a domain-specific empty case. `default(Outcome)` has null Value; JSON null maps to that default when no case accepts null. Nullable wrapper absence and empty underlying storage can both match null. Do not rely on a nullability annotation to make default impossible. Union declarations cannot add instance fields, auto-properties, field-like events or public single-parameter constructors. Custom union implementations must obey the compiler's soundness and access rules; the custom route below takes responsibility for those contracts.

Exhaustiveness diagnoses omitted known cases; it does not validate payload properties or prove persisted data is trustworthy. Use a closed hierarchy when the alternatives need common inherited members (`csharp.closed-hierarchies`).

## Evidence

The original .NET 10/C# 14 and .NET 11 GA/C# 15 projects compile and execute the same case/payload, null/default and JSON ambiguity checks. The .NET 11 consumer probe omits a union case and requires CS8509, promoted to an error. Evidence records retain exact SDKs, dependencies, inputs and actual output. No mocks, Native AOT publishing or UI/client matrix is represented as proof. Portable execution needs no targeted Human proof.

GA refresh (PIN-1287): the declared .NET 11 sample was rebuilt and executed with SDK 11.0.100 and runtime 11.0.0. This refresh does not establish broader platform or API behavior.
