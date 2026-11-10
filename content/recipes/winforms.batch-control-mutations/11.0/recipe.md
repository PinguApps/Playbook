# Recover a nested observation queue after a failed update

## Goal

Fen's queue changes an inherited font, container padding and list contents together. A deliberately failed import demonstrates that update suppression ends even when the mutation does not finish. The scope is not a transaction: the first inserted observation remains.

## Older approach

The .NET10 example pairs SuspendLayout/ResumeLayout on its known container and BeginUpdate/EndUpdate on its list in try/finally. This remains a supported approach. Larger trees require explicit ownership of the corresponding pairs on affected nested containers; suppressing one parent's layout is not recursively suppressing every child's layout.

## Newer approach

{{snippet:n11-fenbatch-program-cs}}

Acquire SuspendPainting(TargetAndDescendants) on the common container before assigning the ambient Font. Its scope suppresses painting on the target and layout on the selected subtree, then restores them during disposal. Traversal selects layout work; it does not separately invoke every descendant's painting hooks.

None leaves layout active; TargetOnly suspends layout only on the target. The predicate overload can choose layout participants while still traversing children of a nonmatching parent. The traversal/predicate overloads require a Control target. Use the parameterless overload when only target painting needs suppression.

Keep the synchronous mutation small and on the UI thread. Collect asynchronous results before opening the scope so a long wait does not freeze a live surface. Do not dispose controls or restructure ownership unexpectedly while relying on the acquired traversal.

The sample overrides protected begin/end hooks on its custom container only to observe framework integration, calling base in both. Ordinary mutation code owns just the disposable scope. Low-level ISupportSuspendPainting begin/end calls are infrastructure rather than the recommended application pattern. Existing ListBox and other framework adapters retain their own update integration.

After the intentional exception, the actual checks require balanced target hooks, resumed layout and a successful additional list mutation. They make no timing, flicker, paint-count or rollback guarantee.

## Migration

1. Build the previous example with its pinned SDK and retain its behavior as the comparison.
2. Use the current canonical implementation with the declared Windows Desktop assets.
3. Run Test-FenDesktop.ps1 and inspect the bounded evidence; retain pending owner checks.

## Constraints

Windows desktop and STA execution are required. Current C# uses language15; previous C# uses14. VB uses16.9 on both releases. No extra NuGet package, cloud provider or workload installation is needed. No experimental API suppression is required. This .NET11 edition has no supported .NET10 package backport.

## Complete canonical inputs

### n10-fenbatch-directory-build-props

{{snippet:n10-fenbatch-directory-build-props}}

### n10-fenbatch-directory-build-targets

{{snippet:n10-fenbatch-directory-build-targets}}

### n10-fenbatch-directory-packages-props

{{snippet:n10-fenbatch-directory-packages-props}}

### n10-fenbatch-fenbatch-csproj

{{snippet:n10-fenbatch-fenbatch-csproj}}

### n10-fenbatch-global-json

{{snippet:n10-fenbatch-global-json}}

### n10-fenbatch-program-cs

{{snippet:n10-fenbatch-program-cs}}

### n11-fenbatch-directory-build-props

{{snippet:n11-fenbatch-directory-build-props}}

### n11-fenbatch-directory-build-targets

{{snippet:n11-fenbatch-directory-build-targets}}

### n11-fenbatch-directory-packages-props

{{snippet:n11-fenbatch-directory-packages-props}}

### n11-fenbatch-fenbatch-csproj

{{snippet:n11-fenbatch-fenbatch-csproj}}

### n11-fenbatch-global-json

{{snippet:n11-fenbatch-global-json}}

### n11-fenbatch-program-cs

{{snippet:n11-fenbatch-program-cs}}

## Evidence

The .NET 11 SDK baseline is now 11.0.100 GA with preview selection disabled. The .NET 10 comparisons retain their declared SDKs. Earlier RC audit details below are historical; use the current sample pins, dependency locks and GA verification records for current coverage. Existing native exclusions and pending Human gates still apply.

Real shown controls, descendant layout suspension, target painting hooks, exception disposal and subsequent layout/list mutation. No flicker or throughput measurement. Exact SDK pins, locked dependency closure and input hashes are recorded beside each sample. The older build is a comparison, not a new .NET10 foundation. Windows x64 RC2 proof is not GA or other-platform proof. No service account, remote provider, workload installation or package publication is required.

## References

[Official RC2 contracts](https://raw.githubusercontent.com/dotnet/core/60871330fce2b3da383461be027ebdf3ce30f8fb/release-notes/11.0/preview/rc2/winforms.md).
