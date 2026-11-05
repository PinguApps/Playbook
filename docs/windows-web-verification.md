# Controlled Windows web and design-time access

PIN-680 uses Pingu's existing Windows host, Ubuntu WSL and Docker Desktop. Pingu
approved this route on 2 November 2026; no VM or new native CI is required.
Isolation here means disposable projects, dedicated loopback resources and
explicit cleanup. It is not a separate Windows security boundary.

## Measured access and remaining gates

On 2 November 2026:

- Windows build 26200; the agent session is **not elevated**. Existing HTTP.sys
  bindings belong to other work and must be preserved.
- `eng/Inspect-WindowsWebAccess.ps1` generated an independent project outside the
  repository and built it with SDK **10.0.402**, `net10.0`, C# 14, no external
  packages and zero warnings/errors. Its private CLI home disables global-tool
  PATH integration and automatic development-certificate creation. Process,
  user and machine PATH comparisons passed. No certificate or binding was created.
- WSL **2.6.3**, kernel **6.6.87.2-1**: `Ubuntu` is a working WSL2 distro,
  Ubuntu **24.04.4**, user `pingu` (non-root, sudo group). `dotnet` is absent from
  its current PATH; `update-ca-certificates` exists. Group membership does not
  establish unattended sudo access. Do not use Docker's internal distro for this.
- Docker server **29.6.1** ran the repository's existing digest-pinned SDK image
  with `--network none`: Ubuntu **26.04.1**, SDK **11.0.100-rc.2.26504.105**.
  No source mounts or published ports were needed. This proves executable Linux
  access, not Windows/WSL certificate propagation or HTTP.sys behaviour.
- Visual Studio Community 2026 **18.11.12306.390** is installed and launchable
  according to `vswhere`; `NuGet.Console.dll` is present. Pingu opened the real
  PMC and returned `$PSVersionTable.PSVersion` = **5.1.26100.9711** and
  `Get-Host` = **Package Manager Host 7.11.0.24**, with NuGet's
  `NuGetConsole.Host.PowerShell.Implementation.NuGetPSHost+Commander` private data.
  This confirms interactive PMC access, not EF command behaviour. Build Tools
  alone would not establish PMC access.
- The same digest-pinned Linux SDK was extracted from Docker into the dedicated
  Ubuntu user cache `/home/pingu/.cache/playbook-access-pin680/sdk`. From outside
  the repository, its absolute `dotnet` executable returned SDK
  **11.0.100-rc.2.26504.105** and successfully ran `dev-certs https --help`.
  No certificate command with trust, import, clean or generation was executed.
  The extracted installation changes neither PATH nor shell profiles. A first
  attempt from the inherited repository directory correctly rejected its .NET 10
  `global.json`; run from the isolated SDK directory instead. `/tmp` did not
  survive between WSL invocations here, so use the dedicated cache directory.

After Pingu performed the dedicated setup, the normal non-elevated agent session
ran the prepared probe on 2 November 2026: HTTP.sys returned **200 OK**, the
expected response body matched, and the exact certificate pin passed on
**.NET 10.0.13**. The server shut down and a separate TCP listener inspection
found zero listeners on port 54483 afterward. Pingu then performed cleanup; the
agent independently confirmed that the exact SSL binding, URL reservation and
`LocalMachine\My` certificate were absent, with zero listeners. Shutdown alone
does not remove those persistent resources; owner cleanup is required each run.

These observations establish the Windows/WSL command route, HTTP.sys HTTPS access
and actual interactive PMC availability required by PIN-680. They are not
compilation/execution evidence for future recipes. WSL trust is **not tested**;
the actual trust operation remains separate Human proof for the later recipe.
Future recipe authors still own canonical sample checks, including authentication,
channel binding, native request properties and EF NoBuild/stale-build behaviour.

A negative execution check before owner setup started and stopped the loopback
HTTP.sys listener, but the HTTPS request failed during TLS negotiation as expected
without its certificate binding. It produced no `PASS` result. This establishes
neither successful HTTPS nor trusted connectivity; no persistent reservation,
certificate or TLS binding was written by that check.

