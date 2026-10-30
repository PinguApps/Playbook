# Handle complex nonfinite results deliberately

## Goal

Keep a fictional signal pipeline's finite measurements, directed infinities and
undefined results distinct when upgrading nongeneric `System.Numerics.Complex`.
Use this when tests or routing rules assume every nonfinite calculation produces
two NaNs. Keep the existing double-based storage if it meets the precision budget;
choosing `Complex<T>` is a separate goal in `numerics.select-complex-precision`.

Use an SDK console project with implicit System imports, nullable enabled and
namespace Playbook.Samples. The comparisons target net10.0/C#14 and net11.0/C#15.
Neither policy needs a package or new language construct.

## Older approach

The station already classifies each computed result before accepting a sample.
An infinite component wins over a companion NaN; without an infinity, any NaN
means undefined. Only the finite branch can enter ordinary finite calculations.
This explicit policy is valid on .NET 10 and remains useful after upgrading.

{{snippet:older}}

On the measured .NET 10 runtime, multiplying the carrier `(+infinity,-infinity)`
by gain `(2,0)` returns `(NaN,NaN)`, so this policy routes it to Undefined. It
cannot recover direction already lost inside the arithmetic. Dividing signal
`(6,-8)` by zero also yields two NaNs; finite division by `(2,0)` returns `(3,-4)`.
If nonfinite inputs are invalid for the application, rejecting them before any
arithmetic remains a legitimate alternative on either release.

## Newer approach

Retain the same classifier and storage. The .NET 11 implementation delegates
much of nongeneric Complex to `Complex<double>` and preserves more useful special
values. The carrier product now has positive/negative infinite components, and
the zero-divisor signal also yields positive/negative infinity. Both route to
DirectedInfinity. Zero divided by zero remains Undefined; finite controls agree.

{{snippet:newer}}

Classification is an application policy, not a claim that every component of an
infinite result is known. For example, the executed hyperbolic cosine of
`(-infinity,+infinity)` changes from two NaNs to `(+infinity,NaN)`: record the known
infinite component and retain the unknown companion. Do not replace NaN with zero
or feed this branch to code requiring two finite components.

## Migration

1. Find assertions and branches that collapse every nonfinite result to one
   failure state. Decide whether infinite signals are meaningful in your domain
   or should be rejected at the input boundary.
2. Keep nongeneric Complex when double components meet the precision requirement.
   Apply the supplied classifier to actual results before consuming them.
3. Retest multiplication and division, their named methods, zero divisors,
   Reciprocal, Abs and both Pow overloads. Do not infer these contracts from
   addition or a finite-only control.
4. Review the complete affected family: Sqrt, Exp, every Log overload, Log10,
   Sin/Cos/Tan, Sinh/Cosh/Tanh and Asin/Acos/Atan. Use representative domain inputs
   for each family; special values do not establish a general accuracy bound.
5. Preserve signed-zero tests where a branch side matters. The original
   `Sqrt((-9,-0))` control changes imaginary component from `+3` to `-3`.
   Inspect bits when zero signs matter, but never assert a universal sign for a
   zero quotient component: Annex G leaves that particular sign unspecified.
6. Refresh saved expectations against the exact runtime you deploy. There is no
   compatibility switch restoring the previous implementation. Keep exact input
   policy or a separately validated numerical algorithm when historical results
   are a protocol requirement.

## Constraints

The official note describes an affected function family, not a promise that
every operation changes or returns infinity for every infinite input. On both
actual artifacts `Complex.Reciprocal(Complex.Zero)` returns Complex.Zero, and
`Abs((infinity,NaN))` returns positive infinity. Preserve these qualifications
rather than deriving reciprocal-zero behavior from the division operator.

Exp at positive infinity with negative imaginary zero preserves negative zero
on the current runtime. Finite Exp at `(0,-0)` preserves it on both artifacts.
Log, transcendental accuracy, rounding and overflow remain numerical concerns;
this recipe supplies no universal error or throughput guarantee. Undefined and
infinite signals still need an explicit application response.

The current route requires .NET 11 framework/runtime references. SDK11/C#15 are
the canonical sample configuration, not minimum syntax requirements for this
classifier. There is no package, package-asset, provider, UI rendering, native
toolchain, workload, service or experimental opt-in dependency. Only Windows x64
CoreCLR JIT execution is measured; no AOT, Mono or other platform execution is
claimed. The previous comparison is not an additional target10 foundation.

## Evidence

Paired SignalPipeline consumers compile and execute with SDK10.0.402/C#14 /
runtime10.0.13 and SDK11.0.100-rc.2.26504.105/C#15 /
runtime11.0.0-rc.2.26504.105. Shared original checks exercise finite multiplication,
division, reciprocal, magnitude, power and root; all named affected functions;
NaN/infinity routing; signed zeros; and old/new failures and corrected results.
Inputs are parsed at runtime, separating these observations from constant folding.
Saved sample records contain commands, exact inputs/results and component bits.
RC2 proof is not GA proof. Comprehensive coverage and actual-GA reconciliation
remain open; no mocks or source-only claims replace these executions.
