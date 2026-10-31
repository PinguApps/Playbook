# Compute the checksum a binary format requires

## Goal

Fenwatch telemetry frames declare a checksum variant for their payload bytes.
Compute exactly that variant, including polynomial, initial/final values,
reflection and wire byte order. Adler/CRC detect accidental corruption; they
do **not authenticate** a sender or prevent malicious modification. Use a
separate authenticated protocol or cryptographic MAC when that is required.

The fictional format has four explicit algorithm choices:

{{snippet:format}}

Its checksum covers payload bytes only, excluding the algorithm tag and checksum
field. A real format must define its own coverage and byte order just as explicitly.

## Older approach

System.IO.Hashing 10.0.13 supplies default CRC32/CRC64, but those defaults are not
CRC32C or CRC64-NVME. It has neither Adler32 nor configurable parameter sets in
the inspected net10 asset. Use a legitimate dependency for the chosen variant or
maintain an explicit implementation. These original, simple fallbacks use the
package's streaming base class; they are not a general-purpose CRC library.

Add `using System.IO.Hashing;` and `using System.Buffers.Binary;` to each file:

{{snippet:algorithms}}

The helper below binds only the format's reviewed 32/64-bit combinations. The
64-bit fallback's output order is specifically the NVME option used here.

{{snippet:older}}

Create a net10.0/C#14 console project with implicit usings and a pinned
`<PackageReference Include="System.IO.Hashing" Version="[10.0.13]" />` (or a
matching central package pin). Review and commit its packages.lock.json and use
locked restore. The default package algorithms remain valid for formats that
actually specify CRC-32/ISO-HDLC or CRC-64/ECMA-182; substituting them for a
different polynomial produces a different checksum, even with the same width.

## Newer approach

Use System.IO.Hashing 11.0.0-rc.2.26504.105 with net11.0. Add
`using System.IO.Hashing;`; keep FrameChecksum in a separate file:

{{snippet:newer}}

One-shot example: `FrameChecksums.Hash("Fenwatch/sequence=412"u8, FrameChecksum.Castagnoli)`.
For segmented input, create an instance, call `Append(headerPayloadBytes)` and
`Append(readingPayloadBytes)`, then `GetHashAndReset()` to get the bytes and start
the next message. `GetCurrentHash()` reads without resetting; `Reset()` discards
an unfinished message. Neither algorithm choice nor buffer segmentation changes
which bytes the format requires you to cover.

| Format option | Normal polynomial | Initial / final XOR | Reflected input/output | ASCII `123456789`: numeric value | Bytes written |
| --- | --- | --- | --- | --- | --- |
| Adler | Not a CRC | s1=1, s2=0, modulus 65521 | No | `091E01DE` | `09 1E 01 DE` |
| Castagnoli / CRC32C | `1EDC6F41` | `FFFFFFFF` / `FFFFFFFF` | Yes | `E3069283` | `83 92 06 E3` |
| CRC64-NVME | `AD93D23594C93659` | all ones / all ones | Yes | `AE8B14860A799888` | `88 98 79 0A 86 14 8B AE` |
| Custom CRC32/MPEG-2 | `04C11DB7` | `FFFFFFFF` / zero | No | `0376E6E7` | `03 76 E6 E7` |

For the custom Crc32ParameterSet.Create route shown here, the polynomial is in
normal representation, not the reversed polynomial. Its reflection option applies
to both input and output; this recipe does not claim arbitrary independent
refin/refout support. The 64-bit sample uses the named Nvme set; custom Crc64
creation is unexecuted. In these assets, reflected CRC
Hash bytes are little-endian, forward CRC bytes big-endian, and Adler big-endian.
HashToUInt32/64 returns the numeric value, not a byte-order declaration. If a real
format uses another wire order, write that numeric value explicitly with
BinaryPrimitives in the required order.

## Migration

1. Read the format specification. Fix the covered bytes, width, polynomial,
   initial/final XOR, reflection and byte order; keep its algorithm tag separate.
2. Create a net11.0/C#15 console project with implicit usings. Pin
   `<PackageReference Include="System.IO.Hashing" Version="[11.0.0-rc.2.26504.105]" />`
   or use the matching central package pin, commit the reviewed lockfile and
   restore with `-p:RestoreLockedMode=true`. The executed asset is lib/net11.0.
3. Include FrameChecksum and the current FrameChecksums helper. Replace the
   fallback with Adler32, the named CRC parameter set, or the explicit custom
   Create call. Do not silently retain a default constructor for CRC32C/NVME.
4. Use the one-shot helper or Append segments in order. Reset between messages;
   do not share mutable hash instances across concurrent frames.
5. Allocate at least four bytes for Adler/CRC32, eight for CRC64. TryHash and
   TryGetHashAndReset return false with bytesWritten=0 for a short destination;
   the tested failed instance Try call leaves state intact. Hash/GetCurrentHash
   destination overloads throw ArgumentException. Check the return before using
   destination bytes and retry with enough space.
6. Match independent reference vectors, numeric values and serialized bytes.
   Exercise empty payloads, segmentation and reset as well as normal frames
   before deleting old code. ZIP archive integrity remains separate guidance.

## Constraints

Primary route: net11.0/.NET 11, C#15 sample mode and exact verified prerelease
package System.IO.Hashing 11.0.0-rc.2.26504.105, compile/runtime lib/net11.0.
Comparison: net10.0/C#14 with System.IO.Hashing 10.0.13 lib/net10.0. No extra
provider, workload, rendering mode, native library, experimental opt-in or service.
The APIs have no required experimental diagnostic opt-in in these artifacts;
the package is prerelease, and final-GA reconciliation remains required.

SDK reproduction pins are 10.0.402 and 11.0.100-rc.2.26504.105, with
reference/runtime 10.0.13 and 11.0.0-rc.2.26504.105. SDK/runtime requirements and
these measured pins are distinct. All behavior here was executed on Windows
x64; no other-platform or performance result is claimed. The RC2 package also
contains an audited net10 asset, but that optional older-target route was not
executed or advertised. No ninth .NET 10 foundation is added.

The fallback supports only the format's combinations above, and the current
custom route uses equal input/output reflection. This is not arbitrary CRC
parameter validation or a new cryptographic library. CRC collisions are possible;
neither a matching CRC nor Adler proves trusted data. Archive ZIP validation and
TAR representation remain with their existing recipes.

## Evidence

`eng/Test-IntegerFormats.ps1 -Record -VerifySources` runs the paired actual
consumers with reviewed locks. CRC RevEng independently publishes CRC32C,
CRC64-NVME and MPEG-2 check values for ASCII 123456789. Adler is independently
evaluated from RFC1950's formula: s1=478, s2=2334, result 091E01DE. Both consumers
match all four numeric values and declared bytes. The old package's default CRCs
are also executed and confirmed to be different variants.

Every option checks one-shot versus Append at empty/middle/end split points,
empty input, the fictional frame and a 20KB payload, reset isolation, read versus
reset semantics, short destination failure and retry. Current static TryHash and
Hash failure overloads are exercised too. Saved records retain exact commands,
inputs/dependencies, runtime, compiler/CoreLib and audit-matching package/reference
hashes. Versioned declarations and parameter/Adler implementation sources qualify
availability, reflection and output order without copying implementation code.

The ordinary locally installed package is inspected using real offline stdio
list/get for discovery ID/title/goal, full canonical snippets, explicit target11 /
edition11, applicability and current evidence. This is RC2 proof, not final GA
or full catalogue readiness. PIN-959, PIN-962 and existing Human gates remain
open; no new recurring native/client matrix or external resource is used.
