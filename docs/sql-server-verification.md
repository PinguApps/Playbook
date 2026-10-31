# Isolated SQL Server verification

Use this Windows authoring route for SQL Server recipes. Pingu authorized its
local Docker provisioning through PIN-676. It accepts no external server or
connection string. It creates only `playbook_<unique_name>` databases in the
labelled `playbook-sql-2025` container. Production access is outside this route.

## Start and check

Requires running Docker Desktop with Linux containers and PowerShell 7. The
provider check also needs the existing .NET SDKs listed below and NuGet access.
Executables are invoked by absolute path; neither setup nor checks change PATH.
The first start builds a roughly 5 GB local image, accepts Microsoft's SQL Server
terms for non-production Developer use, generates credentials, and retains a data
volume. Later starts reuse them. No cloud account or paid service is required.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-345136f8'
& ./eng/SqlServer.ps1 -Action Start
& ./eng/Test-SqlServer.ps1
& ./eng/SqlServer.ps1 -Action Stop
```

Expect the loopback address and assigned port, SQL capability `PASS`, both EF
provider `PASS` messages, duplicate/data-preservation and peer-access `PASS`
messages, and cleanup of every generated database/login. SQL or consumer failures
throw. `Test-SqlServer.ps1` leaves the server running for inspection; stop it when
finished. No second terminal is needed. The check creates only disposable test
data, builds temporary consumers outside the repository, and removes those files.

Override `-Docker` on either script or `-Net10Dotnet`/`-Net11Dotnet` on the provider
check with existing absolute executable paths when installations differ. The
defaults are Docker Desktop's CLI, `C:\Program Files\dotnet\dotnet.exe`, and
`C:\Users\pingu\.dotnet-playbook-11\dotnet.exe`. The dotnetup SDK directory on
this machine advertised SDK 10.0.402 but lacked its runtime configuration; the
check uses the complete installations without changing that shared setup.

## Agent database and consumer access

Choose a unique name for each recipe/run. `New` refuses existing names rather
than resetting data. Each login owns its database, with no server role or database
creation permission. Control-plane actions and `Run` consumers are serialized
within the Windows session; distinct databases separate their data.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-345136f8'
& ./eng/SqlServer.ps1 -Action Start
$database = 'playbook_reef_' + [guid]::NewGuid().ToString('N')
& ./eng/SqlServer.ps1 -Action New -Database $database
try {
  # Executes the real SQL feature checks under this database's restricted login.
  & ./eng/SqlServer.ps1 -Action Query -Database $database -SqlFile ./eng/sql-server/Capabilities.sql
}
finally {
  & ./eng/SqlServer.ps1 -Action Remove -Database $database
  & ./eng/SqlServer.ps1 -Action Stop
}
```

For an original sample, use `-Action Run -Database $database -Executable <absolute
exe> -CommandArguments <string array> -WorkingDirectory <absolute directory>`.
`Run` supplies `PLAYBOOK_SQL_CONNECTION_STRING` only in the child process. Read it
through `Environment.GetEnvironmentVariable` and pass it to the SQL provider.
The checked [provider probe](../eng/sql-server/ProviderProbe.cs) demonstrates this;
`Test-SqlServer.ps1` is its executable caller. Do not print the variable, enable
connection-string logging, or copy it into source, tickets or receipts.

`Query` uses the image's SQLCMD with quoted identifiers, failure exit codes and
120-second query timeout. SQLCMD shell commands and variable substitution are
disabled. Run only trusted authored SQL; the database-owner identity can change
or remove all data in its own database. Local TLS trusts the container certificate;
this is not guidance for trusting certificates on external servers.

## Verified capability baseline

Measured **31 October 2026**, on this Windows machine's Docker Linux engine:

