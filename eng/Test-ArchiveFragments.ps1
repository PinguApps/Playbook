[CmdletBinding()]
param(
  [string]$Net10Dotnet = 'C:\Program Files\dotnet\dotnet.exe',
  [string]$Net11Dotnet = 'C:\Users\pingu\.dotnet-playbook-11\dotnet.exe'
)
$ErrorActionPreference = 'Stop'
# Expected negative compiler/runtime exits are checked explicitly below.
$PSNativeCommandUseErrorActionPreference = $false
$root = Split-Path $PSScriptRoot -Parent
$previous = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
$paths = @{}
foreach ($scope in 'Process','User','Machine') { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$scratch = Join-Path ([IO.Path]::GetTempPath()) ('playbook-archive-fragments-' + [guid]::NewGuid().ToString('N'))
function Invoke-Build([string]$Directory, [string]$Dotnet, [string]$Expected = '') {
  Push-Location -LiteralPath $Directory
  try {
    $output = & $Dotnet build -c Release -p:RestoreLockedMode=true 2>&1 | Out-String
    $code = $LASTEXITCODE
    if ($Expected) {
      if ($code -eq 0 -or $output -notmatch $Expected) { throw "Missing expected compiler rejection $Expected`n$output" }
      Write-Host "PASS: rejected $Expected"
    } elseif ($code -ne 0) { throw $output }
  } finally { Pop-Location }
}
function Copy-Probe([string]$Name, [int]$Major, [string]$Case) {
  $directory = Join-Path $scratch $Case
  New-Item -ItemType Directory -Path $directory -Force | Out-Null
  Get-ChildItem -LiteralPath (Join-Path $root "samples/net$Major.0/$Name") -File | Copy-Item -Destination $directory
  $directory
}
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  foreach ($major in 10,11) {
    $dotnet = if ($major -eq 10) { $Net10Dotnet } else { $Net11Dotnet }
    foreach ($name in 'ReedArchive','AsterObservatory') {
      $directory = Join-Path $root "samples/net$major.0/$name"
      Invoke-Build $directory $dotnet
      & $dotnet (Join-Path $directory "bin/Release/net$major.0/$name.dll")
      if ($LASTEXITCODE -ne 0) { throw "$name runtime check failed." }
    }
  }
  $reverse = @'
using Microsoft.AspNetCore.Components;
using AsterObservatory;
public static class UnsafeAssignment
{
    public static RenderFragment<Observation?> Reverse(RenderFragment<SpecialObservation?> narrow) => narrow;
}
'@
  foreach ($major in 10,11) {
    $dotnet = if ($major -eq 10) { $Net10Dotnet } else { $Net11Dotnet }
    $directory = Copy-Probe 'AsterObservatory' $major "reverse-$major"
    [IO.File]::WriteAllText((Join-Path $directory 'UnsafeAssignment.cs'), $reverse)
    Invoke-Build $directory $dotnet 'CS0266|CS0029'
  }
  $directory = Copy-Probe 'AsterObservatory' 10 'direct-on-10'
  $path = Join-Path $directory 'ObservationView.razor'
  [IO.File]::WriteAllText($path, (Get-Content -LiteralPath $path -Raw).Replace('item => BaseTemplate(item)', 'BaseTemplate'))
  Invoke-Build $directory $Net10Dotnet 'CS0029'

  $directory = Copy-Probe 'ReedArchive' 10 'supplier-on-10'
  [IO.File]::WriteAllText((Join-Path $directory 'Unavailable.cs'), @'
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
public static class Unavailable
{
    public static void Register(IServiceCollection services) => services.TryAddCascadingValueSupplier<CascadingParameterAttribute>(_ => (_, _, _) => throw new NotSupportedException());
}
'@)
  Invoke-Build $directory $Net10Dotnet 'CS1061.*TryAddCascadingValueSupplier'

  # ComponentState access in the real sample emits no BL0006 on this RC2 SDK.
  # A deliberate RenderTree control proves its analyzer is running.
  $directory = Copy-Probe 'ReedArchive' 11 'bl0006-control'
  [IO.File]::WriteAllText((Join-Path $directory 'InternalRenderer.cs'), @'
public static class InternalRenderer
{
    public static object Expose(Microsoft.AspNetCore.Components.RenderTree.Renderer renderer) => renderer;
}
'@)
  Invoke-Build $directory $Net11Dotnet 'BL0006'
  foreach ($case in 'unknown-name','wrong-property','missing-receiver') {
    $directory = Copy-Probe 'ReedArchive' 11 $case
    $path = Join-Path $directory 'ArchivePanel.razor'
    $source = Get-Content -LiteralPath $path -Raw
    $expected = switch ($case) {
      'unknown-name' { $source = $source.Replace('[FromArchive("maps")]', '[FromArchive("unknown")]'); 'Unknown archive collection' }
      'wrong-property' { $source = $source.Replace('@code {', '@code { [FromArchive("maps")] public string Invalid { get; set; } = "";'); 'FromArchive requires CollectionContext' }
      'missing-receiver' { $source = $source.Replace('@implements IArchiveSubscriber', ''); 'Archive recipient must implement IArchiveSubscriber' }
    }
    [IO.File]::WriteAllText($path, $source)
    Invoke-Build $directory $Net11Dotnet
    $output = & $Net11Dotnet (Join-Path $directory 'bin/Release/net11.0/ReedArchive.dll') 2>&1 | Out-String
    if ($LASTEXITCODE -eq 0 -or $output -notmatch $expected) { throw "Missing expected runtime rejection: $case`n$output" }
    Write-Host "PASS: real renderer rejected $case"
  }
  Write-Host 'PASS: real renderer behavior, old-target availability, variance rejection and BL0006 analyzer control.'
  Write-Host "Disposable failure consumers: $scratch"
} finally {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previous
  foreach ($scope in $paths.Keys) {
    if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." }
  }
}
