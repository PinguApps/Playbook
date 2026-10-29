[CmdletBinding()]
param(
  [Parameter(Mandatory)][string]$Playwright,
  [Parameter(Mandatory)][string]$Chromium,
  [string]$Dotnet = (Get-Command dotnet -CommandType Application).Source,
  [string]$Node = (Get-Command node -CommandType Application).Source
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
. (Join-Path $PSScriptRoot 'Evidence.ps1')
$root = Split-Path $PSScriptRoot -Parent
$paths = @{}
foreach ($scope in 'Process','User','Machine') { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$previous = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  & (Join-Path $PSScriptRoot 'Build-SignalRefreshClient.ps1') -Node $Node
  # Actual RC2 analyzer control: the old component reads without subscribing.
  $control = Join-Path ([IO.Path]::GetTempPath()) ('playbook-live-identity-' + [guid]::NewGuid().ToString('N'))
  New-Item -ItemType Directory -Path $control | Out-Null
  try {
    Get-ChildItem -LiteralPath (Join-Path $root 'samples/net11.0/ArchiveAccess') -File |
      Where-Object Name -NotMatch 'evidence|verification' | Copy-Item -Destination $control
    Copy-Item -LiteralPath (Join-Path $root 'samples/net10.0/ArchiveAccess/AccessDesk.razor') -Destination (Join-Path $control 'AccessDesk.razor')
    Push-Location -LiteralPath $control
    try {
      $PSNativeCommandUseErrorActionPreference = $false
      $analyzer = & $Dotnet build ArchiveAccess.csproj -c Release -p:RestoreLockedMode=true 2>&1 | Out-String
      $analyzerExit = $LASTEXITCODE
      $PSNativeCommandUseErrorActionPreference = $true
      if ($analyzerExit -eq 0 -or $analyzer -notmatch 'error BL0013:') { throw "Expected actual unsubscribed BL0013: $analyzer" }
    } finally { Pop-Location }
  } finally {
    $resolved = [IO.Path]::GetFullPath($control)
    if (-not $resolved.StartsWith([IO.Path]::GetFullPath([IO.Path]::GetTempPath()), [StringComparison]::OrdinalIgnoreCase) -or (Split-Path $resolved -Leaf) -notlike 'playbook-live-identity-*') { throw 'Unsafe control cleanup target.' }
    Remove-Item -LiteralPath $resolved -Recurse -Force
  }
  foreach ($major in 10,11) {
    foreach ($name in 'CopperRidgeHub','ArchiveAccess') {
      $kind = if ($name -eq 'CopperRidgeHub') { 'compile-and-run' } else { 'compile' }
      & (Join-Path $PSScriptRoot 'Record-Evidence.ps1') -Sample "samples/net$major.0/$name/$name.csproj" -Dotnet $Dotnet -Kind $kind
    }
  }
  $reportPath = Join-Path $root 'artifacts/identity-tooling/behavior.json'
  & $Node (Join-Path $PSScriptRoot 'Test-LiveIdentity.mjs') $Playwright $Chromium $Dotnet $reportPath
  $report = Get-Content -LiteralPath $reportPath -Raw | ConvertFrom-Json
  foreach ($major in 10,11) {
    foreach ($name in 'CopperRidgeHub','ArchiveAccess') {
      $directory = Join-Path $root "samples/net$major.0/$name"
      $record = Get-Content -LiteralPath (Join-Path $directory 'verification-evidence.json') -Raw | ConvertFrom-Json -AsHashtable
      if (((Get-EvidenceInputs $root $record.sample) | ConvertTo-Json -Compress) -cne ($record.inputs | ConvertTo-Json -Compress)) { throw 'Inputs changed during execution.' }
      $case = @($report.results | Where-Object { $_.major -eq $major -and $_.sample -eq $name })
      if ($case.Count -ne 1) { throw 'Missing real execution.' }
      $record.kind = 'execution'
      $record.command = "& '$PSCommandPath' -Playwright '$Playwright' -Chromium '$Chromium' -Dotnet '$Dotnet' -Node '$Node'"
      $record.scope = if ($name -eq 'CopperRidgeHub') { 'Actual pinned first-party TypeScript source compilation and real old reconnect/current direct and redirected Kestrel WebSocket accept/reject/events. .NET behavior in compile-and-run record. Manual refresh; no Azure, expiry scheduling or production identity-provider proof.' } else { 'Real Chromium Interactive Server grant/revoke with two authentication-state notifications, reloaded user content and unchanged circuit/navigation. Old comparison explicitly refreshes its view. Local circuit-scoped provider, not cookie/JWT propagation or server-data authorization proof.' }
      $record.recordedAt = [DateTimeOffset]::UtcNow.ToString('O')
      $files = @()
      if ($name -eq 'CopperRidgeHub') {
        $files = @('generated/aspnetcore.tar.gz','generated/client/HubConnection.js','generated/client/HttpConnection.js','node_modules/typescript/package.json') | ForEach-Object { Join-Path $root "artifacts/identity-tooling/ts/$_" }
      } else {
        $pack = if ($major -eq 10) { '10.0.13' } else { '11.0.0-rc.2.26504.105' }
        $files = @(Join-Path (Split-Path $Dotnet) "packs/Microsoft.AspNetCore.App.Ref/$pack/analyzers/dotnet/cs/Microsoft.AspNetCore.Components.Analyzers.dll")
      }
      $tooling = @($files | ForEach-Object { @{ file=$_; sha256=(Get-FileHash -LiteralPath $_).Hash.ToLowerInvariant() } })
      $record.output = "PASS: Chromium $($report.browser); Node $($report.node)`n" + (@{case=$case; tooling=$tooling; analyzerControl=$(if ($name -eq 'ArchiveAccess') { $analyzer.Trim() } else { 'Not applicable' })} | ConvertTo-Json -Depth 20)
      [IO.File]::WriteAllText((Join-Path $directory 'behavior.evidence.json'), ($record | ConvertTo-Json -Depth 40).Replace("`r`n","`n") + "`n")
    }
  }
  Write-Host 'PASS: fresh compilation, actual .NET/TypeScript hubs and Interactive Server UI records; PATH unchanged.'
} finally {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previous
  foreach ($scope in $paths.Keys) { if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." } }
}
