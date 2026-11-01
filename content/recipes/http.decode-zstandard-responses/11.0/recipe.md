# Decode Zstandard HTTP responses

## Goal

Negotiate and decode a Zstandard-compressed HTTP response. Fictional Cairn reads
a compressed survey report with a decoded size limit and cancellation budget.
This is response decompression; compressing outgoing requests is a separate goal.

## Older approach

.NET 10 has no DecompressionMethods.Zstandard flag or built-in ZstandardStream.
A deliberately pinned third-party codec is a valid alternative. This original
manual response handler uses ZstdSharp.Port 0.8.6 (the actual lib/net9.0 asset
selected for net10.0). Its publisher supports .NET 5+; the real .NET 10 consumer
is compiled and executed here. This is a managed decoder, with no external native
zstd library. It is not a Microsoft framework backport or a general package endorsement.

{{snippet:older}}

Add the exact ZstdSharp.Port version 0.8.6 to the older project (the canonical
central PackageVersion uses [0.8.6], with a committed packages.lock.json). Keep
the existing .NET 10 SDK/runtime and nullable/implicit usings. The canonical
project declares C#14.0. The adapter advertises zstd/gzip/deflate/br and manually
decodes one outer layer. The inner handler has AutomaticDecompression=None so
two different handlers do not accidentally remove two layers. HTTP deflate uses
ZLibStream. OwnedContent forwards cancellation explicitly and releases both
the decoder and original content when the response is disposed.

## Newer approach

.NET 11 integrates Zstandard response decoding into SocketsHttpHandler.
Choose the supported encodings explicitly instead of relying on All to retain
the same set across future runtime upgrades.

{{snippet:newer}}

Use either configured client with the same bounded reader below. Copy both
types into the same application assembly; Create returns an owner-managed,
reusable HttpClient. Call ReadAsync with the report's Uri and an overall
CancellationTokenSource budget, and dispose the client at owner shutdown.
The fixtures use http://127.0.0.1:<fixture-port>/cairn/report; use an appropriate
HTTPS endpoint for a real service. These clients disable proxies and redirects
for the controlled direct route. Preserve an application's intentional proxy
policy separately when adapting the handler.

{{snippet:reader}}

## Migration

1. Target net11.0 with a .NET 11 SDK/runtime and retain nullable/implicit usings.
   SDK11 RC2/C#15.0 are the measured canonical baseline. No package, workload,
   external provider, rendering mode or experimental opt-in is needed for the
   newer route; System.IO.Compression and its native codec ship with the runtime.
2. Set AutomaticDecompression to Zstandard plus the other formats you intend to
   accept. The handler adds the corresponding Accept-Encoding values. Do not
   advertise a format you cannot decode, or manually duplicate a decoding layer.
   Existing identical zstd negotiation is not duplicated in the tested case.
3. Remove the manual adapter and its ZstdSharp dependency only after exercising
   the new runtime on the real service contract. Keep the package route on .NET 10
   if required; do not install an unsupported .NET 11 framework package on it.
4. Use ResponseHeadersRead and pass cancellation to both request and body reads.
   HttpClient.Timeout covers the request through headers in this mode, so retain
   a caller-owned overall budget for the body. Reject non-success status codes,
   cap decoded output, and dispose the response on success or failure.
5. Check Content-Encoding before interpreting the returned bytes. Automatic
   decompression removes only the last/outermost supported encoding and clears
   Content-Length because the wire length no longer describes the decoded body.
   Other content headers remain. Reading/buffering later can compute a new length.
6. For gzip, zstd, only zstd is removed and the remaining body/header is gzip.
   For zstd, gzip, only gzip is removed and zstd remains. The bounded reader
   deliberately rejects both stacks. If a service requires stacked content,
   implement a separately bounded decoder for each remaining layer, in reverse
   order; do not treat the first decoded bytes as the final report.
7. An unsupported outer encoding is returned unchanged, including its headers
   and encoded length. Reject it explicitly. Malformed zstd fails during body
   reading; .NET 11 throws InvalidDataException in this fixture while the older
   decoder throws ZstdSharp.ZstdException. Do not substitute an empty success.

## Constraints and alternatives

The executed path is Windows x64 CoreCLR SocketsHttpHandler over HTTP/1.1.
Browser/mobile native handlers, other operating systems, HTTP/2/3, dictionaries,
skippable/concatenated frames and truncation semantics are not established here.
No throughput, compression-ratio, security-certification or final-GA claim is made.
The 64 KiB output budget is an application limit, not a complete bound on codec
window memory or CPU. An adversarial service needs appropriate service/input,
timeout and resource policy beyond this small example.

The third-party route has its own maintenance/license obligations. Retaining
gzip/Brotli negotiation with no zstd support is another valid .NET 10 policy when
the server can supply those formats. Gzip bytes are not a Zstandard substitute.
This recipe adds no ninth .NET 10 foundation or unrelated request-compression change.

## Evidence

eng/Test-ProxyResponses.ps1 -Record -VerifySources builds/runs real paired
CairnReports consumers with SDK10.0.402/runtime10.0.13 and
SDK11.0.100-rc.2.26504.105/runtime11.0.0-rc.2.26504.105, zero warnings/errors.
Framework/reference hashes match the runtime/security and data/I/O audits.
The exact resolved ZstdSharp.Port archive, selected/executed lib/net9.0 assembly,
compiler/CoreLib and loaded framework/native codec identities are recorded.
Pinned publisher sources qualify the package and actual framework contracts;
source presence is separate from executed behavior.

Real loopback HTTP exchanges carry actual compressed zstd/gzip/deflate/br frames
and unencoded responses. Checks assert Accept-Encoding without duplicate zstd,
body bytes, Content-Encoding/Content-Length and unrelated headers. Both stacked
orders, unknown outer codings, malformed zstd and a cancelled stalled body are
exercised. The shared reader accepts a valid report, rejects an unknown encoding
and rejects decoded output above 64 KiB. The older adapter's explicit token
forwarding is exercised, rather than assuming a content wrapper preserves it.

The ordinary package is built, packed, installed through an isolated local-only
feed/cache and called with real stdio list/get from an unrelated directory.
Discovery ID/title/goal, full expanded canonical snippets, target11/edition11,
applicability and fresh paired evidence are checked; target10 has no edition.
Unusable retrieval proxies prove proxy-independent offline content, not network
isolation. Other platform/native-handler/GA behavior remains unverified.
PIN-959/PIN-962, existing Human gates and the eight foundations remain open/intact.
