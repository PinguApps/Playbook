# Move customized Identity UI off Bootstrap 4

## Goal

Keep customized sign-in pages rendering when upgrading Identity UI by selecting Bootstrap 5 and migrating the affected markup, styles and published assets.

Exact reproduction uses SDK10.0.402 / framework10.0.13 / C#14 versus SDK11.0.100-rc.2.26504.105 / framework11.0.0-rc.2.26504.105 / C#15. These are tested pins, not minimum supported versions. RC2 is not actual-GA proof. The exact eight approved .NET10 foundations, stable identities, PIN-959/PIN-963/PIN-964 and unrelated Human gates remain unchanged.

## Older approach — .NET 10

Bracken Rescue volunteers use a customized sign-in form. The original .NET10 project deliberately selects Bootstrap4, with form-group, custom-control/custom-checkbox and btn-block markup plus a custom-control CSS selector. This valid historical customization builds, publishes and renders with the actual matching 10.0.13 UI/store/provider closure. Bootstrap5 selection was already available on .NET10; the goal is migration of existing customization, not a new sign-in API.

Create every file below in one standalone VolunteerAccess directory. The empty Directory files isolate parent build/package policy. With the pinned SDK run `dotnet restore VolunteerAccess.csproj --locked-mode`, `dotnet publish VolunteerAccess.csproj -c Release -o ./published`, then `dotnet ./published/VolunteerAccess.dll`. Run from the published output, not from a directory relying on source views. The executable performs real HTTP checks and exits. `dotnet ./published/VolunteerAccess.dll --serve true` keeps an ephemeral loopback host open and prints its login URL; stop it after browser checks.

### Areas/Identity/Pages/_ViewImports.cshtml

{{snippet:n10-areas-identity-pages-viewimports-cshtml}}

### Areas/Identity/Pages/Account/Login.cshtml

{{snippet:n10-areas-identity-pages-account-login-cshtml}}

### Areas/Identity/Pages/Account/Login.cshtml.cs

{{snippet:n10-areas-identity-pages-account-login-cshtml-cs}}

### ArtifactIdentity.cs

{{snippet:n10-artifactidentity-cs}}

### Directory.Build.props

{{snippet:n10-directory-build-props}}

### Directory.Build.targets

{{snippet:n10-directory-build-targets}}

### Directory.Packages.props

{{snippet:n10-directory-packages-props}}

### global.json

{{snippet:n10-global-json}}

### packages.lock.json

{{snippet:n10-packages-lock-json}}

### Pages/Shared/_Layout.cshtml

{{snippet:n10-pages-shared-layout-cshtml}}

### Pages/Shared/_LoginPartial.cshtml

{{snippet:n10-pages-shared-loginpartial-cshtml}}

### Program.cs

{{snippet:n10-program-cs}}

### VolunteerAccess.csproj

{{snippet:n10-volunteeraccess-csproj}}

### wwwroot/css/volunteers.css

{{snippet:n10-wwwroot-css-volunteers-css}}

## Newer approach — .NET 11

Select Bootstrap5 explicitly in the project, or remove both UI-framework properties so the package chooses Bootstrap5. Also change form-group to mb-3, the checkbox wrapper/input/label to form-check/form-check-input/form-check-label, btn-block to w-100 and the authored CSS selector to .form-check. Keep labels, described error spans, autocomplete, antiforgery and the unchanged sign-in handler. The actual CSS comes from the selected Identity package at `/Identity/lib/bootstrap/dist/css/bootstrap.min.css` and the authored `/css/volunteers.css`; no network asset is needed at render time.

Use a separate directory and the same restore/publish/run commands with the declared .NET11 host. To test property removal, remove the XML element from a separate complete project copy and ensure no inherited IdentityDefaultUIFramework selects Bootstrap4. `-p:IdentityUIFrameworkVersion=` fixes a global empty value and prevents the package from assigning its default; it is not the removal route. Publish that copy and run its DLL, then render both login and invalid-form pages. A real warning is reproduced separately with `dotnet build -c Release -p:IdentityUIFrameworkVersion=Bootstrap4 -p:TreatWarningsAsErrors=false`; this is a diagnostic control, never the recommended configuration.

### Areas/Identity/Pages/_ViewImports.cshtml

{{snippet:n11-areas-identity-pages-viewimports-cshtml}}

### Areas/Identity/Pages/Account/Login.cshtml

{{snippet:n11-areas-identity-pages-account-login-cshtml}}

### Areas/Identity/Pages/Account/Login.cshtml.cs

{{snippet:n11-areas-identity-pages-account-login-cshtml-cs}}

### ArtifactIdentity.cs

{{snippet:n11-artifactidentity-cs}}

### Directory.Build.props

{{snippet:n11-directory-build-props}}

