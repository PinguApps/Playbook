# Make authored clients and proxy adapters accepted by the server

## Goal

A fictional lantern relay sends delivery requests to Kestrel. Its hand-written protocol adapter copied transport headers from an incoming connection into a different outbound protocol. Repair the emitting adapter so that its requests have unambiguous framing and only headers valid for the outbound connection.

HTTP/2 and HTTP/3 forbid connection-specific fields. HTTP/1.1 requires consistent body framing. Changing a server's trust, forwarding or routing middleware does not repair malformed bytes rejected before that middleware runs.

## Older approach — tolerant reception is not a wire contract

The .NET 10 comparison sends original raw HTTP/1.1, HPACK HTTP/2 and QPACK HTTP/3 traffic directly to real Kestrel endpoints. In the measured .NET 10.0.13 build, four obsolete transport fields still reach the HTTP/2 and HTTP/3 endpoint: `Proxy-Connection`, `Keep-Alive`, `Transfer-Encoding` and `Upgrade`. `Connection` and invalid `TE` already fail. That tolerance is historical behavior, not advice to retain the malformed emitter.

This serviced .NET 10 build also closes an HTTP/1.1 connection after processing both `Content-Length` and `Transfer-Encoding`. Do not claim every .NET 10 runtime keeps it alive. The corrected requests are valid on both releases, so an existing compliant client needs no migration.

Save these files together in an independent `LanternForwarding` directory. The fixture's header filter and single-framing example are usable before the server upgrade too.

### Directory.Build.props
{{snippet:n10-directory-build-props}}

### Directory.Build.targets
{{snippet:n10-directory-build-targets}}

### Directory.Packages.props
{{snippet:n10-directory-packages-props}}

### global.json
{{snippet:n10-global-json}}

### LanternForwarding.csproj
{{snippet:n10-lanternforwarding-csproj}}

### Program.cs
{{snippet:n10-program-cs}}

### Http3Probe.cs
{{snippet:n10-http3probe-cs}}

## Newer approach — build the outgoing message deliberately

Remove `Connection`, `Proxy-Connection`, `Keep-Alive`, `Transfer-Encoding` and `Upgrade` before emitting an HTTP/2 or HTTP/3 request. Of the `TE` values, retain only `trailers`. Also remove fields named by an incoming `Connection` header. The helper also drops incoming proxy credentials/challenges and the `Trailer` declaration: this fixture does not translate trailers or forward credentials to another proxy. It removes incoming `Host` and `Content-Length` but does not generate their replacements. The outbound writer must produce authority and body framing from its own destination and payload; the wire fixture supplies its local `lantern` authority explicitly.

For a known-length HTTP/1.1 body, emit one correct `Content-Length`. For a streaming HTTP/1.1 writer, emit only chunked transfer framing. The latter is an HTTP/1.1 framing choice, not an HTTP/2 or HTTP/3 header to forward. Preserve ordinary end-to-end application fields.

The newer fixture sends malformed bytes without a high-level client's header normalization hiding them. HTTP/2 rejection may arrive as a protocol-error stream reset, connection GOAWAY or transport abort in this Windows run; the application request counter must still show that no malformed request reached the handler. HTTP/3 checks the actual `H3_MESSAGE_ERROR` stream error. Every corrected request must independently return `OK`. The real handler also rejects leaked proxy credentials/challenges, trailer declarations and the field nominated by the incoming `Connection` header in those corrected controls.

### Directory.Build.props
{{snippet:n11-directory-build-props}}

### Directory.Build.targets
{{snippet:n11-directory-build-targets}}

### Directory.Packages.props
{{snippet:n11-directory-packages-props}}

### global.json
{{snippet:n11-global-json}}

### LanternForwarding.csproj
{{snippet:n11-lanternforwarding-csproj}}

### Program.cs
{{snippet:n11-program-cs}}

### Http3Probe.cs
{{snippet:n11-http3probe-cs}}

## Migration

1. Find the component that writes the outbound request. Inspect its final protocol and wire headers; a normal high-level client may already normalize them.
2. Before translating HTTP/1.1 to HTTP/2 or HTTP/3, strip the connection-specific fields and `Connection`-nominated names. Have the outbound writer recreate authority and framing from the outbound URI and actual body; `ForwardHeaders` only filters inbound fields. Retain `TE: trailers` only where allowed.
3. For HTTP/1.1, choose one framing mechanism. Do not forward a stale `Content-Length` next to chunked transfer encoding. The server processes this ambiguous request then closes; that is distinct from rejecting an HTTP/2 or HTTP/3 message.
4. Recreate each complete sample in a separate directory with the supplied boundaries and SDK pin. Run restore to generate and review the lock file, build Release and execute the DLL with the corresponding absolute `dotnet` host.
5. Expect HTTP/1.1 ambiguous framing to close and corrected framing to permit two responses on one connection. Expect all six malformed HTTP/2 and HTTP/3 cases to fail on RC2, then all corrected controls to receive `OK`. The .NET 10 result intentionally shows four tolerated fields on each newer protocol.
6. Apply the repair to the real adapter and check its downstream request path. There is no invented Kestrel relaxation switch in this recipe. Do not substitute a global parsing/security change for fixing the writer.

## Constraints

- Runtime: this edition targets ASP.NET Core 11 Kestrel behavior. The same corrected messages work in the actual .NET 10.0.13 comparison. SDKs 10.0.402 and 11.0.100-rc.2.26504.105, C# 14/15 and corresponding runtime assets are the saved baselines.
- Packages/assets: no extra managed NuGet dependency or workload is needed. HTTP/3 execution needs working `System.Net.Quic` and native MsQuic, a supported OS, TLS 1.3 and UDP loopback access. Missing support is a failed prerequisite, not a passing skip. Windows x64 used `msquic.dll` 2.5.11.159415436. Other OS routes were not executed.
- The temporary HTTPS certificate is pinned by the HTTP/3 fixture and never added to shared trust. This is not production certificate provisioning. HTTP/2 prior knowledge runs over loopback TCP; HTTP/3 uses encrypted QUIC.
- The header filter is a focused transport repair, not a general reverse proxy. It does not implement authentication, routing, trailer translation, content transformations or hop-by-hop WebSocket tunneling. A proxy library's own validated outbound writer is preferable to hand-writing production HPACK/QPACK.
- `Forwarded` and `X-Forwarded-*` trust configuration is a separate application concern. No browser/rendering mode, database, account or cloud resource is involved here.
- Stable protocol enforcement is prepared using RC2 evidence. Earlier-version servicing can change the comparison's observed behavior; regenerate evidence instead of inferring behavior from the major version. GA reconciliation remains pending.

## Evidence

Each original program was built and run using its actual pinned SDK/runtime and locked restore. Counters assert 23 accepted requests on .NET 10 and 15 on RC2: three HTTP/1.1 handler invocations, corrected HTTP/2 and HTTP/3 controls, plus only the older release's eight tolerated malformed requests. No mock protocol parser or client-side rejection stands in for server execution.

Saved sample records identify commands, native library version in output, dependencies and input hashes. The installed `LanternTransport` scenario separately retrieves the entire body and applicability through real offline stdio list/get with isolated package feed/cache and an unrelated working directory. It verifies edition 11.0 and absence from target 10.0 discovery. These checks do not establish final GA or arbitrary production-proxy compatibility.

Source identity remains `hosting.send-compliant-forwarded-requests`, WEB-079 / WEB-086. See the [official Kestrel protocol change](https://learn.microsoft.com/en-us/aspnet/core/breaking-changes/11/kestrel-strict-protocol-compliance) for the protocol rationale. All essential implementation and reproduction files are included above.
