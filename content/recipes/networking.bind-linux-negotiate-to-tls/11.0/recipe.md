# Bind Linux Negotiate authentication to TLS

## Goal

The fictional Gannet research station supplies its live TLS endpoint binding to
Kerberos authentication. On the measured .NET 11 Linux GSSAPI route, an exchange
with the matching binding succeeds and one bound to a different TLS endpoint is
rejected. Binding and Policy are existing options; the change is usable Linux
server behavior, not a new property declaration.

The .NET 11 GA/MIT Kerberos check rejects both a mismatched binding and an omitted
client binding with BadBinding under PolicyEnforcement.Always. Earlier RC omission
results remain historical. Repeat these admission checks on the actual deployment's
runtime and provider; the measured Linux route does not establish other providers.

## Older approach

Keep the required policy and refuse unsupported authentication. This code compiles
against .NET 10; on the actual .NET10.0.14 Linux runtime its required-policy
construction throws PlatformNotSupportedException before exchanging a token.

{{snippet:older}}

The honest older alternative for this service is to leave the protected operation
unavailable, or use a separately verified supported deployment. Do not catch that
exception and retry with Never/WhenSupported, discard the binding, or describe an
unbound successful exchange as protected. A Windows HTTP.sys test establishes no
Linux result and remains a different recipe.

## Newer approach

The same options compile against .NET 11. The Linux native bridge now forwards
the binding's application data to gss_accept_sec_context. Our actual Kerberos
exchange demonstrates mismatch and omission rejection on GA. The required policy
remains enabled throughout the test.

{{snippet:newer}}

Use the complete GannetProtection class with an application-owned ChannelBinding
for the actual TLS endpoint. For this bounded SHA256-with-RSA certificate route,
the following adapter computes RFC5929 tls-server-end-point from the authenticated
server SslStream.LocalCertificate and exposes the SEC_CHANNEL_BINDINGS layout.
It rejects other certificate signature algorithms rather than guessing a hash.
The canonical consumer independently compares its application data with the real
TLS client's TransportContext.GetChannelBinding(Endpoint) result before any
Kerberos exchange. This is neither a fabricated native return nor a fake TLS
validation callback.

{{snippet:binding}}

The adapter also needs System.Net.Security, System.Runtime.InteropServices,
System.Security.Authentication, System.Security.Authentication.ExtendedProtection,
System.Security.Cryptography, System.Security.Cryptography.X509Certificates and
System.Text imports, as in the canonical Program.cs. On our Linux server,
SslStream.TransportContext.GetChannelBinding(Endpoint) throws because there is no
peer certificate in server-only TLS; do not assume its client-side use works on
the server. Read the actual local certificate, not an unrelated configured file
that could differ after certificate selection. Framework-managed Kestrel binding
exposure is a separate integration route, not what this console sample proves.

Create the binding only after successful TLS authentication. Keep the binding
handle, TLS connection and authentication objects alive throughout the exchange;
dispose authentication first, then binding, then TLS. Never send raw credentials,
keytabs or binding buffers in logs. Exchange opaque authentication tokens through
the established TLS stream. Continue only for ContinueNeeded; accept only a final
Completed status and the expected mechanism/protection. Reject every other status
without retrying under a weaker policy. In particular the measured runtime can
report IsAuthenticated=true after BadBinding: that property alone is insufficient.
Application authorisation remains a separate check after authentication.

## Migration

1. Record the actual runtime and native mechanism/provider. Verify the service's
   required mismatch and omission rejection on that runtime/provider combination.
2. Establish and validate TLS with the expected server name, trust chain and
   certificate. Obtain the endpoint binding from that live connection. This
   adapter supports SHA256-with-RSA only; do not apply its hash to other algorithms.
3. Supply the server binding and explicit Always policy on .NET 11. Reject a
   missing server binding before constructing the authentication context. Leave
   the older unsupported route closed.
4. Test real credentials with matching, different-endpoint and omitted-client
   bindings. Inspect the final status, negotiated mechanism and service admission;
   do not infer security from a reference assembly, constructor or boolean alone.
