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
      @{ Name = 'overlapping union conversion'; Pattern = 'CS0457'; Source = 'using Playbook.Samples; public class Probe { public OverlappingShelf Value = 24; }' },
      @{ Name = 'union conversion cannot chain'; Pattern = 'CS0029'; Source = 'using Playbook.Samples; public class Probe { public ShelfEnvelope Value = 24; }' },
      @{ Name = 'nullable source cannot lift int case'; Pattern = 'CS0029'; Source = 'public union Count(int, string); public class Probe { public Count Value = (int?)24; }' },
      @{ Name = 'union type pattern examines contents'; Pattern = 'CS8121'; Source = 'using Playbook.Samples; public class Probe { public bool Match(ShelfReading x) => x is ShelfReading; }' },
      @{ Name = 'generic overlapping constructors'; Pattern = 'CS0457'; Source = 'using Playbook.Samples; public class Probe { public GenericShelf<string> Value = "C-24"; }' },
      @{ Name = 'provider replaces constructor lookup'; Pattern = 'CS0029'; Source = 'using System.Runtime.CompilerServices; [Union] public class U : U.IUnionMembers { public U(int x) {} public interface IUnionMembers { public static U Create(string x) => new(0); public object? Value { get; } } public object? Value => null; } public class Probe { public U Value = 1; }' },
      @{ Name = 'invalid Value shape'; Pattern = 'CS9386'; Source = 'using System.Runtime.CompilerServices; [Union] public struct U { public U(int x) {} public int Value => 0; } public class Probe { public U Value = 1; }' },
      @{ Name = 'added union case requires arm'; Pattern = 'CS8509'; Source = 'public union U(int, bool, string); public class Probe { public int Match(U x) => x switch { int => 1, bool => 2, null => 0 }; }' },
      @{ Name = 'untyped property uses wrapper'; Pattern = 'CS0117'; Source = 'using Playbook.Samples; public class Probe { public bool Match(Outcome x) => x is { ShelfCode: "C-24" }; }' },
      @{ Name = 'recursive declaration alone'; Pattern = $null; Source = 'public union RecursiveShelf(RecursiveShelf, string);' },
      @{ Name = 'private provider rejected'; Pattern = 'CS9385'; Source = 'using System.Runtime.CompilerServices; [Union] public class U : U.IUnionMembers { private interface IUnionMembers { public static U Create(int x) => new(); public object? Value { get; } } public object? Value => null; }' },
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
