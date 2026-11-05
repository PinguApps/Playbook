#Requires -Version 7.4
[CmdletBinding()]
param(
  [switch]$Record,
  [string]$Net10Dotnet = (Get-Command dotnet -CommandType Application | Select-Object -First 1).Source,
  [string]$Net11Dotnet = (Get-Command dotnet -CommandType Application | Select-Object -First 1).Source
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
$before = @{}
foreach ($scope in 'Process','User','Machine') { $before[$scope] = [Environment]::GetEnvironmentVariable('PATH', $scope) }
$previous = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  foreach ($version in 10,11) {
    $dotnet = if ($version -eq 10) { $Net10Dotnet } else { $Net11Dotnet }
    foreach ($name in 'QuayTransfer','BrackenSettings') {
      $sample = "samples/net$version.0/$name/$name.csproj"
      if ($Record) { & "$PSScriptRoot/Record-Evidence.ps1" -Sample $sample -Dotnet $dotnet }
      else {
        Push-Location (Join-Path $root "samples/net$version.0/$name")
        try { & $dotnet build "$name.csproj" -c Release -p:RestoreLockedMode=true }
        finally { Pop-Location }
      }
      $dll = Join-Path $root "samples/net$version.0/$name/bin/Release/net$version.0-windows/$name.dll"
      $output = (& $dotnet $dll | Out-String).Trim()
      if ($name -eq 'QuayTransfer' -and $output -cne 'EMPTY') { throw "Unexpected default result: $output" }
      if ($name -eq 'BrackenSettings' -and $output -notmatch 'PASS settings screen lifecycle') { throw "Missing lifecycle receipt: $output" }
      Write-Host $output
      if ($version -eq 11 -and $name -eq 'QuayTransfer') {
        $errors = (& $dotnet $dll --errors | Out-String).Trim()
        if ($errors -cne 'ERROR 80004005') { throw "Missing native error propagation: $errors" }
        Write-Host $errors
        if ($Record) {
          $path = Join-Path $root 'samples/net11.0/QuayTransfer/verification-evidence.json'
          $receipt = Get-Content $path -Raw | ConvertFrom-Json -AsHashtable
          $receipt.command += "; $dotnet $dll --errors"
          $receipt.output += "`n$errors"
          $receipt.scope = 'Controlled failing native COM IDataObject through public WPF DataObject, default and opt-in in separate processes. No shared clipboard acquisition or external-owner behavior claimed.'
          [IO.File]::WriteAllText($path, ($receipt | ConvertTo-Json -Depth 30).Replace("`r`n", "`n") + "`n")
        }
      }
    }
  }
}
finally {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previous
  foreach ($scope in 'Process','User','Machine') {
    if ([Environment]::GetEnvironmentVariable('PATH', $scope) -cne $before[$scope]) { throw "PATH changed at $scope scope." }
  }
}
