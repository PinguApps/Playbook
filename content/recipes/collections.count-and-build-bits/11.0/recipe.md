# Build compact flags from spans and count enabled positions

## Goal

Read a packed reservation map or a slice of switches into an owned bit collection, preserving position order and counting only its logical enabled bits.

Use an SDK console project with implicit usings, nullable enabled and System.Collections imported. Place the supplied class in namespace Playbook.Samples. The comparison targets net10.0 / C# 14 and net11.0 / C# 15.

## Older approach

A fictional harbour receives two bytes describing reserved loading slots. On .NET 10, copy the input span to an array for the existing BitArray constructor, then count true values. An input that already is an array can go directly to the array constructor. This remains a valid choice and already gives the collection its own storage.

{{snippet:older}}

## Newer approach

.NET 11 accepts ReadOnlySpan<byte>, ReadOnlySpan<bool> and ReadOnlySpan<int> directly. PopCount counts enabled positions without an application loop. The constructors still copy: they avoid the intermediate array, rather than turning the span into the collection's backing memory.

{{snippet:newer}}

For bytes 0x24 and 0x41, positions 2, 5, 8 and 14 are enabled and PopCount returns 4. Restrict Length to 10 when only ten slots are meaningful; the count becomes 3. Calling Not then yields 7, without counting unused backing-storage bits. An int span represents 32 bits per element, while a bool span represents one bit per element.

## Migration

1. Keep array overloads for array inputs. For spans or stackalloc buffers, pass the selected slice directly on net11.0 instead of calling ToArray.
2. Define the wire bit order explicitly. Within each byte or int, index zero is the least significant bit; elements retain their input order.
3. Set Length to the application's logical bit count when the final byte or word includes padding. Constructor lengths are eight times the byte count or 32 times the int count.
4. Replace a true-value counting loop with PopCount. Preserve any separate per-position work the old loop performed.
5. Treat the resulting BitArray as independently owned mutable data. Later source-buffer writes do not update it, and mutations such as Not change the collection itself.

## Constraints

This is a managed .NET 11 framework API with no additional NuGet package, package asset, provider, workload, rendering mode or experimental opt-in. SDK 11 and C# 15 are the declared sample configuration; the new constructors are runtime-library changes rather than new C# syntax. No supported older-target package route is established. BitArray is not a concurrent reservation ledger or atomic bit set; synchronize shared updates separately. The example makes no speed, allocation benchmark or processor-instruction guarantee. Avoid endian-dependent byte reinterpretation when the wire format uses a different word order.

## Evidence

The actual net10.0 and net11.0 BitCollections projects compile and execute the shared BitCollectionChecks.cs. Checks cover sliced input, copied storage, exact byte/bool/int positions, counts, truncation, inversion with a partial storage word, 35 true bits and empty input. Saved records identify SDK 10.0.402 / C# 14 and SDK 11.0.100-rc.2.26504.105 / C# 15, plus actual runtimes and input hashes. This is managed execution, not mocks, cross-compilation, a performance measurement or GA proof. The full original body, canonical snippets and applicability are bundled offline.
