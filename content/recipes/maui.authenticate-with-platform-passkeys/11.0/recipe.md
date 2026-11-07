# Register and use passkeys through the native authenticator

## Goal

Heron Harbour lets a member register a device credential and later sign in. Device approval produces a response for the server; the app does not decide that a signature or account is valid.

## Older approach

.NET MAUI 10 has no equivalent shared Passkeys contract. Native platform bindings remain a valid integration choice, with separate lifecycle and error handling per platform. Browser-based OAuth through WebAuthenticator is also valid authentication, but is a different protocol and is not a native WebAuthn comparison implementation. No fabricated older Passkeys sample or supported .NET 10 route is claimed.

## Newer approach

{{snippet:samples-net11-0-heronkeys-ceremony-cs}}

Construct Ceremony with Passkeys.Default in the native host. Obtain fresh creation options from the RP server for the authenticated account, pass them to RegisterAsync, and post the resulting JSON back to the same server. For sign-in obtain request options, call SignInAsync and return the JSON for server verification. Preserve the server's correlation/session state across each pair of calls. A returned credential ID or successful device prompt is not sign-in success.

The RP server must generate unpredictable challenges, bind them to the ceremony/account, expire them and consume them once. It validates ceremony type, challenge, RP ID hash, native origin, required user presence/verification, signature and registration attestation policy. Store the verified public credential and counter with its owner. Check account confirmation/lockout policy before establishing a session. Use an established server verifier such as ASP.NET Core Identity; do not replace it with JSON field comparisons. Native API calls need an appropriate session/token design; a demonstration cookie is not a production token architecture.

Start on the active window's UI thread. Cancellation and user dismissal do not mean success; preserve recovery and alternative sign-in. Handle unsupported platforms separately from malformed options and provider failures. PreferImmediatelyAvailable is an optional client behavior flag, ignored on Windows, rather than a server verification policy.

## Migration

1. Retain the existing RP verification library, stable RP ID, authenticated credential-management flow and recovery route. Select the matching platform asset and build the canonical project with its exact SDK pin.
2. Configure the native host's active window and invoke the adapter from a deliberate user action. Fetch server options without rewriting their security fields. Do not log response material or account secrets.
3. For Android, host Digital Asset Links at the RP domain, binding the exact package name and SHA-256 signing certificate; debug and release certificates differ. Accept only their deliberate android:apk-key-hash origins on the server. Require Android 14+ and a configured credential provider/screen lock.
4. For iOS/Mac Catalyst, configure the webcredentials associated-domain entitlement and apple-app-site-association with the exact team/app identity. The server accepts the associated HTTPS origin. iOS API floor is 16; .NET 11's effective Mac Catalyst deployment floor is 17. These integrations are source-reviewed, not built here.
5. On Windows, configure Windows Hello or a compatible FIDO2 key, use Windows 10 1903+, and expect the native origin https:// followed by the RP ID. Windows has no Apple/Android association file. Keep a real active UI window.
6. Post responses to the RP server, check its verified result, and issue a session only there. Test registration, sign-in, denial/cancellation, wrong origin and replay independently when an approved route is available. PIN-936 records an explicit not-tested exclusion; no ceremony or server verification has run.

## Constraints

This sample is a compiled Windows adapter, not a complete MAUI application or a production authentication server. It requires an existing native host and configured relying-party endpoints. No external account, domain provisioning or signing/trust mutation is performed by Playbook. Android/Apple runtime checks are unavailable and explicitly not-tested under PIN-699. The actual native Windows host build failed NETSDK1147 (maui-tizen); this native ceremony is excluded under PIN-699. Adapter compilation must not be described as native proof.

## Evidence

The exact RC2 Windows Essentials asset compiles this adapter with SDK 11 RC3. Immutable platform source establishes JSON ownership, native origin derivation and window requirements. It does not prove a device ceremony or server interoperability. Under PIN-699, Pingu approved the PIN-701 native access exclusion and the PIN-936 not-tested ceremony disposition on 7 November 2026. No Human ceremony is pending for this unavailable route, and it is excluded from publication prerequisites. The exact sample/input identity remains in the existing compilation record. No endpoint, signing/trust setup or physical ceremony was provisioned. No GA, Android, Apple, browser or Windows Hello execution is claimed; no server challenge/response verification result is available.

## Canonical inputs

### samples/net11.0/HeronKeys/Ceremony.cs

{{snippet:samples-net11-0-heronkeys-ceremony-cs}}

### samples/net11.0/HeronKeys/HeronKeys.csproj

{{snippet:samples-net11-0-heronkeys-heronkeys-csproj}}

### samples/net11.0/HeronKeys/global.json

{{snippet:samples-net11-0-heronkeys-global-json}}

### samples/net11.0/HeronKeys/Directory.Build.props

{{snippet:samples-net11-0-heronkeys-directory-build-props}}

### samples/net11.0/HeronKeys/Directory.Build.targets

{{snippet:samples-net11-0-heronkeys-directory-build-targets}}

### samples/net11.0/HeronKeys/Directory.Packages.props

{{snippet:samples-net11-0-heronkeys-directory-packages-props}}
