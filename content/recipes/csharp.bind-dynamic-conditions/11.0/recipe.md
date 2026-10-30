# Preserve dynamic logical operator binding

## Goal

Keep a loading decision's logical result and lazy right-hand work when some input is dynamic. Use a known concrete type that owns the operators, or deliberately request runtime binding. An interface's static operators do not promise that arbitrary runtime implementations provide the concrete operations.

## Older approach

The original policy below can be viewed through `ILoadingPolicy`. This older spelling tries to combine that statically interface-typed left operand with a dynamic right operand:

{{snippet:interface-left}}

With SDK10.0.100/compiler5.0 and net10.0/ref10.0.0, both methods compile. Actual runtime10.0.0 execution throws `RuntimeBinderException` for both operators: the binder cannot apply them to `ILoadingPolicy` and `LoadingPolicy`. This is a failing older implementation, not working logic that an upgrade removed.

SDK10.0.402/compiler5.9 and SDK11 RC2/compiler5.12 reject the same methods with these separate diagnostics:

`error CS7083: Expression must be implicitly convertible to Boolean or its type 'ILoadingPolicy' must not be an interface and must define operator 'false'.`

`error CS7083: Expression must be implicitly convertible to Boolean or its type 'ILoadingPolicy' must not be an interface and must define operator 'true'.`

The first applies to `&&`, the second to `||`. The compiler correction is already present in measured SDK10.0.402. It is not exclusive to C#15 or runtime11. The pinned source covers changes after10.0.100 through11.0.100; metadata `introducedIn:10.0` reflects measured release-family presence, not an exact first compiler build or the original introduction of dynamic syntax.

## Newer approach

Create a console project with `TargetFramework` `net10.0` / `LangVersion` `14.0` for the comparison, or `net11.0` / `15.0` for this edition. Include `using System;`, use namespace `Playbook.Samples`, and add these original types:

{{snippet:policy}}

From `Main`, call `LoadingDecision.Concrete(new LoadingPolicy(true), () => new LoadingPolicy(false), false)`: `Permits` is false and the right factory runs once. With a false left value and `either:false`, the factory runs zero times. With `either:true`, a true left value skips it. `LateBound` gives the same results when both runtime operands are `LoadingPolicy`.

The concrete route checks the implementation explicitly before changing the static operand type. A direct cast would throw for `UnknownPolicy`; the example provides a deliberate `ArgumentException` boundary. `LateBound` instead defers operator lookup and can throw `RuntimeBinderException` for an unknown implementation or an incompatible right value. Neither route supports arbitrary implementations, null operands, or a general policy plug-in contract. Keep a typed Boolean contract if that broader contract is what the application actually needs.

`&&` uses `operator false` to decide whether to skip the right side; `||` uses `operator true`. When the right side is evaluated, the corresponding `&` or `|` produces the result. Replacing them with eager operators or evaluating `right()` before the conditional changes side effects and work counts. The .NET10 corrected comparison uses these same valid alternatives.

## Migration

1. Locate both the static left-operand type and the actual runtime types. Confirm which concrete type owns the required true/false and combination operators.
2. For a known implementation, check or pattern-match it and perform the operation on that concrete variable. Do not cast arbitrary implementations without a failure policy.
3. Choose `dynamic` only when runtime dispatch and its runtime failures are intentional. Decide how null or incompatible values are rejected before exposing this route to untrusted inputs.
4. Keep right-hand work inside its factory and inside `&&`/`||`. Preserve the existing operator implementations and input contract.
5. Run every left/right true/false combination for both operators. Assert the result and exact right-factory invocation count; also test an incompatible implementation.

## Constraints

The served route is .NET11/SDK11/C#15. The framework supplies the Microsoft.CSharp runtime binder; there is no added NuGet package. No provider, optional workload, rendering mode, native toolchain or experimental opt-in is needed. Portable .NET11-supported platforms can use the pattern; actual checks ran on Windows x64. This is a compiler migration edition, with no extra .NET10 foundation or advertised C#15/older-target route.

## Evidence

`eng/Test-CompilerMigrations.ps1 -Record` uses actual SDK10.0.100/ref10.0.0/runtime10.0.0 to compile and execute both old binder failures. Actual SDK10.0.402/ref10.0.13 and SDK11.0.100-rc.2.26504.105/ref11 RC2 must emit exactly two CS7083 errors. Paired corrected projects run all16 route/operator/left/right combinations and check results, zero/one right invocations, concrete rejection and late-bound failure for an unknown policy. Saved records retain exact commands, hashes, compiler/reference/runtime identities and fresh inputs. The historical10.0.100 reproducer is not a recommended prerequisite. These actual RC2 checks are not GA evidence; comprehensive and GA reconciliation remain open.
