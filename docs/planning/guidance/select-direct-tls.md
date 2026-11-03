# Select a direct TLS transport (experimental draft)

Historical planning draft, superseded by the
[served recipe and explicit unavailable-API example](../../../content/recipes/hosting.select-direct-tls/11.0/recipe.md).
The current example remains source-reviewed only; use the recipe's current Evidence section.

Use this decision guide when a Linux host needs explicit control over the
connection implementation serving HTTPS. Intended identity:
`hosting.select-direct-tls/11.0` (PIN-1181 / W11).
The matching public endpoint API remains unavailable in the inspected package;
no direct-TLS sample is offered as working code.

## Applicability and availability

NuGet's DirectTls index returned only `11.0.0-rc.1.26425.128` on 3 November 2026.
Its nuspec targets `net11.0`, requires Connections.Abstractions at least
`11.0.0-rc.1.26425.128` and references Microsoft.AspNetCore.App. Source commit:
`3551975be08744f0418857c5bed8ab1545c5dd47`.

The DLL exposes `WebHostBuilderDirectTlsExtensions` and
`DirectTlsTransportOptions`, but no public `DirectTlsEndpoint`.
XML references to that missing type do not make it callable. Registering
`UseDirectTls` alone does not reconcile documented endpoint selection.

The intended route needs Linux, OpenSSL and matching native runtime TLS support.
It is experimental (`ASPNETCORE_DIRECTTLS_001`). No OpenSSL/runtime combination,
dependency closure or native execution is established here. SDK 11 RC2 is an
intended future baseline; it does not make an RC1 package implement an RC2 API.
Windows compilation would not establish Linux support. No .NET 10 backport,
Windows transport or GA contract is claimed.

## Previous approach: retain ordinary Kestrel HTTPS

A fictional harbour archive serves catalogue requests through Kestrel's usual
socket transport with HTTPS configured on its endpoint. Retain that .NET 10
route when it meets the need. Keep certificate selection and protocol policy
explicit. No direct native OpenSSL equivalent is claimed for the older target;
ordinary HTTPS remains a valid approach.

## Migration decision

1. Establish the need for direct TLS. Retain ordinary socket HTTPS as the
   baseline and fallback while qualifying the alternative.
2. Obtain a pinned supported artifact exposing the documented endpoint API.
   Inspect the public surface and restore its full dependencies. Stop the
   direct-TLS migration here while the endpoint type is absent.
3. Reconcile official instructions with runtime native requirements. Record
   Linux distribution/architecture, OpenSSL, runtime and SDK separately; managed
   metadata does not prove native support.
4. Once the contract exists, author a harbour host with ordinary socket HTTPS
   and explicitly selected direct TLS as separate controls. Configure certificate
   and protocols, and scope any experimental acknowledgement to the relevant API.
5. Exercise actual HTTPS success and deliberate TLS negotiation failure.
   Identify the selected transport independently: a successful response alone
   cannot distinguish direct TLS from default sockets.
6. Record exact conditions and offer only the qualified configuration. Preserve
   the default socket route elsewhere.

Steps 3–6 are a future qualification path. Do not copy an unavailable endpoint
example, fabricate a shim or teach a temporary preview contract as GA.

## Deferred checks

Resolve the artifact mismatch first. Compile original .NET 10/11 hosts, then run
both transport controls and success/failure checks on a suitable Linux runtime.
Follow the [native testing policy](../../contributing.md#native-testing-policy)
for unavailable environments; keep the separate artifact prerequisite explicit.
Source review, cross-compilation and mocks are not native execution.

Canonical samples, evidence, served metadata and installed-tool retrieval remain
outstanding. No trust changes, production resources, native installation or new
CI matrix are included. PIN-959 and actual-GA reconciliation remain open.

## References

- [Pinned DirectTls package](https://www.nuget.org/packages/Microsoft.AspNetCore.Server.Kestrel.Transport.DirectTls/11.0.0-rc.1.26425.128)
- [Official Kestrel endpoint configuration](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/endpoints)
- [W11 audit and availability finding](../audits/web.md)
