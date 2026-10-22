# Compress outgoing HTTP request bodies

## Goal

Upload a repetitive harbour sensor report using an encoding the receiving service understands. Preserve the original media type and let the receiver recover exactly the same text.

## Older approach

.NET 10 can compress into a buffer with GZipStream or BrotliStream, then send ByteArrayContent with the matching Content-Encoding. This is useful when a known compressed Content-Length is required or the buffered bytes will be reused. It also holds the entire compressed representation in memory. There is no .NET 10 built-in ZstandardCompressedContent or ZstandardStream equivalent; a separately reviewed third-party codec would be needed for zstd.

{{snippet:older}}

## Newer approach

.NET 11 provides GZipCompressedContent, BrotliCompressedContent and ZstandardCompressedContent. Each wraps another HttpContent and compresses during serialization. The wrapper sets Content-Encoding and preserves the original content headers except Content-Length. It owns the wrapped content: dispose the outer content after the request finishes. The example starts with an in-memory string; replacing StringContent with StreamContent also avoids first buffering the original source.

{{snippet:newer}}

## Migration

1. Use a net11.0 console project with C# 15.0, nullable and implicit usings enabled. Import System.Net.Http, System.IO.Compression and System.Text. The APIs are in Microsoft.NETCore.App; no NuGet dependency or workload is required.
2. Agree gzip, br or zstd with the server before compressing. Accept-Encoding describes acceptable **responses**; it does not negotiate request compression. AutomaticDecompression also concerns responses. A service which rejects Content-Encoding can return 415; do not silently retry a non-idempotent upload.
3. Create the wrapper, send it, check the response and dispose both response and outer content:

{{snippet:usage}}

Here uploadUri and cancellationToken are supplied by the application. Use its retained client or IHttpClientFactory policy in production.

4. Do not set a stale uncompressed Content-Length. Streaming compression normally leaves length unknown: HTTP/1.1 uses chunked framing, while HTTP/2 and HTTP/3 frame the body themselves. If a gateway requires a length, deliberately buffer compressed bytes as in the older approach.
5. Recreate content and reset/reopen the source for retries; a forward-only stream is not automatically replayable. Pass cancellation to SendAsync/PostAsync. Avoid compressing already-compressed or tiny payloads without measuring the benefit.

## Constraints

The receiver must decode the declared encoding and enforce limits on decompressed size. Compression does not encrypt data; consider length disclosure when secrets and attacker-controlled text share a compression context. This recipe covers desktop/server SocketsHttpHandler use, not a browser transport or rendering integration. Brotli/Zstandard wrappers are unsupported on browser/WASI; no browser/mobile route is claimed. The .NET 11 contracts have no experimental opt-in, but actual proof uses the pinned RC2 SDK/runtime, not GA reconciliation.

## Evidence

Both actual .NET 10/11 projects post through SocketsHttpHandler to a real TCP HTTP/1.1 receiver. It reads Content-Encoding and Content-Type, handles chunked or length framing, decompresses the transmitted bytes, verifies exact original text and checks that this repetitive fixture shrinks. The .NET 11 run exercises all three codecs; .NET 10 exercises gzip/br. This is real loopback protocol execution, not a mock or proof of support at an external server, proxy, TLS endpoint or every HTTP version. Exact artifacts and input hashes are served with the recipe.
