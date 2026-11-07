# Passkey verification access

Pingu approved the [PIN-701 disposition](https://linear.app/pinguapps/issue/PIN-701/01-025-provide-passkey-trust-signing-and-device-prerequisites)
on 7 November 2026 (owner decision recorded in the issue comments): document the native
exclusion; make no trust or signing changes. No relying-party service, account,
credential, certificate trust, domain association or signing identity is provisioned
by this decision. A separate localhost browser setup is outside this task.

## Available checks and excluded route

The canonical native adapter is
[`samples/net11.0/HeronKeys/HeronKeys.csproj`](../samples/net11.0/HeronKeys/HeronKeys.csproj).
It is a library, not a runnable MAUI client or relying-party server. Its
[`verification-evidence.json`](../samples/net11.0/HeronKeys/verification-evidence.json)
records compilation against the Windows asset of Microsoft.Maui.Essentials
**11.0.0-rc.2.26505.5**, targeting **net11.0-windows10.0.19041.0** with SDK
**11.0.100-rc.3.26522.148**. The record retains the exact source commit, dirty
state, input hashes and measured dependency closure. Compilation is reusable only
while those inputs and conditions remain valid; it does not prove device approval.

The [Windows authoring assessment](verification/native-authoring.md) records a
separate MAUI Windows host probe failing **NETSDK1147** (diagnostic names
`maui-tizen`). That assessment measured the separate RC2 SDK root. For PIN-701,
the RC3 root `C:\Users\pingu\AppData\Local\dotnetup\dotnet` was also checked:
its `packs` directory contains no `Microsoft.Maui.Sdk*` pack. There is no
usable canonical native client here for a registration/authentication ceremony.
No workload installation or new native environment is required.

For recipe `maui.authenticate-with-platform-passkeys`, edition `11.0`, retain
`humanProof.status: not-tested` and
`humanProof.untestedSamples: [samples/net11.0/HeronKeys/HeronKeys.csproj]` in the
[existing recipe evidence](../content/recipes/maui.authenticate-with-platform-passkeys/11.0/recipe.json).
The excluded behaviour is the native host, device registration/authentication
and relying-party interoperability. This exclusion is **not a publication
prerequisite** under the [native testing policy](contributing.md#native-testing-policy).
Existing compilation, input freshness, official-source review, provider/security
checks and any other pending Human gates remain required where applicable.

No native ceremony or browser ceremony was executed for PIN-701. Browser approval,
an `IsSupported` result and adapter compilation cannot verify the MAUI route.
For PIN-936, Pingu selected **“Reconcile as not-tested exclusion”** on 7 November
2026 in the implementation conversation. Native host, device registration and
authentication, and server challenge/response verification remain not-tested.
No separate browser setup is authorized and no ceremony result is available.
This disposition leaves no pending Human ceremony for the unavailable route and
does not require another environment or block publication. The existing recipe
evidence retains the exact sample exclusion and compilation/input identity;
it is not relabelled as execution evidence. Feasible local checks and any other
provider/security or Human gates remain unchanged.

## Inspect the recorded exclusion

From any directory, with PowerShell 7, SDK 10.0.403 and Node 22.19+ at the
paths below, build and install the ordinary package into an isolated feed/cache.
The script runs MCP list/get itself from an isolated session outside the worktree:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-846cd1f7'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'maui.authenticate-with-platform-passkeys' -Query 'passkeys' `
  -RecipeEdition '11.0' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin936' -BuildAndPack `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory './artifacts/passkeys/inspection'
```

Expect ID/title/goal, full Markdown and applicability for edition 11.0, current
compilation input freshness and the explicit native not-tested scope. Target
10.0 has no applicable edition. This checks delivered guidance, not a ceremony
or server verifier. Setup downloads Inspector; nothing is publicly published.

## Approval boundary if the route becomes available

The [pinned Windows implementation](https://raw.githubusercontent.com/dotnet/maui/d32b79928709cf53b671336dabd0caef5a679fc2/src/Essentials/src/Passkeys/Passkeys.windows.cs)
requires an active native window on the caller's UI thread and derives the client
origin as `https://` followed by the RP ID. A future test needs a supported host,
Windows Hello or a compatible FIDO2 key, and a separate test relying party whose
verifier accepts that exact origin and validates fresh, single-use challenges,
registration and assertions before issuing a session. Windows does not use the
Apple associated-domain or Android signing-association setup.

Before creating that setup, obtain Pingu's specific approval for the test RP
identity/endpoints, accounts, allowed origins, certificate/trust or signing changes
if needed, credential retention and cleanup. This decision grants no such access
and does not authorize modifying production trust or reusing production credentials.
Do not infer device enrollment or authenticator availability from OS support.

Pingu owns feasible physical/biometric approval and refresh coordination. The
content author first runs accessible checks, then supplies the exact recipe,
sample/input identity, commands, versions, expected result and Human action using
the [existing evidence handoff](contributing.md#ownership-and-handoff). If relevant
inputs or availability change, reassess the route and refresh affected evidence;
do not relabel this exclusion as execution proof or create a second evidence system.
