# Preserve initialized values when generated JSON omits a member

## Goal

Restore a storage plan while keeping its default zone when the JSON omits Zone. Distinguish omission from an explicit replacement or explicit null.

## Older approach

In a net10.0 / C# 14 console project import `System.Text.Json` and `System.Text.Json.Serialization`, use namespace `Playbook.Samples` and standard implicit imports. Put the default in a constructor parameter for this generated path. This explicit construction design is valid and can remain preferable for model invariants.

{{snippet:older}}

## Newer approach

In net11.0 / C# 15 with the same imports, retain the init-property initializer directly. Generated deserialization assigns it after construction only if the payload contains the property.

{{snippet:newer}}

`JsonSerializer.Deserialize("{}", PlanContext.Default.StoragePlan)` keeps North. A payload with `"Zone":"South"` restores South. `"Zone":null` restores null under these options: the initializer is not a validation rule or a null-coalescing fallback.

## Migration

1. Identify defaults previously moved into constructors solely for generated omission handling.
2. Keep explicit constructors where they serve invariants; otherwise restore the initializer and generate the contract on .NET 11.
3. Deserialize through PlanContext.Default.StoragePlan rather than silently switching to reflection.
4. Test omission, explicit replacement and explicit null separately before removing a workaround.
5. Add required/nullability/business validation deliberately if the model rejects null or a specific zone.

## Constraints

Requires framework System.Text.Json on net11.0 with SDK 11 / C# 15; no packages, workloads, providers, rendering mode or experimental opt-in. No older-target route, generic-model guarantee, trimmed/AOT publication or automatic validation is claimed. The fix is about generated property assignment, not defaulting every null input. See `json.generate-private-contracts` for deliberate private-state access.

## Evidence

Actual .NET 10/11 generated contracts compile and run. Both execute missing, explicit South and explicit null inputs and inspect the resulting value. Records preserve actual sample hashes and SDK/runtime output; installed list/get retains this full comparison, edition and applicability offline. No mocks or required Human proof; .NET 11 evidence is RC2 rather than GA.
