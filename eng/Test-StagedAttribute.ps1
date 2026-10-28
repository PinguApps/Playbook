[CmdletBinding()]
param(
  [string]$Net10Dotnet = 'C:\Program Files\dotnet\dotnet.exe',
  [string]$Net11Dotnet = 'C:\Users\pingu\.dotnet-playbook-11\dotnet.exe'
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $false
$root = Split-Path $PSScriptRoot -Parent
$scratch = Join-Path ([IO.Path]::GetTempPath()) ('playbook-staged-attribute-' + [guid]::NewGuid().ToString('N'))
$before = @{}
foreach ($scope in 'Process','User','Machine') { $before[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$previous = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
function Reject-Build([int]$Major, [string]$Case, [string]$Expected, [scriptblock]$Edit) {
  $directory = Join-Path $scratch $Case
  New-Item -ItemType Directory -Path $directory -Force | Out-Null
  Get-ChildItem -LiteralPath (Join-Path $root "samples/net$Major.0/MossGallery") -File | Copy-Item -Destination $directory
  & $Edit $directory
  Push-Location -LiteralPath $directory
  try {
    $dotnet = if ($Major -eq 10) { $Net10Dotnet } else { $Net11Dotnet }
    $output = & $dotnet build -c Release -p:RestoreLockedMode=true 2>&1 | Out-String
    if ($LASTEXITCODE -eq 0 -or $output -notmatch $Expected) { throw "Missing $Expected rejection: $Case`n$output" }
    Write-Host "PASS: $Case rejected with $Expected"
  } finally { Pop-Location }
}
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  Reject-Build 10 'unavailable-on-10' 'CS1061.*SetAttributeValue' {
    param($directory)
    [IO.File]::WriteAllText((Join-Path $directory 'Unavailable.cs'), 'public static class Unavailable { public static void Patch(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder b) => b.SetAttributeValue(0, null); }')
  }
  Reject-Build 11 'experimental-opt-in' 'ASP0032' {
    param($directory)
    $file = Join-Path $directory 'GalleryCard.cs'
    [IO.File]::WriteAllText($file, [IO.File]::ReadAllText($file).Replace('#pragma warning disable ASP0032', '// opt-in removed: ASP0032'))
  }
  Reject-Build 11 'framework-boundary' 'BL0006' {
    param($directory)
    $file = Join-Path $directory 'GalleryRenderer.cs'
    [IO.File]::WriteAllText($file, [IO.File]::ReadAllText($file).Replace('#pragma warning disable BL0006', '// opt-in removed: BL0006'))
  }
} finally {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previous
  foreach ($scope in $before.Keys) { if ($before[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." } }
}
