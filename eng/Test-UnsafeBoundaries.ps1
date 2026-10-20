[CmdletBinding()]
param([string]$Net11Dotnet = 'dotnet')

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$sample = Join-Path $root 'samples/net11.0/UnsafeBoundaries'
$scratch = Join-Path ([IO.Path]::GetTempPath()) ('playbook-unsafe-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory $scratch | Out-Null
$previousNativePreference = $PSNativeCommandUseErrorActionPreference
$previousIntegration = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  $PSNativeCommandUseErrorActionPreference = $false
  Copy-Item (Join-Path $sample 'global.json') $scratch
  $assembly = [Security.SecurityElement]::Escape((Join-Path $sample 'bin/Release/net11.0/UnsafeBoundaries.dll'))
  $consumer = @'
using Playbook.Samples;
public static class Program {
  public static void Main() {
    byte[] frame = [0x35, 0x12];
    fixed (byte* address = frame) {
      if (unsafe(BeaconFrame.ReadRaw(address)) != 0x1235) throw new System.InvalidOperationException();
    }
    var assembly = typeof(BeaconFrame).Assembly;
    if (!System.Linq.Enumerable.Any(assembly.ManifestModule.GetCustomAttributesData(), a => a.AttributeType.Name == "MemorySafetyRulesAttribute" && (int)a.ConstructorArguments[0].Value! == 2)) throw new System.InvalidOperationException("Missing module rules marker.");
    var method = typeof(BeaconFrame).GetMethod("ReadRaw")!;
    if (!System.Linq.Enumerable.Any(method.GetCustomAttributesData(), a => a.AttributeType.Name == "RequiresUnsafeAttribute")) throw new System.InvalidOperationException("Missing caller obligation.");
    System.Console.WriteLine("PASS: external unsafe caller and emitted module/member contracts");
  }
}
'@
  $bodyWithoutContext = @'
public static class Program {
  public static unsafe int Read(int* address) { return *address; }
  public static void Main() { }
}
'@
  $visibility = @'
public static class Hidden { private static unsafe int Read(int* address) { unsafe { return *address; } } }
public static class Program { public static void Main() { int value = 4; int* address = &value; _ = unsafe(Hidden.Read(address)); } }
'@
  $span = @'
public static class Program {
  public static void Main() {
    byte[] frame = [0x35, 0x12];
    fixed (byte* address = frame) {
      var view = new System.ReadOnlySpan<byte>(address, 2);
      if (view[0] != 0x35 || view[1] != 0x12) throw new System.InvalidOperationException();
    }
    System.Console.WriteLine("PASS: pointer span constructor reads the two pinned bytes");
  }
}
'@
  $cases = @(
    @{ Name = 'external caller and metadata'; Source = $consumer; Run = $true },
    @{ Name = 'caller must acknowledge the public unsafe member'; Source = $consumer.Replace('unsafe(BeaconFrame.ReadRaw(address))', 'BeaconFrame.ReadRaw(address)'); Pattern = 'CS9362' },
    @{ Name = 'member modifier does not authorize its body'; Source = $bodyWithoutContext; Pattern = 'CS9360' },
    @{ Name = 'private unsafe member remains inaccessible'; Source = $visibility; Pattern = 'CS0122' },
    @{ Name = 'explicit layout requires a safety decision'; Source = '[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit)] public struct Slot { [System.Runtime.InteropServices.FieldOffset(0)] public int Value; } public static class Program { public static void Main() { } }'; Pattern = 'CS9392' },
    @{ Name = 'unsafe syntax still requires AllowUnsafeBlocks'; Source = $consumer; Allow = 'false'; Pattern = 'CS0227' },
    @{ Name = 'RC2 pointer span constructor still requires unsafe'; Source = $span; Allow = 'false'; Pattern = 'CS9362' },
    @{ Name = 'RC2 pointer span constructor with explicit audit'; Source = $span.Replace('new System.ReadOnlySpan<byte>(address, 2)', 'unsafe(new System.ReadOnlySpan<byte>(address, 2))'); Run = $true },
    @{ Name = 'pointer declaration and pinning alone need no AllowUnsafeBlocks'; Source = 'public static class Program { public static void Main() { int x = 4; int* p = &x; byte[] data = [1, 2]; fixed (byte* first = data) { if (p == null || first == null) throw new System.InvalidOperationException(); } } }'; Allow = 'false'; Run = $true },
    @{ Name = 'pointer dereference requires a context'; Source = 'public static class Program { public static void Main() { int x = 4; int* p = &x; _ = *p; } }'; Allow = 'false'; Pattern = 'CS9360' },
    @{ Name = 'new syntax requires preview language'; Source = $consumer; Language = '15.0'; Pattern = 'CS8652' },
    @{ Name = 'pointer-free contract reaches opted-in consumers'; Source = 'public static class Api { public static unsafe int Read() { return 4; } } public static class Program { public static void Main() { _ = Api.Read(); } }'; Pattern = 'CS9362' },
    @{ Name = 'legacy rules do not enforce pointer-free caller contracts'; Source = 'public static class Api { public static unsafe int Read() { return 4; } } public static class Program { public static void Main() { if (Api.Read() != 4) throw new System.InvalidOperationException(); } }'; Features = ''; Run = $true }
  )
  Push-Location $scratch
  try {
    foreach ($case in $cases) {
      $allow = if ($case.ContainsKey('Allow')) { $case.Allow } else { 'true' }
      $language = if ($case.ContainsKey('Language')) { $case.Language } else { 'preview' }
      $features = if ($case.ContainsKey('Features')) { $case.Features } else { 'updated-memory-safety-rules' }
      @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup><TargetFramework>net11.0</TargetFramework><LangVersion>$language</LangVersion><Features>$features</Features><AllowUnsafeBlocks>$allow</AllowUnsafeBlocks><Nullable>enable</Nullable><TreatWarningsAsErrors>true</TreatWarningsAsErrors><OutputType>Exe</OutputType></PropertyGroup>
  <ItemGroup><Reference Include="UnsafeBoundaries"><HintPath>$assembly</HintPath></Reference></ItemGroup>
</Project>
"@ | Set-Content Probe.csproj
      $case.Source | Set-Content Probe.cs
      # Rebuild is essential: changing compiler switches alone can reuse an incremental output.
      $output = & $Net11Dotnet build Probe.csproj -t:Rebuild -c Release --nologo 2>&1 | Out-String
      $exitCode = $LASTEXITCODE
      if ($case.Pattern) {
        if ($exitCode -eq 0 -or $output -notmatch $case.Pattern) { throw "$($case.Name): unexpected result`n$output" }
        $output -split '\r?\n' | Where-Object { $_ -match 'error CS' } | Select-Object -Unique | Write-Host
      }
      else {
        if ($exitCode -ne 0) { throw $output }
        if ($case.Run) {
          & $Net11Dotnet ./bin/Release/net11.0/Probe.dll
          if ($LASTEXITCODE -ne 0) { throw 'Consumer execution failed.' }
        }
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
