# Export JSON schema for binary buffer values

## Goal

Describe a seal image's binary buffers as base64-encoded JSON strings without maintaining redundant schema transformations. Keep the actual JSON representation separate from downstream schema-validator behavior.

## Older approach

In a net10.0 / C# 14 console project import `System.Text.Json`, `System.Text.Json.Nodes` and `System.Text.Json.Schema`, use namespace `Playbook.Samples` and standard implicit imports. The explicit schema transform supplies contentEncoding for the three binary types. It remains a valid customization when a consumer needs additional schema annotations.

{{snippet:older}}

## Newer approach

In net11.0 / C# 15 with those imports the built-in exporter adds contentEncoding: base64 for byte[], Memory<byte> and ReadOnlyMemory<byte>. Remove the now-redundant transform, retaining other intentional transformations.

{{snippet:newer}}

`ImageSchema.Export()` returns a schema whose properties Bytes, Scratch and Original each declare `"contentEncoding":"base64"`. Serialize/deserialize SealImage through JsonSerializer normally; the values round-trip as binary buffers. The targeted analyzer suppression exists only because this sample deliberately compares an exposed byte[] contract with both memory forms.

## Migration

1. Export the current schema and identify transformations that only add base64 metadata to built-in binary contracts.
2. On net11.0, remove that specific redundant transform and call GetJsonSchemaAsNode for the actual model.
3. Compare each buffer property's annotation and round-trip representative bytes through the serializer.
4. Retain custom transforms for independently required schema rules; automatic annotations do not replace application validation.
5. Recheck the receiving schema tool's dialect and treatment of contentEncoding before relying on enforcement.

## Constraints

Uses framework System.Text.Json on net11.0 with SDK 11 / C# 15. No additional package asset, workload, provider, renderer or experimental opt-in. No older-target package route, custom converter schema, browser/tool validator execution, trimming or Native AOT publication is claimed. contentEncoding describes representation and does not itself prove a validator decodes/rejects data. The older transform does not imply .NET 10 lacks binary serialization; the change removes schema scaffolding.

## Evidence

Real .NET 10/11 builds and executions inspect all three exact schema properties and round-trip distinct byte sequences through each representation. Previous schema uses the explicit transform; RC2 uses built-in output. Saved input/artifact records and the installed list/get harness check canonical code, complete offline guidance and target safety. No mocks, cross-compilation, GA or required Human proof.
