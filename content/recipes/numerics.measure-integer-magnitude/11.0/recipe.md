# Compute integer decimal magnitude without rounding

## Goal

A batch encoder budgets decimal digits for nonnegative integer sequence numbers.
Compute the integer exponent without converting a large integer to floating
point or allocating a decimal string. The format rejects negative numbers and
uses one digit for zero. Those are application rules, not a character-formatting
contract supplied by Log10.

## Older approach

An original .NET 10 helper counts divisions while the value is still at least
ten. Add `using System.Numerics;`:

{{snippet:older}}

`BatchMagnitude.Exponent((Int128)999)` returns 2; `Width` returns 3. The loop
stays exact for integers beyond the range where a floating conversion preserves
every unit. Retaining it is reasonable when keeping the older target.

## Newer approach

.NET 11 adds integer Log10, reached generically through IBinaryInteger:

{{snippet:newer}}

Add `using System.Numerics;`. `T.Log10(value)` requires
`where T : IBinaryInteger<T>`; do not call a static virtual operation on the
interface type itself. Concrete methods are also available:
`int.Log10(999)`, `Int128.Log10(Int128.MaxValue)` and
`UInt128.Log10(UInt128.MaxValue)` are executed by the sample.

For positive input the result is the largest integer exponent e such that
`10^e <= value`. Thus 9 maps to 0, 10 to 1 and 999 to 2. Zero returns zero by
this API's convention, and negative signed input throws ArgumentOutOfRangeException.
This is not a rounded real logarithm. Adding one yields this format's decimal
digit budget; it does not count signs, separators, padding or other formatting.

## Migration

1. Set the input policy: this batch format accepts zero and positive values,
   rejects negative values, and gives zero one digit. If a different format needs
   signs, define that separately; `Abs(Int128.MinValue)` cannot fit in Int128.
2. Target net11.0, import System.Numerics, and include the complete current helper
   in its own source file. Use the generic constraint for shared integer code.
3. Replace only the integer exponent loop with `T.Log10`. Keep the digit-budget
   wrapper and its checked conversion to the application's int budget.
4. Check values just below, at and above each relevant power of ten, including
   the largest representable values. Never route those inputs through double.
5. Verify zero and rejected negative values at the application boundary. Keep
   actual formatting separate if the output also contains a sign or separators.

## Constraints

Primary route: net11.0/.NET 11 framework APIs and canonical C# 15 sample mode.
No NuGet dependency, provider, workload, rendering mode, external service or
experimental opt-in. Reproduction pins: SDK 11.0.100-rc.2.26504.105,
reference/runtime 11.0.0-rc.2.26504.105; comparison SDK 10.0.402, C# 14,
reference/runtime 10.0.13. Requirements are release-family prerequisites;
the exact pins identify actual proof, not a requirement for that precise SDK.

The executed generic types are Int128, UInt128 and int. Both 128-bit maxima have
exponent 38 and digit budget 39; inputs retain every bit. Negative inputs,
including Int128.MinValue, are rejected, not silently treated as an absolute
magnitude. Custom IBinaryInteger implementations may override the default; no
custom-type, unbounded digit-budget, other-platform or performance claim follows.
There is no older-target Log10 route or additional .NET 10 foundation.

## Evidence

`eng/Test-IntegerFormats.ps1 -Record -VerifySources` compiles/runs both actual
dependency sets. Independent BigInteger-generated inputs surround every power
of ten from 10 through 10^38, with exact conversions back to the original value.
Checks cover zero, one, both 128-bit maxima, -1 and signed minimum, generic
reachability, concrete methods and exact exceptions. The samples never convert
the input to floating point or obtain the expected exponent from the same loop.

The existing sample records retain commands, dependencies, actual runtime,
input hashes, measured compiler/CoreLib and audit-matching references on Windows
x64. Versioned official declarations/implementations qualify the API contract.
Real installed offline list/get checks complete canonical Markdown, ID/title/goal,
target11/edition11, applicability and current evidence. RC2 does not establish GA
or other-platform execution; comprehensive PIN-959, GA PIN-962 and existing Human
gates stay open. No recurring native/client matrix is added.