| Surface | Observed baseline and check |
| --- | --- |
| Server | SQL Server 2025 RTM-CU10, `17.0.5035.5`, Enterprise Developer, Ubuntu 24.04.5, `SQL_Latin1_General_CP1_CI_AS` |
| Image | Base digest `sha256:245f6b1c8903edae52cb20fa31419cefcde1a49938bb8c3447099011fc871ca3`; engine and `mssql-server-fts` packages exactly `17.0.5035.5-3` |
| Compatibility | Actually changed the disposable database to 150 and back to 170; provider probes explicitly select 170 |
| JSON | Real `JSON_PATH_EXISTS`, native `json`, `JSON_CONTAINS` with array wildcard, and `CREATE JSON INDEX`; JSON index requires quoted identifiers and clustered primary key |
| Vector | Real `vector(3)`, exact `VECTOR_DISTANCE`, `CREATE VECTOR INDEX` and direct ranked `VECTOR_SEARCH` with five returned rows; test database enables `PREVIEW_FEATURES` |
| Full-text | Installation confirmed; actual catalogue/index population and ranked `CONTAINSTABLE` query in English (1033) |
| Previous provider | .NET SDK `10.0.402`, EF SQL Server `10.0.13`, SqlClient `6.1.7`; actual parameterized insert and read over loopback |
| Current provider | .NET SDK `11.0.100-rc.2.26504.105`, EF SQL Server `11.0.0-rc.2.26504.105`, SqlClient `7.1.0`; same real database probe |
| Dependency safety | Both temporary consumers explicitly pin `System.Security.Cryptography.Pkcs` `10.0.13`, overriding vulnerable 9.x transitive defaults observed during initial restoration |
| Isolation | Duplicate creation refused with existing data preserved; each consumer denied access to another test database; disposable databases and logins removed |

The [Dockerfile](../eng/sql-server/Dockerfile) and provider pins are deliberately
author-maintained. No new automatic Renovate permissions apply; Docker is not an
enabled manager, and temporary consumer manifests are outside the tracked project
inventory. Update engine and full-text packages together, reconcile provider/SDK
pins against recipe requirements, and rerun the actual route after changes.
This does not change root tool/test dependencies or their .NET 10 boundary.

### Limits that remain visible

- SQL Server vector indexing/search remains preview and requires the database
  opt-in. On this exact build, `IF NOT EXISTS (SELECT 1 FROM VECTOR_SEARCH(...))`
  caused an access violation and terminated that session. The equivalent direct
  ranked `SELECT TOP (5) notes.Id, matches.distance ... ORDER BY matches.distance`
  passed. The probe uses that supported documented query shape. Do not infer that
  arbitrary composed queries or hybrid ranking have been verified.
- Compatibility 150 on a 2025 engine is **not SQL Server 2019 execution**. Recipes
  making an older-engine claim must obtain and test that actual engine separately.
- The EF 11 result is RC evidence, not GA evidence. These probes establish access
  and basic provider compatibility, not every recipe's EF translation/migration.
  Each recipe still needs its own focused execution against its exact artifacts.
- No Entra identity/authentication is established; PIN-679 owns that separate gate.
  No cloud/Azure-specific behavior or installed MCP content changes are claimed.

See Microsoft's [Linux full-text setup](https://learn.microsoft.com/en-us/sql/linux/install-upgrade/setup-full-text-search?view=sql-server-ver17),
[JSON containment prerequisites](https://learn.microsoft.com/en-us/sql/t-sql/functions/json-contains-transact-sql?view=sql-server-ver17),
[JSON indexes](https://learn.microsoft.com/en-us/sql/t-sql/statements/create-json-index-transact-sql?view=sql-server-ver17),
and [vector indexing availability](https://learn.microsoft.com/en-us/sql/t-sql/statements/create-vector-index-transact-sql?view=sql-server-ver17).

## Credentials, retention and cleanup ownership

The current Windows account generates random administrator and per-database
credentials. They are DPAPI-encrypted CLIXML under
`%LOCALAPPDATA%\PinguApps\Playbook\SqlServer`, with directory access restricted to
that user. They are not portable credentials or a shared-team secret store.
Passwords enter Docker/SQLCMD through child environments, never command-line
arguments. Docker necessarily retains the administrator password in its container
configuration; anyone controlling this local Docker engine is trusted and can
read it. Do not share full `docker inspect` output, logs containing secrets, or
the credential directory. Neither the scripts nor this document publish secrets.

Agents remove their database/login with `Remove` in `finally`; removal checks
the local credential identity and database ownership marker, plus the container,
volume labels, loopback binding and expected image. It never accepts arbitrary
server destinations or database names outside the prefix. A failed initial setup
may leave credentials or a volume for recovery: preserve them and ask Pingu rather
than silently adopting or deleting an unknown resource.

**Pingu owns eventual container/volume/credential removal**, recovery and refresh
coordination. Agents leave the server stopped when idle. `Stop` retains the
container, `playbook-sql-2025-data` volume, image and encrypted credentials; the
server has no automatic restart policy. Refreshes must preserve other authors'
databases. No destructive whole-server removal command is supplied as a routine
test cleanup step.
