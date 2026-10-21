[CmdletBinding()]
param(
  [string]$Net10Dotnet = 'C:/Program Files/dotnet/dotnet.exe',
  [string]$Net11Dotnet = 'C:/Users/pingu/.dotnet-playbook-11/dotnet.exe',
  [string]$Docker = 'C:/Program Files/Docker/Docker/resources/bin/docker.exe',
  [string]$LinuxImage = 'mcr.microsoft.com/dotnet/sdk@sha256:bfabf51e74c5a7cf2fc4eac852145db86f4867d475257d809518d6a2b0fe71e1',
  [switch]$NativeAot,
  [string]$VisualCppRoot = 'C:/Program Files (x86)/Microsoft Visual Studio/18/BuildTools/VC/Tools/MSVC/14.51.36231',
  [string]$WindowsSdkLibRoot = 'C:/Program Files (x86)/Windows Kits/10/Lib/10.0.26100.0',
  [string]$OutputPath = (Join-Path $PSScriptRoot '../artifacts/native-capabilities.json')
)

# Authoring preflight only; this is not recipe evidence or a CI hardware matrix.
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
if (-not $IsWindows) { throw 'This preflight checks the existing Windows x64 authoring route and its Docker Linux engine.' }
$output = [IO.Path]::GetFullPath($OutputPath)
$work = Join-Path ([IO.Path]::GetTempPath()) ('playbook-native-' + [guid]::NewGuid().ToString('N'))
$paths = @{}
foreach ($scope in @('Process', 'User', 'Machine')) {
  $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH', $scope)
}
$previous = @{}
foreach ($key in @('DOTNET_ADD_GLOBAL_TOOLS_TO_PATH', 'DOTNET_CLI_TELEMETRY_OPTOUT', 'DOTNET_CLI_HOME', 'DOTNET_GENERATE_ASPNET_CERTIFICATE', 'DOTNET_NOLOGO')) {
  $previous[$key] = [Environment]::GetEnvironmentVariable($key)
}
function Invoke-Build([string]$Executable, [string[]]$Arguments) {
  $PSNativeCommandUseErrorActionPreference = $false
  $log = (& $Executable @Arguments 2>&1 | Out-String).Trim()
  if ($LASTEXITCODE -ne 0) { throw $log }
  $log
}
$program = @'
using System.Diagnostics;
using System.Net;
using System.Net.Quic;
using System.Net.Security;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;

Dictionary<string, object?> result = new()
{
    ["os"] = RuntimeInformation.OSDescription,
    ["osArchitecture"] = RuntimeInformation.OSArchitecture.ToString(),
    ["processArchitecture"] = RuntimeInformation.ProcessArchitecture.ToString(),
    ["runtime"] = RuntimeInformation.FrameworkDescription,
    ["vector128"] = Vector128.IsHardwareAccelerated,
    ["vector256"] = Vector256.IsHardwareAccelerated,
    ["vector512"] = Vector512.IsHardwareAccelerated
};
Dictionary<string, bool> instructions = new();
foreach (Type type in typeof(Vector128).Assembly.GetExportedTypes().OrderBy(type => type.FullName))
{
    if (type.Namespace is not ("System.Runtime.Intrinsics.X86" or "System.Runtime.Intrinsics.Arm")) continue;
    PropertyInfo? supported = type.GetProperty("IsSupported", BindingFlags.Public | BindingFlags.Static);
    if (supported?.PropertyType == typeof(bool)) instructions[type.FullName!] = (bool)supported.GetValue(null)!;
}
result["instructions"] = instructions;
result["quicListenerSupported"] = QuicListener.IsSupported;
result["quicConnectionSupported"] = QuicConnection.IsSupported;
result["tlsContextTypes"] = typeof(SslStream).Assembly.GetExportedTypes()
    .Where(type => type.Name.StartsWith("Tls", StringComparison.Ordinal)).Select(type => type.FullName).ToArray();

