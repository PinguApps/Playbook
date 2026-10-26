# Verify Identity lockout boundaries with a controlled clock

## Goal

A fictional mooring service locks a keeper after two failed attempts for seventeen minutes. Test the actual Identity decision before, at and after the stored end without waiting. The newer comparison advances an injected clock; the older comparison honestly tests arranged stored states.

## Older approach — .NET 10

Keep the real UserManager and arrange the stored lockout end on either side of wall time. This can test future/past state without sleeping, and remains useful. The included negative control advances an injected clock by a day and confirms that .NET 10 still regards the user as locked. It then uses SetLockoutEndDateAsync to arrange the past state; it does not claim an equivalent fake-clock integration existed.

#### .NET 10: `Directory.Build.props`

{{snippet:n10-directory-build-props}}

#### .NET 10: `Directory.Build.targets`

{{snippet:n10-directory-build-targets}}

#### .NET 10: `Directory.Packages.props`

{{snippet:n10-directory-packages-props}}

#### .NET 10: `global.json`

{{snippet:n10-global-json}}

#### .NET 10: `ManualClock.cs`

{{snippet:n10-manualclock-cs}}

#### .NET 10: `MemoryLockoutStore.cs`

{{snippet:n10-memorylockoutstore-cs}}

#### .NET 10: `MooringClock.csproj`

{{snippet:n10-mooringclock-csproj}}

#### .NET 10: `Program.cs`

{{snippet:n10-program-cs}}

## Newer approach — .NET 11

Register a ManualClock as TimeProvider before resolving Identity. AccessFailedAsync creates the end timestamp using that clock, and IsLockedOutAsync consults it on every check. Advance the clock across the measured inclusive boundary while leaving stored state untouched. The fixture uses a real framework UserManager and a deliberately small storage adapter; no authentication service is mocked.

#### .NET 11: `Directory.Build.props`

{{snippet:n11-directory-build-props}}

#### .NET 11: `Directory.Build.targets`

{{snippet:n11-directory-build-targets}}

#### .NET 11: `Directory.Packages.props`

{{snippet:n11-directory-packages-props}}

#### .NET 11: `global.json`

{{snippet:n11-global-json}}

#### .NET 11: `ManualClock.cs`

{{snippet:n11-manualclock-cs}}

#### .NET 11: `MemoryLockoutStore.cs`

{{snippet:n11-memorylockoutstore-cs}}

#### .NET 11: `MooringClock.csproj`

{{snippet:n11-mooringclock-csproj}}

#### .NET 11: `Program.cs`

{{snippet:n11-program-cs}}

## Migration

1. Create the complete standalone comparison directory and restore its matching framework pack. The clock fixture is a console project with an ASP.NET Core framework reference, not a web server.
2. Register one test clock in the service collection before Identity is resolved; keep it available to the test driver. Configure the lockout duration and threshold explicitly.
3. Create an enabled user through the real manager and call AccessFailedAsync twice. Check that the first failure does not lock and the second creates the exact expected end.
4. Advance through one tick before, exactly at, and one tick after the end. Assert the real IsLockedOutAsync result and confirm GetLockoutEndDateAsync stayed unchanged.
5. Run both target consumers. Preserve the older state-arrangement distinction and the RC2 inclusive-boundary finding. Add separate integration evidence before extending a claim to tokens, cookies or database concurrency.

## Constraints

Register the controlled TimeProvider before resolving UserManager, in the same service provider. Injecting a clock into a separate application service does not prove Identity uses it. Use the normal system clock in production unless the application deliberately requires another clock.

The .NET 11 RC2 check keeps the user locked one tick before and exactly at the end timestamp, then unlocks one tick after it. The stored LockoutEnd remains unchanged. Do not assume an exclusive boundary or sleep for the configured seventeen minutes.

The .NET 10 UserManager path in this actual comparison reads wall time. Registering TimeProvider does not redirect it. The older fixture sets an already-expired stored timestamp to test the decision; that is useful but does not test the passage of time or the newer injected-clock behavior.

ManualClock controls GetUtcNow only. It does not virtualize timers, Task.Delay, browser time or arbitrary services. The single-user store persists fields in memory and supplies no concurrency guarantees. This is actual Identity execution with a test storage adapter, not a mocked time-sensitive service.

The focused claim is UserManager lockout, including the failed-attempt threshold and expiry. Cookie validation, security stamps, password/reset tokens, external providers, database persistence and sign-in middleware need their own targeted checks; the recipe does not infer those behaviors from this one path.

## Evidence

Both complete original consumers compile with their declared SDK/target/language. Saved compilation records retain exact inputs and artifacts. `eng/Test-JettyGuidance.ps1` separately builds and executes both MooringClock consumers. No fake authorization or mocked UserManager replaces the framework decision. The header credential and memory store are test fixtures, not production security or persistence. The report `docs/verification/authorization-and-clock.md` records requests/rendered output, actual clock boundaries, failed initial assumptions and evidence limits. .NET 11 RC2 execution is not GA verification. These .NET 10 sources are comparisons, not a ninth curated foundation or a declared older-target route.

- [Identity TimeProvider release notes](https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-11?view=aspnetcore-11.0#timeprovider-support-in-aspnet-core-identity)
- [Pinned UserManager clock resolution and inclusive end check](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Identity/Extensions.Core/src/UserManager.cs)
- [Previous-release UserManager wall clock](https://github.com/dotnet/aspnetcore/blob/v10.0.0/src/Identity/Extensions.Core/src/UserManager.cs)
