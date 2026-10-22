# Prioritize outgoing QUIC streams on one connection

## Goal

Give a small urgent dispatch notice preference over an archive transfer sharing one QUIC connection. Preserve timeouts and application flow control even when priorities differ.

## Older approach

.NET 10 supports QUIC streams but has no public QuicStream.Priority. An application can choose when to submit writes, pace background work or use bounded queues. The original comparison submits the notice first; completion of WriteAsync is not proof it reached the peer before archive bytes. This valid approach controls submission rather than the native scheduler.

{{snippet:older}}

## Newer approach

.NET 11 exposes byte Priority and DefaultPriority=127. Zero is lowest, 255 highest. Configure the two outgoing streams before submitting both writes; 240 and 16 express the dispatch policy. This API is not marked experimental in the verified RC2 references.

{{snippet:newer}}

## Migration

1. Use net11.0/C# 15.0, nullable and implicit usings; add Priorities.cs above and this Program.cs. Both QuicListener.IsSupported and QuicConnection.IsSupported must pass. Failure should select a separately designed fallback or fail clearly, never silently report successful QUIC execution.

{{snippet:usage}}

Add this test-only TransportCertificate.cs helper. It uses a private fictional identity and custom-root trust without certificate-store writes. Replace the private fixture trust/revocation policy with the real service's policy in production.

{{snippet:certificate}}

## Constraints

QUIC needs a supported MsQuic installation, TLS 1.3, a mutually supported ALPN, available UDP transport and peer stream capacity. Windows 11/Server 2022 or later provides the supported OS TLS route; Linux needs a compatible libmsquic/OpenSSL installation. macOS native setup is separately required. No package or workload is added to this console project, and Playbook does not install a native transport. Browser/WASI/mobile hosting is outside this recipe. The runtime's support probes do not establish reachability of an arbitrary service.

Priority affects outgoing data relative to other streams on this same connection. It does not prioritize connections, instruct the remote sender, bypass flow/congestion control, preempt bytes already sent or enforce latency/bandwidth/fairness. Higher priority may starve lower-priority work under sustained load. A protocol such as HTTP/3 may have its own priority semantics; setting this property is not an HTTP priority-header mapping. Keep application deadlines, cancellation and a bounded background queue. Do not assert arrival order from localhost timings.

The peer must read each stream until end of stream, and the sender must complete its writing side. The fixture uses completeWrites=true, reads both echoes, and disposes streams, connections, listener and certificate. In production choose meaningful application error codes and close policy. Only Windows x64 native execution is recorded; Linux/macOS setup/support is official-source context, not execution evidence.

## Evidence

Both pinned release projects establish a real loopback QUIC connection and exchange the two original messages on bidirectional streams. RC2 verifies default priority 127, sets 240/16, checks the configured values and exercises 0/255. Windows loads MsQuic 2.5.11.159415436. This tests the actual property/native call and successful transfers, not wire scheduling order, congestion, packet loss, fairness or production throughput. No mock connection or additional CI platform matrix is used. RC2 verification is not GA proof.
