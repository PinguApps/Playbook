# Bound HTTP/2 requests before server settings arrive

## Goal

Avoid an initial burst above a known server's concurrent-stream capacity while a new HTTP/2 connection is waiting for the peer's SETTINGS frame. Keep later server settings authoritative.

## Older approach

.NET 10 has no public InitialHttp2MaxConcurrentStreams setting. A shared application admission gate can bound outstanding requests, including their response lifetime. That policy applies before and after SETTINGS and can span connections; it is useful when an application needs its own persistent cap. It does not expose the same connection initialization control.

{{snippet:older}}

## Newer approach

.NET 11 exposes InitialHttp2MaxConcurrentStreams. The default is 100 and values must be at least one. Set it before the handler starts. The example uses two for a controlled endpoint known to have low initial capacity. It deliberately removes the application gate; the peer can later advertise a different limit.

{{snippet:newer}}

## Migration

1. Use net11.0/C# 15.0, System.Net and System.Net.Http. Framework assets supply these APIs; no package, provider, workload, rendering mode or experimental suppression is needed.
2. Retain a handler/client for the origin. Use an application-supplied cancellation token and an HTTP/2-capable URI. For this comparison, invoke the helper with a shared gate; only the older implementation uses it:

{{snippet:usage}}

3. Keep RequestVersionExact when HTTP/2 is required; a failed HTTP/2 negotiation should not silently turn this check into HTTP/1.1. Production HTTPS requires successful TLS/ALPN negotiation. The authoring fixture uses local cleartext prior-knowledge HTTP/2; it does not establish remote TLS or proxy support.
4. Select the initial value from known server capacity. Until the first SETTINGS, the configured value limits streams on a new connection. After SETTINGS_MAX_CONCURRENT_STREAMS arrives, the peer value takes over, including lower or higher limits. An earlier connection's lower observed limit for the same host may also constrain new connections.
5. Keep a separate application gate if the application needs a permanent total-work cap. Do not confuse this property with InitialHttp2StreamWindowSize (receive bytes), MaxConnectionsPerServer (HTTP/1.1 connections), or HTTP/3 configuration. Enabling additional HTTP/2 connections can change total origin concurrency; the sample leaves it disabled.

## Constraints

Most applications should retain the default. This tuning does not increase a server's capacity or override its advertised limit. All handler settings must be chosen before first use; zero/negative initial values fail. Desktop/server SocketsHttpHandler is demonstrated; browser/WASI/mobile transports are excluded. The API is not experimental in the reviewed contract; actual compilation/execution is RC2, with GA reconciliation pending.

## Evidence

A real TCP HTTP/2 peer withholds its SETTINGS frame. Six concurrent client requests produce only two initial HEADERS streams during a bounded observation window. The peer then advertises three: the .NET 11 client admits a third and stays within that limit while responses are held; the older shared gate retains two. The peer completes all six requests using real HTTP/2 frames. The newer check also rejects zero. This is actual protocol execution, not a mock, Kestrel configuration assertion, timing benchmark or new native CI matrix. A bounded absence observation is not a proof of every scheduler/interleaving; source contracts establish the general rule.
