# Keep registration aligned with confirmation policy

## Goal

Seabrook Aquarium asks new volunteers to confirm their details before joining the
staff portal. Registration must respect the same confirmation requirements as
later sign-in. Apply this when configuring Identity UI or maintaining customized
password and external registration pages.

.NET 11's packaged default UI calls `CanSignInAsync` after creating the account
(and linking an external login), before issuing the application cookie. A denied
user goes to registration confirmation. Keep your requirements enabled. Existing
custom pages still need the explicit gate shown in the original `/Volunteer`
pages below; a framework upgrade does not rewrite them.

## Actual artifacts and supported prerequisites

| Comparison | Exact tested set | Working route |
| --- | --- | --- |
| Previous | SDK10.0.402, .NET/ASP.NET10.0.13, C#14; Identity.UI/Identity.EntityFrameworkCore/EF SQLite10.0.13 | Original Razor Pages gate with CanSignInAsync on both registration paths |
| Current | SDK11.0.100-rc.2.26504.105, matching runtime/framework and three packages, C#15 | Packaged Identity UI, or the same explicit custom-page gate |

These pins identify the reproduction, not minimum servicing releases. The served
edition targets .NET11; the compiled .NET10 comparison preserves a working older
approach without adding a foundation or changing existing recipe identities.
Identity UI is an actual restored package. Its ZIP and loaded DLL hashes match the
source audit, as do the Identity store package and framework reference identities.
The full original package/source distinction matters: .NET10.0.13's **default UI
still has the registration bypass for email-only and phone-only settings** in
these tests, although its later sign-in correctly denies those unconfirmed users.
Do not generalize the separate serviced passkey result from PIN-812 to this UI.

Use an actual Identity store supporting passwords, email, phone confirmation and
external logins. This complete fixture uses matching EF Core Identity/SQLite
packages and a temporary local database, queried through fresh DbContext instances.
The old lock resolves SQLitePCLRaw2.1.13 and SQLite3.53.3; the current lock resolves
SQLitePCLRaw3.0.5 with SQLite3.53.4. The locks, qualification record and runtime
output identify the selected compile/runtime/native assets and hashes. No native
compiler or workload is needed. Actual execution is Windows x64; another RID or
production persistence setup requires its own check.

Each target needs its matching SDK/runtime and packages restored before offline
compilation. Empty Directory files isolate surrounding build/package policy.
Razor Pages renders the real forms; requests obtain antiforgery cookies/tokens and
post them over loopback HTTP. No browser or interactive Blazor rendering is needed
for this server-policy claim. HTML response checks do not establish layout,
accessibility or client-side validation. The API surface needs no experimental
switch; the measured .NET11 baseline is **RC2, not GA**. PIN-959/PIN-963/PIN-964,
actual-GA reconciliation and unrelated Human gates stay open.

## Constraints: policy and observed outcomes

Each setting runs alone in its own temporary host. A separate no-requirements
control checks permitted automatic sign-in. It is not advice to disable policy.
All new users initially have unconfirmed email, no phone and an unconfirmed phone
flag. External registration stores the provider login but does not treat a provider
email claim as a confirmed email. The editable address is unverified input even
when a provider prefills it; retain the required local confirmation check.

| Requirement | .NET10 packaged UI | .NET10 original pages | .NET11 packaged and original pages |
| --- | --- | --- | --- |
| RequireConfirmedEmail | Session issued: reproduced bypass | Confirmation redirect; no application session | Confirmation redirect; no application session |
| RequireConfirmedPhoneNumber | Session issued: reproduced bypass | Confirmation redirect; no application session | Confirmation redirect; no application session |
| RequireConfirmedAccount | Confirmation redirect; no application session | Confirmation redirect; no application session | Confirmation redirect; no application session |
| None, isolated allowed control | Session issued | Session issued | Session issued |

