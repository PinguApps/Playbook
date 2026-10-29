[CmdletBinding()]
param(
  [string]$Net10Dotnet = 'C:/Program Files/dotnet/dotnet.exe',
  [string]$Net11Dotnet = 'C:/Users/pingu/.dotnet-playbook-11/dotnet.exe'
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
$before = @{}
foreach ($scope in 'Process','User','Machine') { $before[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$previousIntegration = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
$cases = @(
  @{ Directory = 'samples/net10.0/QuaySeals'; Name = 'QuaySeals'; Dotnet = $Net10Dotnet; Asset = 'net8.0' },
  @{ Directory = 'samples/net11.0/QuaySeals'; Name = 'QuaySeals'; Dotnet = $Net11Dotnet; Asset = 'net10.0' },
  @{ Directory = 'samples/net10.0/QuaySeals11'; Name = 'QuaySeals11'; Dotnet = $Net10Dotnet; Asset = 'net10.0' },
  @{ Directory = 'samples/net48/QuaySeals'; Name = 'QuaySeals'; Dotnet = $Net10Dotnet; Asset = 'netstandard2.0' }
)
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  foreach ($case in $cases) {
    $sample = "$($case.Directory)/$($case.Name).csproj"
    $legacy = $case.Asset -eq 'netstandard2.0'
    if ($legacy -and -not $IsWindows) { throw 'The retained net48 execution requires Windows with .NET Framework 4.8 or later installed.' }
    & (Join-Path $PSScriptRoot 'Record-Evidence.ps1') -Sample $sample -Dotnet $case.Dotnet -Kind $(if ($legacy) { 'compile' } else { 'compile-and-run' })
    $assets = Get-Content -LiteralPath (Join-Path $root "$($case.Directory)/obj/project.assets.json") -Raw | ConvertFrom-Json -AsHashtable
    $selected = @($assets.targets.Values | ForEach-Object {
      foreach ($entry in $_.GetEnumerator()) {
        if ($entry.Key -like 'Microsoft.Data.Sqlite.Core/*') { $entry.Value.compile.Keys }
      }
    } | Sort-Object -Unique)
    if ($selected.Count -ne 1 -or $selected[0] -cne "lib/$($case.Asset)/Microsoft.Data.Sqlite.dll") {
      throw "Unexpected managed package asset for $sample : $selected"
    }
    Write-Host "Selected $sample -> $selected"
    if ($legacy) {
      # Execute the actual Framework EXE directly; dotnet exec is a different host.
      $directory = Join-Path $root $case.Directory
      $executable = Join-Path $directory 'bin/Release/net48/QuaySeals.exe'
      . (Join-Path $PSScriptRoot 'Evidence.ps1')
      $inputs = Get-EvidenceInputs $root $sample
      $output = (& $executable | Out-String).Trim()
      if ($output -notmatch '^PASS:.*CLR 4\.0\.30319\..*provider 10\.0\.13; SQLite 3\.53\.3$') { throw "Unexpected Framework consumer result: $output" }
      $after = Get-EvidenceInputs $root $sample
      if (($inputs | ConvertTo-Json -Compress) -cne ($after | ConvertTo-Json -Compress)) { throw 'Framework inputs changed during execution.' }
      $record = Get-Content -LiteralPath (Join-Path $directory 'verification-evidence.json') -Raw | ConvertFrom-Json
      $framework = Get-ItemProperty -LiteralPath 'HKLM:/SOFTWARE/Microsoft/NET Framework Setup/NDP/v4/Full'
      $record.kind = 'execution'
      $record.command = $executable
      $record.scope = 'Direct Windows x64 Framework EXE execution: parameterized write, commit, rollback, file reopen and cleanup. Managed asset selection is checked separately; no UWP/Xamarin execution.'
      $record.recordedAt = [DateTimeOffset]::UtcNow.ToString('O')
      $record.output = "$output`nInstalled .NET Framework release: $($framework.Release); version: $($framework.Version)"
      [IO.File]::WriteAllText((Join-Path $directory 'execution.evidence.json'), ($record | ConvertTo-Json -Depth 30).Replace("`r`n","`n") + "`n")
      Write-Host $record.output
    }
  }
  Write-Host 'PASS: four real SQLite consumers, exact managed assets and fresh compilation/execution records.'
}
finally {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previousIntegration
  foreach ($scope in $before.Keys) {
    if ($before[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." }
  }
}
