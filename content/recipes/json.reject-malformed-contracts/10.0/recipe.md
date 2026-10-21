# Reject malformed JSON at an import boundary

## Goal

Reject missing, duplicate or unexpected fields before berth imports reach business rules. Keep structural validation separate from rules such as a nonblank vessel name or a nonnegative crate count. This is one of the eight curated .NET 10 foundations, with an actual .NET 9 comparison.

## Older approach

In net9.0 / C# 13 import `System.Text.Json` and `System.Text.Json.Serialization`, use namespace `Playbook.Samples` with standard implicit collection imports. Combine the existing required-constructor, unmapped-member and nullable checks with explicit duplicate detection. This small flat contract uses a DOM pass; enforce an application input-size limit before parsing. The approach remains valid when custom field diagnostics or framing are needed.

{{snippet:older}}

## Newer approach

In net10.0 / C# 14 import `System.Text.Json` with the same namespace/implicit imports. JsonSerializerOptions.Strict supplies the structural policy without the duplicate-detection DOM pass. Both versions use the same case-sensitive Vessel/Crates shape.

{{snippet:newer}}

`BerthReader.Read("{\"Vessel\":\"Wren\",\"Crates\":6}")` succeeds. Omitting Crates, repeating Crates, adding Colour, supplying null Vessel, quoting the integer or passing a null root fails with JsonException. By contrast, `{"Vessel":"","Crates":-1}` is structurally valid here and still needs business validation. No business-rule acceptance is implied.

## Migration

1. Decide the exact field names, case sensitivity, required inputs and null policy for the imported contract.
2. On .NET 10 use JsonSerializerOptions.Strict, or copy it with `new JsonSerializerOptions(JsonSerializerOptions.Strict)` before making deliberate policy changes.
3. Remove only structural scaffolding now enforced by that policy. Keep size limits and domain checks.
4. Test missing, duplicate, additional, null and wrong-number-representation inputs against the actual model.
5. Run business validation after structural deserialization and before persisting or acting on the import.

## Constraints

The primary route uses framework System.Text.Json on .NET 10, SDK 10 / C# 14. The same edition also has separately executed .NET 11 / SDK 11 / C# 15 applicability. No packages, workloads, providers, rendering modes or experimental flags. .NET 9 is a comparison, not a supported catalogue target. Strict does not make every property mandatory: required constructor parameters and declared required members determine presence; it does not prove collection elements or every generic nullability annotation are validated. The older duplicate check is intentionally scoped to this flat root; it is not a recursive general JSON validator. Preserve separate validation for nested contracts where needed. No older-target package, trimming or Native AOT proof.

## Evidence

Actual .NET 9.0.21, .NET 10.0.13 and .NET 11 RC2 samples compile and execute. All run valid/missing/duplicate/additional/null/quoted-number inputs and demonstrate that a negative crate count still needs business rules. The .NET 11 sample verifies retaining edition 10.0, not inventing a new foundation or an implicit compatibility route. Installed stdio list/get checks the full original body and selected applicability for both supported targets offline. Exact SDK/source-input records are saved; no mocks or required Human proof, and RC2 is not GA evidence.
