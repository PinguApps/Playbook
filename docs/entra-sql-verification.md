# Authorized Entra SQL access

PIN-679 provides a real certificate-backed application identity for SQL checks on
Pingu's Windows machine. Pingu authorized the tenant behind **Temp/Test** on
7 November 2026. This is access preparation for PIN-842 and PIN-958, not proof of
their original EF/distributed-cache samples or their eventual authentication modes.

Microsoft documents [Entra authentication for SQL containers](https://learn.microsoft.com/en-us/sql/linux/security/authentication/container-kubernetes-microsoft-entra-deployment?view=sql-server-ver17)
using a real tenant, registered server application and mounted certificate.
The [server application permissions](https://learn.microsoft.com/en-us/sql/relational-databases/security/authentication-access/azure-ad-authentication-sql-server-setup-tutorial?view=sql-server-ver17#grant-application-permissions)
require administrator consent. An identity emulator cannot supply this real-authentication evidence.
This route provisions no hosted SQL, Azure Arc or Key Vault resources.

## Scope and ownership

| Resource | Authorized scope |
| --- | --- |
| Tenant | `e098693d-4698-42b9-81bb-83881e28a826` (tenant behind Temp/Test; also contains other subscriptions) |
| Server application | `Playbook PIN-679 SQL server`, client ID `c801a4a0-381f-4220-981d-c9f363929c3f` |
| Server directory permissions | Microsoft Graph application roles `Application.Read.All`, `Group.Read.All`, `User.Read.All`; these are tenant-wide read grants, not subscription-scoped |
| Client application | `Playbook PIN-679 SQL client`, client ID `836d8bc3-2e01-4e20-908b-4fbfeb1569a2`; service principal `fd9724c6-f9d4-4e77-8171-eae44d85da8a` |
| Client directory permissions | No Graph application roles; no Azure subscription role assigned by this setup |
| SQL container | `playbook-sql-entra-pin679`, ownership label `com.pinguapps.playbook.owner=PIN-679`, loopback-only dynamic port, no restart policy |
| SQL database | `playbook_entra_pin679`; client has `CONNECT` through its mapped user and `SELECT` on `dbo.IdentityProbe` only; no server role or write permission |
| Engine | Existing author-maintained `playbook-sql-verification:17.0.5035.5` image, ID `sha256:0895ccf159789dc9f66eefc7853a75b934fa8aff2d0b649f3f251bd7329aee6a`; SQL Server 2025 RTM-CU10 `17.0.5035.5`, Enterprise Developer |

The existing `playbook-sql-2025` container and its databases are separate and were
left running. Do not use `eng/SqlServer.ps1` to administer this Entra container.
Pingu owns approval of additional sample-specific SQL grants, certificate rotation,
recovery, and eventual removal of the two registrations/service principals and container.
The client is intentionally insufficient for EF migrations or distributed-cache
table creation. Grant only the operations a future sample actually needs in this
isolated database; do not grant `sysadmin` to the client.

## Credentials and lifetime

Private material is outside Git under
`%LOCALAPPDATA%\PinguApps\Playbook\EntraSql`, with inheritance disabled and a
FullControl ACL for the current Windows user. `server.pfx` and `client.pfx` are
separate RSA certificates with no export password, protected by that directory ACL.
The server PFX is mounted read-only at `/var/opt/mssql/server.pfx`. SQL requires
the passwordless PFX for this container route. Only public certificates were
uploaded to Entra; no client secret was created. Both certificates expire on
**5 February 2027 UTC** (server at 11:48:36; client at 11:49:49).
The current user's DPAPI-encrypted `credentials.xml` contains the random SQL `sa`
password for owner-controlled maintenance; it is not the Entra client credential.
Docker administrators can read the container's administrator password and mounted
server key. Trust this machine and Docker engine accordingly.

Do not print tokens, PFX bytes, passwords, full Docker inspection output, or the
credential file. Do not upload this directory. The client certificate can acquire
real tokens noninteractively until expiry/revocation; this is not an offline test.
No owner browser session or MFA is needed for this certificate route. Interactive,
managed identity, password, EF and cache authentication are unverified here;
PIN-958 retains any owner-only sign-in required by the final original sample.

## Repeat the access check

Requires PowerShell 7, running Docker Desktop Linux containers, the provisioned
private directory under the same Windows account, SDK **10.0.402**, and access to
NuGet and Entra endpoints. No Azure CLI login is required for the client probe.
The helper uses a temporary consumer with exact SqlClient **7.1.0**, Azure.Identity
**1.21.0**, and Pkcs **10.0.13** pins, independently of root package/SDK inheritance.
These are manually maintained verification dependencies, with no Renovate permission
change. The helper sets `DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0` only in its child and
checks process and saved user/machine PATH after execution. It removes its temporary consumer.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-1680d41f'
$docker = 'C:\Program Files\Docker\Docker\resources\bin\docker.exe'
$wasRunning = & $docker inspect --format '{{.State.Running}}' playbook-sql-entra-pin679
if ($LASTEXITCODE) { throw 'PIN-679 container is unavailable.' }
if ($wasRunning -ne 'true') {
    & $docker start playbook-sql-entra-pin679
    if ($LASTEXITCODE) { throw 'Could not start PIN-679 container.' }
    Start-Sleep -Seconds 15 # Allow this retained SQL instance to recover.
}
try {
    & ./eng/Test-EntraSql.ps1
}
finally {
    if ($wasRunning -ne 'true') { & $docker stop playbook-sql-entra-pin679 }
}
```

Measured **7 November 2026** with the actual provisioned identity and container:
real Entra certificate token acquisition and SQL acceptance, the expected fixture
row read, SQL error 229 for an attempted insert and for reading `msdb.dbo.sysjobs`.
Expect three `PASS` lines. The helper discovers the current loopback port rather
than assuming the initial `58306` assignment. Trusting the self-signed SQL TLS
certificate is restricted to this local test; it is separate from the Entra certificates.

The server uses `MSSQL_AAD_CLIENT_ID`, `MSSQL_AAD_PRIMARY_TENANT` and
`MSSQL_AAD_CERTIFICATE_FILE_PATH`; an `sa` maintenance connection created the
external client login and mapped database user. Setup verified actual Graph
`appRoleAssignments`: the initial CLI admin-consent command returned success but
left these empty, so the three documented application-role assignments were
explicitly created before SQL login creation succeeded. A successful CLI exit or
server startup alone is not authentication proof.

Stop this container when idle; stopping preserves its writable-layer database and
the private directory. There is no separately managed data volume or backup.
Removing/recreating the container loses the fixture and SQL principal grants.
Pingu must approve/coordinate removal or re-provisioning; do not rerun the original
local setup scripts blindly or replace credentials underneath a retained container.
This document and helper retain the supported scope and repeatable check, not raw
setup output. PIN-958's completed exact-sample proof is described below.

The owner maintenance query confirmed zero entries in `sys.server_role_members`
for the client; its explicit database permissions were exactly `CONNECT` and
`SELECT`. Denial probes run in a transaction which is always rolled back, so an
accidental future write grant cannot leave an inserted row behind.

Re-provisioning is owner-run; the original local provisioning scripts are not
shipped in this repository. Use the tenant/app IDs, pinned image, ownership label,
certificate settings and three Graph roles above with Microsoft's linked setup
instructions, then recreate the external login, database user and fixture grants.
To revoke the server's directory reads, Pingu can remove its three Graph
application-role assignments or delete its dedicated service principal and app
registration. Stop/remove the dedicated container and retire its local certificate
material as part of coordinated decommissioning; do not remove the shared SQL route.

## Original EF and distributed-cache authentication proof

On 7 November 2026, Pingu approved creation of `dbo.ObservationCache` in this
isolated database and temporary SELECT/INSERT/UPDATE/DELETE grants for the existing
client on that table only, followed by revocation and table removal. Both unchanged
canonical MarshAccess samples (.NET 10 and .NET 11) passed `--connect`: actual EF
connection open/close and SQL distributed-cache write/read/remove. This completes
PIN-958 / H_ENTRA_PROOF for `sql.retain-entra-authentication/11.0`.

`eng/Test-MarshEntra.ps1` uses driver-managed **Active Directory Default**, with
the approved tenant/client/certificate supplied only to each child process.
The child sets AZURE_TOKEN_CREDENTIALS=EnvironmentCredential (supported by both locked Azure.Identity versions), so Default can use only the configured certificate route; no token, callback or custom
provider is injected. No interactive login is required for this route. SqlClient
6.1.7 includes its provider; SqlClient 7.1.0 uses the directly referenced Azure
extension 7.1.0. Exact resolved graphs and source/lock/runner SHA-256 hashes are
retained in [the authentication record](verification/entra-sql.json).

The helper refuses a pre-existing cache table, provisions only the approved table,
checks no cache rows remain, revokes its grants and drops its table in `finally`.
Creation and the per-run ownership marker commit atomically. Cleanup checks that marker even after an uncertain setup failure, so it cannot remove a pre-existing table. It restores the container's original running state and checks process/user/machine
PATH. The measured run returned the initially stopped container to stopped. The
original fixture, SELECT grant, credentials and shared SQL route are preserved.

From an unrelated directory, with Docker Desktop running and the original private
directory available to the same Windows account:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-e7377d10'
# Read-only freshness check; no SQL or identity access.
& ./eng/Test-MarshEntra.ps1 -CheckEvidence
# Repeats the approved isolated temporary table/grants and cleans them up.
& ./eng/Test-MarshEntra.ps1 -Record
```

Expect both samples to print `PASS actual EF login and SQL cache write/read/remove`,
followed by empty-cache and table/grant cleanup confirmations. The installed
dotnetup executable selects SDK10.0.403 and SDK11.0.100-rc.3.26522.148 using each
sample's pin; EF/cache11 remain RC2 packages. This is Windows x64 and SQL Server
2025 RTM-CU10 17.0.5035.5 evidence, not GA, interactive/MFA, managed/workload identity,
migration or production TLS-trust proof. SQL server certificate validation is disabled (TrustServerCertificate=True) only
for this isolated loopback check. Refresh after relevant sample/runner/dependency,
identity/certificate, SQL engine or permission changes; Pingu owns coordination.

If a process is killed or the host crashes before cleanup, ask Pingu to inspect the retained table and revoke its temporary grants before dropping it. The helper refuses to reuse an existing table; do not bypass that refusal.

The helper requires explicit -Record for live work; no-switch invocation fails before any SQL access. Certificate selection follows [the documented Default credential selector](https://learn.microsoft.com/en-us/dotnet/azure/sdk/authentication/credential-chains?tabs=dac).
