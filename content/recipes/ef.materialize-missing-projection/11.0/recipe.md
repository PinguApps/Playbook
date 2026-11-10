# Preserve missing non-entity join results

## Goal

Apply the documented .NET11 missing-object join semantics, with explicit-marker fallback while measured RC packages still fail.

## When to use

Pingu authorized official-documentation guidance on 5 November 2026 despite missing RC behavior. EF11 RC2 and RC3 direct DTO/anonymous projections throw on unmatched rows, including grouped DTO/default struct. Nullable struct composition fails translation. The explicit nullable marker returns -1 and 12 on both editions. Intended direct semantics and final artifact behavior remain pending; compilation and recorded failures are not successful execution proof.

## Migration

1. Retain a nullable scalar marker in the inner projection on the measured RCs. Test matched and unmatched rows separately. A missing row is different from a real row containing nullable values.
2. The official .NET11 docs describe whole non-entity projections materializing as null/default through joins and grouping. Use the intended DTO query in FinalProjection below as a qualification target, not as a claim that the measured RCs work.
3. The current seeded consumer has nests 1 and 2; only nest 1 has a count of 12. Intended reference results are a DTO with 12 and a null object. A nullable struct should remain missing; a non-nullable struct uses its zeroed default. Anonymous projections have the same missing-object boundary as DTOs.
4. Preserve the whole inner object when composing joins/grouping; do not equate arbitrary constructor translation with materialization. Arbitrary record constructors, entity FullJoin and top-per-group navigation are separate goals. Nullable-struct member composition is still untranslated on the measured graph.
5. Run the current sample normally to reproduce the failures and verify the marker returns -1 and 12. Run it with `--qualify-final` against the eventual shipped graph to require direct DTO and grouped results; this deliberately fails on the pinned RC3 package. Extend final qualification to anonymous and nullable/default struct controls before removing the fallback.
6. Pingu explicitly authorized this documentation-based delivery on 5 November 2026. Final-package execution is pending. The official intended behavior is not a guarantee about an unreleased artifact; keep PIN-959 and actual-GA reconciliation open.

## Older approach

The independently pinned .NET10 consumer remains a valid comparison.

### .NET10 Directory.Build.props

{{snippet:n10-directory-build-props}}

### .NET10 Directory.Build.targets

{{snippet:n10-directory-build-targets}}

### .NET10 Directory.Packages.props

{{snippet:n10-directory-packages-props}}

### .NET10 global.json

{{snippet:n10-global-json}}

### .NET10 PloverQueries.csproj

{{snippet:n10-ploverqueries-csproj}}

### .NET10 Program.cs

{{snippet:n10-program-cs}}

## Newer approach

The independently pinned .NET11 consumer records the actual RC boundary.

### .NET11 Directory.Build.props

{{snippet:n11-directory-build-props}}

### .NET11 Directory.Build.targets

{{snippet:n11-directory-build-targets}}

### .NET11 Directory.Packages.props

{{snippet:n11-directory-packages-props}}

### .NET11 FinalProjection.cs

{{snippet:n11-finalprojection-cs}}

### .NET11 global.json

{{snippet:n11-global-json}}

### .NET11 PloverQueries.csproj

{{snippet:n11-ploverqueries-csproj}}

### .NET11 Program.cs

{{snippet:n11-program-cs}}

## Evidence

The .NET 11 SDK baseline is now 11.0.100 GA with preview selection disabled. The .NET 10 comparisons retain their declared SDKs. Earlier RC audit details below are historical; use the current sample pins, dependency locks and GA verification records for current coverage. Existing native exclusions and pending Human gates still apply.

Pingu authorized official-documentation guidance on 5 November 2026 despite missing RC behavior. EF11 RC2 and RC3 direct DTO/anonymous projections throw on unmatched rows, including grouped DTO/default struct. Nullable struct composition fails translation. The explicit nullable marker returns -1 and 12 on both editions. Intended direct semantics and final artifact behavior remain pending; compilation and recorded failures are not successful execution proof.

## Constraints

Pending final qualification blocks release-ready packing. This delivery verifies ordinary development packing; PIN-959 and actual-GA reconciliation retain their own gates.

No final-artifact support is inferred from RCs. Source audit baselines, the eight foundations and existing recipe identities remain unchanged.

## References

- [Official EF11 intended behavior](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-11.0/whatsnew)
- [EF migrations deployment](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/applying)
- [Bounded E24/E25/E26 source audit](https://github.com/PinguApps/Playbook/blob/a7fd46cd658708c25a7e0d032735760c3fe6d63f/docs/planning/audits/ef.md)
