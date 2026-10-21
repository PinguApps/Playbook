[CmdletBinding()]
param(
  [string]$Net11Dotnet = (Get-Command dotnet -CommandType Application).Source
)

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$scratch = Join-Path ([IO.Path]::GetTempPath()) ('playbook-json-contracts-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $scratch | Out-Null
$previousIntegration = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
$previousNativePreference = $PSNativeCommandUseErrorActionPreference
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  Copy-Item -LiteralPath (Join-Path $root 'samples/net11.0/JsonModels/global.json') -Destination $scratch
  Copy-Item -LiteralPath (Join-Path $root 'NuGet.Config') -Destination $scratch
  @'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net11.0</TargetFramework><LangVersion>15.0</LangVersion>
    <Nullable>enable</Nullable><TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  </PropertyGroup>
</Project>
'@ | Set-Content -LiteralPath (Join-Path $scratch 'Probe.csproj')
  $cases = @(
    @{ Name = 'unresolved open generic registration'; Diagnostic = 'SYSLIB1229'; Source = @'
using System.Text.Json.Serialization;
[JsonDerivedType(typeof(Unbound<,>), "unbound")]
public class Parcel<T> { }
public class Unbound<T, U> : Parcel<T> { }
[JsonSerializable(typeof(Parcel<int>))]
public partial class ProbeContext : JsonSerializerContext { }
'@ },
    @{ Name = 'invalid type-level Always ignore'; Diagnostic = 'SYSLIB1226'; Source = @'
using System.Text.Json.Serialization;
[JsonIgnore(Condition = JsonIgnoreCondition.Always)]
public class Parcel { public int Count { get; set; } }
[JsonSerializable(typeof(Parcel))]
public partial class ProbeContext : JsonSerializerContext { }
'@ }
  )
  Push-Location $scratch
  try {
    $PSNativeCommandUseErrorActionPreference = $false
    foreach ($case in $cases) {
      $case.Source | Set-Content -LiteralPath 'Probe.cs'
      $output = (& $Net11Dotnet build Probe.csproj --configuration Release --no-incremental 2>&1 | Out-String)
      if ($LASTEXITCODE -eq 0 -or $output -notmatch $case.Diagnostic) {
        throw "Expected $($case.Diagnostic) for $($case.Name): $output"
      }
      Write-Host "PASS: $($case.Name) rejected with $($case.Diagnostic)."
    }
  }
  finally { Pop-Location }
}
finally {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previousIntegration
  $PSNativeCommandUseErrorActionPreference = $previousNativePreference
  $resolved = [IO.Path]::GetFullPath($scratch)
  $tempRoot = [IO.Path]::GetFullPath([IO.Path]::GetTempPath())
  if (-not $resolved.StartsWith($tempRoot, [StringComparison]::OrdinalIgnoreCase) -or
      -not ([IO.Path]::GetFileName($resolved)).StartsWith('playbook-json-contracts-', [StringComparison]::Ordinal)) {
    throw 'Refusing cleanup outside the dedicated temporary probe directory.'
  }
  Remove-Item -LiteralPath $resolved -Recurse -Force
}
