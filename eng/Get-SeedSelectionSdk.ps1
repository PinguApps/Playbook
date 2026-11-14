[CmdletBinding()]
param([string]$Destination = (Join-Path $PSScriptRoot '../artifacts/seed-selection-sdk'))
& (Join-Path $PSScriptRoot '../samples/net11.0/SeedBankMap/Get-SelectionSdk.ps1') @PSBoundParameters
