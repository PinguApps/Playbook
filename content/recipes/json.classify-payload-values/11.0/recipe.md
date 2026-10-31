# Choose JSON variants from payload values

## Goal

A fictional Mallow Quay tide instrument sends Station and Offset in both record
shapes. Positive offsets mean AboveDatum; negative offsets mean BelowDatum.
Select by the numeric value, then deserialize the entire record. Field-name
matching alone cannot distinguish these messages.

## Older approach

.NET10 has no JsonTypeClassifierFactory. A custom import-only converter remains
a valid solution. It inspects a copy of the reader and deserializes the selected
concrete type with generated metadata, advancing the original reader exactly once.
The converter is attached only to the base type; concrete deserialization does
not recurse into it. Its Write method deliberately rejects serialization.

{{snippet:older}}

Both editions use this complete strict value rule in a separate source file.
Zero, missing/duplicate Offset, nonnumeric values, fractional values, out-of-range
integers and nonobject roots do not match. Extra properties retain normal
serializer handling; this is a value discriminator, not a complete schema validator.

{{snippet:rule}}

## Newer approach

.NET11 supplies a factory for ordinary polymorphic hierarchies as well as unions.
Use the typed base to match TideOffset. Register both derived types and list them
in the source-generation context. No C# union syntax is involved here.

{{snippet:newer}}

The context is an immutable metadata snapshot: declaring type, classifier kind,
candidate lists and discriminator property name. A polymorphic context populates
DerivedTypes; a union context populates UnionCases. Configuration creates the
delegate once for a configured contract; the delegate runs for each non-null
payload. Capture only registered candidates, rather than inventing types from
untrusted text. The sample resolves the two expected registered types at factory
configuration, so a missing registration fails before payload classification.

The delegate receives a defensive reader copy positioned at the start of the
JSON value. Lookahead may consume that copy; the serializer retains its original
position for complete concrete deserialization. This helper makes another copy
explicitly and disposes its temporary JsonDocument. It buffers a finite value;
bound input size separately. No streaming/allocation improvement is claimed.

## Migration

1. Identify the existing wire contract and value rule. Preserve Station and Offset
   names, casing and numeric semantics; choose an explicit policy for zero and
   missing/malformed values. Here both editions reject them with JsonException.
2. Target net11.0. Use the complete newer source and shared OffsetRule in their own
   files. Both console projects enable Nullable and ImplicitUsings. Framework
   System.Text.Json supplies the API; no additional NuGet package is required.
3. Register AboveDatum and BelowDatum with JsonDerivedType and set the base's
   JsonPolymorphic.TypeClassifier to the factory type. Per-type registration wins
   over an applicable conflicting global JsonSerializerOptions.TypeClassifiers
   factory. Global registration is useful when a type has no per-type factory;
   add it before the options become read-only.
4. Generate metadata for the base and every candidate. Both actual consumers set
   JsonSerializerIsReflectionEnabledByDefault=false and execute successful imports.
   Missing metadata is not repaired by returning a Type from the classifier.
   JsonCollectionInfoValues/JsonObjectInfoValues can support hand-built metadata;
   they are not a replacement source generator and are unnecessary for this route.
5. Return only a registered candidate. Returning null means no match and raises
   JsonException. An unregistered return such as string raises NotSupportedException
   in the measured RC2 polymorphic route. Treat both as failed imports; never use
   classification failure as permission to guess a fallback record.
6. Test both property orders and complete selected payloads. For these ordinary
   reference types, JSON null becomes a null reference without invoking the
   classifier; the older converter's default null handling agrees. This does not
   settle custom union-factory null semantics: the pinned factory remarks and
   cumulative union article conflict, and that extension remains unresolved for
   GA reconciliation. Existing measured union examples are not disproved here.

## Constraints

Primary route: .NET11 SDK/runtime, net11.0/C#15. Exact reproduction uses
SDK11.0.100-rc.2.26504.105 with reference/runtime11.0.0-rc.2.26504.105. The older
sample uses SDK10.0.402, net10.0/C#14 and reference/runtime10.0.13. These exact
patches identify proof, separately from release-family prerequisites. Both use
framework assets and SDK/reference-pack source generation, without extra packages.

There is no experimental diagnostic opt-in, native/provider/workload or rendering
prerequisite. Execution is measured on Windows x64. Newer-package-on-net10,
unions, arbitrary schemas, trimming/Native AOT, other-platform and final-GA behavior
are not verified. The structural classifier in json.serialize-closed-results addresses
a different property-name goal; it is not proof of this value-based factory.

## Evidence

eng/Test-ClassifierAuthentication.ps1 -Record -VerifySources builds and executes
both actual dependency sets and retains input hashes, exact commands, compiler,
runtime implementation, source generator and audit-matching framework references.
Positive and negative records retain every Station/Offset value in either order.
Ten malformed/no-match cases reject; reference null succeeds. Current checks prove
attribute precedence over a conflicting global factory, actual global dispatch,
null-return/unregistered exceptions, one configuration versus two delegate calls,
and complete deserialization after the delegate consumes its defensive copy.

Ordinary build/pack and isolated local-feed installation inspect that exact tool
through real stdio list/get from an unrelated directory: discovery ID/title/goal,
full canonical Markdown, explicit target11/edition11, applicability, fresh evidence
and matching structured/text JSON. Target10 has no_applicable_edition. The eight
foundations and existing identities remain unchanged; PIN-959/PIN-962 stay open.
