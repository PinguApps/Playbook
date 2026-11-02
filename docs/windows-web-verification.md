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
  according to `vswhere`; `NuGet.Console.dll` is present. Actual interactive PMC
  opening and command execution remain **pending Pingu**. Build Tools alone
  would not establish PMC access.

These are capability observations, not compilation/execution evidence for future
recipes. HTTP.sys HTTPS execution is **pending owner setup**. WSL trust is
**not tested**. Installed PMC files do not satisfy the interactive availability
criterion. Keep PIN-680 open until those access gates have actual observations.
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

The prepared, successfully built probe for the current handoff is at
`C:\Users\pingu\AppData\Local\Temp\pb-web-pin680-check`. The following owner
commands are prepared for review but have **not been executed**. Pingu runs setup
in an elevated PowerShell window. Inspect the source and generated `access.json`
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
$netsh = 'C:\Windows\System32\netsh.exe'
$bindings = & $netsh http show sslcert
$port = ($access.ipport -split ':')[-1]
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
PowerShell. First compare the displayed binding's certificate hash and application
ID with `certificate.txt` and `access.json`, and the URL reservation with its
recorded prefix/SID. Stop if ownership differs. For partial setup, run only the
deletions for resources actually created; do not delete somebody else's binding.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-7731f343'
$work = 'C:\Users\pingu\AppData\Local\Temp\pb-web-pin680-check'
$access = Get-Content -LiteralPath "$work/access.json" -Raw | ConvertFrom-Json
$thumbprint = (Get-Content -LiteralPath "$work/certificate.txt" -Raw).Trim()
$netsh = 'C:\Windows\System32\netsh.exe'
& $netsh http show sslcert "ipport=$($access.ipport)"
& $netsh http show urlacl "url=$($access.prefix)"
# Only after checking the ownership above:
& $netsh http delete urlacl "url=$($access.prefix)"
& $netsh http delete sslcert "ipport=$($access.ipport)"
$cert = Get-Item -LiteralPath "Cert:\LocalMachine\My\$thumbprint"
if ($cert.FriendlyName -ne "Playbook access $($access.appId)") { throw 'Certificate ownership differs.' }
Remove-Item -LiteralPath "Cert:\LocalMachine\My\$thumbprint" -DeleteKey
```

Confirm both exact reservations are absent afterward. Keep needed observations
before removing the exact scratch directory; do not recursively delete a computed
parent. No helper installs tools or changes persistent environment variables.

## WSL trust and Visual Studio Human handoff

**WSL:** use the existing Ubuntu distro for read-only inspection and isolated
sample files. Before an actual trust check, its author must supply a pinned SDK
installation path, exact canonical sample/input hashes, certificate identity,
Windows and Linux store changes, expected observations and precise cleanup.
Use absolute executables and `DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0` for any isolated
CLI home. Pingu explicitly approves and performs the proposed trust operation.
Do not run `dotnet dev-certs https --clean`, trust a general-purpose test root,
replace existing development certificates or change shell profiles merely to
demonstrate access. Docker's Linux trust store cannot establish WSL-to-Windows
propagation. The missing SDK and actual cross-store observation remain gates;
do not relabel them as successful access or an unavailable-hardware exclusion.

**PMC availability:** in the installed Visual Studio, Pingu opens **Tools → NuGet
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