Every cell covers **both password and external registration**, not only a direct
SignInManager call. The actual default UI is `/Identity/Account`; the independently
authored older alternative is `/Volunteer`. Both use the real framework pipeline.
The default `IUserConfirmation<IdentityUser>` defines account confirmation through
email confirmation; an application replacing that service must test its own rule.
Phone-only policy remains denied after email confirmation alone.

The external fixture is a **controlled local provider**, not OAuth/OIDC and not
Google/GitHub interoperability proof. It uses the real RemoteAuthenticationHandler
challenge/callback, protected state, framework correlation checks and a one-use
local code. The framework issues temporary `Identity.External`, and the actual
Identity external-login callback presents registration confirmation. That temporary
cookie cannot authenticate `/session`. Only `.AspNetCore.Identity.Application`
plus a fresh authenticated request establishes an application session.

The captured email sender records legitimate confirmation links locally; the
actual default ConfirmEmail page decodes/verifies tokens and persists confirmation.
For phone policy the driver checks a generated change-phone token through the real
UserManager. No email or SMS is sent. Confirmation itself grants no session; a new
password login or linked-identity challenge/callback succeeds only after the
required state is confirmed. The harness separately observes denied later sign-in,
including the old packaged UI users that received a session during registration.

## Migration

1. Inventory your actual packaged and customized password/external registration
   pages, store and confirmation service. Retain current confirmation requirements.
   Review both post-creation branches, including external login linking; checking
   only the login page leaves registration unprotected.
2. On .NET11 keep the default UI's CanSignInAsync gate. For custom pages on either
   tested target, use the original handlers below as a small reference: create and
   link successfully, prepare confirmation, call CanSignInAsync, redirect when it
   returns false, and call SignInAsync only when allowed. Do not replace the real
   Identity registration UI/store with a minimal endpoint that skips its checks.
3. Preserve antiforgery, local return URLs and external challenge/state/correlation.
   The original pages reject nonlocal returns before writing a user. Do not issue
   a replacement session after denial. There is no compatibility switch that
   keeps later confirmation requirements while restoring a registration bypass.
4. Configure legitimate confirmation delivery independently. A registration
   confirmation page is a pending outcome, not evidence that email or phone was
   confirmed. Supply a production email sender so development confirmation links
   are not exposed. Plan the phone completion interface separately; the default
   registration-confirmation page is not an SMS workflow.
5. Copy every file for each standalone project below. For the two `/Volunteer`
   page files, put `@page` as their **first line**, followed by their canonical
   body. Keep the supplied lock files. Build and execute each DLL with its matching
   absolute dotnet host using the commands below. The executable starts and stops
   its own temporary hosts, tests all cases, deletes only its own databases and
   prints CHECK/LATER/GUARDS plus a final PASS.
6. Verify actual redirect, persisted confirmation/password/login state, Set-Cookie
   and a fresh authenticated request for each individual policy and the allowed
   control. Missing antiforgery on password/external POSTs, missing external state
   and tampered provider state must not create users or sessions. The original
   pages also check nonlocal return rejection before writes. Keep production HTTPS,
   keys, provider validation, database migrations, authorization and recovery
   separate from this disposable test fixture.

## Reproduce

Use PowerShell7 and set `DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0` before first-run SDK
work, preserving its previous process value afterwards. With the complete files
and lock in each standalone directory, use the matching absolute dotnet executable
from the table below for `build -c Release -p:RestoreLockedMode=true`, then invoke
that executable with `./bin/Release/net10.0/AquariumVolunteers.dll` or
`./bin/Release/net11.0/AquariumVolunteers.dll` respectively. Select the project
directory before building so its own global.json controls SDK resolution.

| Target | Example absolute SDK/runtime host |
| --- | --- |
| net10.0 | `C:/Program Files/dotnet/dotnet.exe` |
| net11.0 | `C:/Users/pingu/.dotnet-playbook-11/dotnet.exe` |

