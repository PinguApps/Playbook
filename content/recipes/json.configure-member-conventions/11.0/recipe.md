# Define JSON naming and omission rules at the type boundary

## Goal

Give ferry entries stable JSON names and deliberate null/default omission rules. Understand which annotation wins before changing a stored or public wire contract.

## Older approach

Target net10.0 / C# 14, import `System.Text.Json` and `System.Text.Json.Serialization`, and use namespace `Playbook.Samples` with standard implicit imports. Repeat explicit property names and ignore conditions where needed. This remains a precise choice when wire names must stay independent of CLR member renaming.

{{snippet:older}}

## Newer approach

Target net11.0 / C# 15 with the same imports. A type-level naming/default-ignore policy removes repetition while members retain deliberate overrides. The conflicting global options make precedence observable.

{{snippet:newer}}

`FerryJson.Write()` emits deck_code=A3, loadCount=0, sailing_name=Dawn and audit=null; Notes and Attempts are omitted. Explicit JsonPropertyName wins over the member naming attribute; member naming wins over type naming, which wins over options, then the original member name. Member JsonIgnore wins over type JsonIgnore, which wins over DefaultIgnoreCondition. A type-level WhenWritingNull leaves a non-nullable numeric zero present, even though the global WhenWritingDefault would omit it. The older example uses Never on that int: a member-level WhenWritingNull on a non-nullable value type is invalid.

## Migration

1. Inventory the actual emitted field names and required omissions before moving annotations.
2. Select a type policy only for members that genuinely share a convention; retain explicit names for versioned fields.
3. Use member overrides for exceptions and remove only redundant attributes.
4. Keep JsonIgnoreCondition.Always off types. Reflection rejects it; the generator reports SYSLIB1226 and ignores the type-level attribute.
5. Compare exact JSON before changing persisted/API contracts, including nulls, zeros and explicit member overrides.

## Constraints

Requires framework System.Text.Json on net11.0, SDK 11 / C# 15. No packages, workloads, providers, rendering mode or experimental flags. PascalCase is also available globally and through JsonKnownNamingPolicy. These checks demonstrate reflection serialization; generated invalid-attribute diagnostics are checked separately. No older-target route, every naming transformation/culture, trimming or Native AOT publication is claimed. Type-level naming affects wire shape, not CLR member names.

## Evidence

Actual .NET 10/11 builds/executions inspect field names and omission precedence, including the zero and forced-null cases. RC2 reflection rejects type-level Always, and the compiler probe requires SYSLIB1226. Saved exact input/toolchain records and complete installed offline list/get checks accompany the recipes. No mocks or targeted Human proof; RC2 is not GA verification.
