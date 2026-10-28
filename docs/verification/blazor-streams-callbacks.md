# Blazor upload and callback migration observations

PIN-1148 / PIN-1149, measured 28 October 2026 on Windows x64 through the existing
Chrome extension connection. These are focused real-browser observations, retained
because compilation cannot establish circuit transfer or component event dispatch.
The canonical samples' `verification-evidence.json` records separately retain the
actual compiler commands, complete input hashes, SDK inventory and dependency closure.

## Assets and boundary

| Sample baseline | SDK / language | Runtime and ASP.NET reference / implicit Internal.Assets |
| --- | --- | --- |
| net10.0 | 10.0.402 / C# 14 | 10.0.13 |
| net11.0 | 11.0.100-rc.2.26504.105 / C# 15 | 11.0.0-rc.2.26504.105 |

No external provider, native SDK, optional workload or production resource was used.
`Inspect-BlazorRemovedApis.ps1 -CompareWith` matched the committed pinned
[artifact/signature inventory](../planning/audits/blazor-removed-apis.assets.json).
That inventory proves the old type/constructor/accessors, all twenty old event
overloads and supported consumer signatures against actual assets; the four new
projects compile against those dependency sets. It does not prove browser behavior.
The generic callback API and bounded file consumer were already supported on .NET 10.

## Real Interactive Server checks

The hosts were built and started by `eng/Test-BlazorMigrationSamples.ps1 -KeepRunning`.
They served actual `blazor.web.js` and connected SignalR circuits, not mocked browser
or file abstractions. Each page first displayed `Interactive: True`.

| Upload input and action | net10.0 result | net11.0 result |
| --- | --- | --- |
| Generated UTF-8 `small.log`, 33 bytes | Accepted, 33 bytes read, processed count 1 | Accepted, 33 bytes read, processed count 1 |
| Generated `large.log`, 1,048,577 bytes | Rejected or transfer failed, zero bytes read, count still 1 | Rejected or transfer failed, zero bytes read, count still 1 |
| Generated `cancel.log`, 1,048,576 bytes; click Cancel after progress | Reading observed at 4,096 bytes; Cancelled at 131,072 bytes, count still 1 | Reading observed at 12,288 bytes; Cancelled at 131,072 bytes, count still 1 |

The byte ceiling is 1,048,576. The over-limit file is rejected by OpenReadStream
before any sample read/processing. It is not a malicious-client security guarantee.
Cancellation is cooperative: bytes can advance between observation, click delivery
and cancellation. The observed 131,072-byte result is not an API guarantee.
The 4 KiB read buffer and 50 ms simulated decoding delay belong to application
work, not transport configuration. No disk path or persistence is used.

The initial real cancellation check failed: a standalone Razor file lacking
`Microsoft.AspNetCore.Components.Web` compiled but left the button unwired.
Adding the import and repeating the same browser actions produced the results
above on both releases. Compilation alone would have missed that defect.

On each GreenhouseValves page, a native keyboard `v` produced Selected: v / updates:
1. A mouse pointer interaction then showed phase: awaiting, confirmation updates:
0 and child completion: 0. After the asynchronous handler returned, the parent
showed Confirmed: mouse / updates: 1 / phase: complete, and child completion: 1.
This exercises the actual component receiver and awaited InvokeAsync path.
The twenty old extension calls and twenty new generic forms compile for all ten
event-argument types; only keyboard and pointer receive real browser interactions.
State-update counts are not render-batch or exactly-once delivery guarantees.

Built sample DLL SHA-256 values used for these observations:

| Sample | SHA-256 |
| --- | --- |
| net10.0/EstuaryLogs | db4be92d88417a69d94412df4121d9fef1aacdaae5b15fd70d93a689dcf738ec |
| net11.0/EstuaryLogs | 1dc5c1a88d91207fa3b8cb5d6d9715804c59835c5717afd967c2cfa6b81144fd |
| net10.0/GreenhouseValves | 7c58274ff2afec062782d1e08902741f3c807bd249f03a5c2fa09ea03e1ecb94 |
| net11.0/GreenhouseValves | 174f57617aeb8c0f7e01afe8c20bd77ed4561ca9346b624881931752362998ba |

The input hashes in the saved sample records identify the reviewed source bytes;
the source-commit/dirty fields identify capture provenance rather than requiring
every unrelated repository commit to trigger compilation. Raw DOM receipts and
screenshots are disposable under `artifacts/blazor-migrations/`.

## Reproduce

Requires both pinned SDKs in the selected .NET installation, PowerShell 7 and a
real browser. From an unrelated starting directory, this selects the exact worktree:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-2aa91b80'
& ./eng/Test-BlazorMigrationSamples.ps1 `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' -KeepRunning
```

Open upload roots `http://127.0.0.1:6210/` and `http://127.0.0.1:6211/`.
Select the three generated logs under `artifacts/blazor-migrations/` in order,
cancelling the last after progress appears. Check the table above, allowing the
partial cancellation count to vary. Open callback roots on ports 6220 and 6221;
focus Valve control, press v once, then click it once with a mouse. Reload to reset.
The helper returns while hosts remain running with `-KeepRunning`; its `hosts.json`
lists the four owned process IDs for cleanup. Without that switch it stops its
hosts after HTTP checks. It does not automate the browser or count HTTP as interaction.

Installed guidance inspection and independent consumer reconstruction:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-2aa91b80'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 -BuildAndPack `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe'
& ./eng/Test-BlazorMigrationConsumer.ps1 `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe'
```

The Inspector helper prints ID/title/goal, full expanded Markdown, applicability
and evidence for `blazor.bound-upload-read` and `blazor.create-typed-event-callback`,
both edition/explicit target 11.0. It installs the exact ordinary package from a
unique local-only feed/cache and performs real stdio calls outside the repository.
It checks complete canonical Markdown, fresh evidence and JSON-text/structured
equality. The second helper builds all four complete consumers extracted only
from those returned bodies, checking the actual SDK and dependency closures.
Unusable proxies establish proxy-independent retrieval, not network isolation;
no new recurring OS/native/browser matrix was introduced.

A separate one-off run installed that same `0.0.0-blazor-migrations` artifact in
the existing digest-pinned .NET 10 SDK image
`mcr.microsoft.com/dotnet/sdk@sha256:e70cdb7f80b0348f5cb85f19a8f670fca061f033d57eed12fa003d58b0e06317`
with `docker run --rm --network none`. Only the local feed, expected installed
responses and disposable stdio driver were mounted, with no checkout or external
network. Both target-11 discovery entries and complete get_recipe objects matched
the Windows installed responses; text/structured equality and clean server
shutdown passed. The PR handoff retains the exact tested package hash and command;
ordinary installation/container receipts remain disposable artifacts.

## Retained limits

RC2 observations are not GA verification. Other browsers/OS, WebAssembly, Hybrid,
production deployment, circuit reconnection and malicious-client upload behavior
were not checked. There is no verified BrowserFileStreamOptions rename or
segment/buffer/timeout equivalence. Those factual gaps remain explicit in R3 and
the recipe. The unchanged `blazor.replace-removed-api` / WEB-089 aggregate remains
outstanding for its other approved families; PIN-959 and PIN-964 remain open.