Adjust installation paths for your machine; no PATH edit, global tool or public
service is needed. Package restore is online preparation; supplied locks fix both
closures. Subsequent retrieval of this guidance is offline. The repository's
`eng/Record-AquariumVolunteers.ps1` runs these real builds/checks and records exact
dependencies, inputs, commands, source hashes and package/reference qualification.

These are complete **test executables**, not a deployable identity product.
Fixture-only HTTP correlation-cookie settings, ephemeral keys, test provider and
test identity selection must not become production configuration. SQLite
EnsureCreated is appropriate here for disposable data, not a migration plan.
No third-party provider, real confirmation delivery, physical authenticator,
passkey/lockout/recovery matrix, security deployment, browser ceremony or GA
execution is claimed. PIN-812's passkey denial remains a separate goal.

## Evidence

The saved compile-and-run records contain fresh execution of 16 registrations per
target, plus denied/confirmed later sign-in and request guards. Separate artifact
records retain the full selected dependency assets, package ZIP/reference hashes
and five checked immutable official sources. Native SQLite and loaded runtime/
compiler identities are printed by the executed program. These qualify actual
Windows x64 server behaviour, independently from offline packaged retrieval.
The installed ordinary artifact is also used to reconstruct and run both complete
consumers from its returned Markdown. No required proof is replaced by a mock.

## Complete canonical files

Create the following paths relative to your chosen sample parent. Each file is
independent original source. Prefix the two Volunteer page bodies in each project
with the exact first line `@page`; all other blocks are the complete file contents.
The lock JSON can be copied without the repository's snippet-marker comments.

## Older approach

Keep the configured policy and explicitly call CanSignInAsync in both original
registration handlers. The packaged .NET10 routes remain in this test executable
only to reproduce their email/phone bypass alongside the working original pages.
Do not expose that comparison host as a production application.

### .NET 10

#### `net10.0/AquariumVolunteers/AquariumVolunteers.csproj`

{{snippet:n10-aquariumvolunteers-csproj}}

#### `net10.0/AquariumVolunteers/ArtifactIdentity.cs`

{{snippet:n10-artifactidentity-cs}}

#### `net10.0/AquariumVolunteers/Directory.Build.props`

{{snippet:n10-directory-build-props}}

#### `net10.0/AquariumVolunteers/Directory.Build.targets`

{{snippet:n10-directory-build-targets}}

#### `net10.0/AquariumVolunteers/Directory.Packages.props`

{{snippet:n10-directory-packages-props}}

#### `net10.0/AquariumVolunteers/global.json`

{{snippet:n10-global-json}}

#### `net10.0/AquariumVolunteers/LocalProvider.cs`

{{snippet:n10-localprovider-cs}}

#### `net10.0/AquariumVolunteers/packages.lock.json`

{{snippet:n10-packages-lock-json}}

#### `net10.0/AquariumVolunteers/Pages/_ViewImports.cshtml`

{{snippet:n10-pages-viewimports-cshtml}}

#### `net10.0/AquariumVolunteers/Pages/Shared/_LoginPartial.cshtml`

{{snippet:n10-pages-shared-loginpartial-cshtml}}

#### `net10.0/AquariumVolunteers/Pages/Volunteer/ExternalLogin.cshtml`

First line: `@page`. Then append this body:

{{snippet:n10-pages-volunteer-externallogin-cshtml}}

#### `net10.0/AquariumVolunteers/Pages/Volunteer/ExternalLogin.cshtml.cs`

{{snippet:n10-pages-volunteer-externallogin-cshtml-cs}}

#### `net10.0/AquariumVolunteers/Pages/Volunteer/Register.cshtml`

First line: `@page`. Then append this body:

{{snippet:n10-pages-volunteer-register-cshtml}}

#### `net10.0/AquariumVolunteers/Pages/Volunteer/Register.cshtml.cs`

{{snippet:n10-pages-volunteer-register-cshtml-cs}}

#### `net10.0/AquariumVolunteers/Program.cs`

{{snippet:n10-program-cs}}

#### `net10.0/AquariumVolunteers/RegistrationChecks.cs`