5. Test unavailable client credentials and unknown service principals. Capture
   exact native dependencies, inputs and results; rerun on provider/runtime changes.
   Require the omission-rejection gate to pass before enabling this route.

## Constraints

Canonical projects select SDK10.0.403/C#14 and SDK11.0.100/C#15,
with actual .NET10.0.14 and .NET11.0.0 GA Linux x64 execution. Both are ordinary
nullable/implicit-usings console consumers using framework System.Net.Security;
there is no NuGet package, workload, experimental diagnostic or managed-NTLM
server opt-in. C#15 is the configured current sample mode, not a new API language
requirement. Preserve the existing eight .NET10 foundation editions.

The measured provider is MIT Kerberos1.20.1-6ubuntu2.10, with matching
libgssapi-krb5-2/libkrb5-3 and OpenSSL3.0.13-0ubuntu3.16 on Ubuntu24.04 x64.
The private realm has a surveyor principal, a service principal
HTTP/station.gannet.test, an acquired client ticket cache and the corresponding
server keytab. KRB5_CONFIG selects its realm/KDC; KRB5CCNAME selects its client
cache; KRB5_KTNAME selects its acceptor keytab. For an application, configure
these privately with least-privilege access; do not commit credentials. DNS/SPN,
clock, ticket lifetime, key version and keytab permissions must match the actual
deployment. No production realm, identity or account is required by the fixture.

The test generates a real private KDC/keytabs only inside a disposable container,
publishes no ports and disconnects its external network after package setup.
TLS uses real loopback sockets, generated short-lived certificates and a private
CustomRootTrust chain policy with ordinary name validation; no trust store is
changed. The mismatched token uses a second endpoint with a different certificate
and travels through the first TLS connection. A different connection using the
same certificate need not have a different tls-server-end-point binding: this is
certificate endpoint binding, not proof of a unique TCP session.

Linux support does not mean all Unix implementations. The publisher's managed
source explicitly excludes macOS because its GSS.framework accepts mismatches.
Other GSSAPI mechanisms/providers, architectures, channel-binding kinds, native
NTLM servers, proxies, browser token binding and HTTP.sys hardening are not
established here. Unsupported/platform and credential failures must stay closed.
Unrelated PIN-962, PIN-959 and Human approvals retain their independent obligations.

## Evidence

The .NET 11 SDK baseline is now 11.0.100 GA with preview selection disabled. The .NET 10 comparisons retain their declared SDKs. Earlier RC audit details below are historical; use the current sample pins, dependency locks and GA verification records for current coverage. Existing native exclusions and pending Human gates still apply.

eng/Test-GannetBinding.ps1 -Record compiles both actual consumers and runs the
real TLS/GSSAPI scenarios using the existing sample-evidence format. It validates
the .NET11 Linux runtime archive against the publisher's SHA512 and records the
digest-pinned .NET10 SDK image, native package versions and managed/native hashes.
Docker and the pinned image are prerequisites; package setup needs network access.
Its finally block removes the container and its KDC, tickets, keytabs and database.

Observed .NET10: OSSupportsExtendedProtection=false; required policy is refused
before the exchange. Observed .NET11: matching returns Completed/Completed with
Kerberos; mismatched and omitted client bindings return server BadBinding;
an unknown service returns TargetUnknown; an absent client cache returns
UnknownCredentials; a missing server binding throws before authentication.

eng/Test-GannetBinding.ps1 -RequireMissingBindingRejection is the stronger
qualification check. It passes on the measured GA/MIT route. Preserve that gate
and rerun it when the runtime or native authentication provider changes.

## References

- [Official .NET 11 TLS channel-binding description](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-11/libraries#tls-handshake-hardening).
- [MIT GSSAPI credential/keytab model](https://web.mit.edu/kerberos/krb5-latest/doc/appdev/gssapi.html).
- [RFC5929 endpoint binding](https://www.rfc-editor.org/rfc/rfc5929#section-4).
- Immutable old/current publisher managed/native source URLs and hashes are in
  samples/GannetAuthentication/sources.json. The source audit's original claims
  remain distinguishable from this actual consumer/provider evidence.
