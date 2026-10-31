# Validate TLS client certificates with an explicitly supplied chain

## Goal

A rehearsal-room service accepts a booking over mutual TLS only when the client's
certificate chains to its approved private root. Supply missing intermediate
certificates explicitly so the handshake does not depend on client-directed issuer
downloads. Keep issuer availability, root trust and revocation as separate choices.

## Older approach

On the measured .NET10 Linux/OpenSSL runtime, leaving CertificateChainPolicy unset
allows server-side AIA issuer downloads during client-certificate validation.
Here the context supplies private client-root trust independently of that policy:

{{snippet:older}}

The loopback client sends only its leaf certificate. Its AIA HTTP URL serves the
missing intermediate. .NET10 downloads it and completes a real handshake and
booking/ACK exchange. This old network fallback can work, but it introduces a
dependency on the issuer endpoint. Explicitly supplying intermediates is also a
valid .NET10 approach and is exercised separately on that runtime.

These shared helpers establish the complete private-fixture contexts:

{{snippet:contexts}}

`serverWithKey` is the private fixture's self-signed server certificate with DNS
SAN `room.test` and server-authentication purpose. `clientWithKey` has
client-authentication purpose and is issued by the private client intermediate.
Both have private keys. The approved root/server certificate come from fixture
setup, never from an untrusted booking. NoCheck deliberately removes revocation
networking from this offline exercise; it is not a production recommendation.

## Newer approach

.NET11 disables missing-intermediate downloads for server-side client validation
when no custom policy is supplied. The same leaf-only/default-policy fixture now
fails with PartialChain and zero issuer requests. Choose an explicit server policy
and provide its missing intermediate:

{{snippet:newer}}

ExtraStore supplies chain-building candidates; it does not make them trusted.
Only the independently approved root goes in CustomTrustStore. A custom policy's
DisableCertificateDownloads value is respected, so set it deliberately. The policy
also requires client-authentication purpose and keeps verification flags strict.

## Migration

1. Establish the application's approved client root, complete intermediate chain,
   server certificate/key, expected server DNS identity and revocation policy.
   Keep certificates alive through authentication and dispose them afterwards.
   The included recorder creates random private fixture material in isolated
   containers; it installs no roots. Real provisioning and key rotation are outside
   this guide's measured scope.
2. In a console project, place RoomContexts and the selected RoomTls helper in
   separate `.cs` files. Create a server context with
   `RoomContexts.Server(serverCertificate, approvedClientRoot)`. For the current
   route call `RoomTls.Create(context, approvedClientRoot, clientIntermediate)`.
   The older helper takes only the context for its default-policy comparison.
3. Create a connected TCP NetworkStream for each peer and wrap each in SslStream.
   On the server call AuthenticateAsServerAsync with the returned options and a
   bounded cancellation token. Concurrently, the client calls
   AuthenticateAsClientAsync with `RoomContexts.Client(clientCertificate,
   approvedServerCertificate)`. Hostname checking remains enabled for `room.test`.
4. Require successful mutual authentication before accepting bookings; exchange
   bytes and verify the reply. Treat AuthenticationException and transport failure
   as rejection. No permissive validation callback is needed: the fixture's
   observation callback returns true only for a present certificate and
   SslPolicyErrors.None. Chain trust alone is not authorization to a room/account;
   retain the application's separate client identity/permission checks.
5. Repeat in fresh process/cache state with the intermediate missing, then supplied,
   an unrelated trusted root and an expired client. Count AIA requests, not just
   successful handshakes. Stop listeners, dispose streams/certificates/keys and
   remove private caches. Keep online revocation tests separate from this NoCheck
   fixture.

## Constraints

Primary target: .NET11/net11.0, C#15 sample mode, framework TLS/X509 APIs. No NuGet
package, workload, rendering mode or experimental opt-in is needed. Actual test
pins are SDK10.0.402/reference10.0.13 versus SDK11.0.100-rc.2.26504.105/reference11RC2;
those describe verification, not universal exact-SDK installation requirements.
The supported server ExtraStore policy also runs on the older runtime.

Execution applicability is Linux x64 OpenSSL in the pinned Azure Linux containers;
Windows builds establish compilation only. Each real run uses private loopback
TCP/TLS and HTTP issuer listeners, fresh CA/key material, private HOME/cache/tmpfs,
a read-only root and network mode none (loopback is available). No shared trust
store is edited. The OS negotiates the TLS version; its observed value is saved.

This is one mutual-TLS flow, independent of experimental caller-owned TLS I/O.
Client-supplied full-chain contexts are another provisioning choice, but are not
taught or claimed as executed here. Compatibility switches and download opt-ins
are also outside the recipe. No online CRL/OCSP, production provisioning,
other-OS/OpenSSL/version/engine or final-GA claim follows. AIA policy is distinct
from Linux's separate issuer-fetch budget.

## Evidence

`eng/Record-CertificateLinks.ps1 -VerifySources` first compiles both real consumers,
capturing compiler/reference/CoreLib hashes and empty package dependencies. It then
executes four fresh digest-pinned runtime containers per release. Loaded libssl/
libcrypto paths/hashes, OpenSSL version, OS/runtime/architecture, certificate input
hashes, commands, callback chain status and cleanup are retained in separate
`linux.evidence.json` records through the existing evidence workflow.

| Case (client sends leaf only) | .NET10.0.13 | .NET11 RC2 |
| --- | --- | --- |
| No custom chain policy | Success, one AIA request and data exchange | PartialChain, zero AIA requests, no accepted booking |
| Server supplies intermediate; downloads disabled | Success, zero requests and data exchange | Success, zero requests and data exchange |
| Unrelated trusted root; intermediate supplied | Rejected, zero requests | Rejected, zero requests |
| Expired client; intermediate supplied | NotTimeValid rejection, zero requests | NotTimeValid rejection, zero requests |

Each fixture stops both listeners and discards its container/private cache/keys.
Compilation and native execution are classified separately. The ordinary packed,
isolated local-feed-installed tool returns discovery ID/title/goal, full canonical
Markdown, applicability and fresh evidence offline from an unrelated directory.
No .NET10 edition is added; eight foundations and existing recipe identities remain
intact. PIN-959/PIN-962 and existing Human gates remain open; RC2 is not GA proof.
