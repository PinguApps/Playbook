[CmdletBinding()]
param([string]$Net11Dotnet = 'dotnet')

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$sample = Join-Path $root 'samples/net11.0/StaticInterfaceHelpers'
$scratch = Join-Path ([IO.Path]::GetTempPath()) ('playbook-static-helpers-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory $scratch | Out-Null
try {
  Copy-Item (Join-Path $sample 'global.json') $scratch
  $assembly = [Security.SecurityElement]::Escape((Join-Path $sample 'bin/Release/net11.0/StaticInterfaceHelpers.dll'))
  @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup><TargetFramework>net11.0</TargetFramework><LangVersion>15.0</LangVersion><Nullable>enable</Nullable><ImplicitUsings>enable</ImplicitUsings><TreatWarningsAsErrors>true</TreatWarningsAsErrors><OutputType>Exe</OutputType></PropertyGroup>
  <ItemGroup><Reference Include="StaticInterfaceHelpers"><HintPath>$assembly</HintPath></Reference></ItemGroup>
</Project>
"@ | Set-Content (Join-Path $scratch 'Probe.csproj')
  Push-Location $scratch
  try {
    $consumer = @'
using Playbook.Samples;
public sealed class GardenSensor : IObservationSource { public string Station => " garden "; }
public static class Program {
  public static void Main() {
    IObservationSource source = new GardenSensor();
    if (IObservationSource.CreateLabel(source.Station, 12) != "GARDEN-012") throw new System.InvalidOperationException();
    System.Console.WriteLine("PASS: external consumer implements only Station and calls the interface helper");
  }
}
'@
    $cases = @(
      @{ Name = 'external implementer and public helper'; Pattern = $null; Source = $consumer },
      @{ Name = 'private helper is inaccessible'; Pattern = 'CS0117'; Source = 'using Playbook.Samples; public static class Program { public static void Main() => IObservationSource.NormalizeStation("x"); }' },
      @{ Name = 'helper is not inherited by implementing class'; Pattern = 'CS0117'; Source = 'using Playbook.Samples; public static class Program { public static void Main() => RoofSensor.CreateLabel("x", 1); }' },
      @{ Name = 'helper is not an instance member'; Pattern = 'CS0176'; Source = 'using Playbook.Samples; public static class Program { public static void Main() { IObservationSource x = new RoofSensor(); x.CreateLabel("x", 1); } }' },
      @{ Name = 'helper cannot be implemented explicitly'; Pattern = 'CS0539'; Source = 'using Playbook.Samples; public sealed class Sensor : IObservationSource { public string Station => "x"; static string IObservationSource.CreateLabel(string x, int n) => x; } public static class Program { public static void Main() { } }' },
      @{ Name = 'static virtual requires type parameter dispatch'; Pattern = 'CS8926'; Source = 'public interface IPolicy { static virtual int Size() => 3; } public static class Program { public static void Main() => IPolicy.Size(); }' },
      @{ Name = 'static abstract adds an implementation obligation'; Pattern = 'CS0535'; Source = 'public interface IPolicy { static abstract int Size(); } public sealed class Policy : IPolicy { } public static class Program { public static void Main() { } }' }
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
          & $Net11Dotnet ./bin/Release/net11.0/Probe.dll
          if ($LASTEXITCODE -ne 0) { throw 'External consumer execution failed.' }
        }
        elseif ($exitCode -eq 0 -or $output -notmatch $case.Pattern) { throw "$($case.Name): unexpected result`n$output" }
        Write-Host "PASS: $($case.Name)"
        if ($case.Pattern) { $output -split '\r?\n' | Where-Object { $_ -match 'error CS' } | Select-Object -Unique | Write-Host }
      }
    }
    finally { $PSNativeCommandUseErrorActionPreference = $previousNativePreference }

    # The same helper surface already works with C# 14/net10.0; do not invent a new-runtime requirement.
    Copy-Item (Join-Path $root 'samples/net10.0/StaticInterfaceHelpers/global.json') global.json -Force
    $source = [Security.SecurityElement]::Escape((Join-Path $sample 'Observations.cs'))
    @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup><TargetFramework>net10.0</TargetFramework><LangVersion>14.0</LangVersion><Nullable>enable</Nullable><ImplicitUsings>enable</ImplicitUsings><TreatWarningsAsErrors>true</TreatWarningsAsErrors><OutputType>Exe</OutputType></PropertyGroup>
  <ItemGroup><Compile Include="$source" Link="Observations.cs" /></ItemGroup>
</Project>
"@ | Set-Content Probe.csproj
    $consumer | Set-Content Probe.cs
    dotnet build Probe.csproj -c Release --nologo
    if ($LASTEXITCODE -ne 0) { throw 'The .NET 10 same-surface comparison failed to compile.' }
    dotnet ./bin/Release/net10.0/Probe.dll
    if ($LASTEXITCODE -ne 0) { throw 'The .NET 10 same-surface comparison failed to execute.' }
    Write-Host 'PASS: the interface helper surface already works on .NET 10 / C# 14'
  }
  finally { Pop-Location }
}
finally {
  $resolved = [IO.Path]::GetFullPath($scratch)
  if (-not $resolved.StartsWith([IO.Path]::GetFullPath([IO.Path]::GetTempPath()), [StringComparison]::OrdinalIgnoreCase)) { throw 'Unexpected scratch path.' }
  Remove-Item -LiteralPath $resolved -Recurse -Force
}