// Check a real OS TLS 1.3 exchange, entirely on loopback, with an ephemeral certificate.
// This does not establish the experimental caller-owned TLS API or QUIC priority behaviour.
using RSA key = RSA.Create(2048);
CertificateRequest request = new("CN=localhost", key, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
using X509Certificate2 generated = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddMinutes(10));
using X509Certificate2 certificate = X509CertificateLoader.LoadPkcs12(generated.Export(X509ContentType.Pfx), null);
using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(20));
using TcpListener listener = new(IPAddress.Loopback, 0);
listener.Start();
Task server = Task.Run(async () =>
{
    using TcpClient socket = await listener.AcceptTcpClientAsync(timeout.Token);
    using SslStream tls = new(socket.GetStream());
    await tls.AuthenticateAsServerAsync(new SslServerAuthenticationOptions
    {
        ServerCertificate = certificate, EnabledSslProtocols = SslProtocols.Tls13
    }, timeout.Token);
    byte[] value = new byte[1];
    await tls.ReadExactlyAsync(value, timeout.Token);
    if (value[0] != 42) throw new InvalidOperationException("TLS payload mismatch.");
    await tls.WriteAsync(value, timeout.Token);
});
using (TcpClient socket = new())
{
    await socket.ConnectAsync(IPAddress.Loopback, ((IPEndPoint)listener.LocalEndpoint).Port, timeout.Token);
    using SslStream tls = new(socket.GetStream(), false, (_, remote, _, _) => remote?.GetCertHashString() == certificate.GetCertHashString());
    await tls.AuthenticateAsClientAsync(new SslClientAuthenticationOptions
    {
        TargetHost = "localhost", EnabledSslProtocols = SslProtocols.Tls13
    }, timeout.Token);
    await tls.WriteAsync(new byte[] { 42 }, timeout.Token);
    byte[] value = new byte[1];
    await tls.ReadExactlyAsync(value, timeout.Token);
    if (value[0] != 42) throw new InvalidOperationException("TLS echo mismatch.");
    result["tls13Loopback"] = tls.SslProtocol.ToString();
}
await server;
result["loadedNativeLibraries"] = Process.GetCurrentProcess().Modules.Cast<ProcessModule>()
    .Where(module => module.ModuleName.Contains("msquic", StringComparison.OrdinalIgnoreCase)
        || module.ModuleName.Contains("schannel", StringComparison.OrdinalIgnoreCase)
        || module.ModuleName.Contains("libssl", StringComparison.OrdinalIgnoreCase))
    .Select(module => new { name = module.ModuleName, path = module.FileName, version = module.FileVersionInfo.FileVersion }).ToArray();
Console.WriteLine(JsonSerializer.Serialize(result));
'@

