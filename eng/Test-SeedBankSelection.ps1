[CmdletBinding()]
param(
  [string]$Net11Dotnet = 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe',
  [string]$Sdk12Dotnet = (Join-Path $PSScriptRoot '../artifacts/seed-selection-sdk/dotnet.exe'),
  [switch]$Record
)
& (Join-Path $PSScriptRoot '../samples/net11.0/SeedBankMap/Test-Selection.ps1') @PSBoundParameters
