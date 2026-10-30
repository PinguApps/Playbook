# Preserve span storage across nested scopes

## Goal

Retain two manifest batches after an inner block finishes, with separate writable contents. A span borrows storage; assigning it does not copy the elements or extend short-lived storage's permitted scope. Choose array ownership or distinct enclosing declarations according to the lifetime you need.

## Older approach

This original failure fixture compiles with SDK10.0.100, compiler5.0 and the actual net10.0/ref10.0.0 assemblies. Do not depend on that acceptance:

{{snippet:escape}}

Include `using System;` and `namespace Playbook.Samples;`. Both SDK10.0.402/compiler5.9 and SDK11 RC2/compiler5.12 reject the assignment with:

`error CS8352: Cannot use variable 'parcelNumbers' in this context because it may expose referenced variables outside of their declaration scope`

Replacing `Span<int>` with `ReadOnlySpan<int>` produces the same rejection. The collection expression's safe context is its declaration block. `scoped` restricts escape; it does not promote the inner collection expression's storage into an outer scope. This is a compiler correction, not a new requirement of runtime11 or a C#15-only syntax feature. The pinned Roslyn document covers changes after SDK10.0.100 through 11.0.100; the measured10.0.402 compiler already contains this one. Metadata `introducedIn:10.0` records that measured release-family presence, not the first exact compiler build or the age of span syntax.

## Newer approach

Create a console project. For the older comparison set `TargetFramework` to `net10.0` and `LangVersion` to `14.0`; for this newer edition use `net11.0` and `15.0`. Add `using System;`, use namespace `Playbook.Samples`, include the following class and call `ManifestBatch.VerifyRetainedParcels()` from `Main`:

{{snippet:storage}}

Each array allocation owns its values independently. The span conversion borrows that array; the inner variable going out of scope does not invalidate the managed array. The two enclosing span declarations also work because their declaration scope covers every use. They require distinct storage for distinct batches. The final control deliberately reuses one buffer: both views read `582,77` after the last write, printed as `shared views=582,77`. Moving one declaration outward fixes a lifetime error but does not create historical snapshots.

These alternatives remain valid on .NET10. The broken fixture is compiled only to observe compiler acceptance/rejection; it is not executed or recommended as a working older implementation.

## Migration

1. Find the collection expression that creates the borrowed storage, not merely the outer span variable that receives it.
2. Identify the final read/write and whether multiple retained views must have independent contents.
3. Use a separately allocated array for each retained batch when ownership must survive the inner block. For a fixed set of local values, move distinct storage declarations into a scope enclosing every use.
4. Keep `scoped` where it expresses the intended non-escape boundary. Do not disable safety checks or return a span over short-lived local storage.
5. Compile with the actual upgraded SDK. Check both retained batches after the block, mutate one and verify the other is unchanged. If deliberate sharing is intended, test that aliasing explicitly.

## Constraints

The served route is .NET11/SDK11/C#15. It uses framework APIs only: no package, provider, optional workload, rendering mode, native toolchain or experimental opt-in. The pattern is portable to .NET11-supported platforms; actual execution was Windows x64. This edition adds no .NET10 catalogue foundation and advertises no newer-language/older-target route.

## Evidence

`eng/Test-CompilerMigrations.ps1 -Record` compiles the original Span and ReadOnlySpan fixtures with the actual SDK10.0.100/ref10.0.0, SDK10.0.402/ref10.0.13 and SDK11.0.100-rc.2.26504.105/ref11 RC2 sets. It requires old acceptance and exact CS8352 failures in both newer compilers. Paired corrected projects build and run; they check independent arrays, independent enclosing spans and deliberate shared-buffer aliasing. Saved compilation and execution records retain commands, hashed inputs, exact artifact identities and outputs. SDK10.0.100 is a historical reproducer only, not a recommended development prerequisite. RC2 proof is not GA verification; the comprehensive and GA reconciliation gates remain open.