{{snippet:n10-registrationchecks-cs}}

## Newer approach

The actual .NET11 packaged UI gates registration with CanSignInAsync. The original
pages remain here to demonstrate that custom interfaces need the same check and
to verify the older approach continues to work. Build and run this separate
project with its matching framework/package/native closure.

### .NET 11

#### `net11.0/AquariumVolunteers/AquariumVolunteers.csproj`

{{snippet:n11-aquariumvolunteers-csproj}}

#### `net11.0/AquariumVolunteers/ArtifactIdentity.cs`

{{snippet:n11-artifactidentity-cs}}

#### `net11.0/AquariumVolunteers/Directory.Build.props`

{{snippet:n11-directory-build-props}}

#### `net11.0/AquariumVolunteers/Directory.Build.targets`

{{snippet:n11-directory-build-targets}}

#### `net11.0/AquariumVolunteers/Directory.Packages.props`

{{snippet:n11-directory-packages-props}}

#### `net11.0/AquariumVolunteers/global.json`

{{snippet:n11-global-json}}

#### `net11.0/AquariumVolunteers/LocalProvider.cs`

{{snippet:n11-localprovider-cs}}

#### `net11.0/AquariumVolunteers/packages.lock.json`

{{snippet:n11-packages-lock-json}}

#### `net11.0/AquariumVolunteers/Pages/_ViewImports.cshtml`

{{snippet:n11-pages-viewimports-cshtml}}

#### `net11.0/AquariumVolunteers/Pages/Shared/_LoginPartial.cshtml`

{{snippet:n11-pages-shared-loginpartial-cshtml}}

#### `net11.0/AquariumVolunteers/Pages/Volunteer/ExternalLogin.cshtml`

First line: `@page`. Then append this body:

{{snippet:n11-pages-volunteer-externallogin-cshtml}}

#### `net11.0/AquariumVolunteers/Pages/Volunteer/ExternalLogin.cshtml.cs`

{{snippet:n11-pages-volunteer-externallogin-cshtml-cs}}

#### `net11.0/AquariumVolunteers/Pages/Volunteer/Register.cshtml`

First line: `@page`. Then append this body:

{{snippet:n11-pages-volunteer-register-cshtml}}

#### `net11.0/AquariumVolunteers/Pages/Volunteer/Register.cshtml.cs`

{{snippet:n11-pages-volunteer-register-cshtml-cs}}

#### `net11.0/AquariumVolunteers/Program.cs`

{{snippet:n11-program-cs}}

#### `net11.0/AquariumVolunteers/RegistrationChecks.cs`

{{snippet:n11-registrationchecks-cs}}

## Official references

- [Official registration confirmation change](https://github.com/dotnet/core/blob/ffed591b0fb8f9aab50ecc28f2c765049fb28d8f/release-notes/11.0/preview/rc1/aspnetcore.md#sign-in-confirmation-requirements-apply-after-registration)
- [Serviced .NET 10 full Register.cshtml.cs](https://github.com/dotnet/aspnetcore/blob/0ef4bbfa3291b306a21e5001cb0491277bdd35bc/src/Identity/UI/src/Areas/Identity/Pages/V5/Account/Register.cshtml.cs)
- [Serviced .NET 10 full ExternalLogin.cshtml.cs](https://github.com/dotnet/aspnetcore/blob/0ef4bbfa3291b306a21e5001cb0491277bdd35bc/src/Identity/UI/src/Areas/Identity/Pages/V5/Account/ExternalLogin.cshtml.cs)
- [Pinned .NET 11 full Register.cshtml.cs](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Identity/UI/src/Areas/Identity/Pages/V5/Account/Register.cshtml.cs)
- [Pinned .NET 11 full ExternalLogin.cshtml.cs](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Identity/UI/src/Areas/Identity/Pages/V5/Account/ExternalLogin.cshtml.cs)
- [Account confirmation and transport prerequisites](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/accconfirm?view=aspnetcore-11.0)
