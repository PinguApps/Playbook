# Explain refused passkey sign-in without bypassing account policy

## Goal

Handle real Identity LockedOut and NotAllowed passkey results in a custom sign-in interface while preserving shared pre-sign-in checks.

The fictional Beacon stores team keeps a loan key and a spare key. People need to recognise their saved keys and understand why a cryptographically valid credential can still be refused. The two independently discoverable recipes share this small, complete test application.

## Older approach — .NET 10

The existing release supports passkeys and user-chosen names. It does not expose the stored AAGUID used by the newer template, so prompt for a name rather than guessing from transport or user-agent strings. Manual naming remains useful for distinguishing two keys from the same provider.

The original .NET 10.0.0 SignInManager source omitted PreSignInCheck for passkeys. The official .NET 11 change note describes that historical behavior. However, the actual .NET 10.0.13 baseline used here already contains the check: the default manager refuses all tested unconfirmed and locked accounts. Both complete consumers therefore use the standard manager and handle the same outcomes. Do not claim all .NET 10 versions bypass policy or add an unnecessary adapter to a serviced app. Source review of v10.0.0 is historical evidence, not runtime execution of that version.

### .NET 10: `BeaconKeys.csproj`

{{snippet:n10-beaconkeys-csproj}}

### .NET 10: `Directory.Build.props`

{{snippet:n10-directory-build-props}}

### .NET 10: `Directory.Build.targets`

{{snippet:n10-directory-build-targets}}

### .NET 10: `Directory.Packages.props`

{{snippet:n10-directory-packages-props}}

### .NET 10: `global.json`

{{snippet:n10-global-json}}

### .NET 10: `KeysPanel.razor`

{{snippet:n10-keyspanel-razor}}

### .NET 10: `MemoryKeyStore.cs`

{{snippet:n10-memorykeystore-cs}}

### .NET 10: `PasskeyLabels.cs`

{{snippet:n10-passkeylabels-cs}}

### .NET 10: `Program.cs`

{{snippet:n10-program-cs}}

### .NET 10: `Proof.cs`

{{snippet:n10-proof-cs}}

### .NET 10: `RenamePanel.razor`

{{snippet:n10-renamepanel-razor}}

### .NET 10: `SignInOutcome.cs`

{{snippet:n10-signinoutcome-cs}}

### .NET 10: `SignInPanel.razor`

{{snippet:n10-signinpanel-razor}}

### .NET 10: `SoftwareKey.cs`

{{snippet:n10-softwarekey-cs}}

## Newer approach — .NET 11

Use the framework passkey sign-in method directly and branch on the actual result in SignInPanel. Keep denial separate from invalid credentials, and never create a success cookie after a refusal. NotAllowed can represent email, phone or account confirmation: direct the person to the existing confirmation workflow without claiming one specific cause.

PasskeyLabels adds a fictional fleet AAGUID suggestion and preserves a selected name. For an existing SDK-generated Individual-auth Blazor app, add that reviewed GUID/name pair to the private KnownAuthenticators dictionary in PasskeyAuthenticators.cs. Retain its built-in entries and network-byte-order Guid conversion, and keep the template add/save/unknown-rename branches and authenticated RenamePasskey page. This dictionary is generated application source, not a service to register in DI. The standalone sample implements only the original fleet suggestion; eng/Test-PasskeyTemplate.ps1 separately compiles and executes the actual SDK template with the same added entry.

### .NET 11: `BeaconKeys.csproj`

{{snippet:n11-beaconkeys-csproj}}

### .NET 11: `Directory.Build.props`

{{snippet:n11-directory-build-props}}

### .NET 11: `Directory.Build.targets`

{{snippet:n11-directory-build-targets}}

### .NET 11: `Directory.Packages.props`

{{snippet:n11-directory-packages-props}}

### .NET 11: `global.json`

{{snippet:n11-global-json}}

### .NET 11: `KeysPanel.razor`

{{snippet:n11-keyspanel-razor}}

### .NET 11: `MemoryKeyStore.cs`

{{snippet:n11-memorykeystore-cs}}

### .NET 11: `PasskeyLabels.cs`

{{snippet:n11-passkeylabels-cs}}

### .NET 11: `Program.cs`

{{snippet:n11-program-cs}}

### .NET 11: `Proof.cs`

{{snippet:n11-proof-cs}}

### .NET 11: `RenamePanel.razor`

{{snippet:n11-renamepanel-razor}}

### .NET 11: `SignInOutcome.cs`

{{snippet:n11-signinoutcome-cs}}

### .NET 11: `SignInPanel.razor`

{{snippet:n11-signinpanel-razor}}

### .NET 11: `SoftwareKey.cs`

{{snippet:n11-softwarekey-cs}}

## Migration

1. Create either complete standalone directory above. From that directory, restore and run with the declared SDK. The executable starts a random loopback port, performs the checks and stops; it leaves no server, account or data resource running.
2. In an existing Identity app, retain its authenticated account management and passkey-capable store. Apply naming only to a successfully verified attestation result; call AddOrUpdatePasskeyAsync and handle failure before navigating. Never accept a caller-provided owner identity.
3. Review an actual authenticator identifier before extending the generated template dictionary. Preserve explicit names. For an unknown or missing identifier, save the credential and navigate to the existing rename flow using its base64url credential ID. On rename, re-resolve the owner and credential, validate a 1–200-character trimmed label and enforce antiforgery. Keep rename available even for a known provider.
4. Use the standard manager on .NET 11 and the tested serviced .NET 10.0.13 comparison. Update an older unserviced application through its normal framework servicing process, then exercise its configured confirmation and lockout policy. Preserve independently required application policy customization. Do not disable confirmation flags or override PreSignInCheck to return success.
5. Connect the custom UI to the returned SignInResult. Offer retry/recovery for LockedOut, the existing confirmation path for NotAllowed and a generic alternative for Failed. Keep success handling in Identity; the rendered panel does not sign anybody in.
6. Re-run both real consumers after changing code, SDK or dependencies, refresh saved evidence, then rebuild and use list_recipes/get_recipe with explicit targetVersion 11.0. The .NET 10 code is a comparison, not another curated foundation or a promised newer API on an older target.

