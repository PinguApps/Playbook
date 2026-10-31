# Bound nesting when reading and writing CBOR

## Goal

A fictional observatory exchanges one calibration value inside nested groups.
Accept at most eight containers and reject a ninth predictably, using the same
finite payload contract when reading and writing CBOR (a binary data format).

## Older approach

The actual System.Formats.Cbor 10.0.13 reader already has a default limit of 64,
backported before this comparison; its writer accepts the tested depth 1001.
The historical .NET 11 breaking note describes an earlier unlimited reader
baseline and therefore does not describe this exact older patch. Neither default
expresses the observatory's limit of eight. The older application checks depth
before opening each container, using these complete original helpers:

{{snippet:older}}

Both releases share the following complete traversal helper in its own source
file. It accepts a scalar Int32 wrapped in any sequence of single-child arrays
or maps whose sole key is "calibration". It reads the whole value, closes every
container and rejects extra members or trailing roots. It is an intentionally
narrow finite-document schema, not an arbitrary CBOR object mapper.

{{snippet:traversal}}

The older explicit budget remains a valid implementation. It can be stricter
than the reader's built-in default; this example does not try to raise that
older default or configure a global compatibility switch.

## Newer approach

System.Formats.Cbor 11 adds per-instance reader/writer options. Configure a finite
MaxDepth on both objects. The existing traversal passes int.MaxValue to its own
guard so the package enforces entry at the selected limit:

{{snippet:newer}}

Keep the common traversal helper too. No default success is inferred from just
constructing an options object: the checks actually read and write the containers.
Reading a ninth container throws CborContentException; writing it throws
InvalidOperationException. The public application helpers accept only nonnegative
finite limits, deliberately rejecting -1 instead of inheriting runtime defaults.

## Migration

1. Inventory the existing finite-payload schema and choose its accepted depth.
   This original schema admits exactly one Int32 leaf inside zero or more
   single-child groups. Eight containers succeed; a ninth must fail. Root scalar
   depth is zero and the first array/map consumes one level.
2. Target net11.0 and reference System.Formats.Cbor 11. Use the complete current
   codec and shared traversal in separate source files with their imports.
   The verified package pin is 11.0.0-rc.2.26504.105, selecting lib/net11.0;
   the older consumer pins 10.0.13 and selects lib/net10.0. Generate/review the
   package lock and restore locked when reproducing the comparison.
3. Carry over conformance, multiple-root and indefinite-encoding choices explicitly.
   These samples use Strict, one root and ConvertIndefiniteLengthEncodings=false.
   Definite and indefinite arrays/maps are accepted under that policy. InitialCapacity=32
   is buffer starting capacity, not a payload maximum or depth limit.
4. Set both MaxDepth values to eight. An options value of -1 selects the runtime
   default: the current reader resolves it to 64 and writer to 1000. Zero permits
   a root scalar but no container; it does not mean unlimited. Values below -1
   throw ArgumentOutOfRangeException at property assignment. No unlimited-depth
   AppContext switch is recommended by this recipe.
5. Handle CborContentException for invalid/deep reads and InvalidOperationException
   for excessive writes as failed operations. Discard failed payloads/partial
   results; the codec returns bytes only after every container closes. Do not catch
   depth failures and return a truncated calibration. Invalid application settings
   throw ArgumentOutOfRangeException separately.
6. Compare accepted bytes and decoded values across both consumers. Exercise
   exact/over-limit depths, maps and arrays, definite/indefinite encodings, malformed
   or truncated data, extra members and trailing roots. Bound incoming bytes and
   string sizes separately before buffering; depth does not limit document width,
   encoded string lengths or all resource consumption.

## Constraints

Primary route: .NET11 SDK/runtime, net11.0/C#15, System.Formats.Cbor 11
lib/net11.0. Exact proof uses SDK11.0.100-rc.2.26504.105 and package/runtime
11.0.0-rc.2.26504.105. The valid older route uses SDK10.0.402, net10.0/C#14,
runtime10.0.13 and package10.0.13 lib/net10.0. Exact SDK patches identify
reproduction; they are not universal consumer requirements.

No cloud account, provider, native asset/toolchain, workload, rendering mode or
experimental opt-in is required. Managed execution is measured on Windows x64.
The package's other target assets, including its newer-package-on-net10 route,
are not executed or advertised here. Actual GA, other-platform and trimming/AOT
behavior remain unverified. This is complete finite-buffer processing;
cbor.read-chunks owns incremental input. Indefinite strings, encoded-value APIs,
multiple roots, canonical sorting and arbitrary CBOR schemas are outside this
sample's claims. Runtime defaults are not an application-specific safety policy.

## Evidence

eng/Test-PrecisionDepth.ps1 -Record -VerifySources executes both actual
CalibrationGroups consumers. Accepted outputs at depth0/1/7/8, maps/arrays and
both definite/indefinite forms match byte-for-byte across releases and decode to
17. Separate checks reject depth9, truncated input, extra members and trailing
roots without returning a partial result. Both raw current objects prove zero
semantics, -1/default resolution, reader64/65 and writer1000/1001 boundaries,
with complete accepted boundary controls and exact exception types. Values below
-1 fail at assignment. The old reader64/65 security-backport boundary and complete
writer1001 control are executed rather than inferred from historical prose.

Saved sample evidence retains commands, resolved package locks, compiler/CoreLib,
runtime and input hashes. Package archive/selected/executed assembly hashes and
reference packs are checked against the original serialization/data-I/O audits.
Immutable implementation sources qualify the documented older-reader drift.

Ordinary build/pack and exact isolated local-feed installation are followed by
real stdio list/get from an unrelated directory, checking ID/title/goal, complete
canonical Markdown, target11/edition11, applicability, fresh evidence and matching
structured/text JSON offline. Target10 returns no_applicable_edition. Existing
IDs, exactly eight foundations, PIN-959/PIN-962 and Human gates remain unchanged;
RC2 execution does not establish final-GA evidence or publication readiness.
