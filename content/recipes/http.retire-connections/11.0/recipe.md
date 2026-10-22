# Retire pooled connections using a cached policy (experimental)

## Goal

Stop reusing connections after an endpoint policy changes, while allowing existing requests to finish. Keep the client long-lived and avoid abandoning healthy pooled transports solely to notice a policy update.

## Older approach

.NET 10 supports PooledConnectionLifetime and PooledConnectionIdleTimeout. A finite lifetime periodically replaces pooled connections, allowing later connections to resolve names again. It is a valid simple operational policy. There is no public per-connection ShouldEvictConnection callback in that release. The comparison uses a deliberately short 250 ms lifetime to make the local test repeatable; choose a production interval for the service's actual requirements. Its delegate parameter makes the limitation explicit rather than pretending the old handler consults it.

{{snippet:older}}

## Newer approach

**Experimental: SYSLIB5008.** The .NET 11 ShouldEvictConnection callback returns Task<bool>. True marks a pooled connection for retirement; it cannot serve new requests and is retired when idle. A request already using it may finish. This sample consults a cheap, thread-safe flag supplied by the application. It is a whole-pool switch; a multi-origin or address-change policy must inspect the provided connection context and maintain an appropriate cached policy per origin.

{{snippet:newer}}

## Migration

1. Use net11.0/C# 15.0 with System.Net.Http. No external package/workload is required. Review the experimental contract before applying the narrowly scoped SYSLIB5008 suppression; a stable containing release does not make this API stable.
2. Create the handler before any request. Retain one HttpClient around it and dispose it at the owner's shutdown. Example application state:

{{snippet:usage}}

Keep the flag set only while that whole-pool policy is intended. A permanent true also retires newly pooled connections. In a real endpoint migration, prefer comparing the connection's endpoint with a cached approved address set so newly valid connections can remain.

3. Use context.DnsEndPoint for the logical origin and context.RemoteEndPoint for the physical peer when known. Proxies can make them differ; a custom connection stream can leave RemoteEndPoint null. Context.ConnectionId can correlate transport telemetry and context.Age reports elapsed age. Do not retain the context after the callback returns.
4. The callback may run concurrently, including while requests are active, and is not promised once per request. Make cache access thread-safe and cheap. Honor its cancellation token, which is canceled if the connection is disposed. Avoid repeated blocking resolution per connection; refreshing an address cache is a separate application responsibility.
5. Choose PooledConnectionLifetime alongside this policy. Infinite lifetime in the sample deliberately isolates callback retirement. A finite maximum lifetime remains valid protection; the callback complements it. MaxConnectionsPerServer=1 is an HTTP/1.1 fixture limit, not an HTTP/2 stream limit.

## Constraints

The callback is not a deadline, forced abort, DNS monitor or guarantee of immediate invocation after a policy update. Use cancellation/timeouts for request termination. Existing active work may outlive the update. Configure the handler before use; changing settings after requests start throws. Desktop/server SocketsHttpHandler is the demonstrated transport; browser/WASI/mobile and rendering integrations are not covered. Experimental API shape and RC2 evidence require GA reconciliation.

## Evidence

The .NET 10 and RC2 projects use a real keep-alive TCP HTTP/1.1 peer which identifies each accepted transport in its response. Two healthy requests reuse the same connection. The older run waits beyond its finite lifetime; the newer run changes the shared flag and performs bounded requests until a different transport is observed. No HttpMessageHandler mock or DNS change is substituted for pool behavior. This proves idle retirement/reconnection, not a hard eviction latency, active-request abort or actual production DNS migration. Active-request completion semantics and context concurrency are source-reviewed official contracts.
