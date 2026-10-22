# Delay selection of newly published packages

## Goal

Give newly published dependencies time before automatic selection, while keeping explicit remediation and reproducible restore decisions visible. Configure a particular source rather than assuming a global package ban.

## Older approach

A valid SDK-10 workflow reviews publication dates and explicitly chooses a version: `dotnet package update Quay.Manifest@1.1.0`. This selects the reviewed version without claiming an automatic age-policy equivalent. Use the repository's existing source; this fictional configuration shows its shape:

{{snippet:older}}

Replace the example.invalid URL with an authorized source. In the controlled comparison, 1.1.0 is thirty days old and 1.2.0 was just published. The older explicit selection produces 1.1.0; that does not establish SDK-10 support for the new cooldown CLI.

## Newer approach

For an existing source named quay, run `dotnet nuget update source quay --min-publish-age-hours 48 --configfile NuGet.Config`. To register an existing authorized endpoint instead, run `dotnet nuget add source <source-url> --name quay --min-publish-age-hours 48 --configfile NuGet.Config`. Neither command provisions a feed. The resulting source configuration is:

{{snippet:newer}}

From the project directory under SDK 11, run `dotnet package add Quay.Manifest` or `dotnet package update`. With the controlled metadata, both select old 1.1.0 rather than fresh 1.2.0. Update also reports that a newer version has not reached the configured age.

An explicit reference to 1.2.0 still restores successfully in the check. Cooldown is **not universal restore exclusion**. RC2 rejects both 1.* and 1.2.* floating references with NU1020, even though 1.* could match old versions. Do not promise that having an eligible old floating match makes restore valid on this baseline.

## Migration

1. Inspect the effective SDK, NuGet.Config hierarchy, source mappings, package references and lock-file policy. Keep credentials and unrelated source settings intact. Use an existing source supplying trustworthy publication metadata.
2. Agree an age for the specific source and configure it with the add/update-source command above. Commit the intended repository configuration; inspect the saved diff and source name.
3. Run package add or update and review the selected versions and any cooldown message. Update or create the project's lock file according to its existing policy, then build and test the real dependency consumer.
4. Resolve NU1020 by replacing floating references with reviewed explicit versions or deliberately revisiting the source policy. Do not suppress the error or describe the setting as a security guarantee.
5. For an urgent fix, review the explicit version and its dependencies deliberately. To remove this source's cooldown, use `dotnet nuget update source quay --min-publish-age-hours 0 --configfile NuGet.Config`, review the diff and rerun selection/restore. A security update should not silently discard the team's age policy.

## Constraints

Publication age is source metadata, not a vulnerability score, signature check or proof of safety. Multiple sources, missing metadata, authentication, source mappings and caches affect real resolution; the controlled single-source result does not establish every feed's behavior. Fresh explicit restore is deliberately possible. Cooldown must not be described as warning suppression or an alternative to NuGet Audit.

The served configuration is language-neutral; no application C# upgrade is needed. The canonical compiled harness pins C# 14/net10.0 and C# 15/net11.0 solely for verification. The target-11 route needs its SDK and an existing compatible package/audit source; no NuGet.Client library reference, provider, UI rendering mode or native workload is required. Package assets remain those of the actual dependency. The synthetic Quay.Manifest fixture contains no executable library code.

## Evidence

Both real SDK consumers compile and execute. A temporary loopback V3 fixture supplies original nupkgs, registration publication timestamps and audit metadata to actual SDK commands with separate package/HTTP caches. The SDK-11 run checks both add and update select 1.1.0, explicit fresh restore succeeds, both floating forms fail with NU1020 and zero removes the setting. These are real NuGet command results against **controlled synthetic metadata**, not production-feed or dependency-security proof. Windows x64 SDKs are 10.0.402 and 11.0.100-rc.2.26504.105. Offline packed-tool retrieval verifies the full body, applicability and target-11 edition; it performs no package selection itself. GA verification remains separate.
