# snippet:complete
[CmdletBinding()]
param([string]$Destination = (Join-Path $PSScriptRoot '../../../artifacts/seed-selection-sdk'))
$ErrorActionPreference = 'Stop'
$version = '12.0.100-alpha.1.26509.101'
$destinationPath = [IO.Path]::GetFullPath($Destination)
$dotnet = Join-Path $destinationPath 'dotnet.exe'
if (-not (Test-Path $dotnet)) {
  $null = New-Item -ItemType Directory -Path $destinationPath -Force
  $archive = Join-Path $destinationPath 'sdk.zip'
  Invoke-WebRequest "https://ci.dot.net/public/Sdk/$version/dotnet-sdk-$version-win-x64.zip" -OutFile $archive
  # Exact bytes acquired from the publisher and reviewed for this conditional route.
  $expected = 'DDAA58A799737D1EF648A8665476E5A8A0E5DCD713D7C3F1A78C1B2F59E9000D9B4381CAF1436B8A490CA58403D23854BC7EC154024832B2E6F5C007A8FF1C51'
  if ((Get-FileHash $archive -Algorithm SHA512).Hash -cne $expected) { throw 'Reviewed SDK archive bytes changed.' }
  Expand-Archive -LiteralPath $archive -DestinationPath $destinationPath
}
if (-not (Test-Path "$destinationPath/sdk/$version/.version")) { throw 'The requested SDK is absent.' }
Write-Output $dotnet
# endsnippet:complete
