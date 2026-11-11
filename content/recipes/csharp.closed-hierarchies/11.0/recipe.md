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

### Accessibility and generic consumers

Closure applies to direct subclasses in the same assembly **and module**, not all descendants. An open child permits external grandchildren; a closed intermediate node and sealed leaves bound that route. Adding `closed` to a public base can reject existing external subclasses. Changing its public direct-case set can break downstream source exhaustiveness; rebuild consumers and review compatibility before release.

{{snippet:hidden-hierarchy}}

Compile these declarations into the owning library. A separately compiled consumer cannot name `DeferredIntake` (CS0122). Matching only `AcceptedIntake` and `OpenIntake` produces CS8509. Either expose the hidden case deliberately, or retain `IntakeDecision => ...` (plus `null => ...` for nullable input). A fallback acknowledges states the consumer cannot name. Do not make a private implementation type public merely to silence a warning.

Once all direct cases are handled, a following base arm is subsumed (CS8510). A base arm placed first likewise subsumes later descendants. Use a base fallback only while some cases remain uncovered. Switch **expressions** report missing cases; switch **statements** need not cover every state and may simply execute no arm. Nullable input adds a separate null case: leaf patterns do not cover it (CS8655).

{{snippet:generic-hierarchy}}

Every type parameter of a generic direct descendant must occur in its base specification: `Invalid<T> : ShelfValue<int>` is rejected. Occurrence inside `T[]` counts. For `ShelfValue<string>`, no construction of `ShelfArray<T>` fits, so `SingleShelf<string>` exhausts the non-null input. For `ShelfValue<string[]>`, both `SingleShelf<string[]>` and `ShelfArray<string>` fit. In `Unknown<T>`, the compiler cannot spell the array element type for an arbitrary `T`; the base arm covers that possible case. Constraints do not refine this analysis: a `StructShelf<T> : ShelfValue<T> where T : struct` still requires a base fallback when matching `ShelfValue<T>` under `where T : class`, even though that leaf cannot be named with this `T`.

`eng/Test-LanguageBinding.ps1` compiles a separate owner DLL and downstream consumers, checking accepted indirect inheritance, rejected direct inheritance, inaccessible cases, fallbacks, null, subsumption and bounded generic cases with SDK 11.0.100/C# 15/ref 11.0.0. Module restrictions are official-source guidance, not a separate multi-module execution test.

Closed limits direct inheritance to the declaring assembly, not to one source file or enclosing type. It is not transitive: an unsealed, non-closed child can allow further inheritance from other assemblies. Use closed intermediate nodes and sealed terminal nodes when that boundary is required. Adding a case is still an evolution event for separately compiled consumers and persisted data; compiler exhaustiveness does not upgrade old binaries.

Null is not an instance of a leaf. This sample returns `absent` for it and persists it as JSON null. Payload validation is a separate responsibility. Inference uses type-derived discriminators; the .NET 10 example uses explicit lowercase names, so these sample wire formats differ. Do not silently replace registrations in a stored or public contract.

The route requires .NET 11 / SDK 11 / C# 15 with no experimental opt-in, workloads, packages, providers or rendering context. The declared sample was executed with GA SDK 11.0.100/runtime 11.0.0; no supported older-target route is claimed. Source generation here uses metadata mode; no Native AOT publish was performed.

## Evidence

Both real target projects compile and execute queued/catalogued and null JSON round trips. An external consumer omitting Catalogued fails with CS8509. A direct external descendant fails with CS9382. The escaped identifiers compile. The saved sample evidence identifies exact artifacts and inputs; the compiler checks are reproduced by eng/Test-ClosedOutcomes.ps1 after building the .NET 11 sample. No mock or cross-compilation substitutes for the recorded portable executions; no targeted Human proof is required.

GA refresh (PIN-1287): the declared .NET 11 sample was rebuilt and executed with SDK 11.0.100 and runtime 11.0.0. This refresh does not establish broader platform or API behavior.
