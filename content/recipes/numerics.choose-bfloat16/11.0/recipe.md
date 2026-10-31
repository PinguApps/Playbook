# Choose reduced-precision binary values deliberately

## Goal

A fictional ridge sensor archive can tolerate small reading errors but must retain
very large and very small magnitudes. Choose the storage format and the calculation
precision separately, then measure the archive's error budget against the inputs.

## Older approach

.NET 10 has float and Half, but no framework System.Numerics.BFloat16 type.
Keeping float is a valid choice when Half's exponent range is too small or when
the application cannot afford reduced precision. This complete original helper
stores the input floats and calculates their mean:

{{snippet:older}}

Float uses 32 bits, with 24 significant binary digits (including the implicit
leading digit for normal values). Half uses 16 bits with 11 significant digits
and a 5-bit exponent. For the measured inputs, converting 1e30 to Half gives
infinity and 1e-30 becomes zero. Do not substitute Half solely because it also
occupies 16 bits. There is no custom bit-encoding workaround in this comparison.

## Newer approach

.NET 11 supplies the 16-bit BFloat16 type: an 8-bit exponent and 8 significant
binary digits for normal values. Its exponent range resembles float's, with
much less precision. Both conversion directions below require explicit casts.
Widen stored values to float before accumulation:

{{snippet:newer}}

The generic numeric interfaces also support BFloat16. Place this complete helper
in a separate source file:

{{snippet:generic}}

Calling Add<BFloat16> rounds the addition back to BFloat16. In the executed check,
1 + converted 0.001 becomes 1, whereas Add<float>(1, 0.001f) is greater than 1.
Exact BFloat16 inputs 1.5 and 2.25 add to 3.75. Generic constraints make an operation
available; they do not grant float precision to a reduced-precision calculation.

## Migration

1. Decide the range and allowed absolute/relative error from the application's
   real data. Preserve float if reduced precision cannot satisfy that requirement.
2. Target net11.0 and add the two complete newer helpers with System.Numerics
   imports. The console project uses Nullable and ImplicitUsings. No package is needed.
3. Convert readings explicitly to BFloat16 at the storage boundary. Conversion
   rounds to nearest with ties to even: 1.00390625 becomes 1; 1.01171875 becomes
   1.015625. Converting back to float represents the stored value exactly but
   cannot recover the discarded digits.
4. For the sample inputs 12.51, 12.6, 12.7 and 12.8, require per-reading absolute
   error <=0.032 and mean error <=0.02. Stored readings are 12.5, 12.625, 12.6875
   and 12.8125. The wider mean is 12.65625 versus approximately 12.6525, with
   measured error about 0.00375. These budgets qualify this batch, not all inputs.
5. Widen before adding when that suits the calculation. Test operations performed
   in BFloat16 separately; repeated rounding/overflow can dominate the storage error.
   The supplied mean rejects an empty batch. Bound batch size separately: float
   accumulation itself can overflow or lose precision on sufficiently large data.
6. Define nonfinite and underflow policy at the application's input/output boundary.
   The helper intentionally preserves conversion behavior; it is not a validator.
   Check signed zero, infinities, NaN, very small/large values and error budgets
   before replacing an existing representation.

## Constraints

The primary route is net11.0/.NET 11 framework APIs, canonical C#15. The older
net10.0/C#14 float route remains valid. No package asset, native provider, GPU,
ML service, workload, rendering mode or experimental diagnostic opt-in is needed.
The examples use managed arrays; element width alone is not a measured allocation,
throughput, hardware-acceleration or universal performance guarantee.

BFloat16 normal range is approximately 1.17549435e-38 through 3.38953139e38;
the smallest positive subnormal is 2^-133 (about 9.18355e-41). The executed 1e30
and 1e-30 stay finite and nonzero with relative error <=1/256. That bound is for
these normal values, not an underflow-region guarantee. 2^-134 ties to zero and
float.MaxValue rounds to infinity. Exact 1.5, -2.75, the minimum normal and the
minimum subnormal are checked. Positive/negative zero retain their signs;
infinities retain their signs and NaN remains NaN. NaN payload identity, all
conversion source types, JSON and binary serialization are not claimed.
Bit-preserving binary encoding stays with numerics.roundtrip-float-bits; JSON
contracts stay with json.use-numeric-collection-contracts.

Reproduction artifacts are SDK10.0.402/C#14/ref-runtime10.0.13 and
SDK11.0.100-rc.2.26504.105/C#15/ref-runtime11.0.0-rc.2.26504.105. These are
exact evidence pins, separate from release-family prerequisites. Windows x64
execution is measured; other platforms, trimming/AOT and actual GA are unverified.

## Evidence

eng/Test-PrecisionDepth.ps1 -Record -VerifySources builds and executes both
SensorArchive consumers using the existing recorder. Assertions cover exact and
deliberately rounded values, magnitude/range, signed zero/infinity/NaN, generic
arithmetic and the explicit batch calculation budget. Saved records retain input
hashes, commands, resolved dependencies (none), SDK/compiler/CoreLib identity and
runtime output. Selected reference hashes match the original data/I/O audit.

The ordinary tool is built/packed and that exact artifact is installed from an
isolated local-only feed/cache. Real stdio list/get from an unrelated directory
checks ID/title/goal, complete canonical Markdown, target11/edition11, applicability,
current paired evidence and equal structured/text responses. Target10 has no
applicable edition: the comparison adds no ninth foundation. RC2 is not GA proof;
PIN-959, PIN-962 and existing Human gates remain open.
