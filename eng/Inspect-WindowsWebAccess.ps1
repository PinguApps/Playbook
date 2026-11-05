#requires -Version 7.4
[CmdletBinding()]
param(
    [ValidateSet('Prepare', 'Run')][string]$Action = 'Prepare',
    [string]$WorkDirectory = (Join-Path ([IO.Path]::GetTempPath()) ('pb-web-' + [guid]::NewGuid().ToString('N'))),
    [string]$Dotnet = (Get-Command dotnet -CommandType Application | Select-Object -First 1).Source,
    [ValidateRange(49152, 65535)][int]$Port = 54483
)

# Capability proof only. No elevation, certificate creation/trust, URL ACL or binding writes.
$ErrorActionPreference = 'Stop'
if (-not $IsWindows) { throw 'Run on the Windows authoring host.' }
$work = [IO.Path]::GetFullPath($WorkDirectory)
$paths = @{}
foreach ($scope in @('Process', 'User', 'Machine')) {
    $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH', $scope)
}
function Invoke-ProbeProcess([string[]]$Arguments) {
    $start = [Diagnostics.ProcessStartInfo]::new($Dotnet)
    $start.WorkingDirectory = $work
    $start.UseShellExecute = $false
    $start.RedirectStandardOutput = $true
    $start.RedirectStandardError = $true
    foreach ($argument in $Arguments) { $start.ArgumentList.Add($argument) }
    $start.Environment['DOTNET_ADD_GLOBAL_TOOLS_TO_PATH'] = '0'
    $start.Environment['DOTNET_GENERATE_ASPNET_CERTIFICATE'] = 'false'
    $start.Environment['DOTNET_CLI_TELEMETRY_OPTOUT'] = '1'
    $start.Environment['DOTNET_CLI_WORKLOAD_UPDATE_NOTIFY_DISABLE'] = 'true'
    $start.Environment['DOTNET_CLI_HOME'] = Join-Path $work 'home'
    $start.Environment['NUGET_PACKAGES'] = Join-Path $work 'packages'
    $process = [Diagnostics.Process]::Start($start)
    try {
        $stdout = $process.StandardOutput.ReadToEndAsync()
        $stderr = $process.StandardError.ReadToEndAsync()
        if (-not $process.WaitForExit(120000)) {
            $process.Kill($true)
            throw 'Probe timed out after 120 seconds; its process tree was stopped.'
        }
        $output = $stdout.GetAwaiter().GetResult() + $stderr.GetAwaiter().GetResult()
        if ($process.ExitCode -ne 0) { throw $output }
        $output.Trim()
    }
    finally { $process.Dispose() }
}
try {
    if ($Action -eq 'Prepare') {
        if (Test-Path -LiteralPath $work) { throw 'Choose a new work directory; existing files are never overwritten.' }
        $addresses = @([Net.IPAddress]::Loopback)
        if ([Net.Sockets.Socket]::OSSupportsIPv6) { $addresses += [Net.IPAddress]::IPv6Loopback }
        foreach ($address in $addresses) {
            $probe = [Net.Sockets.TcpListener]::new($address, $Port)
            try { $probe.Start() }
            catch { throw "Port $Port cannot be bound. Choose an available, unreserved -Port; leave existing Windows reservations unchanged. $($_.Exception.GetBaseException().Message)" }
            finally { $probe.Stop() }
        }
        [IO.Directory]::CreateDirectory($work) | Out-Null
        $files = @{
            'Directory.Build.props' = '<Project />'
            'Directory.Build.targets' = '<Project />'
            'Directory.Packages.props' = '<Project />'
            'NuGet.Config' = '<configuration><packageSources><clear /></packageSources></configuration>'
            'global.json' = '{"sdk":{"version":"10.0.403","rollForward":"disable"}}'
            'Access.csproj' = '<Project Sdk="Microsoft.NET.Sdk.Web"><PropertyGroup><TargetFramework>net10.0</TargetFramework><LangVersion>14.0</LangVersion><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable><TreatWarningsAsErrors>true</TreatWarningsAsErrors></PropertyGroup></Project>'
            'Program.cs' = @'
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Server.HttpSys;

if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException("HTTP.sys requires Windows.");
if (args.Length != 2 || !Uri.TryCreate(args[0], UriKind.Absolute, out var endpoint)
    || endpoint.Scheme != "https" || endpoint.Host != "127.0.0.1")
    throw new ArgumentException("Supply the prepared loopback HTTPS prefix and certificate thumbprint.");
var builder = WebApplication.CreateBuilder(Array.Empty<string>());
builder.WebHost.UseHttpSys(options => {
    options.UrlPrefixes.Add(endpoint.AbsoluteUri);
    options.Authentication.Schemes = AuthenticationSchemes.None;
    options.Authentication.AllowAnonymous = true;
});
await using var app = builder.Build();
app.Run(context => context.Response.WriteAsync("Willow Quay HTTP.sys access"));
using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(20));
await app.StartAsync(deadline.Token);
try {
    using var handler = new HttpClientHandler { UseProxy = false };
    // This probe pins one certificate. It does not establish OS/browser trust.
    handler.ServerCertificateCustomValidationCallback = (_, certificate, _, _) =>
        certificate is not null && string.Equals(certificate.Thumbprint, args[1], StringComparison.OrdinalIgnoreCase);
    using var client = new HttpClient(handler);
    var actual = await client.GetStringAsync(endpoint, deadline.Token);
    if (actual != "Willow Quay HTTP.sys access") throw new InvalidOperationException(actual);
    Console.WriteLine($"PASS: HTTP.sys HTTPS loopback with pinned certificate; {RuntimeInformation.FrameworkDescription}");
}
finally { await app.StopAsync(CancellationToken.None); }
'@
        }
        foreach ($file in $files.GetEnumerator()) {
            [IO.File]::WriteAllText((Join-Path $work $file.Key), $file.Value.Replace("`r`n", "`n") + "`n")
        }
        $sdk = Invoke-ProbeProcess @('--version')
        if ($sdk -ne '10.0.403') { throw "Unexpected SDK: $sdk" }
        Invoke-ProbeProcess @('build', 'Access.csproj', '-c', 'Release') |
            Set-Content -LiteralPath (Join-Path $work 'build.log')
        $manifest = [ordered]@{
            work = $work; dotnet = $Dotnet; sdk = $sdk
            os = [Runtime.InteropServices.RuntimeInformation]::OSDescription
            prefix = "https://127.0.0.1:$Port/playbook-$([guid]::NewGuid().ToString('N'))/"
            ipport = "127.0.0.1:$Port"
            sid = [Security.Principal.WindowsIdentity]::GetCurrent().User.Value
            appId = '{' + [guid]::NewGuid().ToString() + '}'
            build = 'passed'; https = 'pending-owner-setup'; trust = 'not-tested'
        }
        $manifest | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $work 'access.json')
        [pscustomobject]$manifest
    }
    else {
        $manifest = Get-Content -LiteralPath (Join-Path $work 'access.json') -Raw | ConvertFrom-Json
        $Dotnet = $manifest.dotnet
        $thumbprint = (Get-Content -LiteralPath (Join-Path $work 'certificate.txt') -Raw).Trim()
        if ($thumbprint -notmatch '^[A-Fa-f0-9]{40}$') { throw 'Expected the owner-created certificate thumbprint.' }
        $output = Invoke-ProbeProcess @((Join-Path $work 'bin/Release/net10.0/Access.dll'), $manifest.prefix, $thumbprint)
        if ($output -notmatch 'PASS: HTTP.sys HTTPS loopback') { throw $output }
        $output | Set-Content -LiteralPath (Join-Path $work 'https.log')
        $output
    }
}
finally {
    foreach ($scope in $paths.Keys) {
        if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH', $scope)) {
            throw "$scope PATH changed; shared settings were not restored automatically."
        }
    }
}
