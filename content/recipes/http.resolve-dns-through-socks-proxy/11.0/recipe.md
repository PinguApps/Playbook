# Resolve target names through a SOCKS proxy

## Goal

Reach an HTTP target whose hostname is known to a SOCKS5 proxy rather than the
client's local resolver. Fictional Cairn survey stations send reports to
relay.cairn.invalid through a controlled relay. Keep the hostname in the request
URI so the proxy receives it. A numeric destination is already resolved.

## Older approach

.NET 10 SocketsHttpHandler already sends a nonnumeric target hostname in a SOCKS5
CONNECT request using address type 3. Its supported spelling is socks5, which
solves this goal without a custom DNS adapter. Do not apply curl's distinction
between socks5 and socks5h to this older .NET implementation.

{{snippet:older}}

This is an ordinary net10.0 console helper with nullable and implicit usings
enabled, no packages/workloads, and C#14.0 in the canonical project. The .NET 10
runtime rejects the socks5h proxy scheme; changing the URI spelling is not a
backport. Retain socks5 for applications that must run on .NET 10.

## Newer approach

.NET 11 also accepts the explicit socks5h spelling. It selects the same SOCKS5
hostname tunnel behavior as socks5; it does not introduce a separate DNS API or
change the old spelling's behavior.

{{snippet:newer}}

Copy the helper into a nullable/implicit-usings net11.0 application. Create and
reuse the HttpClient with the controlled proxy's port. For this local example
the proxy listens on 127.0.0.1; replace its address intentionally for another
trusted proxy. Send a GET to http://relay.cairn.invalid:<target-port>/cairn/report
with HttpClient.GetAsync and a caller-owned CancellationToken. The .invalid name
is deliberate: the fixture proxy maps it to its own loopback HTTP target.
Dispose the response after reading it and dispose the client at owner shutdown.
Use HTTPS for real confidential reports; the HTTP fixture proves tunneling only.

## Migration

1. Retain the target hostname in the request URI. Do not call local Dns APIs
   first or substitute an IP address when the proxy must select the target.
2. On net11.0, choose socks5h when explicit naming helps configuration readability.
   Keeping socks5 remains valid. Use a .NET 11 SDK/runtime for the newer sample;
   SDK11 RC2/C#15.0 are measured pins, not proof of final GA. No package,
   optional workload, provider SDK, rendering mode or experimental opt-in is needed.
3. Assign the proxy explicitly and set UseProxy=true. Keep any bypass policy
   deliberate; this helper adds no bypass list and disables redirects. Do not
   assume environment proxy settings describe the same path. The proxy address
   itself must be reachable; a hostname for that address can need local DNS.
4. If the proxy requires RFC1929 username/password authentication, pass a
   NetworkCredential obtained from the application's secret source. The tests
   use disposable synthetic credentials. Do not put credentials in the URI,
   source code, logs or an ordinary configuration repository. Proxy credentials
   are separate from the origin server's credentials.
5. Pass cancellation to each HTTP operation and set an overall application
   deadline. Handle HttpRequestException for proxy negotiation/authentication/
   connection failure and OperationCanceledException for cancellation. Do not
   silently retry directly when the proxy is required.
6. Check a controlled nonresolving hostname, proxy-received hostname/port and
   actual target response. Test rejected authentication, no acceptable method,
   refused CONNECT and cancellation while negotiation waits. Retain the old
   socks5 route for mixed-runtime deployments; add no .NET 10 foundation edition.

## Constraints and alternatives

This is SocketsHttpHandler behavior, not a promise for browser-native, mobile
native or another custom HTTP handler. The measured platform is Windows x64
CoreCLR. SOCKS4, SOCKS4a, UDP, IDN conversion, IPv6 literals, HTTPS certificate
validation and production proxy interoperability are outside the executed scope.

SOCKS5 username/password authentication is not encrypted by SOCKS itself. Use an
appropriate trusted transport/network for the proxy; HTTPS protects the HTTP
payload independently. Sending a hostname to one proxy is not proof of global
DNS-leak prevention, anonymity or traffic isolation. Other application calls,
redirects, proxy-address resolution and system networking remain separate.
The negative local DNS check in this suite itself invokes local DNS deliberately.

An unavailable proxy or rejected CONNECT fails this request. The sample installs
no system DNS, hosts entry, route, service or persistent environment setting.
No production resource or public package publication is involved.

## Evidence

eng/Test-ProxyResponses.ps1 -Record -VerifySources compiles and runs both real
CairnRelay consumers on SDK10.0.402/runtime10.0.13 and
SDK11.0.100-rc.2.26504.105/runtime11.0.0-rc.2.26504.105, with zero warnings/errors.
The source manifest pins the actual paired SocksHelper implementations. The
recorder matches framework references to the runtime/security audit and records
compiler/CoreLib hashes, exact commands, inputs and runtime identity.

The loopback fixture implements the actual SOCKS5 wire exchange, receives
ATYP=3 plus relay.cairn.invalid and the target port, opens a real target socket,
and relays its HTTP response. Both unauthenticated and authenticated requests
succeed. Refused CONNECT, invalid credentials, no acceptable authentication
method and cancellation fail; the cancelled negotiation closes the tunnel.
Local DNS cannot resolve the chosen hostname. The old socks5h rejection is
executed separately, rather than inferred from metadata.

The ordinary built/packed tool is installed through a fresh local-only feed/cache
and inspected from an unrelated directory with real stdio list/get. Discovery
ID/title/goal, full canonical Markdown, target11/edition11, applicability and
fresh paired evidence are checked. Target10 returns no applicable edition;
the older comparison remains inside this recipe. Retrieval with unusable proxies
establishes proxy independence, not network isolation. No global DNS privacy,
other-platform/proxy or final-GA proof is claimed. PIN-959/PIN-962 and the exact
eight .NET 10 foundations retain their own obligations.