### Directory.Build.targets

{{snippet:n11-directory-build-targets}}

### Directory.Packages.props

{{snippet:n11-directory-packages-props}}

### global.json

{{snippet:n11-global-json}}

### packages.lock.json

{{snippet:n11-packages-lock-json}}

### Pages/Shared/_Layout.cshtml

{{snippet:n11-pages-shared-layout-cshtml}}

### Pages/Shared/_LoginPartial.cshtml

{{snippet:n11-pages-shared-loginpartial-cshtml}}

### Program.cs

{{snippet:n11-program-cs}}

### VolunteerAccess.csproj

{{snippet:n11-volunteeraccess-csproj}}

### wwwroot/css/volunteers.css

{{snippet:n11-wwwroot-css-volunteers-css}}

## Migration

1. Inventory the effective UI-framework properties, customized pages, shared layouts, local vendor files and CSS selectors before upgrading the matching Identity packages and target.
2. Reproduce the actual Bootstrap4 warning with the pinned package. Choose explicit Bootstrap5 or removal of both selectors, including inherited values. Do not use warning suppression as the migration.
3. Apply the bounded markup/CSS edits above to the customization. Review other bespoke components separately; changing one property cannot migrate their classes or JavaScript behavior.
4. Restore the exact locked dependency graph, build and publish. Launch the published DLL with its published wwwroot and precompiled Razor pages; check both local stylesheets return actual CSS.
5. In a real browser open the printed login URL, submit `not-an-email` with an empty password, and inspect labels, readable errors, checkbox alignment, button width, focus styling and clipping. Repeat for the default-selection publish.
6. Retain antiforgery, local return URLs and the configured confirmation policy. Run the fixture checks, then apply equivalent checks to the actual application before removing old assets.

## Constraints

Changing the property selects packaged UI/assets; it does not rewrite customized Razor or CSS. Only the bounded form/checkbox/button selectors shown here are migrated.

Bootstrap4 still builds in the measured RC2 package but produces an obsolete-support warning. Do not suppress that warning as a migration. Remove BOTH IdentityUIFrameworkVersion and an inherited IdentityDefaultUIFramework when choosing defaults. An empty GLOBAL MSBuild property is not equivalent to removing it.

Keep existing confirmation/lockout/session policy and antiforgery. This fixture uses PasswordSignInAsync, requires confirmed accounts, validates local return URLs and checks denied/allowed fresh principals. The form is not a complete production Identity replacement: add the application's existing 2FA, recovery, external-login and accessibility requirements when applicable.

Fixture-only accounts, ephemeral keys, EnsureCreated database and HTTP loopback are test infrastructure; use production key storage, HTTPS, schema migrations and legitimate account lifecycle services. The browser checks are one Windows Chromium viewport, not all assistive technology or mobile proof.

The complete files, configuration, lock, native-tool acquisition conditions and migration steps above are included offline. Restore/download preparation can need network access; installed recipe retrieval does not execute the samples or acquire missing prerequisites. There is no new recurring platform/browser matrix or public publication.

## Evidence

Both actual dependency sets compile. Published HTTP checks cover custom layout/CSS, missing-token 400, validation, denied unconfirmed account, allowed confirmed account and rejected external return URL. Actual package targets produce the Bootstrap4 warning. Both explicit5 and property-removal copies publish and execute. Chromium152/Electron44 on Windows at1280x800 rendered login and validation for all three publishes: loaded local stylesheets, labeled inputs, 480px panels, 414px buttons, readable red error text and no horizontal overflow; screenshots were visually inspected. This is server validation, not unobtrusive/client JavaScript proof.

Fresh compilation/execution records retain exact commands, source/input hashes, resolved dependency graphs, platform and measured SDK/framework/native identities. Source audit metadata is reconciled with the actual selected artifacts separately from loaded runtime bytes. Run the repository's eng/Test-WebMigrationBatch.ps1 with both absolute SDK hosts and -RecordEvidence to refresh managed/tool/publish proof; browser proof must be captured independently after relevant layout/input changes. Installed ordinary-artifact list/get inspection compares the complete canonical body, goal, target11/edition11, applicability, fresh evidence and explicit unavailable target10 response. Proxy blocking establishes proxy independence, not network isolation or additional runtime execution.

## References

- [Pinned ASP.NET RC1 migration changes retained in RC2](https://github.com/dotnet/core/blob/ffed591b0fb8f9aab50ecc28f2c765049fb28d8f/release-notes/11.0/preview/rc1/aspnetcore.md)
- [Exact Identity UI RC2 package and build assets](https://www.nuget.org/packages/Microsoft.AspNetCore.Identity.UI/11.0.0-rc.2.26504.105)
- [Bootstrap migration reference](https://getbootstrap.com/docs/5.3/migration/)