try {
  New-Item -ItemType Directory -Path $work | Out-Null
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  $env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
  $env:DOTNET_GENERATE_ASPNET_CERTIFICATE = 'false'
  $env:DOTNET_NOLOGO = '1'
  $env:DOTNET_CLI_HOME = Join-Path $work 'home'
  $report = [ordered]@{ recordedAt = [DateTimeOffset]::UtcNow.ToString('O'); kind = 'authoring-preflight'; hosts = @() }
  foreach ($baseline in @(
    @{ dotnet = $Net10Dotnet; sdk = '10.0.402'; target = 'net10.0' },
    @{ dotnet = $Net11Dotnet; sdk = '11.0.100-rc.2.26504.105'; target = 'net11.0' }
  )) {
    $dotnet = (Resolve-Path -LiteralPath $baseline.dotnet).Path
    $directory = Join-Path $work $baseline.target
    New-Item -ItemType Directory -Path $directory | Out-Null
    @{ sdk = @{ version = $baseline.sdk; rollForward = 'disable' } } | ConvertTo-Json | Set-Content (Join-Path $directory 'global.json')
    '<configuration><packageSources><clear /></packageSources></configuration>' | Set-Content (Join-Path $directory 'NuGet.Config')
    "<Project Sdk=`"Microsoft.NET.Sdk`"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>$($baseline.target)</TargetFramework><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable><NoWarn>CA1416</NoWarn></PropertyGroup></Project>" |
      Set-Content (Join-Path $directory 'Probe.csproj')
    $program | Set-Content (Join-Path $directory 'Program.cs')
    Push-Location $directory
    try {
      $sdk = (& $dotnet --version | Out-String).Trim()
      if ($sdk -ne $baseline.sdk) { throw "Expected SDK $($baseline.sdk), received $sdk." }
      $build = Invoke-Build $dotnet @('build', 'Probe.csproj', '-c', 'Release', '--nologo', '--disable-build-servers')
      $actual = (& $dotnet (Join-Path $directory "bin/Release/$($baseline.target)/Probe.dll") | Out-String) | ConvertFrom-Json
      $report.hosts += [ordered]@{ sdk = $sdk; target = $baseline.target; build = $build; probe = $actual; workloads = (& $dotnet workload list | Out-String).Trim() }
      if ($NativeAot) {
        # A separate minimal app establishes the native linker/executable route,
        # without treating reflection in the inventory probe as AOT-compatible.
        $native = Join-Path $directory 'native'
        New-Item -ItemType Directory -Path $native | Out-Null
        $linker = (Resolve-Path -LiteralPath (Join-Path $VisualCppRoot 'bin/Hostx64/x64/link.exe')).Path
        $libraries = @((Join-Path $VisualCppRoot 'lib/x64'), (Join-Path $WindowsSdkLibRoot 'um/x64'), (Join-Path $WindowsSdkLibRoot 'ucrt/x64'))
        foreach ($library in $libraries) { $null = Resolve-Path -LiteralPath $library }
        $escapedLibraries = [Security.SecurityElement]::Escape(($libraries -join ';'))
        "<Project Sdk=`"Microsoft.NET.Sdk`"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>$($baseline.target)</TargetFramework><PublishAot>true</PublishAot><IlcUseEnvironmentalTools>true</IlcUseEnvironmentalTools></PropertyGroup><ItemGroup><AdditionalNativeLibraryDirectories Include=`"$escapedLibraries`" /></ItemGroup></Project>" |
          Set-Content (Join-Path $native 'Native.csproj')
        'System.Console.WriteLine("Native AOT route: " + System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture);' |
          Set-Content (Join-Path $native 'Program.cs')
        '<configuration><packageSources><clear /><add key="nuget.org" value="https://api.nuget.org/v3/index.json" /></packageSources></configuration>' |
          Set-Content (Join-Path $native 'NuGet.Config')
        # Bypass vcvarsall/SDK tool discovery: it changes the child shell PATH.
        $nativeBuild = Invoke-Build $dotnet @('publish', (Join-Path $native 'Native.csproj'), '-c', 'Release', '-r', 'win-x64', '--nologo', '--disable-build-servers', "-p:CppLinker=$linker")
        $nativeOutput = (& (Join-Path $native "bin/Release/$($baseline.target)/win-x64/publish/Native.exe") | Out-String).Trim()
        if ($nativeOutput -ne 'Native AOT route: X64') { throw 'Unexpected native executable output.' }
        $dependencies = (Get-Content -LiteralPath (Join-Path $native 'obj/project.assets.json') -Raw | ConvertFrom-Json).libraries.PSObject.Properties.Name | Sort-Object
        $report.hosts[-1].nativeAot = @{ build = $nativeBuild; output = $nativeOutput; linker = $linker; linkerVersion = (Get-Item -LiteralPath $linker).VersionInfo.FileVersion; libraries = $libraries; dependencies = @($dependencies) }
      }
    } finally { Pop-Location }
  }
  $report.docker = [ordered]@{
    version = (& $Docker version --format 'client={{.Client.Version}} server={{.Server.Version}} os={{.Server.Os}} arch={{.Server.Arch}}' | Out-String).Trim()
    host = (& $Docker info --format 'os={{.OperatingSystem}} arch={{.Architecture}} kernel={{.KernelVersion}}' | Out-String).Trim()
    builder = (& $Docker buildx inspect | Select-String '^(Name|Driver|Status|BuildKit version|Platforms|Endpoint):' | ForEach-Object Line) -join "`n"
    image = $LinuxImage
    linuxInventory = (& $Docker run --rm --network none --cap-drop ALL --security-opt no-new-privileges $LinuxImage /bin/sh -c '
      set -eu
      uname -m
      cat /etc/os-release
      /usr/share/dotnet/dotnet --list-sdks
      /usr/bin/openssl version
      for tool in cc clang ld emcc podman; do command -v "$tool" || true; done
      /sbin/ldconfig -p | /usr/bin/grep -E "libmsquic|libssl" || true
      /usr/bin/grep -m1 "^flags" /proc/cpuinfo
    ' | Out-String).Trim()
  }
  New-Item -ItemType Directory -Path (Split-Path $output -Parent) -Force | Out-Null
  $report | ConvertTo-Json -Depth 15 | Set-Content -LiteralPath $output
} finally {
  foreach ($key in $previous.Keys) {
    if ($null -eq $previous[$key]) { Remove-Item -LiteralPath "Env:$key" -ErrorAction SilentlyContinue }
    else { [Environment]::SetEnvironmentVariable($key, $previous[$key]) }
    if ($previous[$key] -cne [Environment]::GetEnvironmentVariable($key)) { throw "Unexpected $key change after preflight." }
  }
  # Only the unique directory allocated above is removed, never a caller-supplied path.
  if (Test-Path -LiteralPath $work) {
    if ((Resolve-Path -LiteralPath $work).Path -cne [IO.Path]::GetFullPath($work)) { throw 'Unexpected temporary directory resolution.' }
    Remove-Item -LiteralPath $work -Recurse -Force
  }
  foreach ($scope in $paths.Keys) {
    if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH', $scope)) {
      throw "Unexpected $scope PATH change; inspect shared settings before changing them."
    }
  }
}
Write-Host "PASS: two compiled runtime probes, TLS 1.3 loopback and Docker inventory (NativeAot=$NativeAot). Report: $output"
