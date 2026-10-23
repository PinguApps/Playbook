[CmdletBinding()]
param(
  [string]$BaseUri = 'http://127.0.0.1:5418',
  [string]$Net11Dotnet = 'C:\Users\pingu\.dotnet-playbook-11\dotnet.exe',
  [switch]$ProbeSharing,
  [string]$OutputPath = (Join-Path $PSScriptRoot '../artifacts/pin788/http.evidence.json')
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
$report = [ordered]@{ kind = 'actual-http-execution'; recordedAt = [DateTimeOffset]::UtcNow.ToString('O'); requests = @() }
function Read-Options([string]$Uri) {
  $response = Invoke-WebRequest $Uri
  $match = [regex]::Match($response.Content, '<!--Blazor-Configuration:([A-Za-z0-9+/=]+)-->')
  if (-not $match.Success) { throw "Missing actual browser configuration: $Uri" }
  $json = [Text.Encoding]::UTF8.GetString([Convert]::FromBase64String($match.Groups[1].Value))
  return [ordered]@{ uri = $Uri; status = $response.StatusCode; options = ($json | ConvertFrom-Json -AsHashtable); bodySha256 = [Convert]::ToHexStringLower([Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes($response.Content))) }
}
foreach ($path in @('/server', '/wasm', '/auto')) {
  $entry = Read-Options "$BaseUri$path"
  $options = $entry.options
  if ($options.server.reconnectionMaxRetries -ne 3 -or $options.server.reconnectionRetryIntervalMilliseconds -ne 1000 -or
      $options.server.reconnectionDialogId -cne 'tide-reconnect' -or $options.ssr.disableDomPreservation -ne $false -or
      $options.ssr.circuitInactivityTimeoutMs -ne 120000 -or $options.webAssembly.environmentName -cne 'TideDesk') {
    throw "Unexpected endpoint/component merge or conversion: $($entry | ConvertTo-Json -Depth 10)"
  }
  $report.requests += $entry
}
if ($ProbeSharing) {
  # A deliberately unsafe diagnostic consumer, kept separate from the uniform
  # canonical policy. Two independent requests visit the same real endpoint.
  $probe = Join-Path $root ('artifacts/pin788/sharing-' + [guid]::NewGuid().ToString('N'))
  New-Item -ItemType Directory -Path $probe -Force | Out-Null
  $files = @{
    'Directory.Build.props' = '<Project><PropertyGroup><Nullable>enable</Nullable><ImplicitUsings>enable</ImplicitUsings><TreatWarningsAsErrors>true</TreatWarningsAsErrors></PropertyGroup></Project>'
    'Directory.Packages.props' = '<Project><PropertyGroup><ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally></PropertyGroup></Project>'
    'global.json' = '{"sdk":{"version":"11.0.100-rc.2.26504.105","rollForward":"disable"}}'
    'Probe.csproj' = '<Project Sdk="Microsoft.NET.Sdk.Web"><PropertyGroup><TargetFramework>net11.0</TargetFramework><LangVersion>15.0</LangVersion></PropertyGroup></Project>'
    'Program.cs' = @'
using Microsoft.AspNetCore.Components;
using TideProbe;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
var app = builder.Build();
app.UseAntiforgery();
app.Use(async (context, next) =>
{
    var first = BrowserOptions.GetBrowserOptions(context);
    context.Response.Headers["X-Probe-Same-Lookup"] = ReferenceEquals(first, BrowserOptions.GetBrowserOptions(context)).ToString();
    context.Response.Headers["X-Probe-Metadata-Identity"] = ReferenceEquals(first, context.GetEndpoint()?.Metadata.GetMetadata<BrowserOptions>()).ToString();
    await next(context);
});
app.MapRazorComponents<App>().AddInteractiveServerRenderMode().WithBrowserOptions(options => options.InteractiveServer.ReconnectionMaxRetries = 7);
app.Run();
'@
    'App.razor' = @'
@namespace TideProbe
@using Microsoft.AspNetCore.Components.Routing
@using Microsoft.AspNetCore.Components.Web
<!DOCTYPE html><html><head><base href="/" /></head><body>
<Router AppAssembly="typeof(App).Assembly"><Found Context="route"><RouteView RouteData="route" /></Found></Router>
<Live @rendermode="new InteractiveServerRenderMode()" />
</body></html>
'@
    'Probe.razor' = @'
@namespace TideProbe
@page "/probe"
@using Microsoft.AspNetCore.Components
@if (Retries.HasValue)
{
    <ConfigureBrowser Options="Policy" />
}
<p>Sharing probe</p>
@code {
    [SupplyParameterFromQuery] public int? Retries { get; set; }
    private BrowserOptions Policy => new() { InteractiveServer = { ReconnectionMaxRetries = Retries } };
}
'@
    'Live.razor' = '@namespace TideProbe
<p>Interactive marker for the real Blazor Web App configuration emission.</p>'
  }
  foreach ($name in $files.Keys) { [IO.File]::WriteAllText((Join-Path $probe $name), $files[$name].Replace("`r`n", "`n") + "`n") }
  $previous = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
  $process = $null
  Push-Location $probe
  try {
    $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
    & $Net11Dotnet build Probe.csproj -c Release | Write-Host
    $start = [Diagnostics.ProcessStartInfo]::new($Net11Dotnet)
    $start.WorkingDirectory = $probe
    $start.UseShellExecute = $false
    $start.CreateNoWindow = $true
    $start.RedirectStandardOutput = $true
    $start.RedirectStandardError = $true
    foreach ($argument in @((Join-Path $probe 'bin/Release/net11.0/Probe.dll'), '--urls', 'http://127.0.0.1:5398', '--Logging:LogLevel:Default', 'Warning')) { $start.ArgumentList.Add($argument) }
    $start.Environment['DOTNET_ADD_GLOBAL_TOOLS_TO_PATH'] = '0'
    $process = [Diagnostics.Process]::Start($start)
    $stdout = $process.StandardOutput.ReadToEndAsync()
    $stderr = $process.StandardError.ReadToEndAsync()
    for ($i = 0; $i -lt 100; $i++) {
      try { $initial = Read-Options 'http://127.0.0.1:5398/probe'; break }
      catch { if ($process.HasExited -or $i -eq 99) { throw }; Start-Sleep -Milliseconds 100 }
    }
    $override = Read-Options 'http://127.0.0.1:5398/probe?retries=1'
    $following = Read-Options 'http://127.0.0.1:5398/probe'
    $identity = (Invoke-WebRequest 'http://127.0.0.1:5398/probe').Headers
    $sequence = @($initial, $override, $following)
    if (($sequence.options.server.reconnectionMaxRetries -join ',') -cne '7,1,1' -or
        $identity['X-Probe-Same-Lookup'][0] -cne 'True' -or $identity['X-Probe-Metadata-Identity'][0] -cne 'True') {
      throw "Sharing probe did not reproduce the stated RC2 behaviour: $($sequence | ConvertTo-Json -Depth 10)"
    }
    $inputs = [ordered]@{}
    foreach ($name in ($files.Keys | Sort-Object)) { $inputs[$name] = (Get-FileHash (Join-Path $probe $name)).Hash.ToLowerInvariant() }
    $report.sharingProbe = @{ requests = $sequence; sameLookup = $true; sameMetadataInstance = $true; inputs = $inputs; artifactSha256 = (Get-FileHash (Join-Path $probe 'bin/Release/net11.0/Probe.dll')).Hash.ToLowerInvariant(); scope = 'Separate local RC2 diagnostic consumer; not a framework-wide security finding or GA claim.' }
  }
  finally {
    if ($null -ne $process) { if (-not $process.HasExited) { $process.Kill($true) }; $process.WaitForExit(); $report.probeStdout = $stdout.GetAwaiter().GetResult(); $report.probeStderr = $stderr.GetAwaiter().GetResult(); $process.Dispose() }
    $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previous
    Pop-Location
  }
}
[IO.File]::WriteAllText([IO.Path]::GetFullPath($OutputPath), (ConvertTo-Json $report -Depth 20).Replace("`r`n", "`n") + "`n")
Write-Host 'PASS: real generated browser options, precedence and scalar conversions.'
if ($ProbeSharing) { Write-Host 'PASS: separate-request RC2 sharing probe reproduced 7,1,1; process stopped.' }
