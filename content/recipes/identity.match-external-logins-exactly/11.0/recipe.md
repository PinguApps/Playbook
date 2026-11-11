# Match external login identities exactly

## Goal

Preserve exact external provider names and opaque keys when looking up or removing logins, even when database collation ignores case or trailing spaces.

Saltmarsh Observatory links an observer to TidalAuthority's opaque Station-A7 key. Never lowercase, trim or otherwise normalize these identities.

## Documented change and observed limitation

The pinned official documentation describes ordinal verification after an indexed lookup in .NET11. Actual Identity EF11.0.0 tested here still returns and removes collation-only matches. An independently downloaded NuGet.org ZIP matches the cache (SHA256 f2335d26e82ae6c8c7bd39aace7d0777ab5c5c2d73b7a8b86fe0d2186d79bf03); its net11.0 DLL is 0b058f6b5707ed7091f5c21b96c603d03a7bbe4d9c2323453234e457fe640b59. Pingu approved documenting the limitation and providing an explicit guard. Do not assume that deploying this version enforces the documented change.

## Migration

1. Inventory stored provider identities and the deployed collation.
2. Register the guarded store and preserve both lookup and removal ownership checks.
3. Exercise exact and collation-only identities against the actual provider before rollout.


Keep the indexed SQL predicate over LoginProvider and ProviderKey. Materialize the bounded candidate and verify BOTH original strings with StringComparison.Ordinal before resolving a user or removing a row. Do not scan every login or put an untranslatable client comparison into SQL. Case-insensitive key uniqueness can still prevent storing ordinal-distinct identities; this guard does not change schema uniqueness.

The complete samples override IUserLoginStore operations in ExactLoginStore. Register it in place of the default store on the existing Identity builder using AddUserStore<ExactLoginStore>, adapting user/context/key types. Preserve other Identity operations and policy. Custom IUserLoginStore implementations own equivalent lookup AND removal checks, owner verification, cancellation, persistence and concurrency policy. Direct store calls save changes explicitly; UserManager uses its normal update path.

Both older and newer consumers intentionally use the explicit guard. They print unguarded observations separately from guard assertions. The actual SQLite collation ignores case and trailing ASCII spaces on indexed key columns. This verifies the hazard and guard without claiming every SQL Server collation or deployment provider.

## Older approach: guarded .NET10 comparison

SDK10.0.404, C#14.0. Save the files together, restore once, then restore --locked-mode and run --configuration Release. The executable checks behavior and exits.

### Directory.Build.props

{{snippet:n10-directory-build-props}}

### Directory.Build.targets

{{snippet:n10-directory-build-targets}}

### Directory.Packages.props

{{snippet:n10-directory-packages-props}}

### ObservatoryLogins.csproj

{{snippet:n10-observatorylogins-csproj}}

### Program.cs

{{snippet:n10-program-cs}}

### global.json

{{snippet:n10-global-json}}

### packages.lock.json

{{snippet:n10-packages-lock-json}}

## Newer approach: guarded .NET11 comparison

SDK11.0.100, C#15.0. Save the files together, restore once, then restore --locked-mode and run --configuration Release. The executable checks behavior and exits.

### Directory.Build.props

{{snippet:n11-directory-build-props}}

### Directory.Build.targets

{{snippet:n11-directory-build-targets}}

### Directory.Packages.props

{{snippet:n11-directory-packages-props}}

### ObservatoryLogins.csproj

{{snippet:n11-observatorylogins-csproj}}

### Program.cs

{{snippet:n11-program-cs}}

### global.json

{{snippet:n11-global-json}}

### packages.lock.json

{{snippet:n11-packages-lock-json}}

## Evidence and local verification

Real SQLite lookup/removal for exact, case-different, trailing-space and missing identities; wrong-owner protection; explicit ordinal guard and separate built-in observations. Saved records retain actual SDK, dependencies, commands and input hashes. Run eng/Test-ObservatoryAuthentication.ps1; -RecordEvidence refreshes affected records. Windows x64 only. No browser, other-provider or other-OS deployment claim. The eight .NET10 foundations remain unchanged. PIN-967 and unrelated Human/experimental gates remain separate.

## Official reference

[Documented GA contract, with the observed limitation above](https://github.com/dotnet/AspNetCore.Docs/blob/fd01aafa737af65203d3bd4ea08f1920305c52a9/aspnetcore/breaking-changes/11/identity-login-ordinal-equality.md)

## Constraints

Only the explicit guard behavior is established by the samples. Keep production authentication, authorization, HTTPS and key management; fixture HTTP and ephemeral keys are local test configuration. Other providers, hybrid/PAR paths and other operating systems require their own focused validation.
