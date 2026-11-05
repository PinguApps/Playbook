# Query an HTTP.sys request property safely

## Goal

A harbour diagnostics endpoint needs the native Server Name Indication result for its actual TLS request. Query the supported HTTP.sys property while the request is alive, using the matching Windows header layout and bounded storage. Do not substitute the HTTP Host header for TLS SNI.

## Older approach

The .NET10 sample retains a working HTTPS endpoint and pinned-certificate client. Its public IHttpSysRequestPropertyFeature does not expose the generic TryGetRequestProperty API. If existing code only needs a separately exposed handshake field, keep that supported field; generic property access requires a deliberately maintained native host or migration to the new managed surface. This comparison does not invent a .NET10 generic-property backport or expose private request queue handles through reflection.

## Newer approach

The .NET11 sample reads property4, HttpRequestPropertySni. Windows SDK10.0.26100.0 http.h defines HTTP_REQUEST_PROPERTY_SNI as WCHAR Hostname[256] followed by ULONG Flags: 512 UTF-16LE bytes plus four little-endian flag bytes, total516. The qualifier is empty because SNI has no qualifier. This Windows layout is not a portable structure definition for arbitrary properties.

The actual IP-literal HTTPS request returns an empty hostname and flags2 (NO_SNI). Empty and 515-byte destinations return false with bytesReturned=0 on this qualified Windows26200/.NET11 RC2 route. The exact516-byte destination returns true and bytesReturned=516. The XML contract suggests size discovery with an empty buffer, but the measured fixed-layout property does not provide the size there. Use the reviewed fixed size; never allocate from an unchecked returned integer or parse a failed query. Other properties can have different requirements.

The invalid property int.MaxValue throws HttpSysException with native error87. Missing operating-system API support is documented to throw InvalidOperationException; an OS lacking the API was not available for execution here. This is an explicit not-tested limitation, not a mocked success. Catch the documented exceptions at the application's diagnostic boundary and report unsupported access rather than fabricating a value. A missing feature on a different server also requires a supported fallback. Do not retain a live request feature after request completion.

The fixture's host is real Windows HTTP.sys with HTTPS, not a feature stub. It also verifies channel-binding exposure. Kestrel and IIS are different hosts; do not assume they expose this HTTP.sys feature. No cached ClientHello flag or system-wide registry change is needed for this SNI property.
### n10-directory-build-props
{{snippet:n10-directory-build-props}}

### n10-directory-build-targets
{{snippet:n10-directory-build-targets}}

### n10-directory-packages-props
{{snippet:n10-directory-packages-props}}

### n10-global-json
{{snippet:n10-global-json}}

### n10-harbourrequests-csproj
{{snippet:n10-harbourrequests-csproj}}

### n10-program-cs
{{snippet:n10-program-cs}}

### n11-directory-build-props
{{snippet:n11-directory-build-props}}

### n11-directory-build-targets
{{snippet:n11-directory-build-targets}}

### n11-directory-packages-props
{{snippet:n11-directory-packages-props}}

### n11-global-json
{{snippet:n11-global-json}}

### n11-harbourrequests-csproj
{{snippet:n11-harbourrequests-csproj}}

### n11-program-cs
{{snippet:n11-program-cs}}


## Migration

1. Confirm Windows HTTP.sys, HTTPS and the target11 framework. Keep the prior version until migration is deliberate. Exact sample baselines are SDK10.0.403/runtime10.0.13/C#14 and SDK11.0.100-rc.3.26522.148/runtime11 RC2/C#15.
2. Obtain a dedicated owner-authorized loopback certificate binding and URL reservation. Preserve all existing resources. The access runbook is repository authoring context; production guidance requires the application's own approved binding and authentication configuration.
3. Set PLAYBOOK_HTTPS_PREFIX to the reserved HTTPS prefix and PLAYBOOK_CERTIFICATE to its exact SHA1 thumbprint for this fixture. Restore each project, build Release and execute its DLL with the matching absolute dotnet executable. The callback pins this one certificate and is not general trust or hostname validation.
4. Expect the old HTTPS comparison to pass. Expect the new fixture to check Legacy/Medium/Strict exposure, empty/small/exact SNI buffers, no-SNI flags and invalid-property error87. These checks are Windows/x64 execution, not cross-compilation or authentication-enforcement proof.
5. For a real diagnostic endpoint, preserve authentication and authorization, validate the documented property layout and successful byte count before parsing, handle unavailable features and native errors, and use the request feature only during the live request. Refresh the focused checks when those inputs change.
6. Stop the fixture and have the owner remove only this test's exact reservation, binding and certificate, using its saved ownership manifest. Shutdown does not remove persistent resources.

## Constraints

No external NuGet packages, database, account, optional workload or rendering mode is required. Framework assets are Microsoft.AspNetCore.App at the declared net10/net11 targets. The native property layout comes from the matching Windows SDK header; the managed sample needs no native compilation. Actual authoring uses Windows26200 x64; other Windows versions, IIS, authenticated relay rejection and absent-native-API execution are not tested. APIs have stable intent; actual artifacts remain RC2, not final GA evidence. Existing product-wide compilation and GA gates remain open. No public package publication or production resource provisioning is performed.

## Evidence

The official HTTP_REQUEST_PROPERTY reference defines SNI and its native structure; the managed TryGetRequestProperty reference in the source metadata defines buffer handling and exceptions. Saved sample evidence records exact compilation/execution commands and input hashes; they establish only the claims described above.
