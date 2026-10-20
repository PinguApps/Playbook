# Resolve generic JSON converters and derived contracts

## Goal

Represent reusable dispatch notices and scalar badges consistently across closed generic types. Use open generic registrations when the serializer can infer exactly one valid closed converter or derived type, while retaining explicit wire discriminators.

## Older approach

Target net10.0 / C# 14. Import `System.Text.Json`, `System.Text.Json.Serialization`, and `System.Text.Json.Serialization.Metadata`; use namespace `Playbook.Samples` and standard implicit System imports. Register the concrete derived form used by this application and use a converter factory to close badge converters. This is a valid design, especially when only a fixed set of contracts is allowed.

{{snippet:older}}

The older notice registration covers Notice<int>; it does not purport to be a reusable registration for every T. The badge factory does support other closed types through reflection.

## Newer approach

Target net11.0 / C# 15 and retain those imports. Open registrations let the serializer resolve Dispatch<int> for Notice<int> and Dispatch<string> for Notice<string>. The converter attribute supplies BadgeConverter<int> or BadgeConverter<string> directly, so the factory disappears.

{{snippet:newer}}

Serialize through the base, for example `JsonSerializer.Serialize<Notice<int>>(new Dispatch<int> { Value = 23, Dock = "Quay" })`, and deserialize through Notice<int>. The `dispatch` discriminator selects Dispatch<int> and restores Dock. `JsonSerializer.Serialize(new Badge<int>(31))` writes `31`, not an object wrapper. For the generated int contract use `NoticeContext.Default.NoticeInt32` in both operations; do not assume unlisted closed types are generated.

## Migration

1. Identify each closed root and preserve its established discriminator and converter wire representation.
2. Replace a per-closed-type registration with an open derived type only when every argument can be resolved and satisfies constraints.
3. Replace a converter factory with an open converter attribute only when its total generic arity matches the target's arity.
4. List the actual closed root types in generated contexts and compile them. Reflection support alone does not prove generated metadata contains every closure.
5. Round-trip each intended closure and fail invalid configurations during validation instead of suppressing diagnostics.

## Constraints

Requires .NET 11 framework System.Text.Json, SDK 11 / C# 15; no extra packages, workloads, providers, rendering modes or experimental opt-ins. No older-target route or trimmed/Native AOT publish claim. Derived resolution supports reordered, partially concrete and nested argument patterns, but must find one unambiguous closed type. Ground-type mismatch, unbound arguments, incompatible shape and violated constraints fail with InvalidOperationException. Generic converter constraints must also hold. SYSLIB1229 identifies unresolved derived registrations during generation; suppression does not create a working contract. The executed samples demonstrate direct binding, not every nested/reordered pattern.

## Evidence

Real .NET 10/11 builds and serializer executions verify the derived payload/discriminator, generated int-root metadata and int/string badge conversion. RC2 additionally executes a second derived closure and rejects an incompatible open registration. The diagnostic probe compiles an invalid generated registration and requires SYSLIB1229. Exact artifacts/input hashes are saved; installed MCP checks body, discovery and edition offline. RC proof is distinct from GA and from AOT execution. No required Human proof.
