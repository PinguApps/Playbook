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

The linked-list example is not thread-safe against concurrent mutation. It supplies zero-based integer positions only; the separate range example below supplies a complete range/list-pattern contract. Null checks happen in the implementation because extension calls can receive null. Bracket notation does not imply constant-time lookup.

### Lookup and unsupported contexts

Resolution tries applicable instance indexers, then implicit instance Index/Range indexing, then explicit extension indexers, then implicit extension indexing. For example, an instance Length plus integer indexer continues to handle `receiver[^1]` even if an imported extension declares an Index indexer. For count lookup, search instance scope before extension scopes; within each scope prefer Length to Count. An instance Count therefore wins over an extension Length.

Arrays and strings keep their special element-access binding: declaring an extension indexer does not make new bracket argument types available on them. Extension members are not considered for `base[...]`. Extension indexers cannot appear in expression trees, so an IQueryable selector containing one fails during compilation, before any provider runs. Use a deliberate client-side boundary only when loading and processing those results is appropriate; a static helper compiling inside a tree does not establish provider translation.

Type inference uses the receiver and index arguments, excluding the assigned value. With `extension<T>(int receiver)` and a setter `T this[int slot, T key]`, `receiver[0, null] = new object()` cannot infer T. Passing a typed key succeeds. An explicit static setter call can infer from its final value argument because that is an ordinary method argument.

Extension indexers cannot have init accessors or abstract, virtual, override, new, sealed, partial or protected forms (including protected accessibility combinations). They need a named receiver; static indexers and auto-accessor storage are not supplied. Ref-returning accessors and expression bodies remain possible under their ordinary rules.

### Optional from-end, range and list-pattern access

Use this independent fictional archive type when a copied slice is appropriate. Count, an integer indexer and Slice together support implicit Index/Range access and list patterns. Put these declarations in the same namespace and project settings as above; call RangeShelfWorkflow.Check to exercise the contract. Its module initializer also runs the check when the sample loads.

{{snippet:ranges}}

`^1` selects the last label; `1..^1` copies the middle label, and the list pattern captures a copied tail. Index and slice bounds are checked before copying. Copy mutation leaves the source unchanged. This example exposes a mutable array to make copy isolation visible; it is not a concurrent snapshot API. A null shelf is outside this example's contract. Extension counts participate in countability and implicit indexing, but do not automatically provide collection-expression spread presizing.

The IndexerName attribute changes the emitted accessor name to Label. `ArchiveShelfAccess.get_Label(shelf, 1)` is a compiled call to the actual static implementation, with the receiver first. Ordinary unnamed extension indexers expose get_Item/set_Item; a setter takes receiver, index arguments, then value. Selecting the declaring static class explicitly can resolve ambiguity between imported extension containers. IndexerName controls metadata/accessor naming, not bracket lookup priority, so renaming it alone does not resolve competing bracket candidates.

Canonical RC1 notes stabilize extension indexers in C# 15; the recorded artifacts are GA SDK 11.0.100/runtime 11.0.0. No older-target C# 15 route, package, workload, provider or rendering requirement is claimed.

## Evidence

Both canonical CollectionLike projects compile on their actual targets (net10.0/C# 14, net11.0/C# 15) and execute reads, middle-node replacement, preserved sequence/length and null/negative/past-end rejection. A rejected write leaves the list unchanged. Exact saved SDK/runtime/input records accompany the samples; focused execution is distinct from source-reviewed general lookup restrictions. No native execution, mocks or cross-compilation is claimed. Full source snippets and instructions are embedded in the offline tool.

GA refresh (PIN-1287): the declared .NET 11 sample was rebuilt and executed with SDK 11.0.100 and runtime 11.0.0. This refresh does not establish broader platform or API behavior.

The expanded GA sample executes range/list matching, rejected bounds, copy isolation, the named static accessor, instance implicit-indexer precedence and instance Count versus extension Length lookup. `eng/Test-IndexerJumpContracts.ps1` compiles real consumers against SDK 11.0.100/reference assemblies 11.0.0, including expression-tree rejection (CS9296), assignment inference, array/string/base rejection and valid static setter inference. No database/provider execution is claimed.