## Prepare and execute the HTTP.sys access probe

This helper only builds or runs a bounded HTTPS request. It cannot elevate or
change certificate stores, trust, URL reservations, TLS bindings or firewall rules.
It uses a unique URL path, anonymous authentication and a 20-second application
deadline; the helper kills its own child process tree after 120 seconds. HTTPS
uses an exact certificate thumbprint callback local to the test client, with no
proxy. This does **not** prove OS/browser trust or certificate-name validation.

Preparation was checked from `C:\`, outside the worktree. In PowerShell 7.4+:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-7731f343'
$access = & ./eng/Inspect-WindowsWebAccess.ps1
$access # Expect build=passed, https=pending-owner-setup, trust=not-tested.
$access.work # Save this exact scratch path for setup, execution and cleanup.
```

Prepare probes IPv4 loopback and IPv6 when supported before creating the fixture. A port
can be reserved by Windows even when no listener is visible: during the RC3 refresh,
54957 and 54958 were in the excluded range 54881–54980 and HTTP.sys returned error32.
Port55457 passed the bind checks and the paired real HTTPS requests; the owned
certificate, binding and URL reservation were removed afterwards. Choose another
available `-Port` when needed; do not remove Windows reservations.

`-Port` selects the port during Prepare only; Run uses the saved prefix. If you
prepare another port, change the owner-selected `$port` literal in both owner
blocks below to that same value. Use the prepared directory for `-WorkDirectory`.

The prepared, successfully built probe for the current handoff is at
`C:\Users\pingu\AppData\Local\Temp\pb-web-pin680-check`. The following owner
setup and cleanup commands were executed by Pingu for the observation above.
Use a newly prepared scratch directory for another run; the retained handoff
directory still contains its historical certificate thumbprint marker. Pingu runs setup
in an elevated **PowerShell 7.4+** window, using the same Windows account as
Prepare. Inspect the source and generated `access.json`
before execution. It creates one short-lived self-signed certificate in
`LocalMachine\My` (not a trusted root), one loopback-only binding and one URL
reservation for the original user's SID. No existing resource may be reused.
An error after certificate creation can leave partial setup; retain the scratch
directory and use the exact-resource cleanup below before retrying.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-7731f343'
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$work = 'C:\Users\pingu\AppData\Local\Temp\pb-web-pin680-check'
$access = Get-Content -LiteralPath "$work/access.json" -Raw | ConvertFrom-Json
$port = 54483 # Owner-selected port, never taken from the scratch manifest.
$sid = [Security.Principal.WindowsIdentity]::GetCurrent().User.Value
$appId = [guid]::Empty
if ($access.ipport -cne "127.0.0.1:$port" -or
    $access.prefix -cnotmatch "^https://127\.0\.0\.1:$port/playbook-[0-9a-f]{32}/$" -or
    $access.sid -cne $sid -or
    -not [guid]::TryParseExact([string]$access.appId, 'B', [ref]$appId)) {
    throw 'Manifest is outside the approved loopback/account scope. Stop.'
}
$netsh = 'C:\Windows\System32\netsh.exe'
$bindings = & $netsh http show sslcert
if ($bindings | Select-String -SimpleMatch ":$port") {
    throw 'Port appears in an existing binding. Stop; prepare a different port.'
}
if (Test-Path -LiteralPath "$work/certificate.txt") { throw 'Setup already started; inspect and clean up first.' }
$cert = New-SelfSignedCertificate -DnsName localhost -CertStoreLocation 'Cert:\LocalMachine\My' `
    -FriendlyName "Playbook access $($access.appId)" -NotAfter (Get-Date).AddDays(2)
$cert.Thumbprint | Set-Content -LiteralPath "$work/certificate.txt"
& $netsh http add sslcert "ipport=$($access.ipport)" "certhash=$($cert.Thumbprint)" `
    "appid=$($access.appId)" certstorename=MY
