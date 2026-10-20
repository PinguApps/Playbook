## Goal

Add deliberate indexed read/write access to a type you cannot modify. Use an extension indexer when bracket syntax improves a collection-like API and the lookup cost and failure policy remain clear. This fictional archive adapts the framework LinkedList<T>; no receiver declaration is changed.

## Older approach — .NET 10

Ordinary extension methods provide the same node selection and mutation in net10.0 / C# 14. Method syntax remains a valid option on newer targets and makes traversal cost easier to signal in an API name.

{{snippet:older}}

## Newer approach — .NET 11

Target net11.0, use a .NET 11 SDK and set LangVersion to 15.0. Enable ImplicitUsings or import System and System.Collections.Generic. Put the static classes below in a file with the file-scoped declaration `namespace Playbook.Samples;`, then import Playbook.Samples at call sites. The named receiver in the extension block enables instance indexer access. IndexWorkflow shows actual bracket reads and assignment, using the same zero-based node walk as before.

{{snippet:newer}}

Reading slot 1 of shelf-c, shelf-a, shelf-b returns shelf-a. Assigning shelf-r at that slot changes just the middle node; the list still has three nodes in the same order. Null receivers throw ArgumentNullException. Negative and past-end slots throw ArgumentOutOfRangeException before mutation.

## Migration

1. Confirm net11.0 / C# 15 support. Keep ReadAt/WriteAt on .NET 10; extension blocks there support properties and methods, but do not provide extension indexers.
2. Put the extension block in a top-level, non-generic static class. Give the receiver a name, retain the generic element parameter, and use only the receiver's accessible API.
3. Import the extension namespace where bracket syntax is needed. Preserve the existing bounds/null policy in both accessors rather than silently returning a default value.
4. Change reads and writes together, using IndexWorkflow as the call-site pattern. Check the collection contents after valid and rejected assignments.
5. Review runtime cost before exposing the notation: this linked-list indexer walks from the head and is O(n). For frequent random access, choose an appropriate indexed collection or retain an explicit traversal method.

## Constraints

An extension does not add storage, private-member access, interface implementation or a new virtual slot to LinkedList<T>. Existing applicable instance members take precedence; an extension cannot replace their semantics. Namespace visibility and ordinary extension lookup affect availability; competing extension namespaces may make lookup ambiguous. Resolve that by controlling imports or retaining an explicit helper method.

This example is not thread-safe against concurrent mutation. It promises zero-based integer positions only, with no from-end or range contract. Null checks happen in the implementation because extension calls can receive null. Bracket notation does not imply constant-time lookup.

Canonical RC1 notes stabilize extension indexers in C# 15; the recorded artifacts are RC2, with GA reconciliation pending. No older-target C# 15 route, package, workload, provider or rendering requirement is claimed.

## Evidence

Both canonical CollectionLike projects compile on their actual targets (net10.0/C# 14, net11.0/C# 15) and execute reads, middle-node replacement, preserved sequence/length and null/negative/past-end rejection. A rejected write leaves the list unchanged. Exact saved SDK/runtime/input records accompany the samples; focused execution is distinct from source-reviewed general lookup restrictions. No native execution, mocks, cross-compilation or GA proof is claimed. Full source snippets and instructions are embedded in the offline tool.
