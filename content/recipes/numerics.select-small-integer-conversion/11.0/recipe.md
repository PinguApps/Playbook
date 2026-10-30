# Select a small integer conversion policy

## Goal

A fictional survey recorder stores a sample's offset in two bytes. Decide whether
an out-of-range number should saturate to a bound, be rejected, or deliberately
lose high integer bits. Conversion alone does not establish that an offset fits
the current record.

## Older approach: .NET 10

Create an SDK console project targeting `net10.0`, `LangVersion=14.0`,
`ImplicitUsings=enable` and `Nullable=enable`. Add this `OffsetPolicy.cs` class.
It needs no NuGet package.

{{snippet:older}}

The explicit saturation policy remains valid. It handles NaN/infinities before a
checked small conversion. Historical unchecked direct casts could use an int
conversion followed by narrowing; out-of-range/NaN results were not a portable
contract. Do not encode one machine's old sentinel or low bits as that contract.

## Newer approach: .NET 11 CoreCLR

Use `net11.0`, the sample's C# 15 mode and the same console settings. This class
keeps rejection and buffer bounds explicit while allowing the runtime's small
cast to implement saturation.

{{snippet:newer}}

Finite fractions truncate toward zero. Below/above-range values, including
infinities, saturate at the following bounds; NaN becomes zero:

| Destination | Minimum | Maximum |
| --- | --- | --- |
| sbyte | -128 | 127 |
| byte | 0 | 255 |
| short | -32768 | 32767 |
| ushort | 0 | 65535 |
| char (numeric code unit) | 0 | 65535 |

The change covers unchecked double/float casts and corresponding Half operators
that convert through float. `double.ConvertToInteger<TInteger>` and
`float.ConvertToInteger<TInteger>` agree for these destinations on the measured
current runtime. Both ConvertToInteger and ConvertToIntegerNative APIs already
exist in the previous reference set; this is not their introduction.
The two `Convert` overloads demonstrate the double/float generic APIs. For a
float native policy use `float.ConvertToIntegerNative<ushort>(value)` instead.

Use a numeric char value only when the data contract calls for a UTF-16 code unit;
saturation does not validate text. Checked casts still reject overflow with
OverflowException. They truncate representable fractions: a checked cast alone
can accept a small negative fraction as zero. `CheckedOffset` therefore rejects
negative and nonfinite raw inputs first, rejects representation overflow, then
checks the converted result against the actual record length. An offset equal to
that length is invalid. The checked policy never silently repairs an invalid slot.

## Choose the intended alternative

`Native` asks the runtime for efficient platform-native conversion.
`ViaSaturatedInt` chooses saturation to int followed by integer narrowing;
`ViaNativeInt` chooses native int conversion followed by narrowing. These are
different policies. Neither native route guarantees all historical outputs across
runtimes, architectures, hardware revisions or instruction sets. Integer narrowing
itself is unchanged, as are the earlier wider-integer/vector migration boundaries.

The original runtime-parsed `70019` control produces 65535 with the explicit
ushort saturation policy and 4483 with the explicit saturated-int/narrowing policy
on both tested dependency sets. The native result is recorded, not prescribed.
The record writer actually stores byte 81 at offset 23 for input `23.875` and
length 24. It rejects input 24, negative fractions, NaN, infinities and zero length;
70019 is representation overflow even if a larger buffer exists.

## Migration

1. Identify which conversions feed storage, indices or lengths. Select saturation,
   checked rejection, native conversion or explicit intermediate/narrowing per field.
2. Replace accidental reliance on historical unchecked overflow with the selected
   operation. Retain the working explicit .NET 10 policy where appropriate.
3. Validate raw values when malformed data must fail, then validate the final
   offset against the actual buffer. Do not use NaN-to-zero as data validation.
4. Run the destination matrix on the actual deployment runtime. Include in-range
   fractions, both bounds and adjacent overflow values, both infinities and NaN.
   Compare native observations without asserting them as a portable baseline.

## Constraints

Primary applicability is .NET 11 **CoreCLR**; samples use managed framework APIs
with no added package/assets, native toolchain, workload, provider, rendering mode
or opt-in. No publisher experimental label applies to these selected operations.
The official note also names CoreCLR's interpreter and Native AOT, and explicitly
excludes Mono from this change. Only Windows x64 CoreCLR JIT was actually run
here. Interpreter, Native AOT, Mono, other OS/architecture and hardware paths need
their own actual runs before claiming verification; this recipe does not establish
them or offer Mono saturation through the direct-cast implementation.

## Evidence

Paired artifacts: SDK10.0.402, runtime/ref10.0.13 and C#14 versus
SDK11.0.100-rc.2.26504.105, runtime/ref11.0.0-rc.2.26504.105 and C#15.
These are exact reproduction baselines, separate from consumer .NET11 SDK/runtime
release-family requirements. Saved compilation/execution evidence contains commands,
inputs, measured OS/architecture and compiler/reference/runtime hashes. All five
destinations execute double/float plus representative Half values (including Half's
own finite-range/precision limits), checked overflow, conversion APIs and real
buffer validation. Legacy out-of-range outputs remain measured observations.

RC2 evidence is not GA reconciliation. The previous consumer is a comparison,
not a new target10 catalogue foundation. Comprehensive coverage, actual-GA and
existing Human gates remain open.

## Official reference

- [Pinned small integer compatibility note, platform boundary and alternatives](https://github.com/dotnet/docs/blob/a109e77898150ddb66e3000ce315d23fcc2700e9/docs/core/compatibility/jit/11/fp-to-small-integer.md).
