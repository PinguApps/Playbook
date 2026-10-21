[CmdletBinding()]
param(
  [string]$Net10Dotnet = 'dotnet',
  [string]$Net11Dotnet = 'dotnet'
)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
$previous = [Environment]::GetEnvironmentVariable('DOTNET_EnableHWIntrinsic')
try {
  $env:DOTNET_EnableHWIntrinsic = '0'
  foreach ($baseline in @(@{ Target = 'net10.0'; Dotnet = $Net10Dotnet }, @{ Target = 'net11.0'; Dotnet = $Net11Dotnet })) {
    $runtimeExecutable = (Get-Command $baseline.Dotnet -CommandType Application -ErrorAction Stop).Source
    foreach ($family in @('VectorLanes', 'SupportedInstructions')) {
      $dll = Join-Path $root "samples/$($baseline.Target)/$family/bin/Release/$($baseline.Target)/$family.dll"
      $output = (& $runtimeExecutable $dll | Out-String).Trim()
      Write-Host $output
      $expected = if ($family -eq 'VectorLanes') { 'accelerated=False' } else { 'bias=scalar, products=scalar' }
      if ($output -notmatch 'PASS' -or -not $output.Contains($expected)) { throw "Disabled-intrinsic behavior was not established: $dll" }
    }
  }
  Write-Host 'PASS: both actual target DLLs preserve lane and numeric behavior with hardware intrinsics disabled. Advanced native execution is not established.'
}
finally {
  if ($null -eq $previous) { Remove-Item -LiteralPath Env:/DOTNET_EnableHWIntrinsic -ErrorAction SilentlyContinue }
  else { $env:DOTNET_EnableHWIntrinsic = $previous }
}
