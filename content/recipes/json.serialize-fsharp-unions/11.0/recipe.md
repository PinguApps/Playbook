# Round-trip F# union values with built-in JSON support

## Goal

Use built-in reflection-based System.Text.Json serialization when F# union cases and payload values must round-trip in an untrimmed JIT application.

## Older approach

Add `open System.Text.Json` before the definitions in both versions.

The original wire record makes the case name and fields explicit, then maps them back to the union. This is a valid .NET 10 boundary and gives control over the wire contract. Its shape differs from the new built-in representation. Built-in .NET 10 union serialization throws NotSupportedException in the actual comparison; a custom converter is another possible route, not demonstrated here.

{{snippet:older}}

## Newer approach

System.Text.Json from the .NET 11 shared framework recognizes Shipment without a custom converter or JsonUnion attribute. Waiting serializes as a JSON string; Loaded writes a $type discriminator and its named fields. Deserialization restores the case, code and decimal mass. Use the union type explicitly at the boundary.

{{snippet:newer}}

## Migration

1. Target net11.0 with the .NET 11 SDK, LangVersion 11.0 and FSharp.Core 11. The sample pins RC2 for reproducibility. Use framework System.Text.Json 11; no extra JSON package is needed.
2. Call Serialize<Shipment> and Deserialize<Shipment>, or the typed wrappers shown here, in an untrimmed application with reflection enabled.
3. Treat the wire format as a contract migration. Preserve the old DTO mapping for stored data or clients until compatibility is deliberately handled. Validate malformed input and business rules at your application boundary.

## Constraints

- Built-in F# union support is reflection-only: no source generation, trimming or Native AOT support.
- Do not add JsonUnionAttribute to this F# model or infer F# support from C# union generator results.
- Case and field names form part of the default wire contract. Renaming them can break persisted messages. These checks cover public class union cases, not every supported union shape.

No provider, workload or rendering mode is required. The declared samples run on Windows with .NET 10 and .NET 11 RC2; other .NET-supported platforms require their normal runtime and globalization support. No older-target SDK/package route is advertised.

## Evidence

The exact SDK pins, resolved FSharp.Core dependencies, input hashes, build commands and actual execution output accompany this recipe. The saved records cover the .NET 10 and .NET 11 samples. The separate compiler probe confirms stable spreads, preview constructor rejection/acceptance and ordinary culture formatting. Reflection-free diagnostics use their own --reflectionfree executable; ordinary formatting is not substituted for that evidence. No trimmed or Native AOT publish/run, cross-compilation or mocks are claimed. RC2 evidence is not GA verification.

Official sources: [Official F# union support and restrictions](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/supported-types#f-discriminated-unions); [Built-in F# union converter](https://source.dot.net/System.Text.Json/System/Text/Json/Serialization/Converters/FSharp/FSharpUnionConverter.cs.html).
