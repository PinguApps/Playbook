[CmdletBinding()]
param(
  [string]$Net11Dotnet = (Get-Command dotnet -CommandType Application).Source
)

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$scratch = Join-Path ([IO.Path]::GetTempPath()) ('playbook-fsharp-boundaries-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory $scratch | Out-Null
$previousNativePreference = $PSNativeCommandUseErrorActionPreference
$previousIntegration = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  Copy-Item (Join-Path $root 'samples/net11.0/FSharpApiPreview/global.json') $scratch
  Copy-Item (Join-Path $root 'NuGet.Config') $scratch
  $reference = Join-Path $root 'samples/net11.0/FSharpApiPreview/FSharpApiPreview.fsproj'
  $packages = Join-Path $root 'artifacts/fsharp-packages'
  @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net11.0</TargetFramework><LangVersion>preview</LangVersion><OutputType>Exe</OutputType>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors><DisableImplicitFSharpCoreReference>true</DisableImplicitFSharpCoreReference>
    <DisableImplicitLibraryPacksFolder>true</DisableImplicitLibraryPacksFolder><RestorePackagesPath>$packages</RestorePackagesPath>
  </PropertyGroup>
  <ItemGroup><PackageReference Include="FSharp.Core" Version="11.0.101-rc2.26504.105" />
    <ProjectReference Include="$reference" /><Compile Include="Probe.fs" />
  </ItemGroup>
</Project>
"@ | Set-Content (Join-Path $scratch 'Probe.fsproj')
  Push-Location $scratch
  try {
    $PSNativeCommandUseErrorActionPreference = $false
    $cases = @(
      @{ Name = 'external public SRTP extension with defining module open'; Flags = @(); Pattern = $null; Source = 'module Probe
open System
open Playbook.Samples.ApiPreview
open Playbook.Samples.ApiPreview.HarbourExtensions
if Manifest.width (Uri("https://reed.example/dock")) <> 12 then failwith "External extension failed"' },
      @{ Name = 'external extension requires preview'; Flags = @('-p:LangVersion=11.0'); Pattern = 'FS0193'; Source = 'module Probe
open System
open Playbook.Samples.ApiPreview
open Playbook.Samples.ApiPreview.HarbourExtensions
let result = Manifest.width (Uri("https://reed.example/dock"))' },
      @{ Name = 'external extension requires module scope'; Flags = @(); Pattern = 'FS0193'; Source = 'module Probe
open System
open Playbook.Samples.ApiPreview
let result = Manifest.width (Uri("https://reed.example/dock"))' },
      @{ Name = 'preview rejects positional selected API'; Flags = @(); Pattern = 'FS3923'; Source = 'module Probe
open Playbook.Samples.ApiPreview
let result = DockBooking().Reserve(12, 3)' },
      @{ Name = 'default language does not enforce named attribute'; Flags = @('-p:LangVersion=11.0'); Pattern = $null; Source = 'module Probe
open Playbook.Samples.ApiPreview
if DockBooking().Reserve(12, 3) <> 9 then failwith "Call changed"' },
      @{ Name = 'preview accepts reordered names'; Flags = @(); Pattern = $null; Source = 'module Probe
open Playbook.Samples.ApiPreview
if DockBooking().Reserve(departures = 3, arrivals = 12) <> 9 then failwith "Named call changed"' },
      @{ Name = 'computation rethrow requires preview'; Flags = @('-p:LangVersion=11.0'); Pattern = 'FS3350'; Source = 'module Probe
let operation: Async<int> = async {
    try return failwith "Dock failure"
    with _ -> return reraise ()
}' }
    )
    foreach ($operator in @('|||', '&&&', '^^^')) {
      $cases += @{ Name = "char enum $operator rejected"; Flags = @('-p:LangVersion=11.0'); Pattern = 'FS0001'; Source = "module Probe`ntype LegacyAccess = Unload = 'A' | Refuel = 'B'`nlet result = LegacyAccess.Unload $operator LegacyAccess.Refuel" }
    }
    foreach ($case in $cases) {
      $case.Source | Set-Content Probe.fs
      # Keep the referenced preview library's language mode unchanged.
      $flags = @($case.Flags)
      $language = if ($flags.Count) { '11.0' } else { 'preview' }
      $project = Get-Content Probe.fsproj -Raw
      $project = $project -replace '<LangVersion>[^<]+</LangVersion>', "<LangVersion>$language</LangVersion>"
      $project | Set-Content Probe.fsproj
      $output = & $Net11Dotnet build Probe.fsproj -c Release --nologo -t:Rebuild 2>&1 | Out-String
      if ($case.Pattern) {
        if ($LASTEXITCODE -eq 0 -or $output -notmatch $case.Pattern) { throw "$($case.Name): $output" }
        $output -split '\r?\n' | Where-Object { $_ -match 'error FS' } | Select-Object -Unique | Write-Host
      }
      else {
        if ($LASTEXITCODE -ne 0) { throw "$($case.Name): $output" }
        & $Net11Dotnet ./bin/Release/net11.0/Probe.dll
        if ($LASTEXITCODE -ne 0) { throw "$($case.Name) failed at runtime." }
      }
      Write-Host "PASS: $($case.Name)"
    }
  }
  finally { Pop-Location }
}
finally {
  $PSNativeCommandUseErrorActionPreference = $previousNativePreference
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previousIntegration
  $resolved = [IO.Path]::GetFullPath($scratch)
  if (-not $resolved.StartsWith([IO.Path]::GetFullPath([IO.Path]::GetTempPath()), [StringComparison]::OrdinalIgnoreCase)) { throw 'Unexpected scratch path.' }
  Remove-Item -LiteralPath $resolved -Recurse -Force
}
