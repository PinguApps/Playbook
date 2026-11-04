# Capture clean migration SQL

## Goal

Produce a berth-database deployment artifact containing executable SQL while retaining diagnostic information for troubleshooting.

## Older approach

EF10's output option writes the SQL directly to a file, avoiding the need to classify log lines mixed into stdout. This supported route remains valid in EF11. Filtering diagnostic prefixes is brittle and can discard valid SQL or admit a new log format.

{{snippet:n10-capture}}

## Newer approach

EF11 separates generated script stdout from diagnostic stderr. Capture the two streams independently and check the process exit code before using the artifact. Build first and use no-build so build output does not become part of a deployment payload. Verbose diagnostics exercise the separation without any log filtering.

{{snippet:n11-capture}}

## Migration

1. Build the intended project and configuration before generating SQL.
2. Remove any log-line filters from the script pipeline.
3. Redirect stdout to the SQL artifact and stderr to a separate diagnostics file.
4. Reject output from a failed command. Review and validate the script against the intended database engine before deployment.
5. Keep explicit context selection when databases have separate schema ownership.

## Constraints

This stream behavior is for dotnet-ef11 scripting, not arbitrary application logging or other shell commands. Startup code must also avoid writing its own diagnostics to stdout. The sample uses a matching EF11 RC2 tool and net11.0 provider; no older-target route is declared. The historical comparison uses EF10.0.13/net10.0 with output. PowerShell7, SDK11 RC2/C#15 and SDK10.0.402/C#14 are the measured environment. SQLite local execution does not prove another provider or GA. No browser, renderer, workload, service account or production database is required.

## Evidence

Test-FleetTooling captures a real verbose EF11 migrations script with separate stdout/stderr, requires nonempty diagnostics and executes the unfiltered stdout payload against fresh SQLite. The expected model table is present. EF10's output-file script also executes. Canonical projects compile under their declared locked releases; these checks execute real CLI and database artifacts on Windows x64. Installed retrieval separately verifies the packaged full guidance offline. No deployment is performed.

## Complete canonical sources



{{snippet:n10-commands}}

{{snippet:n10-directory-build-props}}

{{snippet:n10-directory-build-targets}}

{{snippet:n10-directory-packages-props}}

{{snippet:n10-fleettooling-csproj}}

{{snippet:n10-global-json}}

{{snippet:n10-program-cs}}



{{snippet:n11-commands}}

{{snippet:n11-directory-build-props}}

{{snippet:n11-directory-build-targets}}

{{snippet:n11-directory-packages-props}}

{{snippet:n11-fleettooling-csproj}}

{{snippet:n11-global-json}}

{{snippet:n11-program-cs}}


{{snippet:n10-migration-20261104182917-initialbeacon}}

{{snippet:n10-migration-20261104182917-initialbeacon-designer}}

{{snippet:n10-migration-beaconcontextmodelsnapshot}}

{{snippet:n10-migration-20261104182857-initialberth}}

{{snippet:n10-migration-20261104182857-initialberth-designer}}

{{snippet:n10-migration-berthcontextmodelsnapshot}}

{{snippet:n11-migration-20261104182926-initialbeacon}}

{{snippet:n11-migration-20261104182926-initialbeacon-designer}}

{{snippet:n11-migration-beaconcontextmodelsnapshot}}

{{snippet:n11-migration-20261104182923-initialberth}}

{{snippet:n11-migration-20261104182923-initialberth-designer}}

{{snippet:n11-migration-berthcontextmodelsnapshot}}

## References

Official EF11 changes, CLI and breaking-change references are listed in structured sources. Complete guidance above remains available offline.
