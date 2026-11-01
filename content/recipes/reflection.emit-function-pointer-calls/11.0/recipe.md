# Emit calls from typed function-pointer signatures

## Goal

Generate a call adapter for fictional Slate Fen quarry: adjust a managed load
count and scale a count through Windows' native MulDiv function. Use this when a
trusted runtime code generator already needs an indirect call. Ordinary application
code can call a typed function pointer directly or use a normal interop declaration;
emitting IL is useful only when runtime generation is part of the requirement.

The factory snippets include imports and the Playbook.Samples namespace. Copy one
factory and the consumer into separate files in the same console project. Enable
AllowUnsafeBlocks in its project properties; the pointer typeof expressions and
consumer use unsafe C#. The types are internal and called within that assembly.

## Older approach

.NET 10 supports emitted indirect calls already. Supply the opcode, managed or
unmanaged calling convention, return type and parameter types separately. This is
still valid on .NET 11. A managed standard call and native StdCall are different
signatures even when their argument types match.

{{snippet:older}}

This net10.0/C#14 comparison uses framework APIs without a NuGet package or workload.
It is not a new .NET 10 catalogue foundation. The explicit signature setup remains
an honest older alternative; dynamic code and valid pointer lifetimes are required
on both releases.

## Newer approach

.NET 11 adds ILGenerator.EmitCalli(Type). On DynamicMethod's IL generator, pass an
actual function-pointer Type, such as typeof(delegate* managed<int, int, int>) or
typeof(delegate* unmanaged[Stdcall]<int, int, int, int>). The last type argument
is the return type. A delegate class such as Func<int,int> is not a function-pointer
Type. The typed overload carries parameter, return and calling-convention metadata
together; it does not supply a function address or validate the address's real ABI.

{{snippet:newer}}

Both versions use this complete consumer. For example, Evaluate(6, 5, 7, 2) returns
(37, 21). Its native library handle remains alive until the generated call returns;
neither the pointer nor its generated delegate escapes that lifetime.

{{snippet:consumer}}

MulDiv takes three Windows INT arguments and returns INT. The signature is
unmanaged Stdcall. Windows x64 uses its unified native calling convention; these
checks do not distinguish x86 Stdcall stack cleanup from Cdecl. No native helper
compiler or third-party package is required. The example is explicitly Windows-only.

## Migration

1. Target net11.0 using a .NET 11 SDK/runtime. The sample uses C#15.0 with
   AllowUnsafeBlocks=true, nullable/implicit usings and warnings as errors. No
   preview language switch, experimental opt-in, package, provider, workload or
   rendering integration is required. Exact RC2 pins below record tested artifacts.
2. Before creating a DynamicMethod, test RuntimeFeature.IsDynamicCodeSupported.
   If false, fail clearly or choose an existing statically compiled adapter.
   These factories carry RequiresDynamicCode so callers see the generation
   requirement during analysis. This recipe supplies no Native AOT execution route.
3. Establish the real target signature from its declaration. Keep managed and
   unmanaged pointer kinds and the exact ABI, parameter order/width, return type
   and any calling-convention modifiers together. Replace the old EmitCalli
   signature arguments with the corresponding function-pointer Type. Do not
   reinterpret an arbitrary address as evidence that its signature is correct.
4. Emit arguments in declaration order, then the function address, then EmitCalli
   and Ret. The dynamic method's own parameter list describes its callable wrapper;
   the nint address parameter is consumed by calli, not passed to the target.
5. Obtain addresses only from known matching functions. This example uses & on
   its static managed method and an explicitly loaded system-directory native DLL.
   Keep its native library loaded during every use and free only the owned handle.
   A pointer from a managed delegate would additionally require keeping that
   delegate alive; this example does not create one that way.
6. Execute known argument/return checks through the generated delegate and an
   independent direct typed pointer. Test safe target failure results and reject
   null/non-pointer signature Types without executing malformed IL. Never invoke
   a deliberately mismatched calling convention or dangling pointer as a test.

## Constraints and alternatives

The new overload does not make arbitrary emitted IL safe, establish native
marshalling, infer ownership, preserve last-error state, or turn a raw address into
a checked pointer. MulDiv's -1 is a documented zero-divisor/overflow sentinel, not
an exception; a valid calculation can also produce -1. This example tests known
failure inputs rather than treating every -1 as a distinguishable error.

Use trusted generator inputs. Keep a direct typed pointer call or a static interop
declaration when no runtime adapter is needed. Support of this overload by the
DynamicMethod generator does not establish support by every ILGenerator subclass.
Non-function-pointer Types and null are rejected before emitting the call.

The audited API is not marked experimental. Execution covers CoreCLR DynamicMethod,
managed signatures and Windows x64 MulDiv only. Other native libraries, x86/Arm
ABIs, Mono, browser/WASI, custom generators, Native AOT and final-GA execution are
unverified. The false dynamic-code branch check is injected; it is not an actual
run on a runtime that forbids dynamic code.

## Evidence

eng/Test-RuntimeCalls.ps1 -Record -VerifySources builds and runs actual
net10.0/C#14 and net11.0/C#15 consumers using SDK10.0.402/runtime10.0.13 and
SDK11.0.100-rc.2.26504.105/runtime11.0.0-rc.2.26504.105. Saved compilation records
include exact commands, inputs, empty resolved package sets, compiler and loaded
CoreLib identities, and audit-matching System.Runtime, Reflection.Emit.ILGeneration
and Reflection.Emit.Lightweight reference hashes. Paired official contracts and
DynamicILGenerator implementations are pinned in runtime-calls.sources.json.

Both consumers execute the canonical adapter, signed managed calls, and native
positive/negative rounding, zero-divisor and overflow inputs. Direct typed MulDiv
calls agree with the generated calls. Loaded kernel32/kernelbase versions and
hashes are recorded. Both exercise the injected false guard; the current consumer
rejects null and a delegate class as signature Types. No malformed IL, invalid
address, persistent native installation, external service or recurring matrix is used.

The ordinary installed package is checked through real stdio list/get from an
unrelated directory for discovery, complete canonical Markdown, target11/edition11,
applicability and fresh saved evidence offline. PIN-959/PIN-962 and existing Human
gates stay open; the exact eight .NET 10 foundations are unchanged.
