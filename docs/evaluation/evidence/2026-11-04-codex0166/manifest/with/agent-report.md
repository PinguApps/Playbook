I’ll check the project’s target framework and consult Playbook, then implement and test the comparison using the pinned SDK.

The project targets .NET 11/C# 15. Playbook’s in-memory `FullJoin` guidance applies: it preserves both unmatched sides, with explicit sorting and separate duplicate validation. I’ll use ordinal comparison, nullable quantities, and validation on each manifest before building the report.

The implementation and checks are ready. The pinned SDK is installed, but `dotnet build` is triggering a blocked network request. I’m checking whether its local build tools can run without that request; the console project needs no packages or workloads.

The project builds with the pinned SDK’s local MSBuild, and all 65 checks pass. They cover empty inputs, equal quantities, zero versus missing values, ordinal ordering under Turkish culture, shuffled inputs, distinct Unicode forms, and duplicates on either side.

Implemented `Reconciliation.Compare`, preserving the public interface.

It retains all keys and quantities, uses null for missing values, sorts ordinally, and rejects duplicates with `ArgumentException`.

Consulted applicable .NET 11 Playbook guidance. Built using the pinned SDK’s local MSBuild and passed all 65 checks.
