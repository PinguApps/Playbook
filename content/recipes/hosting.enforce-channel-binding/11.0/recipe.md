# Expose or require transport binding for authenticated requests

## Goal

A harbour operations service wants its Windows authentication policy to account for the TLS connection. Choose the HTTP.sys hardening level explicitly and inspect the endpoint channel-binding token without logging its bytes. Token availability is diagnostic information, not proof that an authentication exchange was protected.

## Older approach

The .NET 10 comparison serves a real HTTP.sys HTTPS request using the existing owner-prepared certificate. Its public ITlsConnectionFeature has no TryGetChannelBindingBytes method and HttpSysOptions has no hardening-level property. Keep a valid existing Windows authentication deployment and its explicitly reviewed Extended Protection configuration; do not replace authentication with an unrelated browser token-binding feature. This bounded comparison does not implement a native authentication-policy shim.

## Newer approach

The .NET 11 sample chooses each HttpAuthenticationHardeningLevel and reads ITlsConnectionFeature.TryGetChannelBindingBytes with ChannelBindingKind.Endpoint inside the request. Legacy neither validates channel binding nor exposes its token. Medium validates supplied tokens but permits their absence. Strict requires tokens on authenticated requests. Medium is the .NET11 default; choose deliberately rather than relying on an upgrade default.

The fixture deliberately permits anonymous requests so it can isolate token exposure from account/domain configuration. Real HTTPS requests on Windows build26200 observed no token for Legacy and 85 bytes for Medium/Strict. Anonymous success under Strict does not demonstrate acceptance or rejection of authenticated clients. Never describe these checks as relay-attack proof.

Kestrel obtains connection binding from its TLS transport; HTTP.sys and IIS obtain it through their request features. HTTP.sys options do not configure Kestrel or IIS. Check feature availability and handle an unavailable binding; HTTPS alone does not make every binding kind supported. No IIS hosting execution or Kestrel authentication-enforcement result is claimed by this sample.

Save each version's complete files in its own directory. The shared current fixture also exercises the separate native-property recipe.
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

1. Confirm Windows HTTP.sys, HTTPS and the target11 framework. Keep the prior version until migration is deliberate. Exact sample baselines are SDK10.0.402/runtime10.0.13/C#14 and SDK11.0.100-rc.2.26504.105/runtime11 RC2/C#15.
2. Obtain a dedicated owner-authorized loopback certificate binding and URL reservation. Preserve all existing resources. The access runbook is repository authoring context; production guidance requires the application's own approved binding and authentication configuration.
3. Set PLAYBOOK_HTTPS_PREFIX to the reserved HTTPS prefix and PLAYBOOK_CERTIFICATE to its exact SHA1 thumbprint for this fixture. Restore each project, build Release and execute its DLL with the matching absolute dotnet executable. The callback pins this one certificate and is not general trust or hostname validation.
4. Expect the old HTTPS comparison to pass. Expect the new fixture to check Legacy/Medium/Strict exposure, empty/small/exact SNI buffers, no-SNI flags and invalid-property error87. These checks are Windows/x64 execution, not cross-compilation or authentication-enforcement proof.
5. For a real service, preserve authentication and authorization, select the hardening policy compatible with its clients/proxy topology, handle unavailable features/errors and avoid logging token bytes. Refresh focused security checks when those inputs change.
6. Stop the fixture and have the owner remove only this test's exact reservation, binding and certificate, using its saved ownership manifest. Shutdown does not remove persistent resources.

## Constraints

No external NuGet packages, database, account, optional workload or rendering mode is required. Framework assets are Microsoft.AspNetCore.App at the declared net10/net11 targets. The native property layout comes from the matching Windows SDK header; the managed sample needs no native compilation. Actual authoring uses Windows26200 x64; other Windows versions, IIS, authenticated relay rejection and absent-native-API execution are not tested. APIs have stable intent; actual artifacts remain RC2, not final GA evidence. Existing product-wide compilation and GA gates remain open. No public package publication or production resource provisioning is performed.

## Evidence

See the official references in this recipe's source metadata. Microsoft Learn's HTTP_REQUEST_PROPERTY reference defines the property/structure; the HttpAuthenticationHardeningLevel reference defines the security modes. Saved sample evidence records exact compilation/execution commands and input hashes; they establish only the claims described above.
