# Race IPv4 and IPv6 connections deliberately

## Goal

Connect an Alder survey relay to a host with IPv4 and IPv6 addresses, return one
usable TCP socket and clean up the other attempts. Choose the connection policy
explicitly when replacing application-owned address scheduling. This is a client
connection decision, not a server listener's DualMode setting.

Copy each snippet into its own source file. It includes imports and the
Playbook.Samples namespace; add `using Playbook.Samples;` in the calling file.
Both console projects enable nullable and implicit usings, warnings as errors,
and use only Microsoft.NETCore.App. There is no NuGet package or workload.

## Older approach

.NET 10 supports DNS lookup and cancellable socket connections but has no
ConnectAlgorithm enum or overload. A valid application can resolve once, keep the
resolver's order and stagger attempts. This example waits 250 milliseconds between
scheduled starts, returns the first successful socket, cancels remaining work and
awaits every attempt before disposing losers. That delay is application policy;
this small scheduler does not implement all of RFC 8305. It also waits for the
whole DNS answer before connecting. Keep a mature existing scheduler when those
choices are important; the framework addition does not make it incorrect.

{{snippet:older}}

Call with `using Socket socket = await RelayConnector.ConnectAsync(host, port,
token);` after adding `using System.Net.Sockets;`. The caller owns the returned
socket. The comparison project targets net10.0 with C# 14.0; it adds no .NET 10
catalogue foundation. The explicit address-list method also makes a controlled
failed-first-address test possible without changing DNS or routes.

## Newer approach

The .NET 11 addition is the **static**, event-based
Socket.ConnectAsync(SocketType, ProtocolType, SocketAsyncEventArgs, ConnectAlgorithm)
overload. It is not a new Task/ValueTask instance overload. Use a DnsEndPoint
with AddressFamily.Unspecified (the constructor default) to leave both families
eligible. The wrapper translates its completion contract into a Task and retains
ownership until a connected socket is returned.

{{snippet:newer}}

For the fictional relay use relay.alder.test and its configured TCP port in a real
environment. The reproducible fixture substitutes localhost with checked loopback
answers; it never queries that fictional name or modifies the system resolver.
Add `using System.Net.Sockets;` at the call site, then call
`using Socket socket = await RelayConnector.ConnectAsync(host, port,
ConnectAlgorithm.Parallel, token);`. A CancellationTokenSource with a timeout can
bound the whole operation. Retain the using scope for the socket's entire use.

## Migration

1. Target net11.0 using a .NET 11 SDK/runtime. The canonical project declares
   C# 15.0; no preview language switch or experimental diagnostic suppression is
   needed for this reviewed API. Exact SDK11RC2/runtime11RC2 versions below are
   verification pins, separate from the recipe's release-family requirements.
2. Choose Default to retain the runtime's default sequential-address policy in
   the reviewed RC2 implementation, or Parallel to race IPv4 and IPv6 attempts.
   RC2 resolves the two address families separately for Parallel. It is not a
   promise to start every resolved address simultaneously, preserve an old
   scheduler's ordering/delay, prefer IPv6, or select a particular peer.
3. Pass a DnsEndPoint without constraining its address family. Parallel requires
   OS IPv4/IPv6 support for the dual-family route. A family-constrained endpoint
   cannot race both families; an IPEndPoint represents a single address. Inspect
   socket.RemoteEndPoint after success if the selected peer matters. A successful
   TCP connection does not authenticate the relay; add the application's TLS or
   other identity checks separately.
4. Subscribe to Completed before starting. A return of true means pending and
   completion arrives through the event; false means synchronous completion and
   no Completed event. Process both paths exactly once, inspect SocketError and
   obtain the actual socket from ConnectSocket. Do not dispose event args while
   the operation is still pending, or reuse them for another concurrent connect.
5. Bridge cancellation with Socket.CancelConnectAsync. Await completion before
   releasing event args, dispose a socket returned during a cancellation race,
   and transfer only the winner to the caller. Failure/cancellation can complete
   while another family has already reached the listener's accept queue. A server
   still needs its normal abandoned-connection handling.
6. Replace only the scheduling that this policy actually covers. Test dual-family
   success, either family failing, both failing and cancellation in your network.
   Do not derive a fixed latency improvement from loopback results; DNS delays,
   routing, refusal versus timeout, address order and remote services differ.

## Constraints and alternatives

These are shared framework networking APIs with no provider selection or UI/
rendering integration. Execution here is Windows x64 with Winsock/DNSAPI and both
loopback families available; other platforms, WAN blackholes and DNS outage
behaviour are not measured. The API has no experimental annotation in the
reviewed RC2 reference. This does not establish final GA behaviour.

If the application needs a single selected family, fixed address ordering, custom
DNS records, an existing connection pool or a fully controlled stagger policy,
retain the corresponding explicit approach. Parallel is a choice, not a general
resilience guarantee. Neither example sends application credentials or provisions
a production relay. No persistent DNS, hosts-file or route edit is required.

## Evidence

eng/Test-NetworkBuffers.ps1 -Record -VerifySources builds and executes the real
paired consumers using SDK10.0.402/runtime10.0.13 and
SDK11.0.100-rc.2.26504.105/runtime11.0.0-rc.2.26504.105. There are no resolved
package dependencies. Saved records retain input hashes, commands, actual loaded
CoreLib and compiler identities, audit-matching socket/name-resolution references,
and Windows ws2_32.dll/dnsapi.dll versions and hashes.

The fixture checks localhost resolves only to loopback and includes ::1 and
127.0.0.1. It binds separate IPv4/IPv6 sockets to the same ephemeral port; leaving
one bound but non-listening supplies real connection refusals. Both algorithms
connect with both/either listener, report the selected peer and exchange a byte
with that exact connection; with neither listener, the connect throws SocketException. Raw
event-args checks assert one event for pending operations and none for synchronous
completion; these Windows runs observed the pending branch. The source contract
also requires the synchronous branch handled by the wrapper.

Pre-cancellation and cancellation during pending TCP connects are exercised for
both algorithms. The latter fixture saturates a private IPv4 accept queue with
owned connections, checks the operation is pending, cancels and drains it, then
disposes filler sockets/listener. The older scheduler executes failed-first-v6
fallback and cancellation of delayed attempts. No mock, fixed-latency claim,
recurring OS matrix or public DNS dependency is involved. The ordinary installed
tool serves this complete canonical content offline. PIN-959 and PIN-962 retain
their comprehensive and actual-GA gates; the exact eight foundations are unchanged.
