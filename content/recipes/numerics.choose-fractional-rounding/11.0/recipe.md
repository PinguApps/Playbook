# Choose explicit fractional rounding semantics

## Goal

An observatory exports measurements with two fractional digits. Decide whether
those digits describe the **exact stored binary measurement**, a **decimal source
quantity**, or an agreed **scale/round/divide policy**. They can give different
answers. A runtime upgrade is a useful point to make that choice explicit.

## Older approach: .NET 10

Create an SDK console project targeting `net10.0`, with `LangVersion=14.0`,
`ImplicitUsings=enable` and `Nullable=enable`. Put this class in `ExportPolicy.cs`.
No package is needed for the scalar policies.

{{snippet:older}}

The older binary route remains useful when its results match the export contract.
It accepts digits 0–15 for double, 0–6 for float. Its intermediate multiplication
can move a value onto a midpoint or lose fractional information. Decimal source
quantities and the bounded explicit policy work on both releases.

## Newer approach: .NET 11

Use `net11.0` and the sample's C# 15 mode with the same project settings. The
method signatures are already available on .NET 10; the runtime behaviour changes.

{{snippet:newer}}

`Math.Round(double, digits[, mode])` and `MathF.Round(float, digits[, mode])`
round the exact binary input, then return the nearest representable result. This
does not turn binary input into the decimal quantity the author may have intended.
Read decimal text as decimal, or use decimal literals, when that is the contract.

These runtime-parsed original inputs were executed on the paired pinned artifacts
with two digits and `ToEven`:

| Input / representation | .NET 10 | .NET 11 RC2 |
| --- | --- | --- |
| double `10.025`, just above the decimal midpoint | 10.02 | 10.03 |
| double `10.055`, just below the decimal midpoint | 10.06 | 10.05 |
| float `4.015`, below the midpoint | 4.02 | 4.01 |
| float `4.025`, above the midpoint | 4.02 | 4.03 |
| double `222222222222222.5` | 222222222222222.47 | 222222222222222.5 |
| decimal `10.025m`, exact midpoint | 10.02 | 10.02 |

Negative counterparts are checked too. The unchanged no-digits overloads still
round `2.5` to 2. Decimal overloads retain their own digits range and semantics.
For binary double/float, negative digits remain invalid; double digits 16/17 and
float digits 7/9 now succeed. At 17+/9+ respectively the value is unchanged.
Invalid rounding modes remain invalid; nonnegative digits do not legalize them.
`double.Round`, `float.Round`, generic `IFloatingPoint<T>.Round`, `Half.Round`
and `NFloat.Round` delegate to these paths. Representative actual calls are
checked; NFloat evidence here is the 64-bit Windows representation, not a 32-bit run.

## Batch measurements

For the optional batch add `System.Numerics.Tensors` **11.0.0-rc.2.26504.105**
to the net11 project and put this class in `BatchExport.cs`. The package selects
`lib/net11.0/System.Numerics.Tensors.dll`. The matching .NET 10 comparison uses
package **10.0.13**, selecting its actual net10 asset; no newer-package-on-net10
route is advertised. These are author-maintained pins, not floating dependencies.

{{snippet:batch}}

Pass separate output with at least the input length, or use the same-start span
for in-place output. Shifted overlapping spans are rejected. Digits zero and
nonzero, all five modes, independent and in-place double spans, float controls,
short capacity, negative digits and invalid modes are executed against both real
packages. Unused output tail stays untouched. Capacity/mode failures in these
checks leave the destination untouched; this is not a general transactional API
promise. Nonzero digits use the scalar type's rounding, so the runtime matters
as well as the package. No acceleration/performance claim follows.

## Migration

1. Inventory stored baselines and downstream consumers. Choose binary measurement,
   decimal source quantity or the explicitly bounded scale policy for each field.
2. Keep the source type and rounding mode visible at the boundary. Do not cast a
   binary value to decimal and assume that recovers the original decimal text.
3. Update only expectations for the selected policy. Run the midpoint, sign,
   large-value and digits-range controls on the application's actual runtime.
4. For batch exports, pin the package, check output ownership/capacity and compare
   real scalar and span results. Recheck inputs before changing dependency pins.

The explicit scale policy can still lose precision and rejects nonfinite/scaling
overflow here. Its 0–15 bound is an application choice. It does not reproduce
every historical runtime's magnitude guard, rounding, signed-zero or overflow
behaviour. Large finite values that the runtime leaves alone may be rejected by
this helper. Preserve bit patterns separately when the protocol requires them.

## Constraints

The recipe's primary route is .NET 11 on CoreCLR. Reproduction pins are SDK
10.0.402 / runtime and reference pack 10.0.13 / C# 14, and SDK
11.0.100-rc.2.26504.105 / runtime and reference pack 11.0.0-rc.2.26504.105 /
C# 15. Those exact SDKs are test artifacts, separate from consumer release-family
requirements. Scalar APIs need no package. The optional tensor package is pinned
above. Actual selected types/rounding methods have no Experimental attribute and
compile without opt-ins. No provider, rendering mode, workload, native library,
account or service is required.

## Evidence

Saved compile-and-run evidence records exact commands, input hashes, locked
dependencies, runtime-parsed inputs, results and compiler/reference/runtime/package
hashes. Windows x64 CoreCLR JIT is measured; interpreter, Native AOT, Mono and
other OS/architecture execution are not established. RC2 is not actual-GA proof.
The .NET 10 comparison is not a ninth foundation or a target10 catalogue edition.
Comprehensive coverage, actual-GA reconciliation and existing Human gates remain open.

## Official references

- [Pinned Math/MathF compatibility note](https://github.com/dotnet/docs/blob/a109e77898150ddb66e3000ce315d23fcc2700e9/docs/core/compatibility/core-libraries/11/math-round-digits.md).
- [Package-matched TensorPrimitives.Round implementation](https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/runtime/src/libraries/System.Numerics.Tensors/src/System/Numerics/Tensors/netcore/TensorPrimitives.Round.cs).
