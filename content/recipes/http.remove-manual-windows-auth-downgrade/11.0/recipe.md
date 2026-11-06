# Remove a verified manual Windows-authentication downgrade

## Goal

The fictional Gannet survey client talks to a trusted Windows-authenticated HTTPS
service. Remove its manual HTTP/1.1 override only after the actual .NET 11 client
successfully handles the service's HTTP/2 authentication challenge. Authentication
still completes over HTTP/1.1; this change does not make NTLM work over HTTP/2.

## Older approach

The .NET 10 comparison deliberately requests HTTP/1.1 for this known service.
This remains a valid route for applications running on .NET 10.

{{snippet:older}}

Our actual .NET 10.0.14 client receives 401 over HTTP/2 when it instead asks for
HTTP/2 with RequestVersionOrLower. The manual HTTP/1.1 request authenticates with
the existing Windows identity and returns 200. A newer SDK alone does not change
the runtime handling the request.

## Newer approach

Ask for HTTP/2 while allowing the runtime to retry over HTTP/1.1. In .NET 11,
SocketsHttpHandler recognises the Negotiate/NTLM challenge and performs that retry.

{{snippet:newer}}

Copy the complete helper into a net11.0 application with nullable and implicit
usings enabled. The canonical example selects C#15.0; this request syntax does
not itself require C#15. Reuse an application-owned HttpClient. For an explicitly
trusted Windows-auth service, its SocketsHttpHandler can use
CredentialCache.DefaultNetworkCredentials. Keep credential scope, redirects and
proxy policy deliberate; do not attach the current identity to arbitrary URLs.
Send the helper's request with SendAsync and a caller-owned CancellationToken,
then check IsSuccessStatusCode and the expected application response. Dispose the
request and response. Default credentials use the existing sign-in; the helper
does not provision accounts, obtain tickets or fix an incorrect SPN.

## Migration

1. Identify the specific manual Version11 override and the endpoint that required
   it. Record successful authentication and the actual response protocol first.
2. Run the same request on the actual .NET 11 runtime with Version20 and
   RequestVersionOrLower. Keep certificate validation and credentials unchanged.
3. Confirm the authentication challenge is followed by a successful HTTP/1.1
   response. Check server authentication and application authorisation as well
   as status; a public 200 response is insufficient proof.
4. Remove only that verified override. Retain manual HTTP/1.1 for the .NET 10
   route and any service whose tested client/server combination still needs it.
5. Keep RequestVersionExact when HTTP/2 is a requirement. Test anonymous,
   unauthorised and certificate-name failures; fallback must not turn them into
   successful access. Refresh the check when handler, proxy or server changes.

## Constraints

This is the SocketsHttpHandler route, not browser fetch or a native mobile HTTP
handler. It needs a service that permits HTTP/1.1, HTTPS with valid name/trust,
and an authorised identity compatible with the chosen authentication mechanism.
Kerberos needs its actual ticket/SPN environment; an observed NTLM exchange does
not establish Kerberos support for a deployment. Do not disable HTTP/2 globally,
relax TLS verification or weaken a strict version policy to force a passing test.

The fixture uses Windows x64 Kestrel with both protocols, the existing local
Windows sign-in and real SSPI NTLM. Its private short-lived localhost certificate
is trusted only by each client's CustomRootTrust chain policy. Name validation
remains enabled; no certificate store, trust, URL reservation, account, firewall
or persistent environment change is made. No ports are published beyond loopback.
Each case uses a fresh handler, so a prior authenticated connection cannot satisfy
a later negative case. The server is stopped and disposed on completion/failure.

The console consumers use SDK10.0.403/.NET10.0.14/C#14 and
SDK11.0.100-rc.3.26522.148/.NET11RC3/C#15. The fixture's separately locked
Microsoft.AspNetCore.Authentication.Negotiate and System.DirectoryServices.Protocols
versions are 10.0.14 and 11.0.0-rc.3.26522.148. The request helper itself needs no
NuGet package or workload. There is no experimental opt-in. RC evidence prepares
edition11.0 guidance; final-GA reconciliation remains open under PIN-962/PIN-959.

## Evidence

Run eng/Test-GannetGateway.ps1 -Record -VerifySources from the repository root
using the pinned SDK installations. It compiles both real dependency sets,
verifies the immutable publisher-source hashes, and captures the existing sample
evidence format. The canonical request helper is exercised by the matching case.

Both runtimes: manual authentication returns 200 HTTP/1.1 with NTLM; strict
HTTP/2 returns 401 without fallback; anonymous access remains 401; the public
control proves real HTTP/2 with 200; an authenticated but forbidden request is
403; a wrong certificate name fails TLS. Automatic mode returns 401 HTTP/2 on
.NET10 and authenticated 200 HTTP/1.1 on .NET11. No domain/Kerberos, production,
proxy, other OS or final-GA result is inferred. Linux TLS binding and HTTP.sys
hardening are separate goals.

## References

- [Official .NET 11 networking changes](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-11/libraries#http2-automatic-downgrade-for-windows-authentication).
- [Windows authentication with Kestrel](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/windowsauth?view=aspnetcore-11.0).
- Exact older/current publisher source URLs and hashes are retained in
  samples/GannetAuthentication/sources.json; compilation and execution evidence
  accompanies each canonical project. No future behaviour is inferred from a
  public property or a release headline.