& $netsh http add urlacl "url=$($access.prefix)" "sddl=D:(A;;GX;;;$($access.sid))"
& $netsh http show sslcert "ipport=$($access.ipport)"
& $netsh http show urlacl "url=$($access.prefix)"
```

After setup, the agent can run this from the original **non-elevated** session:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-7731f343'
& ./eng/Inspect-WindowsWebAccess.ps1 -Action Run `
    -WorkDirectory 'C:\Users\pingu\AppData\Local\Temp\pb-web-pin680-check'
```

Expect `PASS: HTTP.sys HTTPS loopback with pinned certificate` and the actual
runtime version. The helper stops the server and saves `https.log` in the scratch
directory. Return failure output without broadening permissions or disabling TLS.
The .NET 10 access probe does not establish the .NET 11 recipe APIs or GA proof.

Pingu then inspects and removes only the resources from this run in elevated
PowerShell 7.4+, using the same Windows account. First compare the displayed binding's certificate hash and application
ID with `certificate.txt` and `access.json`, and the URL reservation with its
recorded prefix/SID. Stop if ownership differs. For partial setup, run only the
deletions for resources actually created; do not delete somebody else's binding.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-7731f343'
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$work = 'C:\Users\pingu\AppData\Local\Temp\pb-web-pin680-check'
$access = Get-Content -LiteralPath "$work/access.json" -Raw | ConvertFrom-Json
$port = 54483
$sid = [Security.Principal.WindowsIdentity]::GetCurrent().User.Value
$appId = [guid]::Empty
if ($access.ipport -cne "127.0.0.1:$port" -or
    $access.prefix -cnotmatch "^https://127\.0\.0\.1:$port/playbook-[0-9a-f]{32}/$" -or
    $access.sid -cne $sid -or
    -not [guid]::TryParseExact([string]$access.appId, 'B', [ref]$appId)) {
    throw 'Manifest is outside the approved loopback/account scope. Stop.'
}
$thumbprint = (Get-Content -LiteralPath "$work/certificate.txt" -Raw).Trim()
if ($thumbprint -notmatch '^[A-Fa-f0-9]{40}$') { throw 'Invalid certificate thumbprint.' }
$netsh = 'C:\Windows\System32\netsh.exe'
$binding = (& $netsh http show sslcert "ipport=$($access.ipport)") -join "`n"
$reservation = (& $netsh http show urlacl "url=$($access.prefix)") -join "`n"
if ($binding -notmatch [regex]::Escape($access.appId) -or
    $binding -notmatch [regex]::Escape($thumbprint) -or
    $reservation -notmatch [regex]::Escape("D:(A;;GX;;;$sid)")) {
    throw 'Binding or URL reservation ownership differs. Stop.'
}
$cert = Get-Item -LiteralPath "Cert:\LocalMachine\My\$thumbprint"
if ($cert.FriendlyName -ne "Playbook access $($access.appId)") { throw 'Certificate ownership differs.' }
& $netsh http delete urlacl "url=$($access.prefix)"
& $netsh http delete sslcert "ipport=$($access.ipport)"
Remove-Item -LiteralPath "Cert:\LocalMachine\My\$thumbprint" -DeleteKey
```

Confirm both exact reservations are absent afterward. Keep needed observations
before removing the exact scratch directory; do not recursively delete a computed
parent. No helper installs tools or changes persistent environment variables.

## Reproduce the WSL command route

The following preparation uses the already approved, repository-pinned SDK image;
it needs no network inside the container, host certificate mounts or sudo.
It leaves a disposable SDK archive and an isolated Ubuntu SDK directory for
inspection. Use PowerShell 7.4+ and the existing Ubuntu distro. The exact image
digest remains author-maintained; it is not permission for automatic SDK updates.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-7731f343'
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$docker = 'C:\Program Files\Docker\Docker\resources\bin\docker.exe'
$wsl = 'C:\Windows\System32\wsl.exe'
$image = 'mcr.microsoft.com/dotnet/sdk@sha256:bfabf51e74c5a7cf2fc4eac852145db86f4867d475257d809518d6a2b0fe71e1'
$tag = [guid]::NewGuid().ToString('N')
$work = Join-Path ([IO.Path]::GetTempPath()) "pb-wsl-$tag"
New-Item -ItemType Directory -Path $work | Out-Null
$id = & $docker create --network none $image /bin/tar -C /usr/share/dotnet -czf /tmp/sdk.tgz .
try {
    & $docker start --attach $id
    & $docker cp "${id}:/tmp/sdk.tgz" "$work/sdk.tgz"
}
finally { & $docker rm $id }
$archive = & $wsl --distribution Ubuntu --exec /usr/bin/wslpath -u "$work/sdk.tgz"
$linuxRoot = "/home/pingu/.cache/playbook-access-$tag"
& $wsl --distribution Ubuntu --exec /bin/sh -c 'set -eu; mkdir "$1"; /bin/tar -xzf "$2" -C "$1"; cd "$1"; /usr/bin/env DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0 DOTNET_GENERATE_ASPNET_CERTIFICATE=false DOTNET_CLI_TELEMETRY_OPTOUT=1 DOTNET_CLI_HOME="$1/home" DOTNET_ROOT="$1" DOTNET_ROOT_X64="$1" "$1/dotnet" --version; /usr/bin/env DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0 DOTNET_GENERATE_ASPNET_CERTIFICATE=false DOTNET_CLI_TELEMETRY_OPTOUT=1 DOTNET_CLI_HOME="$1/home" DOTNET_ROOT="$1" DOTNET_ROOT_X64="$1" "$1/dotnet" dev-certs https --help' sh $linuxRoot $archive
"Windows scratch: $work"
"Ubuntu SDK: $linuxRoot"
```

