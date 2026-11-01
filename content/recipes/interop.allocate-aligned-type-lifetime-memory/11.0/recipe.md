# Allocate aligned native memory with type lifetime

## Goal

Allocate aligned unmanaged state whose lifetime follows a runtime type. Fictional
Merewick stores a parcel index in 64-byte-aligned storage belonging to a collectible
plug-in type. Use this when the type's loader lifetime is the intended owner;
choose explicit allocation/free ownership when storage must be released earlier.

## Older approach

.NET 10 already has RuntimeHelpers.AllocateTypeAssociatedMemory(Type, int), but
that overload does not let the caller request an alignment. Do not infer a 64-byte
guarantee from a particular observed address. NativeMemory.AlignedAlloc and
AlignedFree remain a valid explicit ownership route. Here the operation owns the
allocation, writes it, and frees it in finally. This is deliberately a shorter
lifetime than the type, rather than an automatic type-unload equivalent.

{{snippet:older}}

Copy into an unsafe-enabled nullable/implicit-usings console project and call
TypeLedger.Write(pluginType, 257, 64) from the same assembly. A longer-lived older
owner must coordinate disposal with all native users; a bare pointer cannot do
that. Never mix AlignedFree with the ordinary Free API.

## Newer approach

.NET 11 adds the alignment argument. The runtime's loader allocator owns the
block, which is released if and when the associated type unloads. The example
explicitly initializes the bytes; it assumes no zero-initialization guarantee.

{{snippet:newer}}

The guard rejects an empty application buffer, although the raw current API
accepts size zero. No byte may be accessed for a zero-sized allocation. Sizes
need not be an alignment multiple: the real suite writes 257 bytes at alignments
1, 8, 16, 64 and 256, measuring remainder zero and both ends of the buffer.

## Migration

1. Target net11.0 with a .NET 11 SDK/runtime. The sample declares C#15.0 and
   AllowUnsafeBlocks=true for pointer-to-span access. There is no package backport,
   workload, external provider, rendering requirement or experimental opt-in in
   this route. .NET 10/C#14 keeps the explicit allocator/free comparison.
2. Select a real runtime Type, such as CreateType()'s result from a
   RunAndCollect dynamic assembly. An unfinished TypeBuilder, TypeDelegator or
   null is not a valid runtime type for the new allocation overload. The fixture
   requires RuntimeFeature.IsDynamicCodeSupported; no Native AOT/Mono/browser
   collectible-type route is claimed.
3. Supply a nonnegative byte size and positive power-of-two alignment. The raw
   overload rejects negative size with ArgumentOutOfRangeException and invalid
   alignment with ArgumentException. The example additionally rejects zero size.
   Apply an application size cap before allocation; allocation failure remains
   possible. The suite avoids deliberately exhausting memory.
4. Replace the explicit allocation only when type lifetime is appropriate.
   Never call NativeMemory.Free/AlignedFree on runtime-owned memory. Repeated
   calls allocate additional storage, with no per-block release method; cache or
   reuse a block deliberately when needed. Noncollectible types can retain their
   memory for the process lifetime. This API is not a general disposable buffer.
5. Keep the owner strongly reachable until every pointer/span/native callback
   has finished. GC.KeepAlive(owner) after the last access handles this synchronous
   example. A raw address, Span<byte> or nint does not root the type. Asynchronous
   or native users need coordinated lifetime ownership and must finish before
   releasing the type. KeepAlive in the allocating method does not protect later
   callers after it returns.
6. Verify actual address alignment, initialization, reads/writes and owner
   release in the intended runtime. Stop accessing the pointer before permitting
   collection; never demonstrate failure by reading freed memory.

## Constraints and alternatives

Lifetime follows the runtime loader's associated type ownership, not lexical
scope or a deterministic unload deadline. Collection is nondeterministic. The
fixture keeps a collectible type alive across GC and storage access, then returns
only a weak reference. Later full collections observe that type becoming
unreachable, without retaining or dereferencing its addresses. This verifies the
safe ownership pattern and collectible release; it does not instrument the native
allocator's exact reclamation time or prove leak-free arbitrary application code.

The test measures Windows x64 CoreCLR. Other runtimes, ABIs, Native AOT,
cross-process sharing, managed-object storage, concurrent mutation and performance
are outside this evidence. Use a SafeHandle-style or other explicit disposable
owner when deterministic early freeing is required; validate that owner's own
concurrency/disposal protocol separately. No universal native struct-layout
guarantee follows from one requested alignment.

## Evidence

eng/Test-LifetimeSignatures.ps1 -Record -VerifySources builds and runs both
TypeLedger consumers with SDK10.0.402/runtime10.0.13 and
SDK11.0.100-rc.2.26504.105/runtime11.0.0-rc.2.26504.105. Saved records include
commands, exact input hashes, dependencies, compiler/CoreLib/CoreCLR identities
and System.Runtime references matched to the source audit. Source pins separately
qualify the public contract and native loader allocation. Both build without
warnings/errors; no optional workload or native compiler is required.

Checks cover measured alignment, initialized boundary bytes across GC, collectible
type release, negative size, non-power-of-two alignments and current raw null/
delegated type rejection and zero-size acceptance without dereference. Installed
offline stdio list/get checks discovery, full canonical content, target11/edition11,
applicability, fresh evidence and target10 rejection. This adds no .NET 10
foundation. RC2 execution is not GA verification; PIN-959/PIN-962 and existing
Human gates remain open. No recurring platform matrix or publication is added.
