## Goal

Expose a computed property on a type you cannot edit. Use a C# 14 extension property for a cheap, side-effect-free observation of existing public state. This fictional shelf workflow asks whether an IReadOnlyCollection<T> contains pending entries without wrapping or subclassing it.

## Older approach — .NET 9

C# 13 does not declare extension properties. An ordinary extension method is the supported equivalent for this computation, and remains valid on later releases. It observes Count every time and explicitly rejects a null receiver.

{{snippet:older}}

## Newer approach — .NET 10 foundation

Target net10.0 with a .NET 10 SDK and LangVersion 14.0. Enable ImplicitUsings or import System and System.Collections.Generic. Put the static classes below in a file with the file-scoped declaration `namespace Playbook.Samples;` and import Playbook.Samples at call sites. A named receiver inside the extension block supplies the state for the computed getter; ShelfWorkflow demonstrates property-style access without parentheses.

{{snippet:newer}}

The same foundation remains applicable on net11.0 with a .NET 11 SDK and C# 14 mode; its separately compiled sample preserves this body and edition 10.0. It does not require the C# 15 indexer or collection-construction additions.

## Migration

1. Inspect the application's target and language mode. On net9.0 / C# 13, keep the method approach; do not enable C# 14 on an unsupported target merely to get property syntax.
2. Choose an observation that fits property semantics. Keep expensive work, I/O, asynchronous operations and deliberate actions as methods.
3. Put the generic extension block in the non-generic static ShelfExtensions class. Use a named receiver and only accessible public state. Keep the null check.
4. Import Playbook.Samples and change HasPending() reads to HasPending. A List<string> implements the receiver interface; adding an entry makes the computed value true and clearing it makes the value false again.
5. Check empty, mutated, cleared and null receivers. The property does not cache the result or introduce storage into the collection.

## Constraints

Extension properties cannot supply instance fields or alter the receiver's interfaces, layout or private members. An existing applicable instance member wins over an extension. Competing namespaces can create ambiguity; control imports or retain the explicit helper method. The containing class must be top-level, static and non-generic; generic parameters belong on the extension block.

This computed observation is not synchronization: another thread may change a mutable collection after the read. It adds no indexer and does not turn IEnumerable<T> into a count-bearing collection. No package, asset, provider, rendering restriction, workload or experimental opt-in is required for this example. The .NET 11 verification is RC2-only and still needs GA reconciliation.

## Evidence

The actual net9.0/C# 13, net10.0/C# 14 and net11.0/C# 14 ExtensionProperties projects compile and execute the same empty/add/clear/null checks. Saved records retain SDK 9.0.319, 10.0.402 and 11.0.100-rc.2.26504.105, actual runtime output, canonical source/configuration hashes and build/run commands. Execution proves recomputation after mutation; official reference review supports the broader declaration and lookup restrictions. No C# 15 functionality, native checks, mocks, cross-compilation or GA execution is claimed. The complete comparison is available offline, including to an explicitly .NET 10 caller.
