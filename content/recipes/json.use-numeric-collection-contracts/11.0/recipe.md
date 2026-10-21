# Serialize numeric values and read-only collection contracts

## Goal

Exchange cargo measurements and unique bay identifiers without hiding unsupported numeric formats or dropping overflow fields. Use the .NET 11 built-in numeric and collection contracts only on assets that actually contain them.

## Older approach

In net10.0 / C# 14 import `System.Text.Json` and `System.Text.Json.Serialization`, use namespace `Playbook.Samples` and standard implicit collection imports. Use concrete HashSet/Dictionary contracts and ordinary decimal/float where those meet the application's representation needs. BFloat16 and the IEEE Decimal32/64/128 types have no .NET 10 framework equivalent: this sample does not emulate their precision or pretend the old values are wire-format interchangeable.

{{snippet:older}}

## Newer approach

In net11.0 / C# 15 also import `System.Numerics` and `System.Text.Json.Nodes`. The framework supplies the four numeric converters. IReadOnlySet<T> deserialization creates a HashSet<T>; read-only dictionary extension data receives a mutable Dictionary implementation.

{{snippet:newer}}

Call `JsonSerializer.Serialize(new CargoMeasure((Decimal32)2, (Decimal64)3, (Decimal128)4, (BFloat16)5))` and deserialize as CargoMeasure. This checks the supported types, not a business choice of precision. `JsonSerializer.Deserialize<CargoManifest>("{\"Bays\":[3,3,5],\"revision\":2}")` yields unique bays 3/5, retains the preinitialized retained=8 entry and replaces revision=1 with 2. Source-generated calls use CargoContext.Default.CargoManifest. JsonObject extension data writes its children directly into the containing object; it does not add an Extra wrapper.

## Migration

1. Choose numeric range/precision and collection semantics explicitly; do not silently replace decimal/float contracts.
2. Target net11.0 for the new framework numeric types/converters. The audited RC2 System.Text.Json package's net10.0 asset lacks the four numeric metadata converters; no blanket older-target route is claimed.
3. Replace concrete-set DTO scaffolding with IReadOnlySet only where deserializing to HashSet is appropriate.
4. Use string-keyed object/JsonElement read-only dictionary extension data deliberately; review the copy-and-overwrite behavior for initialized entries.
5. Round-trip numeric values and inspect set/overflow output. Check number handling, dictionary keys and generated contracts for the paths the application actually uses.

## Constraints

Uses framework System.Text.Json and System.Numerics on net11.0, SDK 11 / C# 15; no additional packages, workloads, providers, rendering or experimental flags. JsonMetadataServices exposes BFloat16Converter/Decimal32Converter/Decimal64Converter/Decimal128Converter and CreateIReadOnlySetInfo for metadata tooling; the sample uses ordinary serializer/generated-context APIs rather than constructing metadata by hand. Exact numeric range/rounding, nonfinite encodings and all key formats are outside the demonstrated small integral-value checks. See `json.describe-binary-schema` for buffer schema metadata. No trimmed/AOT or GA publication proof.

## Evidence

Actual .NET 10/11 builds and executions verify the declared numeric representations, all four new converters, Decimal64 number-handling and dictionary-key round trips, duplicate set removal, generated read-only-set contracts, initialized extension-data merge/overwrite and JsonObject flattening. The pinned serialization asset audit establishes the net10/net11 converter difference. Installed offline MCP tests retrieve this complete body, requirements and edition. No mock execution, cross-compilation or required Human proof; .NET 11 is measured RC2 evidence.
