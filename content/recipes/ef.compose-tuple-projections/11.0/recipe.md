# Compose database queries over tuple-valued projections

## Goal

Keep tuple constructor keys usable in later grouping, filtering and joins; retain anonymous keys on EF10.

## When to use

Actual SQLite execution: ValueTuple/Tuple grouped keys, ValueTuple joined key, one-element arity and nested Rest. EF10 rejects these shapes; anonymous grouped key works. Arbitrary record constructors, all tuple arities, other providers and GA are excluded.

The shared PloverQueries consumer includes companion missing-projection controls. FinalProjection qualifies that companion recipe's deferred final behavior.

## Migration

1. Keep EF10's anonymous key when comparing or retaining an older application. It executes the same grouped count of two without tuple-member translation.
2. For EF11, construct ValueTuple or Tuple explicitly inside the expression tree. Tuple literal syntax is not supported in expression trees. Group on the constructor result, then access Item1/Item2 in later query steps.
3. Run the joined-key control to select the count of 12 through the tuple's first member. Run the one-element ValueTuple and eight-element nested Rest controls before using those bounded shapes.
4. Execute the consumer with the pinned SDK and locked graph. Inspect actual CommandExecuted SQL and exact results; ToQueryString alone is not execution evidence.
5. Preserve provider and constructor limits. These checks do not establish arbitrary records, every tuple arity, other providers, NativeAOT or GA behavior. The .NET10 anonymous alternative remains valid and no ninth foundation is introduced.

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

Actual SQLite execution: ValueTuple/Tuple grouped keys, ValueTuple joined key, one-element arity and nested Rest. EF10 rejects these shapes; anonymous grouped key works. Arbitrary record constructors, all tuple arities, other providers and GA are excluded.

## Constraints

No final-artifact support is inferred from RCs. Source audit baselines, the eight foundations and existing recipe identities remain unchanged.

## References

- [Official EF11 intended behavior](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-11.0/whatsnew)
- [EF migrations deployment](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/applying)
- [Bounded E24/E25/E26 source audit](https://github.com/PinguApps/Playbook/blob/a7fd46cd658708c25a7e0d032735760c3fe6d63f/docs/planning/audits/ef.md)
