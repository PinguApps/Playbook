# Preserve deliberate numeric conversion intent

## Goal

Choose decimal source values, binary analysis values or an agreed legacy significant-digit representation when runtime or SDK upgrades change numeric conversion results.

Use an SDK console project, implicit System imports and namespace Playbook.Samples. Import System.Globalization and System.Numerics. Older examples target net10.0 / C# 14; newer examples target net11.0 / C# 15. The partial class can be used on its own for this goal.

## Older approach

A fictional import utility receives binary sensor measurements and separately reads invoice text. The same explicit casts, invariant parsing and ties-to-even calls below are valid .NET 10 APIs. On the measured older runtime a double parsed from "2.71" converts to decimal 2.71, reflecting its older significant-digit conversion. Do not mistake that result for evidence that the binary input was exactly decimal 2.71.

{{snippet:older}}

## Newer approach

Keep decimal source text in InvoiceAmount, or start with a decimal literal such as 2.71m. Use FromBinary only when the actual binary source value is the intended input: the measured conversion yields 2.709999999999999964472863212. RoundInvoice specifies two decimal places and ties to even; 8.125 becomes 8.12 and 8.135 becomes 8.14 on both targets. ApproximateCount makes the intentional loss of arbitrary-integer precision visible.

{{snippet:newer}}

## Preserve an agreed archive representation

A fictional instrument archive has an explicit interchange policy: seven significant decimal digits for a float reading and fifteen for a double reading. A separate analysis path uses the actual binary source; original decimal calibration text never passes through a binary type. These are three different intentions. The following complete helper compiles and runs on both .NET 10 and .NET 11; making the archive policy explicit was already valid on .NET 10.

{{snippet:policy}}

`LegacyReading` uses invariant `G7` / `G15` formatting, including exponent notation, then parses with `NumberStyles.Float`. Significant digits span the whole quantity; they are not seven or fifteen decimal places. Formatting rounds nearest with ties to even and treats signs symmetrically: double 123456789012344.5 becomes 123456789012344, while 123456789012345.5 becomes 123456789012346; their negative counterparts mirror those results. The float controls use 1234566.5 and 1234567.5. These binary-exact halfway inputs distinguish ties-to-even from truncation or rounding away from zero.

The helper deliberately throws `ArgumentOutOfRangeException` for NaN or either infinity, and `decimal.Parse` throws `OverflowException` when the formatted finite value is out of range. Tiny values can round to decimal zero; decimal's scale is at most 28. The policy does not preserve signed zero, a decimal scale/byte encoding, NaN payloads or every historical boundary quirk. It can lose source precision by design. Keep the original source or agreed text when those distinctions matter. This original allocating implementation favors clarity; it makes no allocation/performance claim.

## Check conversion families and compiler constants

Parsed runtime-input checks exercise constructors, casts, `Convert`, relevant `CreateChecked` / `CreateSaturating` / `CreateTruncating`, and `IConvertible` routes for float/double-to-decimal and decimal-to-float/double. Half's explicit and generic routes are checked independently. Generic modes agree for these finite in-range fixtures; this does not promise identical overflow/nonfinite behavior for every generic mode.

The original decimal fixtures expose three different intermediate-rounding changes:

| Source decimal | .NET 10 destination bits | .NET 11 RC2 destination bits |
| --- | --- | --- |
| 3000000000000.015625 to double | 4285D3EF79800021 | 4285D3EF79800020 |
| 3.000000119209289550781250001 to float | 40400000 | 40400001 |
| 5.00195312501 to Half | 4500 | 4501 |

Negative controls mirror the signs. These are bounded observations, not a proof of universal single rounding: the pinned Half implementation still converts decimal through **double**. Decimal 5.001953125000000000000000001 converts to Half 4500 on both artifacts even though its exact value lies above the midpoint. Retain that known limitation when exact nearest Half is required; neither a newer target nor this recipe supplies an unavailable general correction.

For integer counts, 2^90 + 2^37 + 1 changes double bits from 4590000000000000 to 4590000000000001; 2^70 + 2^46 + 1 changes float bits from 62800000 to 62800001. Both signs and advertised generic routes are executed. BigInteger-to-Half has finite, ties-to-even and overflow controls: 16393 rounds to 16400 on **both** runtimes, 2049/2051 round to 2048/2052, 65504 remains finite and 65520 becomes infinity. All integers in the finite Half range are exactly representable in double, so no fictional changed finite result is offered for that conversion.

