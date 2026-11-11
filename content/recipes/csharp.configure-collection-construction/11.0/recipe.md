## Goal

Create a collection with its comparison policy or reserved space supplied at construction. Use C# 15 collection-expression arguments when configuration and elements belong together. In this fictional archive, shelf queues keep duplicates and order, while bay labels compare without case.

## Older approach — .NET 10

Target net10.0 with C# 14. Explicit constructors and factory calls already express these policies correctly. Keep them when that form is clearer or an older target is required. The list reserves room for twelve entries; the set and custom builder collapse equivalent bay labels.

{{snippet:older}}

## Newer approach — .NET 11

Target net11.0, use a .NET 11 SDK and set LangVersion to 15.0. No package reference, workload or preview language switch is needed. Place one `with(...)` element first; its arguments configure the target constructor or builder rather than becoming collection elements. Put CollectionWorkflow in a file with the file-scoped declaration `namespace Playbook.Samples;`; enable ImplicitUsings or import System and System.Collections.Generic.

{{snippet:newer}}

The custom collection below is shared by both samples. Its non-generic builder exposes accessible static Create overloads. Configuration precedes the final ReadOnlySpan<string> parameter; the target collection supplies an enumerable element type. An ordinary expression without arguments uses the span-only overload and retains ordinal, case-sensitive comparison.

{{snippet:builder}}

Queue() produces shelf-c, shelf-a, shelf-c in that order and reserves at least twelve slots. Labels() and BuiltLabels() contain two distinct labels and accept BAY-Q as a lookup for bay-q. Capacity is storage headroom, not Count, a maximum size or a performance guarantee.

## Migration

1. Check the application's target and SDK separately. Keep the explicit .NET 10 implementation unless net11.0 / C# 15 is supported by the application.
2. Retain the concrete collection type, constructor arguments, comparer and element order. Replace only the construction expression with the corresponding first-position `with(...)` form.
3. For a custom builder, include the complete collection and builder declarations above. Keep the span parameter last and choose overloads that preserve the intended default and configured policies.
4. If a method named `with` was intended to produce an element, escape that call as `@with(...)`. Both canonical samples use the escape and return the single element 21. An unescaped first call is parsed as construction arguments in C# 15. The tested SDK 10.0.403 already recognizes that syntax and rejects it in C# 14 mode with CS8652; use the escape there too rather than enabling preview.
5. Exercise duplicate/case handling, sequence order and capacity after the change. Verify the default builder overload separately; changing a comparer can change which data survives.

## Constraints

### Bind the target before configuration

The presence of `with` affects whether a candidate collection conversion exists; its argument values/types do not select an outer method overload or infer element type arguments. They select the constructor or builder only after the collection target is known. For overloads `Store(List<string>)` and `Store(HashSet<string>)`, `Store([with(StringComparer.OrdinalIgnoreCase), "bay"])` remains ambiguous (CS0121), even though the comparer fits only the set constructor. Assign the expression to a `HashSet<string>` local, then call `Store(labels)`.

An empty `Store<T>(HashSet<T>)` call with `[with(StringComparer.OrdinalIgnoreCase)]` cannot infer `T` from that argument (CS0411). Supply `Store<string>(...)`, or provide an explicitly typed target. Ordinary element expressions can still contribute inference.

{{snippet:binding}}

`List<int> values = [with(default)]` is also ambiguous: the default literal fits both capacity and enumerable constructors. Use `capacity: default`, `default(int)`, or an explicitly typed argument. Use `comparer:` for the comparer overload rather than relying on an untyped default. These fixes retain the concrete/interface restrictions and span-last builder contract above.

### Preserve evaluation behavior

Configuration arguments evaluate once, left to right, before element expressions; element expressions evaluate once in source order. For `[with(capacity: Reserve()), First(), .. Fetch(), Last()]`, the calls occur in that order. Calling `Fetch()` to obtain an enumerable is different from running its iterator body. Enumeration may be delayed relative to later element evaluation; do not rely on where iterator side effects interleave. Materialize a side-effecting source explicitly before construction when that ordering matters. Capacity getters, enumeration and allocation optimizations are not a scheduling contract.

Collection arguments belong to bracket collection expressions. Classic `new List<int>(12) { 1, 2 }` initializers still pass configuration to the constructor and elements to `Add`; `with(...)` is not an initializer directive. Dictionary key/value expression proposals are separate: do not infer shipped `["bay": 1]` syntax from proposal motivation. The pinned GA compiler rejects it; use `new Dictionary<string,int>(comparer) { ["bay"] = 1 }` for that contract.

`eng/Test-LanguageBinding.ps1` verifies CS0121/CS0411 failures and corrected target/type-argument/named-argument calls using SDK 11.0.100/C# 15/ref 11.0.0. It also checks rejection of initializer directives and proposed dictionary syntax. Proposal examples do not override these observed artifacts.

The target type controls available constructor/factory arguments. Do not infer a comparer overload from IEnumerable<T> alone: read-only sequence interfaces only accept an empty with element; ICollection<T>/IList<T> support empty or capacity arguments. Arrays and spans do not accept this construction element. Dynamic arguments are unsupported. Constructor overload ambiguity still needs an explicit argument type or name. Builder support here is verified for the supplied string collection, not every third-party builder.

This is stabilized C# 15 syntax according to canonical RC1 notes, with GA SDK 11.0.100 compilation/execution. No C# 15-on-net10.0 route is offered: a compiler accepting a combination does not establish supported language/target compatibility. No provider, rendering mode or native platform behavior is involved.

## Evidence

The canonical net10.0/C# 14 and net11.0/C# 15 CollectionLike projects compile and run the same behavior checks. Saved records identify exact SDKs (10.0.403 and 11.0.100), actual runtime output, source/configuration hashes and commands. Execution checks order/duplicates, reserved capacity, case-insensitive lookup/addition, configured/default builder overloads and the escaped method call. Official source review supplies the broader target restrictions; those are not all separate negative compiler tests. No mocks, cross-compilation or native checks are claimed. The package embeds this full body and canonical snippets for offline retrieval.

GA refresh (PIN-1287): the declared .NET 11 sample was rebuilt and executed with SDK 11.0.100 and runtime 11.0.0. This refresh does not establish broader platform or API behavior.
