# Generate JSON contracts for deliberately included private state

## Goal

Save and restore a locker's seal without making the seal a public application property. Include only the private state the wire contract intentionally exposes; C# access restrictions do not make serialized data confidential.

## Older approach

Use a net10.0 / C# 14 console project with `System.Text.Json` and `System.Text.Json.Serialization`, namespace `Playbook.Samples` and standard implicit imports. An explicit DTO and mapping keep private state encapsulated while giving the generator an accessible representation. This remains a good boundary for independently versioned storage contracts.

{{snippet:older}}

## Newer approach

Target net11.0 / C# 15 with the same imports. JsonInclude opts the private property into the contract and JsonConstructor selects the private constructor. The generated context can reach these supported members through generated accessors. Save and Load explicitly use that generated contract.

{{snippet:newer}}

`Locker.Load(Locker.Create(73).Save()).InspectSeal()` returns 73. The wire object is `{"Seal":73}` in both versions. There is no general automatic serialization of every private member.

## Migration

1. Decide which private state belongs in storage; retain the DTO if wire shape should evolve independently of the model.
2. Apply JsonInclude to intentional fields/properties and JsonConstructor to the constructor that restores them.
3. Declare the model in a JsonSerializerContext and pass its generated JsonTypeInfo to both operations.
4. Build with the actual .NET 11 generator; inspect diagnostics rather than assuming reflection success proves generation.
5. Round-trip private state through public observations and review the emitted JSON for accidental disclosure.

## Constraints

Uses net11.0 framework System.Text.Json with SDK 11 / C# 15, ordinary metadata generation and no additional package, workload, provider, rendering or experimental flags. No optional older-target route or AOT publish proof. Source generation supports deliberately included inaccessible accessors/fields and a selected inaccessible constructor on this release; arbitrary unsupported layouts and generic unsafe-accessor shapes are not inferred from one successful class. This recipe uses a nongeneric private property/constructor. The related `json.preserve-omitted-initializers` and `json.bind-reference-constructors` recipes cover different construction decisions.

## Evidence

Actual net10.0 and net11.0 builds/executions check exact wire shape and the restored seal through the public observer. The newer route is explicitly source-generated, including the private constructor, without reflection fallback. Saved inputs/toolchain records and complete installed offline MCP retrieval accompany it. No cross-compilation, mocks, Native AOT or GA execution is claimed; no targeted Human proof is needed.
