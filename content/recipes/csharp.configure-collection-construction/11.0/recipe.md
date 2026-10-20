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
4. If a method named `with` was intended to produce an element, escape that call as `@with(...)`. Both canonical samples use the escape and return the single element 21. An unescaped first call is parsed as construction arguments in C# 15. The tested SDK 10.0.402 already recognizes that syntax and rejects it in C# 14 mode with CS8652; use the escape there too rather than enabling preview.
5. Exercise duplicate/case handling, sequence order and capacity after the change. Verify the default builder overload separately; changing a comparer can change which data survives.

## Constraints

The target type controls available constructor/factory arguments. Do not infer a comparer overload from IEnumerable<T> alone: read-only sequence interfaces only accept an empty with element; ICollection<T>/IList<T> support empty or capacity arguments. Arrays and spans do not accept this construction element. Dynamic arguments are unsupported. Constructor overload ambiguity still needs an explicit argument type or name. Builder support here is verified for the supplied string collection, not every third-party builder.

This is stabilized C# 15 syntax according to canonical RC1 notes, with real RC2 compilation/execution. GA reconciliation remains pending. No C# 15-on-net10.0 route is offered: a compiler accepting a combination does not establish supported language/target compatibility. No provider, rendering mode or native platform behavior is involved.

## Evidence

The canonical net10.0/C# 14 and net11.0/C# 15 CollectionLike projects compile and run the same behavior checks. Saved records identify exact SDKs (10.0.402 and 11.0.100-rc.2.26504.105), actual runtime output, source/configuration hashes and commands. Execution checks order/duplicates, reserved capacity, case-insensitive lookup/addition, configured/default builder overloads and the escaped method call. Official source review supplies the broader target restrictions; those are not all separate negative compiler tests. No mocks, cross-compilation, native checks or GA execution are claimed. The package embeds this full body and canonical snippets for offline retrieval.
