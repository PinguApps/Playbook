# Isolated local Cosmos verification

Pingu authorized this free local route through PIN-677. It uses Microsoft's
**legacy Linux Docker emulator**, not an Azure account or production data. Requires
PowerShell 7 on this Windows authoring machine and running Docker Desktop with
Linux x64 containers. No Azure subscription, cloud credentials or Windows emulator
installation is needed. Executables use absolute paths; setup does not change PATH,
Windows certificate trust, firewall rules or persistent environment variables.

## Start and check

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-412e0cd4'
& ./eng/Cosmos.ps1 -Action Start
try {
  & ./eng/Test-Cosmos.ps1
}
finally {
  & ./eng/Cosmos.ps1 -Action Stop
}
```

The first start downloads the digest-pinned legacy image and creates only the
labelled `playbook-cosmos-legacy` container. Its sole published port is a dynamically
assigned **127.0.0.1** HTTPS gateway; direct-mode ports are not published. Startup
prints the actual endpoint. The container is limited to 4 GiB, has no mounts or
privileged mode, and has no automatic restart policy. Subsequent starts reuse it.
No second terminal is needed. The provider check requires NuGet access and the
existing SDK installations below. It creates temporary consumers outside the
repository, then removes them and their disposable databases on success or a caught
exception. An abruptly killed process can leave its temporary directory or database;
use the printed database name for recovery rather than deleting other runs' data.
It leaves the emulator running; stop it when finished.

Expect NoSQL lifecycle and both EF provider `PASS` messages, token handoff, batch
rollback and bulk-write checks, plus explicit indexing capability results. A
required consumer or pinned indexing-policy baseline failure throws. Setup and consumer processes have a
ten-minute operation timeout; investigate a timeout rather than assuming a pass.

Override `-Docker` on either script or `-Net10Dotnet`/`-Net11Dotnet` on the provider
check with existing absolute executable paths if installations differ. Defaults:
Docker Desktop's CLI, `C:\Program Files\dotnet\dotnet.exe`, and
`C:\Users\pingu\.dotnet-playbook-11\dotnet.exe`. SDKs/packages are manually pinned;
the temporary projects inherit no root build/package configuration. Docker is not
an enabled Renovate manager; no automatic update permissions are added.

## Agent access and test-data ownership

Choose a fresh name per recipe/run. `New` refuses existing databases. `Remove`
requires both a `playbook_` name and the database's route marker; it does not adopt
or reset an existing database. Container ownership, image, bindings, network and
mounts are checked before lifecycle actions. Commands are serialized in the Windows
session so `Stop` cannot interrupt a route-managed `Run`.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-412e0cd4'
& ./eng/Cosmos.ps1 -Action Start
$database = 'playbook_reef_' + [guid]::NewGuid().ToString('N')
$created = $false
try {
  & ./eng/Cosmos.ps1 -Action New -Database $database
  $created = $true
  # Run a trusted consumer as described below.
}
finally {
  if ($created) { & ./eng/Cosmos.ps1 -Action Remove -Database $database }
  & ./eng/Cosmos.ps1 -Action Stop
}
```

Use `-Action Run -Database $database -Executable <absolute exe>
-CommandArguments <string array> -WorkingDirectory <absolute directory>` for a
trusted sample. It supplies `PLAYBOOK_COSMOS_ENDPOINT`, `PLAYBOOK_COSMOS_KEY`,
`PLAYBOOK_COSMOS_DATABASE` and `PLAYBOOK_COSMOS_CERT_SHA256` only to the child.
The key is Microsoft's **public emulator key**, not a cloud credential. Do not
substitute Azure credentials or external endpoints. Returned diagnostics redact
the emulator key; no secret files or real account keys are created.

The checked [provider consumer](../eng/cosmos/ProviderProbe.cs) and
[transport helper](../eng/cosmos/EmulatorTransport.cs) demonstrate access. Configure
**Gateway** mode and **LimitToEndpoint**, because service metadata can advertise
internal endpoints unavailable through the published port. Use a separate
`HttpClient` per Cosmos client. The helper pins the certificate obtained directly
from the owned container; it does not disable certificate checks globally or add
certificates to Windows. Use that factory for the emulator consumer only.

All code runs under the current Windows user. The master emulator key permits
access to all databases in this dedicated container. Unique names and markers
prevent accidental lifecycle collisions; they are **not a tenant authorization
boundary or a sandbox for untrusted consumers**. The local Docker engine and its
other containers are trusted. No Entra/RBAC, resource-token isolation or production
authentication claim is established.

## Verified baseline and limits

Measured **31 October 2026** on this machine's Docker Linux amd64 engine:

