[CmdletBinding()]
param(
  [Parameter(Mandatory)][string]$Net10Dotnet,
  [Parameter(Mandatory)][string]$Net11Dotnet
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
  foreach ($major in 10,11) {
    $dotnet = if ($major -eq 10) { $Net10Dotnet } else { $Net11Dotnet }
    $sample = "samples/net$major.0/SeedExchange/SeedExchange.csproj"
    & (Join-Path $PSScriptRoot 'Record-Evidence.ps1') -Sample $sample -Dotnet $dotnet -Kind compile
    $directory = Join-Path $root "samples/net$major.0/SeedExchange"
    $recordPath = Join-Path $directory 'verification-evidence.json'
    $record = Get-Content -LiteralPath $recordPath -Raw | ConvertFrom-Json -AsHashtable
    Push-Location -LiteralPath $directory
    try {
      $modes = if ($major -eq 10) { @('retained') } else { @('custom','standard','tokens') }
      foreach ($mode in $modes) {
        $arguments = @("bin/Release/net$major.0/SeedExchange.dll", '--verify', 'true')
        if ($mode -eq 'standard') { $arguments += @('--standard','true') }
        if ($mode -eq 'tokens') { $arguments += @('--tokens','true') }
        $record.command += "`n& '$dotnet' " + ($arguments -join ' ')
        $run = (& $dotnet @arguments | Out-String).Trim()
        Write-Host $run
        $record.output += "`nMODE $mode`n$run"
      }
      $pack = if ($major -eq 10) { '10.0.13' } else { '11.0.0-rc.2.26504.105' }
      foreach ($assembly in 'Microsoft.AspNetCore.Http.Abstractions.dll','Microsoft.AspNetCore.dll') {
        $reference = Join-Path (Split-Path $dotnet) "packs/Microsoft.AspNetCore.App.Ref/$pack/ref/net$major.0/$assembly"
        $record.output += "`nREFERENCE $reference sha256=$((Get-FileHash -LiteralPath $reference).Hash.ToLowerInvariant())"
      }
      $record.output += "`nCOMPILER " + (& $dotnet (Join-Path (Split-Path $dotnet) "sdk/$($record.sdk)/Roslyn/bincore/csc.dll") -version | Out-String).Trim()
    } finally { Pop-Location }
    if (((Get-EvidenceInputs $root $sample) | ConvertTo-Json -Compress) -cne ($record.inputs | ConvertTo-Json -Compress)) { throw 'Inputs changed during execution.' }
    $record.kind = 'compile-and-run'
    $record.scope = 'Matching SDK compilation and actual disposable Kestrel/HTTP requests, DI invocation/handler counts, missing/opaque Origin, CORS policy and JSON/token boundaries. Browser evidence is separate in docs/verification/streaming-and-form-protection.md; no forged HTTP headers substitute for that browser proof.'
    $record.recordedAt = [DateTimeOffset]::UtcNow.ToString('O')
    [IO.File]::WriteAllText($recordPath, ($record | ConvertTo-Json -Depth 40).Replace("`r`n","`n") + "`n")
  }
} finally {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previous
  foreach ($scope in $paths.Keys) { if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." } }
}
Write-Host 'PASS: paired real SDK/framework consumers and HTTP controls; PATH unchanged. Browser controls must be run separately.'
