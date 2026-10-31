# Choose integer division and remainder rules

## Goal

A coordinate tiler assigns negative as well as positive positions to tiles. Choose
the quotient convention deliberately, then keep the corresponding remainder.
For example, `-7 / 3` with Floor gives tile `-3`, offset `2`; truncation gives
tile `-2`, offset `-1`. Both satisfy `a = q*b + r`, but describe different grids.

## Older approach

.NET 10 has generic integer `DivRem` with truncation. Adjust its quotient and
remainder together. This original helper also validates the application's mode
before doing arithmetic. Add `using System.Numerics;` to the file.

{{snippet:older}}

Use `TileDivision.Split(-7, 3, TileRounding.Floor)`. The enum belongs to this
application, not .NET 10. This remains a valid approach for applications retaining
that target; no floating-point conversion or Math.Round is needed.

## Newer approach

.NET 11 supplies `DivisionRounding` and static virtual interface operations.
Keep the application's upfront validation, then delegate the adjustment:

{{snippet:newer}}

Add `using System.Numerics;`. Call
`TileDivision.Split(-7, 3, DivisionRounding.Floor)`. The operation is reached
through `T.DivRem` under `where T : IBinaryInteger<T>`; do not call it on the
interface type itself. The three-argument default is not exposed as
`int.DivRem(a,b,mode)` in these assets. Use a constrained generic helper even
for an int caller. `T.Divide(a,b,mode)` returns only the quotient and
`T.Remainder(a,b,mode)` only the remainder; both use the same constraint.
The extra `ISignedNumber<T>` on this application helper restricts its domain to
signed coordinates, rather than promising all rounding results fit an unsigned T.

| Mode | Quotient | Nonzero remainder | `-7,3`: `(q,r)` | `-7,-3`: `(q,r)` |
| --- | --- | --- | --- | --- |
| Truncate | Toward zero | Same sign as dividend | `(-2,-1)` | `(2,-1)` |
| Floor | Toward negative infinity | Same sign as divisor | `(-3,2)` | `(2,-1)` |
| Ceiling | Toward positive infinity | Opposite sign to divisor | `(-2,-1)` | `(3,2)` |
| AwayFromZero | Away from zero | Opposite sign to dividend | `(-3,2)` | `(3,2)` |
| Euclidean | Makes remainder nonnegative | `0 <= r < abs(b)` | `(-3,2)` | `(3,2)` |

For every mode, exact division has remainder zero. Otherwise `abs(r) < abs(b)`.
Use a wider integer for verification of `q*b+r` and `abs(b)` at fixed-width
boundaries; those intermediate calculations can themselves overflow.

## Migration

1. Write the grid's rule for negative coordinates and divisor direction. Floor
   often fits positive tile widths; Euclidean gives nonnegative offsets even if
   the divisor is negative. Do not pick a mode just from its name.
2. Target net11.0 and build against its reference pack. Put the complete current
   helper in a separate source file with System.Numerics imported.
3. Replace the application enum with DivisionRounding at each call and retain
   validation if the value can come from configuration, casting or deserialization.
4. Replace manual paired adjustments with `T.DivRem`. Use a quotient-only or
   remainder-only helper only when that is the actual contract needed.
5. Check all operand signs, exact divisions, zero divisor and fixed-width overflow.
   Compare both outputs and the widened arithmetic identity before removing the
   old helper. Test unsupported mode values on exact and inexact divisions.

## Constraints

Primary route: net11.0/.NET 11 framework APIs, canonical C# 15 sample mode and
generic math. No package, provider, workload, rendering mode or experimental
opt-in. Actual verification uses SDK 11.0.100-rc.2.26504.105 and reference/runtime
11.0.0-rc.2.26504.105; previous consumer uses SDK 10.0.402, C# 14 and
reference/runtime 10.0.13. These are reproduction pins, separate from the
release-family prerequisites. No .NET 10 catalogue route or ninth foundation.

The executed int zero-divisor case throws DivideByZeroException.
DivRem of signed minimum divided by -1 throws OverflowException for int, long
and Int128: its positive quotient cannot fit. The helper does not convert that
failure into a wrapped tile. Generic custom implementations can override the
static operations; the constraint alone cannot certify their semantics.

In the executed RC2 default, an unsupported mode on `6/3` returns `(2,0)` because
the zero-remainder shortcut precedes enum validation; `7/3` throws
ArgumentException. Our helper rejects both first with ArgumentOutOfRangeException.
Do not rely on the raw shortcut as a supported enum value. Unsigned evidence is
limited to raw `uint` truncation `7/3 = (2,1)`; unsigned adjusted remainders,
custom integers and other-platform execution are not advertised here.

## Evidence

`eng/Test-IntegerFormats.ps1 -Record -VerifySources` captures both real consumers
through the existing Record-Evidence workflow. The shared checks execute int,
long, Int128 and BigInteger over five modes and all operand signs, compare an
independent small-value quotient oracle, and verify the identity in BigInteger.
Current generic Divide/Remainder agree with DivRem. Failures cover zero divisor,
three signed-minimum overflows and invalid enum values on exact/inexact inputs;
the raw RC2 shortcut and uint truncation are executed separately.

Saved records contain commands, inputs, exact dependencies, runtime location,
compiler/CoreLib hashes and audit-matching reference hashes on Windows x64.
Pinned official declarations and default implementation resolve availability and
reachability. The ordinary locally installed package is inspected through real
stdio list/get for ID/title/goal, full canonical Markdown, explicit target11 /
edition11, applicability and fresh evidence. RC2 is not GA proof; PIN-959 and
PIN-962 remain open, alongside existing Human gates. No recurring platform matrix.
