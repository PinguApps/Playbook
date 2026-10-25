# Device-bound session availability: PIN-692

Reviewed **25 October 2026**. `WEB-071` / `identity.bind-session-to-device`
is **unavailable on the pinned .NET 11 RC2 baseline**. Close this RC availability
finding; do not create a recipe using the removed RC1 implementation. This is
an availability disposition, not an exclusion of experimental APIs or a claim
that GA has shipped. The separate [PIN-964 / GA_WEB](https://linear.app/pinguapps/issue/PIN-964)
must recheck actual final artifacts before stable publication.

## Reconciled evidence

The [replay](../../../eng/Inspect-DbscAvailability.ps1) and
[recorded evidence](dbsc-availability.assets.json) retain immutable official source
URLs/hashes, package hashes, embedded source identity, dependency minima and
metadata results. They extend [PIN-709's web audit](web.md); that earlier audit
left this availability decision with PIN-692.

| Evidence | Finding and consequence |
| --- | --- |
| [Cumulative include](https://github.com/dotnet/AspNetCore.Docs/blob/ecf7aca5136aea60aa8db7330f66ac799d3edcf7/aspnetcore/release-notes/aspnetcore-11/includes/device-bound-sessions.md) | Still advertises experimental DBSC over cookie authentication. This source is stale against RC2; experimental eligibility is unchanged. |
| [Canonical RC1 notes](https://github.com/dotnet/core/blob/60871330fce2b3da383461be027ebdf3ce30f8fb/release-notes/11.0/preview/rc1/aspnetcore.md#experimental-device-bound-session-credentials-support) and [RC1 public API](https://github.com/dotnet/aspnetcore/blob/c3325eeb6b47bc6383c127d4f4827dc9642a2b6e/src/Security/Authentication/DeviceBoundSessions/src/PublicAPI.Unshipped.txt) | Historical implementation and registration APIs existed. Retain this positive control rather than treating the candidate as fictional or never published. |
| [Canonical RC2 notes](https://github.com/dotnet/core/blob/60871330fce2b3da383461be027ebdf3ce30f8fb/release-notes/11.0/preview/rc2/aspnetcore.md#changes-since-the-previous-preview) | Explicitly removes the implementation and directs RC1 experimenters to an existing supported authentication scheme. This is removal, not merely an experimental warning. |
| [Merged removal PR #69479](https://github.com/dotnet/aspnetcore/pull/69479), commit [4a708708](https://github.com/dotnet/aspnetcore/commit/4a70870818bf5662f21b0c28f9a8c267dccb77f0) | The RC2 tag resolves to this commit; the former DBSC public-API path returns 404. Source deletion corroborates the release note. |
| [Official NuGet version index](https://api.nuget.org/v3-flatcontainer/microsoft.aspnetcore.authentication.deviceboundsessions/index.json) | Only `0.11.0-rc.1.26425.128` is published at review time. Note the experimental package's **0.11** version line. The matching `0.11.0-rc.2.26504.105` package returns 404. |
| Actual RC1 package ZIP / PE metadata | `lib/net11.0/Microsoft.AspNetCore.Authentication.DeviceBoundSessions.dll` contains `AddDeviceBoundSession` and DBSC types. Nuspec source is VMR commit `3551975be08744f0418857c5bed8ab1545c5dd47`; it records RC1 framework dependency minima, IdentityModel `8.19.2` and CBOR. This is historical artifact evidence, not a supported RC2 route. |
| Actual `Microsoft.AspNetCore.App.Ref` **11.0.0-rc.2.26504.105** ZIP / PE metadata | All **132** `ref/net11.0` assemblies were inspected: no DBSC type or method names. The package was not folded into the framework surface. Framework absence alone would be insufficient for an out-of-band package; source deletion and the package index complete the bounded finding. |

The live official repository heads were resolved during this audit. dotnet/core
advanced from PIN-709's `ffed591b` to `60871330`; the RC1/RC2 source hashes are
unchanged. AspNetCore.Docs remains at `ecf7aca5`. The RC2 implementation pin agrees
with the existing web baseline. No final-release API or browser support is inferred
from the old package remaining downloadable, version numbers, or source prose.

## Disposition and retained history

- **16 October:** inventory retained an unresolved availability candidate because
  cumulative/RC1 guidance conflicted with RC2 removal. This was never an exclusion
  for being experimental; the original 156-row planning count remains historical.
- **24 October:** PIN-709 retained PIN-692 as availability owner while auditing
  the other web surfaces.
- **25 October:** PIN-692 closes that current finding as **unavailable on pinned
  RC2**, using both official source and actual package/API evidence. The stable
  candidate ID and its history remain in the [web register](../inventory/02-web-data.md).
- **GA_WEB / PIN-964:** independently inspect final official notes, package index,
  actual final package/reference APIs and source before stable publication. RC
  evidence cannot be relabelled as GA proof. If DBSC returns, separately scope
  and estimate an original .NET 10/11 recipe before marking it included. Require
  actual package/dependency pins and opt-ins, browser/version/flag and protocol
  prerequisites, compile proof and a real registration/refresh/proof-of-possession
  runtime check. A browser's experimental implementation is not assumed usable.

No served recipe, MCP tool, application project, dependency or CI policy changes.
The old RC1 APIs are recorded only as audit history. No fallback DBSC API is
invented, and no additional delivery ticket is justified while it is unavailable.

## Replay and limits

Prerequisites: PowerShell 7 and network access to official raw GitHub/NuGet.
No SDK installation, package restore, build or browser setup is needed. From a
fresh PowerShell window:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-1d97f900'
& ./eng/Inspect-DbscAvailability.ps1 `
    -OutputPath ./artifacts/dbsc.replayed.json `
    -CompareWith ./docs/planning/audits/dbsc-availability.assets.json
$audit = Get-Content ./artifacts/dbsc.replayed.json -Raw | ConvertFrom-Json
$audit.disposition
$audit.packageIndex.versions
$audit.rc1Package.symbols | Where-Object { $_ -eq 'method AddDeviceBoundSession' }
$audit.rc2References.assemblies.Count
$audit.rc2References.dbscSymbols.Count
$audit.gaRecheck
```

Expect `PASS`, the unavailable/GA-not-verified disposition, the sole RC1 version,
the positive registration-method control, **132**, **0**, and PIN-964's final
artifact recheck. A missing source/archive, unexpected HTTP response, changed
version index or evidence mismatch fails the replay and requires reassessment.
The report under `artifacts/` may be deleted after inspection.

This is metadata/source inspection, not compilation, an application restore,
RC1-on-RC2 compatibility proof, or a live browser/protocol/security test. It does
not establish that an archived RC1 binary cannot execute on a newer runtime.
No final GA artifacts were verified. No inspected code is executed or installed;
PATH, certificate trust, persistent settings and running services are untouched.
See the [verification ledger](../../verification/PIN-692.md) for executed checks.
