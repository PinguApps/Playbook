[CmdletBinding()]
param([string]$Net11Dotnet = 'dotnet')

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$scratch = Join-Path ([IO.Path]::GetTempPath()) ('playbook-fsharp-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory $scratch | Out-Null
$previousNativePreference = $PSNativeCommandUseErrorActionPreference
try {
  Copy-Item (Join-Path $root 'samples/net11.0/FSharpDiagnostics/global.json') $scratch
  @'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup><TargetFramework>net11.0</TargetFramework><LangVersion>11.0</LangVersion><OutputType>Exe</OutputType><TreatWarningsAsErrors>true</TreatWarningsAsErrors></PropertyGroup>
  <ItemGroup><Compile Include="Probe.fs" /></ItemGroup>
</Project>
'@ | Set-Content (Join-Path $scratch 'Probe.fsproj')
  Push-Location $scratch
  try {
    $PSNativeCommandUseErrorActionPreference = $false
    $cases = @(
      @{ Name = 'stable named and anonymous spreads'; Flags = @(); Pattern = $null; Source = 'module Probe
type Load = { Mass: decimal }
type Transfer = { ...Load; Dock: int }
let original = { Mass = 4.25M }
let transfer: Transfer = { ...original; Dock = 6 }
let copy = {| ...transfer; Dock = 8 |}
if transfer.Mass <> 4.25M || copy.Dock <> 8 || transfer.Dock <> 6 then failwith "Spread changed"' },
      @{ Name = 'record constructor needs preview'; Flags = @(); Pattern = 'FS[0-9]+'; Source = 'module Probe
type Load = { Mass: decimal }
let value = Load(4.25M)' },
      @{ Name = 'record constructor with preview'; Flags = @('-p:LangVersion=preview'); Pattern = $null; Source = 'module Probe
type Load = { Mass: decimal }
let value = Load(Mass = 4.25M)
if value.Mass <> 4.25M then failwith "Constructor changed"' },
      @{ Name = 'ordinary culture formatting'; Flags = @(); Pattern = $null; Source = 'module Probe
open System.Globalization
let mass = 4.25M
CultureInfo.CurrentCulture <- CultureInfo.GetCultureInfo("de-DE")
let explicitValue = mass.ToString("F2", CultureInfo.GetCultureInfo("en-GB"))
if $"{mass}" <> "4.25" || $"{explicitValue}" <> "4.25" then failwith "Culture changed"' }
    )
    foreach ($case in $cases) {
      $case.Source | Set-Content Probe.fs
      $output = & $Net11Dotnet build Probe.fsproj -c Release --nologo @($case.Flags) 2>&1 | Out-String
      if ($case.Pattern) {
        if ($LASTEXITCODE -eq 0 -or $output -notmatch $case.Pattern) { throw "$($case.Name): $output" }
        $output -split '\r?\n' | Where-Object { $_ -match 'error FS' } | Select-Object -Unique | Write-Host
      }
      else {
        if ($LASTEXITCODE -ne 0) { throw $output }
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
  $resolved = [IO.Path]::GetFullPath($scratch)
  if (-not $resolved.StartsWith([IO.Path]::GetFullPath([IO.Path]::GetTempPath()), [StringComparison]::OrdinalIgnoreCase)) { throw 'Unexpected scratch path.' }
  Remove-Item -LiteralPath $resolved -Recurse -Force
}
