# Validate tensor clamp bounds consistently

## Goal

A fictional calibration importer accepts one lower/upper pair for a batch, or one
pair per reading. Reject any pair that fails `lower <= upper` **before writing any
output**. Library exception behavior is not the importer's validation policy.

## Older approach: .NET 10

Use an SDK console project with `net10.0`, C# 14, implicit usings and nullable
enabled. Add `System.Numerics.Tensors` **10.0.13**. The actual package selects
`lib/net10.0/System.Numerics.Tensors.dll`. Put this complete class in its own file:

{{snippet:older}}

This explicit check remains valid. Do not replace it with a try/catch around
`TensorPrimitives.Clamp`: some older vectorized calls already accept reversed
bounds. Checking each element while writing output also fails the all-or-nothing
validation requirement when an invalid pair occurs late in a batch.

## Newer approach: .NET 11

Use `net11.0`, the sample's C# 15 mode, the same project settings and
`System.Numerics.Tensors` **11.0.0-rc.2.26504.105**. Its selected asset is
`lib/net11.0/System.Numerics.Tensors.dll`. Keep the same explicit contract:

{{snippet:newer}}

For example, readings `[-2, 2, 4]` with bounds `[1,3]` produce `[1,2,3]`.
Equal bounds `[1,1]` produce all ones. A reversed scalar pair `[4,3]`, or a
reversed pair at the final reading, throws before any output element changes.
The helper checks lengths/capacity first. `!(lower <= upper)` also rejects NaN
bounds for float; it does not reject NaN readings or define a finite-only policy.

The Preview 7 headline needs a type qualification. Vectorizable float now uses
`Min(Max(x,lower),upper)` even for short scalar processing; nonvectorizable decimal
still delegates to `decimal.Clamp` and can throw. These are actual measured calls
with every pair reversed `[4,3]`, not a promise for all implementations:

| Raw call / input length | Tensor 10.0.13 | Tensor 11 RC2 |
| --- | --- | --- |
| float, scalar bounds, 1 / 3 / 33 | ArgumentException | accepts; writes 3 |
| float, per-element bounds, 1 / 3 | ArgumentException | accepts; writes 3 |
| float, per-element bounds, 33 | accepts; writes 3 | accepts; writes 3 |
| decimal, either bound form, 1 / 3 / 33 | ArgumentException | ArgumentException |

Lengths 1 and 3 exercise short processing; 33 is not a multiple of the supported
float vector widths and covers a batch with a remainder. Actual machine evidence
records hardware capabilities. The guide does not infer a particular generated
instruction sequence or acceleration from input length. The supplied validation
has the same result for both types and all lengths.

## Migration

1. Identify whether calibration bounds are scalar or per reading. Retain the
   source values; do not silently swap or clamp invalid bounds into a different policy.
2. Validate capacity, bound lengths and **every** relevant pair before calling
   the library. Equal bounds are valid. Choose any additional finite-value policy explicitly.
3. Pass independent destination storage with enough capacity. The shown helper
   does not bypass the library's shifted-overlap restrictions or snapshot concurrent inputs.
4. Check reversed, equal and ordered bounds on the application's exact package,
   with short and remainder-bearing spans and an invalid final pair. Assert both
   untouched failed output and correct successful values, including untouched output tail.

An explicit scalar loop is an alternative when a tensor dependency is unnecessary;
it still needs the complete validation pass before writes. This recipe advertises
only the two shown float/decimal span-input overloads. Mixed scalar/span bounds,
other numeric types, concurrent mutation and newer-package-on-net10 routes need
their own checks. The unchanged-output claim concerns these validation failures,
not a general transactional guarantee for arbitrary exceptions or custom numbers.

## Constraints

Primary applicability is .NET 11 with the exact tensor package above; use a .NET 11
SDK/reference pack. Reproduction pins are SDK10.0.402 / C#14 / runtime-ref10.0.13
and SDK11.0.100-rc.2.26504.105 / C#15 / runtime-ref11RC2. These exact artifacts
are test baselines, separate from release-family SDK/runtime requirements. No
provider, native library/toolchain, workload, rendering mode, account or service
is required. The selected TensorPrimitives type/methods have no Experimental
annotation and require no opt-in. Sample dependencies remain author-maintained.

## Evidence

Both actual packages compile and execute scalar/per-element float and decimal
controls at lengths 1/3/33. Reversed/equal/ordered pairs, an invalid final pair,
wrong lengths, short output and NaN bound checks assert untouched rejected output,
correct clamped results and unused tail. Raw exception types and complete inputs/
outputs are captured separately from the corrected policy assertions. Compiler,
reference, runtime and selected package assets have exact identities/hashes in
the existing sample evidence records, refreshed by `eng/Record-CalibrationHost.ps1`.

Only Windows x64 CoreCLR JIT execution is established. Other platforms/engines,
newer package on an older target and actual-GA execution are not verified. RC2
evidence does not close PIN-962 or comprehensive PIN-959 coverage. The older
comparison adds no target10 edition or ninth foundation. Existing Human gates remain.

## Official references

- [Pinned Preview 7 library note](https://github.com/dotnet/core/blob/ffed591b0fb8f9aab50ecc28f2c765049fb28d8f/release-notes/11.0/preview/preview7/libraries.md).
- [Package-matched Clamp implementation and type qualification](https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/runtime/src/libraries/System.Numerics.Tensors/src/System/Numerics/Tensors/netcore/TensorPrimitives.Clamp.cs).
- [Pinned tensor declarations](https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/runtime/src/libraries/System.Numerics.Tensors/ref/System.Numerics.Tensors.netcore.cs).
