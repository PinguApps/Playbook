# Preserve colour channel order across binary boundaries

## Goal

A fictional harbour signal legend exchanges four distinct colour components with
a binary format. Preserve each component while switching RGBA to ARGB and choosing
the format's packed integer convention. This is representation, not colour processing.

## Older approach

.NET 10 can use application-owned value records and explicit shifts. It does not
supply the new System.Numerics.Colors types. Keep a valid existing representation
when retaining that target. These complete original types live in `SignalFormats`:

{{snippet:older-types}}

The legend's caller constructs RGBA, reorders into ARGB, copies four components
and returns both packed conventions:

{{snippet:older}}

## Newer approach

.NET 11 supplies Rgba<T>, Argb<T> and byte-component packed conversion helpers.
Replace the application-owned types with the framework types, using the complete
helper below in its own source file:

{{snippet:newer}}

For R=0x12, G=0x34, B=0x56, A=0x78, RGBA big-endian is 0x12345678 and
little-endian is 0x78563412; ARGB big-endian is 0x78123456 and little-endian
is 0x56341278. `ToArgb` and `ToRgba` reorder without changing components.
`Rgba.CreateBigEndian`/`CreateLittleEndian` and the corresponding Argb methods
invert the respective uint encodings. Those static helpers take uint and return
byte-component values; they do not pack float components into a uint.

These numbers specify byte significance independently of host endianness. A uint
is not itself a wire byte array: use the binary format's explicit endian writer
when serializing it. Do not apply host-native byte copying and assume the same bytes.

## Migration

1. Identify the format's component sequence and uint convention separately. Use
   distinct values for all four channels; all-zero or grey inputs hide swaps.
2. Target net11.0 and import System.Numerics.Colors. Add the complete current
   helper; remove only the application-owned types whose callers it replaces.
3. Construct Rgba<T> from R,G,B,A or Argb<T> from A,R,G,B. Their generic component
   constraint is `struct`; the checks demonstrate byte and float, not conversion
   between those types. Choose ToArgb/ToRgba for sequence changes.
4. Select the byte-only big-/little-endian helper required by the format. Verify
   both known packed words and inverse creation, rather than just a self-round-trip.
5. Pass at least four elements to the span constructor and CopyTo. Larger inputs
   consume the first four; a larger destination retains its remaining elements.
   Inputs/destinations of length 0 through 3 throw ArgumentException before copying.
6. Use Equals or EqualityComparer for component equality. Equal same-type values
   have equal hashes; do not persist hashes, demand different hashes for unequal
   values, or assume RGBA and ARGB objects compare equal across their types.

## Constraints

Primary route: net11.0/.NET 11 framework APIs, canonical C#15, Nullable and
ImplicitUsings enabled. No NuGet package, package asset, provider, workload,
rendering mode, native drawing API, device or experimental opt-in is needed.
The older C#14/net10.0 implementation remains a fair alternative. There is no
advertised framework-colour route on net10.0 and no new .NET10 foundation.

The generic structures store components verbatim: float values -2.5, 1.25, 300
and 0.5 survive construction, copy and reordering. They do not normalize or clamp
ranges, convert colour spaces, premultiply alpha, blend, or validate a display
format. Equality uses component equality, not perceptual colour equivalence.
Only byte components have the demonstrated packed uint helpers. MAUI colour
inheritance migration and native painting remain separate goals.

Exact reproduction: SDK10.0.402/C#14/reference-runtime10.0.13 versus
SDK11.0.100-rc.2.26504.105/C#15/reference-runtime11.0.0-rc.2.26504.105.
These identify measured artifacts; the route requires the release family rather
than that exact SDK patch. Execution was Windows x64 only. Other-platform,
performance and final-GA execution are not claimed.

## Evidence

`eng/Test-ChannelExtrema.ps1 -Record -VerifySources` builds/runs both original
SignalLegend consumers through the existing evidence recorder. Distinct channel
assertions establish the four known words and inverse constructors. Four packed
inputs include zero, mixed bytes and uint maximum. Byte and float checks cover
reorder, lengths 0..5, unchanged short-copy destinations and untouched tails,
component/object equality and HashSet/equal-hash consistency.

Saved records retain exact commands, dependencies (none), SDK/compiler/CoreLib
identity, actual runtime and input hashes. Selected reference hashes match the
original data/I/O audit; pinned official sources qualify availability and limits.
Ordinary installed stdio list/get checks full canonical Markdown, ID/title/goal,
explicit target11/edition11, applicability and fresh evidence offline. RC2 evidence
is not GA verification; PIN-959, PIN-962 and existing Human gates remain open.
