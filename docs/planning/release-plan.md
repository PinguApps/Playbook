# Release plan

Status: policy accepted in Round 7, including Pingu's rejection of the additional releasable-change guard. This records the release design; implementation and launch actions still await confirmation of the completed shared plan.

## Package version and channels

Use independent package SemVer: first stable `1.0.0`, with `1.0.0-preview.N` if publishing before GA. Package version is separate from the .NET release documented. Use patch for corrections, minor for compatible recipe/capability additions, and major for incompatible public tool/contract or stable recipe-ID changes. Keep the reference Release Drafter label/version mechanism.

Preview publication is manual only. First public publication, including a preview, requires accepted comprehensive coverage plus eight foundations, required example compilation, focused checks, package/client evidence, and the small manual evaluation. A preview discloses the RC evidence baseline and remaining GA uncertainty; the preview label is not a waiver of those checks. First stable publication additionally waits for the .NET 11 GA reconciliation.

Weekly stable draft publication starts only after the first manual stable release, at the accepted Sunday 18:17 UTC slot. Require an existing published stable release and a stable draft, checking both SemVer and GitHub release flags. The reference workflows' any-published-release gate would also count prereleases; this narrow channel-gate adjustment was accepted in Q46.

Preserve the standard reference weekly behavior: new commits on `main` plus a suitable draft can trigger publication, including documentation, CI, test, and maintenance changes. Pingu rejected the proposed shipped-path and empty-release-note guards in Q47. Do not add either guard or require a package-content difference. Retain the reference no-new-commits/no-draft safeguards and the independently accepted stable-channel gate.

## Identity and artifacts

Retain the existing `PinguApps/Playbook` GitHub repository, with `PinguApps.Playbook` for NuGet/solution/project and `playbook` for the CLI. The actual origin is `https://github.com/PinguApps/Playbook.git`; no rename is needed. This resolves the earlier requested repository identity before configuring links and NuGet trusted publishing.

Preserve the six workflow roles and NuGet OIDC pattern. Pack once; verify the actual artifact, pass the same package to the brief Windows/macOS smoke jobs, and push that verified artifact without repacking. This applies the already accepted packed-consumer evidence standard. Merging/releasing remains Pingu's responsibility unless explicitly requested; the accepted future weekly automation is distinct from permission to activate or publish now.

## Remaining preparation

Complete the source register and recipe breakdown; this planning map is not an exhaustive feature/API audit. Choose exact supported SDK, workload, package, and action pins during implementation using current official artifacts and the approved references. Confirm the actual NuGet package/trusted-publisher identity and GitHub schedule state before enabling publication. Report checks actually run and any missing client/platform evidence.
