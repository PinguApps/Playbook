# Restore required EF design services

## Goal

Keep Fleet's database-development utility working after an EF upgrade without distributing design tooling to runtime-only applications.

## Older approach

EF10 Tools carries a dependency on Design. FleetDesign can therefore register design services through a transitive reference. This is a valid dependency graph for that release; an explicit reference is also valid and communicates ownership more clearly.

{{snippet:n10-program-cs}}

## Newer approach

EF11 Tools and Tasks no longer bring Design into the consumer. Reference Design directly in the project that calls its services. FleetDesign calls AddEntityFrameworkDesignTimeServices, so it needs compile assets as well as runtime assets: PrivateAssets prevents propagation to dependants, while IncludeAssets permits this project to compile against Design. Do not add Design to unrelated application libraries.

{{snippet:n11-fleetdesign-csproj}}

{{snippet:n11-program-cs}}

## Migration

1. Identify the project that invokes design services or runs the affected tooling.
2. Add the matching Design package there. Keep it private; expose compile assets only when directly calling Design APIs.
3. Restore and compile the actual consumer. Register the real services and run its design-time operation.
4. Leave runtime-only projects alone. Tools is for Visual Studio PMC and Tasks for MSBuild integration; neither substitutes for Design.

## Constraints

This edition is for net11.0 with EF11 RC2; the historical net10.0 comparison uses EF10.0.13. No EF11-on-net10 route is claimed. C#15/14, SDK11 GA/SDK10.0.403 and exact dependency locks are included. FleetDesign is a console service-registration consumer, not a provider-specific migration test. It requires no renderer, workload, account or external resource. Windows x64 execution does not establish another platform or GA compatibility.

## Evidence

The .NET 11 SDK baseline is now 11.0.100 GA with preview selection disabled. The .NET 10 comparisons retain their declared SDKs. Earlier RC audit details below are historical; use the current sample pins, dependency locks and GA verification records for current coverage. Existing native exclusions and pending Human gates still apply.

Test-FleetDesign copies each actual project to a disposable directory, removes its direct Design dependency, then builds. EF10 succeeds through Tools; EF11 fails with the missing Design surface. Restoring the direct reference compiles and runs real service registration. No mocks or Visual Studio execution are involved. Sample compilation records separately identify the exact packages and source inputs. Tasks dependency removal is an official package contract; this test exercises the Tools consumer path.

## Complete canonical sources

{{snippet:n10-directory-build-props}}

{{snippet:n10-directory-build-targets}}

{{snippet:n10-directory-packages-props}}

{{snippet:n10-fleetdesign-csproj}}

{{snippet:n10-global-json}}



{{snippet:n11-directory-build-props}}

{{snippet:n11-directory-build-targets}}

{{snippet:n11-directory-packages-props}}



{{snippet:n11-global-json}}




## References

Official EF11 changes, CLI and breaking-change references are listed in structured sources. Complete guidance above remains available offline.