Expect the pinned SDK version followed by the development-certificate command's
help, including `--trust`, without actually running trust. Stop on any nonzero
exit code. The .NET 11 RC2 command route does not establish a .NET 10 comparison
or GA behaviour. Retain or remove only the exact printed scratch/SDK directories
after inspecting their resolved paths; never delete the parent cache or home.

## WSL trust and Visual Studio Human handoff

**WSL:** use the existing Ubuntu distro for read-only inspection and isolated
sample files. Before an actual trust check, its author must supply a pinned SDK
installation path, exact canonical sample/input hashes, certificate identity,
Windows and Linux store changes, expected observations and precise cleanup.
Use absolute executables and `DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0` for any isolated
CLI home. The command route above establishes SDK access, not trust execution.
Pingu explicitly approves and performs the proposed trust operation.
Do not run `dotnet dev-certs https --clean`, trust a general-purpose test root,
replace existing development certificates or change shell profiles merely to
demonstrate access. Docker's Linux trust store cannot establish WSL-to-Windows
propagation. The actual cross-store observation remains a later recipe proof gate;
do not relabel command help as trusted connectivity or a hardware exclusion.

**PMC availability (observed above):** in the installed Visual Studio, Pingu opens **Tools → NuGet
Package Manager → Package Manager Console** and runs `$PSVersionTable.PSVersion`
and `Get-Host`. Return the displayed host/version and any initialization failure.
This is the minimum interactive access observation; it does not run EF commands
or prove NoBuild semantics. The later recipe author supplies an isolated solution,
exact EF Tools/Design packages, current/stale build inputs and expected command
results before the separate interactive recipe proof. Never substitute `dotnet ef`
or an ordinary PowerShell process for actual Visual Studio PMC.

Pingu owns these feasible Human approvals and refresh coordination under the
[native testing policy](contributing.md#native-testing-policy). Refresh access
observations when OS/tool versions, account permissions or the route change.
Retain canonical recipe evidence using the existing format; routine logs belong
in ignored artifacts or scratch, not new per-ticket evidence reports.

Sources: [HTTP.sys setup and Windows-only support](https://learn.microsoft.com/aspnet/core/fundamentals/servers/httpsys),
[development certificates and WSL host trust](https://learn.microsoft.com/dotnet/core/additional-tools/self-signed-certificates-guide),
[Docker's WSL backend](https://docs.docker.com/desktop/features/wsl/),
[Visual Studio PMC](https://learn.microsoft.com/nuget/consume-packages/install-use-packages-powershell).