Only net11 exercises actual BFloat16: decimal 5.01562500001 and BigInteger 2^70 + 2^62 + 1 convert to bits 40A1 and 6281, with decimal generic-route controls. There is no .NET 10 BFloat16 equivalent in this comparison. Exact protocol bits belong to the existing `numerics.roundtrip-float-bits` guidance; do not recreate an old bit pattern by relying on an approximate numeric conversion.

The same source also contains real compile-time conversions:

{{snippet:constants}}

Both net10 consumers use C# 14 and reference pack 10.0.13. SDK 10 embeds NominalReading 6.43 and NominalScale bits 4285D3EF79800021; SDK 11 RC2 embeds 6.429999999999999715782905696 and 4285D3EF79800020. Both assemblies then execute on the **actual .NET 10.0.13 runtime**, where the independent no-inline parsed-input conversion still yields 6.43 and bits 4285D3EF79800021. Reflection reads the embedded fields separately (`GetRawConstantValue` for double; the decimal constant attribute through field reflection). This is execution, not cross-compilation or an assumption that target framework determines constant folding. Building with a new SDK can change archived expectations without changing the target or deployed runtime.

## Migration

1. Classify each input as decimal text/literal, binary measurement or exact integer; avoid converting decimal source through double.
2. Retain direct invariant decimal parsing or m-suffixed literals for decimal intent; use a numeric conversion when preserving the binary source is intentional.
3. If an agreed legacy decimal representation is required, call the explicit G7/G15 helper and test range, nonfinite, small/large and tie boundaries. Retain original decimal source text/literals for decimal intent.
4. Specify decimal display digits and MidpointRounding separately. Do not confuse decimal places with significant digits or a numeric value with protocol bits.
5. Rebuild the same older target with each SDK, compare embedded constants, then run each assembly on the actual older runtime. Keep runtime-input conversions separate. Re-run stored expectations after either upgrade; there is no old-conversion compatibility switch.

## Constraints

The compatibility note describes correctly rounded conversions; the concrete families and retained Half limitation above qualify that broad claim against the actual RC2 assets. The earlier sample also converts 2^80 + 2^27 + 1 to double: older bits end in 0000, newer bits end in 0001. Floating destinations still lose precision. Direct NaN/infinity-to-System.Decimal casts throw OverflowException; the archive helper instead explicitly rejects nonfinite input before formatting. Its significant-digit policy is not a general recreation of every historical algorithm or representation.

The newer route uses framework-provided managed APIs on .NET 11; no additional package/assets, workload, provider, UI rendering context or experimental opt-in is required. Language mode 15.0 and SDK 11 are the current sample configuration, not a claim that the archive helper requires new syntax. The ordinary comparison and SDK11/net10 constant experiment actually execute on .NET 10. They do not make BFloat16 available there or add a ninth .NET 10 foundation. No older-target package route is advertised.

## Evidence

Real target executions assert the original decimal/BigInteger values plus all bounded archive, destination-width and rebuild checks above. This is not an exhaustive conversion audit, a Math.Round migration, a protocol bit-encoding implementation or an actual-GA claim.

Canonical samples are samples/net10.0/NumericGuidance and samples/net11.0/NumericGuidance with shared NumericGuidanceChecks.cs. Saved evidence records compile-and-run with SDK 10.0.402 / C# 14 and SDK 11.0.100-rc.2.26504.105 / C# 15, on runtimes .NET 10.0.13 and .NET 11.0.0-rc.2.26504.105. Their exact inputs, commands and outputs accompany this recipe. RC2 evidence is not GA proof; no mocks, native devices, providers, AOT or general performance guarantees are claimed.

The added canonical ArchiveNumbers/ArchiveChecks files are compiled by net10.0/InstrumentArchive, net11.0/InstrumentArchive and net10.0-sdk11/InstrumentArchive. The recorder captures exact compiler and selected reference-pack hashes, actual runtime assembly hashes, commands, inputs and empty package closures; SDK11's net10 reference pack is actually resolved from its restored 10.0.13 targeting pack. Supported consumer release families are separate from those reproducibility pins. Comprehensive publication, actual-GA reconciliation and existing Human gates stay open.

- [Pinned official conversion mitigation](https://github.com/dotnet/docs/blob/a109e77898150ddb66e3000ce315d23fcc2700e9/docs/core/compatibility/core-libraries/11/decimal-biginteger-floating-point-conversions.md).
- [Pinned Half implementation](https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/runtime/src/libraries/System.Private.CoreLib/src/System/Half.cs).