## Constraints

Identity passkey UI only. AAGUID naming is a template/application concern, not a framework naming service. Built-in automatic names alone are excluded: customize the mapping and preserve the rename fallback. .NET 10 comparison does not declare an older-target route or a ninth foundation.

Authenticator names and AAGUIDs are presentation hints, not proof of device identity, attestation trust, ownership, hardware storage or authorization. The fictional Beacon fleet GUID must be replaced with a reviewed real mapping for production. Decode AAGUID bytes in network order.

After successful attestation in an existing app, save through UserManager and check the result before redirecting. Unknown authenticators still need a rename page. Preserve a user-selected name, including after a mapping update. Resolve a rename credential within the signed-in owner collection; validate the name and protect the POST with antiforgery.

Use SignInManager.PasskeySignInAsync and handle LockedOut, NotAllowed and Failed without issuing a replacement cookie. .NET 11 performs shared PreSignInCheck. Never weaken PreSignInCheck or confirmation settings to make a passkey succeed: that changes other sign-in paths too.

The original .NET 10.0.0 source omitted PreSignInCheck on passkey sign-in, as the .NET 11 change note describes. The actually tested .NET 10.0.13 runtime already includes the policy check. Use the serviced framework and handle its results; do not add an unnecessary sign-in adapter or weaken shared policy. This is a patch-level reconciliation, not a claim that the original omission applies to every .NET 10 installation.

The complete BeaconKeys executable is an isolated loopback self-test that starts and stops automatically. MemoryKeyStore holds one fictional account; SoftwareKey signs assertions for pre-seeded public keys. Real Identity verifies challenges, origin, signatures and policy; registration/attestation, physical authenticators, browser credential APIs, database persistence and concurrency are outside the proof.

The custom Razor sign-in panel is rendered through actual HTTP requests. This proves returned HTML and policy/cookie outcomes, not browser focus, layout, assistive technology behavior or a physical WebAuthn ceremony. Do not expose this fixture as an application or copy its mutable test options/store into production.

In a real application retain the existing HTTPS/RP ID, challenge lifecycle, authenticated account management, passkey-capable persistence, recovery/confirmation flow and template antiforgery protections. The sample uses HTTP loopback solely for software-protocol checks and ephemeral data-protection keys; it does not configure production TLS.

Stable API surface; actual .NET 11 evidence uses RC2, not GA. No experimental opt-in, workload, account, provisioning or package publication is needed for the focused checks.

## Evidence

Saved compile-and-run records identify the exact SDKs, dependencies, source hashes and actual runtime output. The real request path covers confirmation through RequireConfirmedAccount, RequireConfirmedEmail and RequireConfirmedPhoneNumber; future lockout; wrong-origin rejection; absence of an application cookie on refusals; and cookie issuance after valid confirmation. Rename is exercised through the rendered form and actual antiforgery-protected POST, with saved-name and cross-owner checks. The store is an explicit fixture; Identity is not mocked.

The actual SDK 11 RC2 web template archive SHA-256 is 419395f52fdb5648d36e5224e44da952f4de3b97fa74688d21d089efff038b75. eng/Test-PasskeyTemplate.ps1 extracts its licensed PasskeyAuthenticators implementation into ignored temporary output, adds the original fleet entry and compiles/runs known Windows Hello, custom fleet, explicit-name precedence, missing/malformed and unknown-identifier checks. It tests the real mapping code, not a reimplementation. The canonical HTTP consumer separately proves the rename fallback. Neither is evidence of physical authenticator interoperability.

The installed-package Reqnroll scenario uses the actual ordinary nupkg, isolated local feed/cache and an unrelated working directory. It compares full expanded Markdown and complete applicability after real stdio list/get, checks edition 11.0 and rejects target 10.0. Local proxy blocking is not network namespace isolation. Source review, compilation, software protocol execution, physical ceremony and GA verification remain distinct.

- [ASP.NET Core 11 authenticator naming](https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-11?view=aspnetcore-11.0#infer-passkey-display-name-from-authenticator)
- [Passkey sign-in policy change](https://learn.microsoft.com/en-us/aspnet/core/breaking-changes/11/passkey-signin-enforces-confirmation-lockout)
- [Pinned .NET 11 SignInManager implementation](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Identity/Core/src/SignInManager.cs)
- [Previous-release SignInManager implementation](https://github.com/dotnet/aspnetcore/blob/v10.0.0/src/Identity/Core/src/SignInManager.cs)
- [Patched .NET 10.0.13 shared policy](https://github.com/dotnet/aspnetcore/blob/v10.0.13/src/Identity/Core/src/SignInManager.cs)
- [Identity WebAuthn prerequisites and storage](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/passkeys/?view=aspnetcore-11.0)
- [Pinned template authenticator mapping](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/ProjectTemplates/Web.ProjectTemplates/content/BlazorWeb-CSharp/BlazorWebCSharp.1/Components/Account/PasskeyAuthenticators.cs)
