# Retain Entra authentication after a SQL driver upgrade

## Goal

Restore built-in Entra authentication for EF SQL Server and SQL distributed cache when upgrading to SqlClient 7, preserving the intended host identity.

## When to use

EF SQL Server and SQL distributed cache share this dependency-migration goal. SqlClient 6.1.7 includes built-in Entra providers; SqlClient 7.1.0 requires the directly referenced matching Microsoft.Data.SqlClient.Extensions.Azure 7.1.0 for driver-managed Active Directory authentication. Active Directory Default selects credentials available to the host; review that selection explicitly. Replace obsolete Active Directory Password with a supported host-appropriate flow. No password mode is retained in these samples. Application-supplied AccessToken/AccessTokenCallback does not require this extension merely to supply a token.

## Older approach

The .NET 10 consumer uses SqlClient 6.1.7, whose built-in provider is included in the driver. Active Directory Default is already a valid older approach. Keeping it preserves the host credential-selection policy; the dependency upgrade does not require changing identities.

### .NET 10 global.json

{{snippet:n10-global-json}}

### .NET 10 Directory.Build.props

{{snippet:n10-directory-build-props}}

### .NET 10 Directory.Build.targets

{{snippet:n10-directory-build-targets}}

### .NET 10 Directory.Packages.props

{{snippet:n10-directory-packages-props}}

### .NET 10 MarshAccess.csproj

{{snippet:n10-marshaccess-csproj}}

### .NET 10 Program.cs

{{snippet:n10-program-cs}}

## Newer approach

Reference both driver and Azure extension explicitly. Keep EF and SQL cache aligned on the intended server, database and authentication mode. The program checks registration without contacting a service; --connect is a separate authorized operation.

### .NET 11 global.json

{{snippet:n11-global-json}}

### .NET 11 Directory.Build.props

{{snippet:n11-directory-build-props}}

### .NET 11 Directory.Build.targets

{{snippet:n11-directory-build-targets}}

### .NET 11 Directory.Packages.props

{{snippet:n11-directory-packages-props}}

### .NET 11 MarshAccess.csproj

{{snippet:n11-marshaccess-csproj}}

### .NET 11 Program.cs

{{snippet:n11-program-cs}}

## Migration

1. Inspect the resolved driver in every executable EF/cache host. A transitive driver upgrade can change authentication independently of the target framework.
2. For SqlClient 7 built-in Entra modes, directly reference the matching Azure extension in each host and restore its full dependency graph.
3. Replace Active Directory Password with an identity flow supported by the deployment. Interactive login fits a developer session; managed/workload identity fits appropriately configured hosted workloads. Review Default credential selection rather than assuming it chooses the intended principal.
4. Run both canonical projects with their pinned SDKs. Confirm the provider and EF/cache configuration checks pass.
5. On an authorized isolated target, precreate the cache table, grant only required access, supply MARSH_SQL_CONNECTION and run --connect. Check EF login and cache write/read/remove. PIN-958 owns this pending Human gate; configuration checks are insufficient.

## Constraints

Actual SDK10.0.403/C#14/EF10.0.13/cache10.0.13/SqlClient6.1.7 versus SDK11.0.100-rc.3.26522.148/C#15/EF11 and cache11.0.0-rc.2.26504.105/SqlClient7.1.0/Azure extension7.1.0. Exact asset graphs are locked in each sample; runtime and package version are separate constraints. No older-runtime EF11 route is offered. Windows x64 compilation and configuration execution only; no rendering mode, workload or experimental opt-in. RC evidence is not GA verification.

PIN-679 provides a certificate-backed SELECT-only test identity, not a Default credential session or cache grants. Do not substitute mocked tokens or bypass its restrictions. Keep pending H_ENTRA_PROOF visible until exact sample authentication is proven. SQL password and Windows integrated security do not require this extension. No resources are provisioned by this recipe.

## Evidence

The canonical projects compile and execute local registration checks. Their verification records retain exact resolved dependencies and source hashes. Real authentication and cache operations remain pending PIN-958; PIN-965 owns GA reconciliation.

## References

- [SqlClient Entra authentication](https://learn.microsoft.com/en-us/sql/connect/ado-net/sql/azure-active-directory-authentication?view=azuresqldb-current)
- [SqlClient authentication migration](https://github.com/dotnet/SqlClient/wiki/Migration-Security-and-Authentication)
