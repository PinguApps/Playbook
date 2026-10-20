[CmdletBinding()]
param([string]$Net11Dotnet = 'dotnet')

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$sample = Join-Path $root 'samples/net11.0/ClosedOutcomes'
$scratch = Join-Path ([IO.Path]::GetTempPath()) ('playbook-closed-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory $scratch | Out-Null
try {
  Copy-Item (Join-Path $sample 'global.json') $scratch
  $assembly = [Security.SecurityElement]::Escape((Join-Path $sample 'bin/Release/net11.0/ClosedOutcomes.dll'))
  @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup><TargetFramework>net11.0</TargetFramework><LangVersion>15.0</LangVersion><Nullable>enable</Nullable><TreatWarningsAsErrors>true</TreatWarningsAsErrors></PropertyGroup>
  <ItemGroup><Reference Include="ClosedOutcomes"><HintPath>$assembly</HintPath></Reference></ItemGroup>
</Project>
"@ | Set-Content (Join-Path $scratch 'Probe.csproj')
  Push-Location $scratch
  try {
    $cases = @(
      @{ Name = 'missing union case'; Pattern = 'CS8509'; Source = 'using Playbook.Samples; public static class Probe { public static int Match(Outcome x) => x switch { ArchiveReceipt => 1, null => 0 }; }' },
      @{ Name = 'missing hierarchy case'; Pattern = 'CS8509'; Source = 'using Playbook.Samples; public static class Probe { public static int Match(IntakeState? x) => x switch { Waiting => 1, null => 0 }; }' },
      @{ Name = 'external direct descendant'; Pattern = 'CS9382'; Source = 'using Playbook.Samples; public record Intruder : IntakeState;' },
      @{ Name = 'inference on an open type'; Pattern = 'SYSLIB1243'; Source = '[System.Text.Json.Serialization.JsonPolymorphic(InferClosedTypePolymorphism = true)] public class OpenState; [System.Text.Json.Serialization.JsonSerializable(typeof(OpenState))] internal partial class Context : System.Text.Json.Serialization.JsonSerializerContext;' },
      @{ Name = 'escaped contextual identifiers'; Pattern = $null; Source = 'public class @closed; public class @union; public class Use { public @closed First = new(); public @union Second = new(); }' }
    )
    $previousNativePreference = $PSNativeCommandUseErrorActionPreference
    try {
      $PSNativeCommandUseErrorActionPreference = $false
      foreach ($case in $cases) {
        $case.Source | Set-Content Probe.cs
        $output = & $Net11Dotnet build Probe.csproj -c Release --nologo 2>&1 | Out-String
        $exitCode = $LASTEXITCODE
        if ($null -eq $case.Pattern) {
          if ($exitCode -ne 0) { throw $output }
        }
        elseif ($exitCode -eq 0 -or $output -notmatch $case.Pattern) { throw "$($case.Name): unexpected result`n$output" }
        Write-Host "PASS: $($case.Name)"
        if ($case.Pattern) { $output -split '\r?\n' | Where-Object { $_ -match 'error (CS|SYSLIB)' } | Select-Object -Unique | Write-Host }
      }
    }
    finally { $PSNativeCommandUseErrorActionPreference = $previousNativePreference }
  }
  finally { Pop-Location }
}
finally {
  $resolved = [IO.Path]::GetFullPath($scratch)
  if (-not $resolved.StartsWith([IO.Path]::GetFullPath([IO.Path]::GetTempPath()), [StringComparison]::OrdinalIgnoreCase)) { throw 'Unexpected scratch path.' }
  Remove-Item -LiteralPath $resolved -Recurse -Force
}

