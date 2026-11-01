# Parse and format endpoints in UTF-8 buffers

## Goal

Read and write numeric network endpoints in the caller-owned fields of a fictional
Kestrel Ridge survey ledger. Use this route when a parser or writer already has
UTF-8 bytes and should avoid intermediate text strings. Parsing still creates an
IPEndPoint and IPAddress; this is not a claim of allocation-free networking.
The ledger uses a complete field slice, not a streaming parser with consumed-byte
tracking. There is no DNS lookup or socket connection.

Copy each snippet into its own source file. It includes imports and the
Playbook.Samples namespace; add `using Playbook.Samples;` in the calling file.
The console projects enable nullable/implicit usings and warnings as errors.

## Older approach

.NET 10 already parses endpoint strings and ReadOnlySpan<char>. For UTF-8 input,
decode to a string, use the existing parser, and encode ToString() for output.
This adapter rejects malformed UTF-8 and checks capacity before writing. It
remains valid when strings are already present or the conversion cost is small.
Its failed small-buffer write leaves the entire destination untouched; the newer
TryFormat contract does not promise that stronger behaviour.

{{snippet:older}}

The comparison targets net10.0, C# 14.0 and Microsoft.NETCore.App, with no NuGet
package or workload. It adds no .NET 10 catalogue foundation.

## Newer approach

.NET 11 adds IPEndPoint.Parse/TryParse(ReadOnlySpan<byte>) and public TryFormat
overloads for Span<byte> and Span<char>. The type implements ISpanParsable,
IUtf8SpanParsable, ISpanFormattable and IUtf8SpanFormattable, allowing generic span
consumers to use it too. The byte overloads avoid the adapter's intermediate
strings; they do not avoid the endpoint objects or guarantee a speedup.

{{snippet:newer}}

For example, pass `"[2001:db8::41]:4701"u8` to TryRead. After success, call TryWrite
with a destination slice inside your record buffer. Consume or transmit only
`destination[..written]` after a true return. Use `using System.Net;` if the caller
declares an IPEndPoint variable. The documentation addresses 192.0.2.41 and
2001:db8::41 are fictional values; the example performs no network I/O.

## Migration

1. Target net11.0 using a .NET 11 SDK/runtime. The canonical project declares
   C# 15.0 and has no package reference, workload, native toolchain, provider
   selection, rendering mode or experimental opt-in. Exact RC2 pins below record
   what was tested, not a requirement to use that one prerelease build forever.
2. Replace only the UTF-8 decode/encode adapter where the surrounding code already
   owns bytes. Pass the complete endpoint field, excluding record delimiters.
   Retain ReadOnlySpan<char> parsing for existing character input; use the new
   Span<char> formatter when the output sink expects characters instead of bytes.
3. Preserve the endpoint grammar: IPv4 with an optional colon/port, bracketed
   IPv6 when a port is supplied, and a port in 0..65535. An address without a port
   parses with port zero; zero is a valid API value, not necessarily an acceptable
   remote service port. Unbracketed IPv6 is an address, not an unambiguous trailing
   port. The parser accepts numeric IP syntax, not hostnames or URIs, and is not a
   strict canonical dotted-quad validator. Apply stricter ledger policy separately.
4. Prefer TryParse for untrusted input; false returns a null result. Parse throws
   FormatException on malformed bytes or an invalid endpoint. Reject malformed
   brackets, negative/nondecimal/out-of-range ports and invalid UTF-8. Do not parse
   exception messages or silently substitute a different endpoint.
5. Size the caller's destination and check TryFormat's bool and written count.
   A short destination returns false with zero written; its supplied slice may
   have changed. Discard that slice or retry in a larger buffer. Bytes outside the
   supplied span remain protected; do not infer rollback of the interior. A true
   result identifies the exact prefix to use; unused capacity is not payload.
6. Keep numeric port/address rules independent of culture. The reviewed formatter
   emits invariant numeric endpoint text, brackets IPv6 and includes the port.
   Generic parsing/formatting interfaces use the same endpoint grammar rather
   than creating a custom format/provider language. Test exact-fit, larger and
   too-small slices in the actual surrounding buffer.

## Constraints and alternatives

This recipe covers framework text conversion. It does not resolve a host, verify
reachability, authorize an address, authenticate a peer, validate a URI or provide
network framing. If an application needs hostnames, a particular allowed subnet,
nonzero ports or a strict spelling, validate that separate contract explicitly.
TryRead does not trim record padding or accept a partial streaming field.

No provider or rendering integration is involved. Windows x64 consumers are
executed here; other platforms are not measured. The reviewed API is unannotated
as experimental, but RC2 evidence does not establish final-GA verification.
Keep the string adapter where its explicit all-or-nothing write matters, or
stage new formatted bytes in scratch storage before committing a record.

## Evidence

eng/Test-NetworkBuffers.ps1 -Record -VerifySources compiles and runs actual
net10.0/C#14 and net11.0/C#15 consumers using SDK10.0.402/runtime10.0.13 and
SDK11.0.100-rc.2.26504.105/runtime11.0.0-rc.2.26504.105, without packages. It
compares System.Net.Primitives/System.Runtime reference hashes to the original
runtime/security audit, records compiler and loaded CoreLib identities, exact
commands, inputs and fresh behaviour output through the existing evidence workflow.

Both consumers check IPv4 and bracketed IPv6, ports zero/65535, absent-port zero,
malformed brackets/ports, overflow and invalid UTF-8. They verify input guards,
unchanged input, exact-fit/larger output, exact written lengths, zero/one/one-byte-
short destinations, and unchanged bytes outside each supplied output slice.
The .NET 11 consumer also executes throwing byte Parse, char TryFormat and generic
UTF-8 parsing/formatting interfaces. Failed-write interior rollback is deliberately
not asserted. No benchmark, live service, native environment or new recurring
matrix is required. Installed stdio retrieval checks the complete canonical
Markdown and explicit target/edition/applicability offline. PIN-959 and PIN-962
remain open, and the exact eight approved .NET 10 foundations are unchanged.
