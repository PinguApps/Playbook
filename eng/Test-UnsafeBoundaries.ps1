[CmdletBinding()]
param([string]$Net11Dotnet = 'dotnet')

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$sample = Join-Path $root 'samples/net11.0/UnsafeBoundaries'
$scratch = Join-Path ([IO.Path]::GetTempPath()) ('playbook-unsafe-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory $scratch | Out-Null
$previousNativePreference = $PSNativeCommandUseErrorActionPreference
$previousIntegration = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
$savedPaths = @{}
foreach ($scope in 'Process','User','Machine') { $savedPaths[$scope] = [Environment]::GetEnvironmentVariable('PATH', $scope) }
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
    @{ Name = 'unsafe constructor struct constraint'; Source = 'struct Beacon { public unsafe Beacon() {} } class Program { static T Make<T>() where T:struct => new T(); static void Main() { _ = Make<Beacon>(); } }'; Pattern = 'CS9376' },
    @{ Name = 'audited constructor constraint'; Source = 'class Beacon { public unsafe Beacon() {} } class Program { static T Make<T>() where T:new() => new T(); static void Main() { _ = unsafe(Make<Beacon>()); } }'; Run = $true },
    @{ Name = 'LibraryImport generated safe declaration'; Source = 'partial class Program { [System.Runtime.InteropServices.LibraryImport("beacon")] public static safe partial int Read(); static void Main() {} }' },
    @{ Name = 'legacy pointer signature compatibility'; Source = 'class Program { static void Main() { _ = Legacy.Echo(null); } }'; Pattern = 'CS9363' },
    @{ Name = 'legacy function pointer signature compatibility'; Source = 'class Program { static void Main() { _ = Legacy.Callback(); } }'; Pattern = 'CS9363' },
    @{ Name = 'unsafe property backing field is independent'; Source = 'class Program { public unsafe int Value { get => field; set => field = value; } static void Main() { var p = new Program(); unsafe { p.Value = 2; if(p.Value != 2) throw new System.Exception(); } } }'; Run = $true },
    @{ Name = 'TFM default diagnostic'; Source = "#error version`nclass Program { static void Main() {} }"; Language = ''; Features = ''; Pattern = 'CS8304:.*Language version: 15.0' },
    @{ Name = 'explicit stable diagnostic'; Source = "#error version`nclass Program { static void Main() {} }"; Language = '15.0'; Features = ''; Pattern = 'CS8304:.*Language version: 15.0' },
    @{ Name = 'default mode cleaned source'; Source = 'class Program { static void Main() {} }'; Language = ''; Features = ''; Run = $true },
    @{ Name = 'explicit mode cleaned source'; Source = 'class Program { static void Main() {} }'; Language = '15.0'; Features = ''; Run = $true },
    @{ Name = 'extern needs explicit safety'; Source = 'class Program { [System.Runtime.InteropServices.DllImport("beacon")] public static extern int Read(); static void Main() {} }'; Pattern = 'CS9389' },
    @{ Name = 'extern safe and unsafe declarations'; Source = 'class Program { [System.Runtime.InteropServices.DllImport("beacon")] public static safe extern int Read(); [System.Runtime.InteropServices.DllImport("beacon")] public static unsafe extern int Write(); static void Main() {} }' },
    @{ Name = 'packed field requirement not enforced by this compiler'; Source = '[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack=1)] struct Slot { public int Value; } class Program { static void Main() {} }' },
    @{ Name = 'unsafe field write'; Source = 'class Program { static unsafe int Value; static void Main() { Value = 1; } }'; Pattern = 'CS9362' },
    @{ Name = 'safe contextual name escape'; Source = 'class @safe {} class Program { static void Main() { _ = new @safe(); } }' },
    @{ Name = 'unsafe constructor new constraint'; Source = 'class Beacon { public unsafe Beacon() {} } class Program { static T Make<T>() where T:new() => new T(); static void Main() { _ = Make<Beacon>(); } }'; Pattern = 'CS9376' },
    @{ Name = 'override cannot strengthen caller obligation'; Source = 'class Base { public virtual void Read() {} } class Program:Base { public override unsafe void Read() {} static void Main() {} }'; Pattern = 'CS9364' },
    @{ Name = 'interface cannot strengthen caller obligation'; Source = 'interface I { void Read(); } class Program:I { public unsafe void Read() {} static void Main() {} }'; Pattern = 'CS9365' },
    @{ Name = 'delegate conversion requires audit'; Source = 'class Program { static unsafe void Read() {} static void Main() { System.Action action = Read; action(); } }'; Pattern = 'CS9362' },
    @{ Name = 'delegate conversion with audit'; Source = 'class Program { static unsafe void Read() {} static void Main() { System.Action action = unsafe((System.Action)Read); action(); } }'; Run = $true },
    @{ Name = 'local function requires audit'; Source = 'class Program { static void Main() { unsafe void Read() {} Read(); } }'; Pattern = 'CS9362' },
    @{ Name = 'partial modifiers must match'; Source = 'partial class Program { static partial void Read(); static unsafe partial void Read() {} static void Main() {} }'; Pattern = 'CS0764' },
    @{ Name = 'property and accessor modifiers conflict'; Source = 'class Program { public unsafe int Value { safe get => 1; } static void Main() {} }'; Pattern = 'CS9396' },
    @{ Name = 'event accessor cannot carry modifier'; Source = 'class Program { public event System.Action Changed { unsafe add {} remove {} } static void Main() {} }'; Pattern = 'CS1609' },
    @{ Name = 'skip locals init requires stack audit'; Source = 'class Program { [System.Runtime.CompilerServices.SkipLocalsInit] static void Main() { System.Span<int> values = stackalloc int[2]; values.Clear(); } }'; Pattern = 'CS9361' },
    @{ Name = 'initialized stack allocation'; Source = 'class Program { [System.Runtime.CompilerServices.SkipLocalsInit] static void Main() { System.Span<int> values = stackalloc int[] { 1, 2 }; if(values[0] != 1) throw new System.Exception(); } }'; Run = $true },
    @{ Name = 'await audited call'; Source = 'class Program { static unsafe System.Threading.Tasks.Task Read() => System.Threading.Tasks.Task.CompletedTask; static async System.Threading.Tasks.Task Main() { await unsafe(Read()); } }'; Run = $true },
    @{ Name = 'fixed cannot cross await'; Source = 'class Program { static async System.Threading.Tasks.Task Main() { byte[] bytes = [1]; fixed(byte* p = bytes) { await System.Threading.Tasks.Task.Yield(); } } }'; Pattern = 'CS9398' },
    @{ Name = 'external caller and metadata'; Source = $consumer; Run = $true },
    @{ Name = 'caller must acknowledge the public unsafe member'; Source = $consumer.Replace('unsafe(BeaconFrame.ReadRaw(address))', 'BeaconFrame.ReadRaw(address)'); Pattern = 'CS9362' },
    @{ Name = 'member modifier does not authorize its body'; Source = $bodyWithoutContext; Pattern = 'CS9360' },
    @{ Name = 'private unsafe member remains inaccessible'; Source = $visibility; Pattern = 'CS0122' },
    @{ Name = 'explicit layout requires a safety decision'; Source = '[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit)] public struct Slot { [System.Runtime.InteropServices.FieldOffset(0)] public int Value; } public static class Program { public static void Main() { } }'; Pattern = 'CS9392' },
    @{ Name = 'unsafe syntax still requires AllowUnsafeBlocks'; Source = $consumer; Allow = 'false'; Pattern = 'CS0227' },
    @{ Name = 'GA pointer span constructor still requires unsafe'; Source = $span; Allow = 'false'; Pattern = 'CS9362' },
    @{ Name = 'GA pointer span constructor with explicit audit'; Source = $span.Replace('new System.ReadOnlySpan<byte>(address, 2)', 'unsafe(new System.ReadOnlySpan<byte>(address, 2))'); Run = $true },
    @{ Name = 'pointer declaration and pinning alone need no AllowUnsafeBlocks'; Source = 'public static class Program { public static void Main() { int x = 4; int* p = &x; byte[] data = [1, 2]; fixed (byte* first = data) { if (p == null || first == null) throw new System.InvalidOperationException(); } } }'; Allow = 'false'; Run = $true },
    @{ Name = 'pointer dereference requires a context'; Source = 'public static class Program { public static void Main() { int x = 4; int* p = &x; _ = *p; } }'; Allow = 'false'; Pattern = 'CS9360' },
    @{ Name = 'new syntax requires preview language'; Source = $consumer; Language = '15.0'; Pattern = 'CS8652' },
    @{ Name = 'pointer-free contract reaches opted-in consumers'; Source = 'public static class Api { public static unsafe int Read() { return 4; } } public static class Program { public static void Main() { _ = Api.Read(); } }'; Pattern = 'CS9362' },
    @{ Name = 'legacy rules do not enforce pointer-free caller contracts'; Source = 'public static class Api { public static unsafe int Read() { return 4; } } public static class Program { public static void Main() { if (Api.Read() != 4) throw new System.InvalidOperationException(); } }'; Features = ''; Run = $true }
  )
  Push-Location $scratch
  try {
    New-Item -ItemType Directory Legacy | Out-Null
    '<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net11.0</TargetFramework><LangVersion>14.0</LangVersion><AllowUnsafeBlocks>true</AllowUnsafeBlocks></PropertyGroup></Project>' | Set-Content Legacy/Legacy.csproj
    'public static unsafe class Legacy { public static int* Echo(int* p) => p; public static delegate*<void> Callback() => null; }' | Set-Content Legacy/Legacy.cs
    $legacyBuild = & $Net11Dotnet build Legacy/Legacy.csproj -c Release --nologo 2>&1 | Out-String
    if ($LASTEXITCODE -ne 0) { throw $legacyBuild }
    foreach ($case in $cases) {
      $allow = if ($case.ContainsKey('Allow')) { $case.Allow } else { 'true' }
      $language = if ($case.ContainsKey('Language')) { $case.Language } else { 'preview' }
      $features = if ($case.ContainsKey('Features')) { $case.Features } else { 'updated-memory-safety-rules' }
      @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup><TargetFramework>net11.0</TargetFramework><LangVersion>$language</LangVersion><Features>$features</Features><AllowUnsafeBlocks>$allow</AllowUnsafeBlocks><Nullable>enable</Nullable><TreatWarningsAsErrors>true</TreatWarningsAsErrors><OutputType>Exe</OutputType></PropertyGroup>
  <ItemGroup><Reference Include="UnsafeBoundaries"><HintPath>$assembly</HintPath></Reference></ItemGroup>
  <ItemGroup><Compile Remove="Legacy/**/*.cs" /><Reference Include="Legacy"><HintPath>Legacy/bin/Release/net11.0/Legacy.dll</HintPath></Reference></ItemGroup>
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

    # Build the exact offline setup without the repository's shared sample properties or test program.
    $legacyPath = [IO.Path]::GetFullPath((Join-Path $scratch 'Legacy'))
    if (-not $legacyPath.StartsWith([IO.Path]::GetFullPath($scratch) + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) { throw 'Unexpected legacy scratch path.' }
    Remove-Item -LiteralPath $legacyPath -Recurse -Force
    $projectSource = Get-Content (Join-Path $sample 'UnsafeBoundaries.csproj') -Raw
    $properties = [regex]::Match($projectSource, '(?s)<!-- snippet:project -->\r?\n(.*?)\s*<!-- endsnippet:project -->').Groups[1].Value
    '<Project Sdk="Microsoft.NET.Sdk">' + $properties + '</Project>' | Set-Content Probe.csproj
    Copy-Item (Join-Path $sample 'Frames.cs') Frames.cs
    $programSource = Get-Content (Join-Path $sample 'Program.cs') -Raw
    [regex]::Match($programSource, '(?s)// snippet:program\r?\n(.*?)// endsnippet:program').Groups[1].Value | Set-Content Probe.cs
    $output = & $Net11Dotnet build Probe.csproj -t:Rebuild -c Release --nologo 2>&1 | Out-String
    if ($LASTEXITCODE -ne 0) { throw "Offline setup did not compile: $output" }
    $result = & $Net11Dotnet ./bin/Release/net11.0/Probe.dll | Out-String
    if ($LASTEXITCODE -ne 0 -or $result.Trim() -cne '4661') { throw "Offline setup returned unexpected result: $result" }
    Write-Host 'PASS: exact offline project, implementation and program compile and return 4661 outside the repository'
  }
  finally { Pop-Location }
}
finally {
  $PSNativeCommandUseErrorActionPreference = $previousNativePreference
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previousIntegration
  foreach ($scope in 'Process','User','Machine') {
    if ([Environment]::GetEnvironmentVariable('PATH', $scope) -cne $savedPaths[$scope]) { throw "$scope PATH changed; preserve shared settings and investigate." }
  }
  $resolved = [IO.Path]::GetFullPath($scratch)
  if (-not $resolved.StartsWith([IO.Path]::GetFullPath([IO.Path]::GetTempPath()), [StringComparison]::OrdinalIgnoreCase)) { throw 'Unexpected scratch path.' }
  Remove-Item -LiteralPath $resolved -Recurse -Force
}
