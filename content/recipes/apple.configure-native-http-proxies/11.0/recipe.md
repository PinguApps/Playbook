# Route Apple HTTP sessions through a deliberate proxy

## Goal

Configure native Apple HTTP sessions per destination/proxy policy and keep CONNECT proxy credentials separate from origin authentication.

## Newer approach

Tern's survey uploader sends sensitive records only to a fixed HTTPS origin. Give
each destination/proxy policy its own long-lived client. Configure the handler
before the first send, obtain credentials from the application's secret source,
and dispose the client when that route is retired. Never embed passwords in a URI.
The canonical proxy has no local bypass or bypass list. The factory disables redirects: check any redirect against your origin
allowlist before sending with another route. BaseAddress does not restrict absolute
request URIs; the caller must enforce that boundary.

NSUrlSessionHandler freezes proxy configuration for the session using its first
destination. An IWebProxy bypass list or GetProxy decision is not reevaluated for
each later destination. Sharing one handler across differently routed hosts can
send a request directly or through the wrong proxy. Even a bypassed first request
can determine later routing. Avoid a destination-dependent proxy on a shared client.

UseProxy=false overrides system proxies; UseProxy=true with Proxy=null uses system
configuration. A custom proxy must use an http URI. An https proxy URI means TLS
to the proxy itself and is rejected; HTTPS destinations over HTTP CONNECT remain
supported. Do not confuse those two protocols.

Proxy.Credentials takes precedence over DefaultProxyCredentials. These are proxy
credentials, separate from handler Credentials for origin challenges. Native proxy
authentication is supported for HTTPS CONNECT. For a plain HTTP destination the
native stack does not deliver that proxy challenge; an authenticating proxy can
return 407 without these credentials being used. Do not add Proxy-Authorization
to origin requests to work around this limitation.

Origin Digest authentication is a separate capability. Configure an origin-scoped
CredentialCache with the expected Digest scheme on handler Credentials before any
send, using a secret supplied by the caller. Do not reuse proxy passwords as origin
credentials. Preserve TLS certificate validation. A successful CONNECT does not
prove origin authentication or permission to access a resource.

{{snippet:net11-ternproxy-csproj}}

## Older approach

The .NET10 comparison uses SocketsHttpHandler with an explicit proxy. It is a valid
managed route with different platform integration. It is not evidence that the
new native handler's proxy behavior exists on .NET10. Applications that require
native system networking can retain OS-configured proxies on the older workload.
Neither choice makes a per-request proxy selector safe on the new native session.

## Migration

1. Inventory origins, bypass rules, proxy schemes and required auth schemes.
2. Partition clients by destination/proxy policy; configure before first use.
3. Replace the managed factory only where native integration is intended.
4. Keep proxy and origin credentials separate; preserve certificate validation.
5. On an available Apple host, use controlled origins and a proxy recording
   CONNECT/407 challenges. Check bypass-first versus proxied-first behavior,
   HTTPS credentials, plain HTTP 407 and rejected secure-proxy URIs. Those Apple
   endpoint observations have not run here; source review cannot establish them.

## Canonical samples

### net10-directory-build-props

{{snippet:net10-directory-build-props}}

### net10-route-cs

{{snippet:net10-route-cs}}

### net10-ternproxy-csproj

{{snippet:net10-ternproxy-csproj}}

### net11-directory-build-props

{{snippet:net11-directory-build-props}}

### net11-route-cs

{{snippet:net11-route-cs}}

### net11-ternproxy-csproj

{{snippet:net11-ternproxy-csproj}}

## Constraints

The selected runtime, native toolchain and route policy must match applicability. No signing, trust changes or service provisioning are part of these examples.

## Evidence

The .NET 11 SDK baseline is now 11.0.100 GA with preview selection disabled. The .NET 10 comparisons retain their declared SDKs. Use the current sample pins, dependency locks and GA verification records for current coverage. Existing native exclusions and pending Human gates still apply.

The canonical net11 proxy C# compiles against the pinned GA iOS reference assembly via eng/Test-AppleRoutes.ps1 -CompileReference. This is reference compilation, not a native workload build or execution.


SDK10.0.402/C#14 and SDK11.0.100/C#15 are intended sample pins. No preview language syntax or experimental API opt-in is used. The .NET10 examples compile on Windows; no native app execution is claimed. Exact Apple GA sources at 34b3b25d471c22f2a62c36ec0971b021d0a0e000 are checked by eng/Test-AppleRoutes.ps1. Current workload compilation, native publishing/signing, controlled Apple endpoint behavior and runtime/registrar execution are explicitly not-tested under PIN-699 because Apple runtime/Xcode is unavailable. Source review is not compilation or execution. No pending Human proof, package publication or GA certification is claimed. Packed-tool list/get verifies offline guidance delivery only. See applicability for exact source and asset identities.
