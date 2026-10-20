# Match every permitted state in an inheritance family

## Goal

Keep an intake workflow's states in one owned inheritance family and make callers account for each direct case. Choose this when states share a base contract, rather than merely wrapping unrelated types. A queued museum intake carries a queue number; a catalogued intake carries its shelf location.

## Older approach

.NET 10 supports abstract bases and sealed leaves. A private-protected constructor restricts ordinary subclass construction to derived types in the owning assembly. The assembly can still add cases, and the compiler does not treat the base as a closed set: matching retains a defensive fallback. Explicit JsonDerivedType entries define its persisted contracts. Those registrations remain useful when stable external discriminator names matter.

Create a net10.0 console project with LangVersion 14.0, `using System.Text.Json;`, `using System.Text.Json.Serialization;` and namespace `Playbook.Samples`:

{{snippet:older}}
{{snippet:older-context}}

## Newer approach

In net11.0 / C# 15, mark the root closed and keep the leaves sealed. A closed root is implicitly abstract. Consumers can match the known descendants without a catch-all. Null remains a separate input to handle. The source-generated metadata context opts into inference at build time as well as the per-type setting.

Use the same imports and namespace:

{{snippet:newer}}
{{snippet:newer-context}}

`StateWorkflow.Describe(new Waiting(17))` returns `queued:17`; a `new Catalogued("C-24")` returns `catalogued:C-24`. Both round-trip through Serialize/Deserialize. See `json.infer-closed-polymorphism` before changing an existing persisted contract.

## Migration

1. Identify the assembly that owns every direct descendant. Move a directly derived external type into that assembly or keep an intentionally open hierarchy.
2. Replace the abstract root modifier with closed. Do not combine closed with an explicit abstract, sealed or static modifier. Keep leaf types sealed if the whole family should remain bounded.
3. List each case in consumer switch expressions and include null when the input may be absent. Remove a catch-all only after a warning-as-error build proves all cases are covered.
4. Review serialization separately. Keep explicit registrations for established wire names, or adopt the generated context and inference with an intentional data migration.
5. Rebuild downstream consumers when changing the set. Escape existing contextual type names as `@closed` and `@union` where C# 15 requires it.

## Constraints

Closed limits direct inheritance to the declaring assembly, not to one source file or enclosing type. It is not transitive: an unsealed, non-closed child can allow further inheritance from other assemblies. Use closed intermediate nodes and sealed terminal nodes when that boundary is required. Adding a case is still an evolution event for separately compiled consumers and persisted data; compiler exhaustiveness does not upgrade old binaries.

Null is not an instance of a leaf. This sample returns `absent` for it and persists it as JSON null. Payload validation is a separate responsibility. Inference uses type-derived discriminators; the .NET 10 example uses explicit lowercase names, so these sample wire formats differ. Do not silently replace registrations in a stored or public contract.

The route requires .NET 11 / SDK 11 / C# 15 with no experimental opt-in, workloads, packages, providers or rendering context. Only RC2 artifacts were executed; this is not GA verification or a supported older-target route. Source generation here uses metadata mode; no Native AOT publish was performed.

## Evidence

Both real target projects compile and execute queued/catalogued and null JSON round trips. An external consumer omitting Catalogued fails with CS8509. A direct external descendant fails with CS9382. The escaped identifiers compile. The saved sample evidence identifies exact artifacts and inputs; the compiler checks are reproduced by eng/Test-ClosedOutcomes.ps1 after building the .NET 11 sample. No mock or cross-compilation substitutes for the recorded portable executions; no targeted Human proof is required.
