# Bound a 32-bit managed heap

## Goal

Bound the managed heap of Marsh Observatory's disposable x86 image worker. Use
.NET 11's runtime limit rather than treating an application tile counter as a
limit on all managed allocations. This is a managed-heap budget, not a process
memory cap, address-space reservation or guarantee that an allocation succeeds.

## Older approach

The actual .NET 10.0.13 Windows x86 runtime ignores HeapHardLimit: the same 32 MiB
configuration allowed this bounded control to retain 128 MiB. Keep an explicit
application budget on that runtime. The original worker stops at sixteen 1 MiB
tiles before allocating another tile. This bounds only these retained payloads;
object headers, temporary objects and other components are outside that counter.
It is not an equivalent whole-heap enforcement mechanism.

{{snippet:older}}

## Newer approach

Use net11.0 and an actual Windows x86 runtime. This complete project sets the
runtime configuration value in decimal bytes. The sample inherits OutputType=Exe,
nullable and implicit usings from samples/Directory.Build.props; set those in a
standalone project. UseAppHost=false makes the selected x86 dotnet host explicit.

{{snippet:project}}

The same worker with no application argument deliberately retains tiles until
the configured heap cannot satisfy another allocation. It catches
OutOfMemoryException, stops accepting work, clears its retained tiles and exits.
This controlled demonstration must run in its own disposable process. It never
tries to exhaust the machine's memory: the loop also has a 128 MiB payload ceiling.

{{snippet:newer}}

## Migration

1. Identify the process architecture, not only the OS architecture. Compile with
   PlatformTarget=x86 and launch the DLL through the actual x86 runtime. The
   program rejects a 64-bit process and prints the loaded runtime/architecture.
2. On .NET 10 retain the application admission budget; run the older sample with
   the application argument. Do not claim HeapHardLimit enforces that budget.
3. On .NET 11 add the RuntimeHostConfigurationOption shown above before process
   startup. System.GC.HeapHardLimit is a JSON integer in decimal bytes. The
   alternative DOTNET_GCHeapHardLimit environment variable uses hexadecimal;
   do not copy a decimal string into that variable. Avoid conflicting settings.
4. Build using each sample's pinned SDK from its sample directory. Launch the
   produced MarshHeap.dll through the matching x86 dotnet executable. Run the
   current sample with no arguments for the heap test and application for the
   smaller application-budget check. The older no-argument control intentionally
   exits unsuccessfully when its assertion proves the runtime ignored the limit.
5. Check GC.GetGCMemoryInfo().TotalAvailableMemoryBytes, retained payload and the
   actual failure. The current control reports 33554432 bytes and catches OOM
   before retaining that many payload bytes. Leave headroom for runtime objects.
6. Measure process WorkingSet64 separately. Reassess native buffers, stacks,
   executable pages and other process memory under a separate process/container
   policy when a total-memory limit is required. Remove a custom limiter only
   after deciding whether it also implemented useful application admission rules.

## Constraints

Measured Windows x86 CoreCLR only, hosted on Windows x64. Other 32-bit OS/runtime
routes, server-GC variations, per-object-heap overrides, percentage settings and
live reconfiguration are not established here. A successful x64 build alone is
not proof of 32-bit enforcement. No optional workload, package, provider or UI
integration is required. The examples declare C#14/.NET10 and C#15/.NET11; the
feature is runtime configuration, not new C# syntax.

An OOM handler cannot make arbitrary application recovery safe. This worker
performs only a controlled stop; avoid allocating an elaborate error response
after exhaustion. The limit includes GC bookkeeping/commit overhead and does
not promise exactly 32 tiles. RSS can exceed 32 MiB while the managed limit works.

## Evidence

eng/Test-NativeBoundaries.ps1 -Lane Windows -Record builds both independently
pinned consumers and runs real Windows x86 .NET10.0.13 and .NET11 RC2. Evidence
records retain commands, inputs, actual compiler/reference/runtime identities,
configured limit, bounded retained bytes, RSS and observed OOM/control outcomes.
The records distinguish actual SDK10.0.402 and SDK11.0.100-rc.2.26504.105 from
application prerequisites. No actual GA or other-platform execution is claimed.
Installed ordinary-package stdio inspection checks the complete canonical
Markdown, applicability and evidence. PIN-959/PIN-962 remain open; no ninth .NET10
foundation, recurring native matrix, release or persistent environment change is added.
