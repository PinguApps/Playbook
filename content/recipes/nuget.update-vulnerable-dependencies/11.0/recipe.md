# Update vulnerable dependencies under strict warnings

## Goal

Apply a reviewed dependency fix without permanently weakening warnings-as-errors. Keep the distinction between locating an available fix, updating the reference and validating the resulting application.

## Older approach

The original SDK-10 project has TreatWarningsAsErrors=true and a direct Quay.Manifest 1.0.0 reference. Controlled audit metadata marks that version high severity and identifies 1.1.0 as outside the affected range. An ordinary restore fails with NU1903. On the recorded older SDK, `dotnet package update --vulnerable` also fails its preliminary restore.

A narrow temporary exception permits the update while retaining other warning failures. Add this property group to the project or an already imported policy file:

{{snippet:older}}

Run `dotnet package update --vulnerable`, review the reference change, then remove the temporary WarningsNotAsErrors additions and restore/build/test again. An explicit reviewed fixed-version update is another deliberate workflow. Do not disable all audit or all warnings merely to get past discovery.

## Newer approach

Keep the project's original strict policy:

{{snippet:newer}}

Run `dotnet package update --vulnerable` from the project directory using SDK 11. Its in-memory preview restore handles audit warnings NU1901 through NU1904 without turning them into the failure that prevents locating the fix. The policy file itself is not relaxed. The controlled example updates 1.0.0 to safe 1.1.0 and a subsequent ordinary strict restore succeeds.

The exception is local to this command's preview operation. It does not establish general warning suppression, disable audit on subsequent restores, or make unrelated dependency failures acceptable. With Quay.Missing 1.0.0 also referenced, ordinary restore reports NU1101 and the vulnerability update fails without changing Quay.Manifest 1.0.0. RC2's command prints Restore failed rather than repeating NU1101; inspect the ordinary restore for the diagnostic.

## Migration

1. Confirm the selected SDK and effective TreatWarningsAsErrors, WarningsAsErrors, WarningsNotAsErrors, NoWarn and NuGetAudit settings. Inspect package and audit sources and their existing authentication without committing credentials.
2. Obtain current advisory information from an authorized audit source. Review affected package versions, available compatible fixed versions and direct versus transitive ownership. A missing audit result is not evidence that no vulnerability exists.
3. Keep TreatWarningsAsErrors enabled and run `dotnet package update --vulnerable` using SDK 11. On the demonstrated older workflow only, use the narrowly scoped temporary audit-warning exception above and remove it after updating.
4. Inspect the project and lock-file changes. Check cooldown and source-mapping policies too; the demonstration disables its synthetic source cooldown before this separate vulnerability check. It does not promise that security updates bypass age restrictions.
5. Run an ordinary restore with the final strict policy, then compile and test the affected real consumer. If unrelated restore errors remain, fix them first and retry; do not count a partial or unchanged reference as remediation. Recheck audit output and deployment requirements for the application.

## Constraints

The controlled advisory and package identity are fictional. The test validates client command behavior, not a real CVE remediation, package safety or the security of the application. No transitive dependency, central-package-management or every-severity combination is claimed tested; diagnose the actual dependency graph. Offline Playbook retrieval contains this complete guidance but does not download current advisories or update anything.

This language-neutral project policy requires no application-language migration. The verification harnesses compile at C# 14/net10.0 and C# 15/net11.0. The declared target-11 SDK route uses inbox NuGet; no NuGet.Client package, provider, rendering mode or native workload is required. Actual package assets, sources and credentials are consumer-specific prerequisites. Preserve warning policy beyond the audit-specific preview behavior.

## Evidence

Real SDK-10/11 builds execute a loopback V3 fixture with synthetic vulnerability information and actual package payloads. Both reproduce strict NU1903 restore failure. SDK 10 requires the demonstrated temporary exception; RC2 updates without it. Both reach 1.1.0, restore with strict policy afterwards, and stop on a missing unrelated package while retaining 1.0.0. This is execution against controlled metadata, not a mocked CLI or production security proof. Exact baselines are SDK 10.0.402 and 11.0.100-rc.2.26504.105 on Windows x64. The actual packed-tool scenario separately verifies complete offline guidance, applicability and edition safety. GA reconciliation remains separate.
