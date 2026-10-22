[CmdletBinding()]
param(
  [string]$Docker = 'C:/Program Files/Docker/Docker/resources/bin/docker.exe',
  [string]$Net10Image = 'mcr.microsoft.com/dotnet/sdk@sha256:0d99ebf1ccad15037b5c98c9fe8979d6b8dbac23812feae2277a0b277b5c5ffa',
  [string]$Net11Image = 'mcr.microsoft.com/dotnet/sdk@sha256:bfabf51e74c5a7cf2fc4eac852145db86f4867d475257d809518d6a2b0fe71e1'
)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
. (Join-Path $PSScriptRoot 'Evidence.ps1')
$root = Split-Path $PSScriptRoot -Parent
$results = Join-Path $root 'artifacts/crash-evidence'
New-Item -ItemType Directory -Path $results -Force | Out-Null
foreach ($target in @('net10.0', 'net11.0')) {
  $sample = "samples/$target/CrashEvidence/CrashEvidence.csproj"
  $before = Get-EvidenceInputs $root $sample
  $image = if ($target -eq 'net10.0') { $Net10Image } else { $Net11Image }
  $program = @'
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
. /source/eng/Evidence.ps1
$target = $env:CRASH_TARGET
$sample = "samples/$target/CrashEvidence/CrashEvidence.csproj"
$inputs = Get-EvidenceInputs /source $sample
foreach ($relative in $inputs.Keys) {
  $destination = Join-Path /work $relative
  New-Item -ItemType Directory -Path (Split-Path $destination -Parent) -Force | Out-Null
  Copy-Item -LiteralPath (Join-Path /source $relative) -Destination $destination
}
Set-Location "/work/samples/$target/CrashEvidence"
$sdk = (& /usr/share/dotnet/dotnet --version | Out-String).Trim()
$pin = Get-Content global.json -Raw | ConvertFrom-Json
if ($sdk -cne $pin.sdk.version) { throw "Unexpected SDK: $sdk" }
$sdkInfo = (& /usr/share/dotnet/dotnet --info | Out-String).Trim()
$workloads = (& /usr/share/dotnet/dotnet workload list | Out-String).Trim()
$output = (& /usr/share/dotnet/dotnet build CrashEvidence.csproj --configuration Release -p:RestoreLockedMode=true -p:NuGetAudit=false | Out-String).Trim()
$output += "`n" + (& /usr/share/dotnet/dotnet "./bin/Release/$target/CrashEvidence.dll" | Out-String).Trim()
if ($output -notmatch 'PASS: configured crash report') { throw $output }
@{ sdk = $sdk; sdkInfo = $sdkInfo; workloadInfo = $workloads; output = $output; host = [Runtime.InteropServices.RuntimeInformation]::OSDescription + ' / ' + [Runtime.InteropServices.RuntimeInformation]::OSArchitecture } |
  ConvertTo-Json -Depth 5 | Set-Content "/results/$target.json"
Write-Host $output
'@
  & $Docker run --rm --init --network none --ulimit core=0 -e DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0 -e DOTNET_GENERATE_ASPNET_CERTIFICATE=false `
    -e DOTNET_CLI_TELEMETRY_OPTOUT=1 -e "CRASH_TARGET=$target" `
    --mount "type=bind,source=$root,target=/source,readonly" --mount "type=bind,source=$results,target=/results" `
    $image /usr/bin/pwsh -NoProfile -Command $program
  $actual = Get-Content (Join-Path $results "$target.json") -Raw | ConvertFrom-Json
  $after = Get-EvidenceInputs $root $sample
  if ((ConvertTo-Json $before -Compress) -cne (ConvertTo-Json $after -Compress)) { throw 'Crash inputs changed during verification.' }
  $record = [ordered]@{
    kind = 'compile-and-run'; sample = $sample; target = $target; sdk = $actual.sdk
    language = $(if ($target -eq 'net10.0') { '14.0' } else { '15.0' })
    command = "eng/Test-CrashEvidence.ps1; image=$image; /usr/share/dotnet/dotnet build CrashEvidence.csproj --configuration Release -p:RestoreLockedMode=true -p:NuGetAudit=false; /usr/share/dotnet/dotnet ./bin/Release/$target/CrashEvidence.dll"
    scope = 'Actual configured Linux x64 CoreCLR child crash in a disposable network-disabled container; original crash frame checked in the generated report. No Windows, Apple, mobile, NativeAOT, debugger or production-crash proof.'
    recordedAt = [DateTimeOffset]::UtcNow.ToString('O'); sourceCommit = (& 'C:/Program Files/Git/cmd/git.exe' -C $root rev-parse HEAD).Trim()
    sourceDirty = [bool](& 'C:/Program Files/Git/cmd/git.exe' -C $root status --porcelain -- samples eng/Test-CrashEvidence.ps1)
    inputs = $after; dependencies = @(); workloads = @(); nativeToolchains = @()
    host = $actual.host; sdkInfo = $actual.sdkInfo; workloadInfo = $actual.workloadInfo; output = $actual.output
  }
  [IO.File]::WriteAllText((Join-Path $root "samples/$target/CrashEvidence/verification-evidence.json"), (ConvertTo-Json $record -Depth 30).Replace("`r`n", "`n") + "`n")
}
