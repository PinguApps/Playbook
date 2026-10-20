[CmdletBinding()]
param(
  [string]$Docker = (Get-Command docker -CommandType Application).Source,
  [string]$Image = 'mcr.microsoft.com/dotnet/sdk@sha256:bfabf51e74c5a7cf2fc4eac852145db86f4867d475257d809518d6a2b0fe71e1'
)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
. (Join-Path $PSScriptRoot 'Evidence.ps1')
$root = Split-Path $PSScriptRoot -Parent
$sample = 'samples/net11.0/ProcessExecution/ProcessExecution.csproj'
$before = Get-EvidenceInputs $root $sample
$results = Join-Path $root 'artifacts/process-platforms'
New-Item -ItemType Directory -Path $results -Force | Out-Null
$program = @'
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
. /source/eng/Evidence.ps1
$sample = 'samples/net11.0/ProcessExecution/ProcessExecution.csproj'
$inputs = Get-EvidenceInputs /source $sample
foreach ($relative in $inputs.Keys) {
  $destination = Join-Path /work $relative
  New-Item -ItemType Directory -Path (Split-Path $destination -Parent) -Force | Out-Null
  Copy-Item -LiteralPath (Join-Path /source $relative) -Destination $destination
}
Set-Location /work/samples/net11.0/ProcessExecution
$sdkInfo = (& /usr/share/dotnet/dotnet --info | Out-String).Trim()
$workloads = (& /usr/share/dotnet/dotnet workload list | Out-String).Trim()
$PSNativeCommandUseErrorActionPreference = $false
$output = (& /usr/share/dotnet/dotnet build ProcessExecution.csproj --configuration Release -p:RestoreLockedMode=true 2>&1 | Out-String).Trim()
if ($LASTEXITCODE -ne 0) { throw $output }
$PSNativeCommandUseErrorActionPreference = $true
$output += "`n" + (& /usr/share/dotnet/dotnet ./bin/Release/net11.0/ProcessExecution.dll | Out-String).Trim()
if ($output -notmatch 'PASS:') { throw 'Linux process execution did not pass.' }
@{ sdkInfo = $sdkInfo; workloadInfo = $workloads; output = $output; host = [Runtime.InteropServices.RuntimeInformation]::OSDescription + ' / ' + [Runtime.InteropServices.RuntimeInformation]::OSArchitecture } |
  ConvertTo-Json -Depth 5 | Set-Content /results/linux.json
Write-Host $output
'@
& $Docker run --rm --init --network none -e DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0 -e DOTNET_CLI_TELEMETRY_OPTOUT=1 `
  --mount "type=bind,source=$root,target=/source,readonly" --mount "type=bind,source=$results,target=/results" `
  $Image pwsh -NoProfile -Command $program
$actual = Get-Content (Join-Path $results 'linux.json') -Raw | ConvertFrom-Json
$after = Get-EvidenceInputs $root $sample
if ((ConvertTo-Json $before -Compress) -cne (ConvertTo-Json $after -Compress)) { throw 'Process inputs changed during Linux verification.' }
$record = [ordered]@{
  kind = 'compile-and-run'; sample = $sample; target = 'net11.0'; sdk = '11.0.100-rc.2.26504.105'; language = '15.0'
  command = "eng/Test-ProcessPlatforms.ps1 -Docker '$Docker' -Image '$Image'; container: /usr/share/dotnet/dotnet build ProcessExecution.csproj --configuration Release -p:RestoreLockedMode=true; /usr/share/dotnet/dotnet ./bin/Release/net11.0/ProcessExecution.dll"
  scope = 'Linux x64 native process execution in an isolated network-disabled container; original input files copied unchanged. macOS not executed.'
  recordedAt = [DateTimeOffset]::UtcNow.ToString('O'); sourceCommit = (& (Get-Command git -CommandType Application).Source -C $root rev-parse HEAD).Trim()
  sourceDirty = [bool](& (Get-Command git -CommandType Application).Source -C $root status --porcelain -- samples eng/Test-ProcessPlatforms.ps1)
  inputs = $after; dependencies = @(); workloads = @(); nativeToolchains = @()
  host = $actual.host; sdkInfo = $actual.sdkInfo; workloadInfo = $actual.workloadInfo; output = $actual.output
}
[IO.File]::WriteAllText((Join-Path $root 'samples/net11.0/ProcessExecution/linux.evidence.json'), (ConvertTo-Json $record -Depth 30).Replace("`r`n", "`n") + "`n")
