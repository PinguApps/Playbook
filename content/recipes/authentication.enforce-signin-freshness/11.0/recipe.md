# Enforce requested sign-in freshness

## Goal

Validate signed authentication time against the effective max_age request, preserving protected transaction state and rejecting missing, invalid, stale or future assertions.

Saltmarsh Observatory requires a recent provider sign-in before accepting an observer. A valid signature authenticates the assertion; signed auth_time must also satisfy requested freshness.

## Documented change and observed limitation

The pinned official documentation describes built-in .NET11 max_age/auth_time validation. Actual OpenIdConnect11.0.0 here accepts missing, invalid, stale and future auth_time without the guard. Run --observe-builtin to observe it; default execution enables the guard and asserts rejection. Both complete consumers disclose this limitation instead of treating the package as fixed.

## Request, state and validation

Effective max_age can originate from options, OpenIdConnectChallengeProperties, AdditionalAuthorizationParameters or OnRedirectToIdentityProvider. Four independent schemes exercise each source alone. Compose existing redirect customization first, then call Freshness.Capture last. It reads the final outgoing value, parses nonnegative integral seconds using invariant culture, and records it in AuthenticationProperties.Items BEFORE the handler protects state. An absent value removes the entry and adds no auth_time requirement. Do not trust callback parameters as evidence of the original requested age.

Freshness.Validate inspects raw signed JSON from the already signature/issuer/audience-validated ID token, before accepting a session. It rejects missing, string, fractional, negative and out-of-range NumericDate values. Elapsed time may equal max_age plus ClockSkew; a future time may equal current time plus ClockSkew. Deterministic boundary controls complement real handler checks. Keep nonce, correlation and protected-state validation. Neither a signed assertion nor this guard proves a physical authentication ceremony or protects against a compromised provider.

## PAR, flow boundaries and compatibility switch

For PAR, finish max_age customization in OnRedirectToIdentityProvider before capturing state and pushing. The documented framework contract rejects OnPushAuthorization changes while validation is active unless the push is skipped. With this workaround prohibit late mutation, or disable PAR until final request/state binding is verified. The fixture tests implicit single-ID-token callbacks, not PAR, code redemption, hybrid's two tokens or external-provider interoperability. A final-token event does not establish validation of BOTH hybrid ID tokens; verify each token path before applying this workaround to another flow.

The documented temporary process-wide switch is Microsoft.AspNetCore.Authentication.OpenIdConnect.DisableMaxAgeValidation. Set it with AppContext.SetSwitch(name, true) before authentication, or in runtimeOptions.configProperties. Its documented default is false. It removes requested freshness protection across ALL schemes/challenges. Remove it or set false after provider correction. The actual unguarded artifact already accepts invalid assertions here; the samples do not use or need this switch, and it does not disable the explicit guard.

This check neither changes local cookie lifetime nor covers refresh-token redemption or UserInfo. Keep those policies independently. The fixture uses ephemeral Data Protection, a local RSA signer and HTTP loopback cookies for automated checks only. Production requires HTTPS, secure cookies, durable keys, provider signing-key lifecycle and existing authentication/authorization policy. No external identity provider or paid service is contacted.

## Older approach: guarded .NET10 comparison

SDK10.0.404, C#14.0. Save the files together, restore once, then restore --locked-mode and run --configuration Release. The executable checks behavior and exits.

### BoundaryChecks.cs

{{snippet:n10-boundarychecks-cs}}

### Directory.Build.props

{{snippet:n10-directory-build-props}}

### Directory.Build.targets

{{snippet:n10-directory-build-targets}}

### Directory.Packages.props

{{snippet:n10-directory-packages-props}}

### Freshness.cs

{{snippet:n10-freshness-cs}}

### ObservatoryFreshness.csproj

{{snippet:n10-observatoryfreshness-csproj}}

### Program.cs

{{snippet:n10-program-cs}}

### global.json

{{snippet:n10-global-json}}

### packages.lock.json

{{snippet:n10-packages-lock-json}}

## Newer approach: guarded .NET11 comparison

SDK11.0.100, C#15.0. Save the files together, restore once, then restore --locked-mode and run --configuration Release. The executable checks behavior and exits.

### BoundaryChecks.cs

{{snippet:n11-boundarychecks-cs}}

### Directory.Build.props

{{snippet:n11-directory-build-props}}

### Directory.Build.targets

{{snippet:n11-directory-build-targets}}

### Directory.Packages.props

{{snippet:n11-directory-packages-props}}

### Freshness.cs

{{snippet:n11-freshness-cs}}

### ObservatoryFreshness.csproj

{{snippet:n11-observatoryfreshness-csproj}}

### Program.cs

{{snippet:n11-program-cs}}

### global.json

{{snippet:n11-global-json}}

### packages.lock.json

{{snippet:n11-packages-lock-json}}

## Evidence and local verification

Actual signed-token OIDC handler: four independent max_age sources and absent control; valid/missing/invalid/string/fractional/negative/range/stale/future cases; signature, nonce and state controls. Separate deterministic invariant parsing and inclusive skew-boundary tests. Saved records retain actual SDK, dependencies, commands and input hashes. Run eng/Test-ObservatoryAuthentication.ps1; -RecordEvidence refreshes affected records. Windows x64 only. No browser, other-provider or other-OS deployment claim. The eight .NET10 foundations remain unchanged. PIN-967 and unrelated Human/experimental gates remain separate.

## Official reference

[Documented GA contract, with the observed limitation above](https://github.com/dotnet/AspNetCore.Docs/blob/fd01aafa737af65203d3bd4ea08f1920305c52a9/aspnetcore/breaking-changes/11/openid-connect-max-age-validates-auth-time.md)

## Migration

1. Inventory every requested freshness source and existing redirect/token event.
2. Compose customization before Capture and Validate before accepting the session; preserve all protocol checks.
3. Run the complete paired consumers and verify the deployed provider/token flows before rollout.

## Constraints

Only the explicit guard behavior is established by the samples. Keep production authentication, authorization, HTTPS and key management; fixture HTTP and ephemeral keys are local test configuration. Other providers, hybrid/PAR paths and other operating systems require their own focused validation.