- Legacy emulator `2.14.28.0 (b9ee566d)`, image digest
  `sha256:9a4f69121f20c1d6502db84625c71ebb441ab61b01880a7b2d3d8031c6dac429`.
- .NET SDK `10.0.402` / EF Cosmos `10.0.13` and SDK
  `11.0.100-rc.2.26504.105` / EF Cosmos `11.0.0-rc.2.26504.105`.
  Both consumers explicitly pin Cosmos SDK `3.63.1` and Newtonsoft.Json `13.0.4`.
  EF 11 is RC evidence, not GA proof.
- Both providers executed async insert/query/update. Independent SDK clients
  carried a real session token and read the saved value. A conflicting
  same-partition transactional batch rolled back its preceding write. Eight
  opted-in SDK bulk writes persisted with the expected count.
- Actual create/read/delete, duplicate refusal, and certificate/ownership-marker
  rejection passed. Path/composite,
  three-dimensional flat-vector and full-text policy definitions were accepted
  and read back with nonempty index definitions. Their query execution and
  performance are **not verified** by these access probes.

Each recipe still needs its own focused query, persistence and migration evidence
against its exact artifacts. Policy acceptance/read-back alone proves no index
execution or performance. Local session-token handoff does not establish cloud
replica lag, consistency enforcement, multi-region behaviour or availability.
Neither successful SDK bulk calls nor a successful batch establishes a throughput
improvement or cross-partition atomicity. No RU/cost/performance or Azure service
parity is claimed. Unsupported features remain explicit gaps for affected recipes;
closing this environment ticket does not complete their Human/runtime proof.

The image reported **132 days remaining in its evaluation period** at first start.
This pinned baseline is not permanent: Pingu owns refresh coordination before
expiry or when a sample requires a different service capability. Update the pin
deliberately and rerun the actual checks; never relabel old evidence as fresh.
If the container/image is missing or ownership checks fail, preserve retained data
and investigate rather than silently replacing it.

**Restart persistence failed in the actual stop/start check**, despite
`AZURE_COSMOS_EMULATOR_ENABLE_DATA_PERSISTENCE=true`. The database marker returned
HTTP 404 after restart. Treat this image's data as disposable, valid only within
the running session; do not depend on that flag for retention. `Stop` refuses while
**any database remains**, so one author cannot routinely stop another author's
active data. Clean up your own marked database first. If another author still has
data, leave the emulator running. Manual Docker shutdown, Docker Desktop restarts
or host crashes remain outside that protection.

An interrupted `New` or failed compensating cleanup can strand a database before
its marker is written. `Remove` deliberately refuses an unmarked database; ask
Pingu to inspect/recover that exact database rather than bypassing ownership checks.
The Stop refusal lists remaining database names to help identify interrupted runs.

`Stop` retains the empty container and its writable layer, not a promise of data
durability. **Pingu owns eventual container/image removal** and recovery decisions.
No whole-emulator reset is part of an ordinary test run. Leave it stopped when idle
and empty. The failed restart check is a bounded emulator limitation, not a Cosmos
cloud durability result.

## Future switch to vNext

Keep legacy as the approved default. Do not substitute `vnext-latest` automatically.
Microsoft's [vNext support table](https://learn.microsoft.com/en-us/azure/cosmos-db/emulator-linux)
currently documents missing authentication enforcement, partial indexing, no
full-text index, and a .NET bulk-execution limitation. Some operations accept
metadata without implementing the underlying feature. The [legacy emulator
overview](https://learn.microsoft.com/en-us/azure/cosmos-db/emulator) also documents
differences from the Azure service; neither route establishes cloud parity.

Reassess when a specific released vNext image claims the capabilities needed by
**all our Cosmos recipes**, including path/composite/vector/full-text policies and
actual relevant queries, EF translations and stored-document migrations, SDK bulk
writes, same-partition batch semantics, and session-token handling. Verify the
legacy gaps too; vNext need not reproduce accidental legacy defects. Keep external
service behaviour that neither emulator implements explicitly outside local proof.

For a switch, pin that vNext release/digest in a separate candidate container and
run both provider baselines plus each affected recipe's focused checks. Verify
HTTPS, authentication/trusted-local access limits, lifecycle/isolation, cleanup and
restart persistence. Require actual feature execution, not HTTP success or accepted
configuration. Preserve the legacy container/data while comparing. Switch only in
a reviewed change with refreshed capability observations and recipe evidence;
record any remaining gaps honestly. No polling job or cloud fallback is configured.

Official setup: [develop with the emulator](https://learn.microsoft.com/en-us/azure/cosmos-db/how-to-develop-emulator).
