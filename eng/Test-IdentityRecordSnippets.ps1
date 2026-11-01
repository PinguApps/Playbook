[CmdletBinding()]
param(
  [string]$InspectionDirectory = (Join-Path $PSScriptRoot '../artifacts/identity-records/inspection'),
  [ValidateSet('certificates.encode-registered-identifiers','dns.inspect-typed-records')]
  [string[]]$RecipeId = @('certificates.encode-registered-identifiers','dns.inspect-typed-records'),
  [string]$Net10Dotnet = 'C:/Program Files/dotnet/dotnet.exe',
  [string]$Net11Dotnet = 'C:/Users/pingu/.dotnet-playbook-11/dotnet.exe'
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
$session = Join-Path ([IO.Path]::GetTempPath()) ('playbook-id-snippets-' + [guid]::NewGuid().ToString('N'))
$paths = @{}
foreach ($scope in 'Process','User','Machine') { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$previous = @{}
foreach ($key in 'DOTNET_ADD_GLOBAL_TOOLS_TO_PATH','DOTNET_GENERATE_ASPNET_CERTIFICATE') { $previous[$key] = [Environment]::GetEnvironmentVariable($key) }
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  $env:DOTNET_GENERATE_ASPNET_CERTIFICATE = 'false'
  foreach ($id in $RecipeId) {
    $recipe = (Get-Content (Join-Path $InspectionDirectory "$id.json") -Raw | ConvertFrom-Json).result.structuredContent.recipe
    if ($recipe.id -cne $id) { throw 'Wrong installed response.' }
    $blocks = [regex]::Matches($recipe.markdown, '(?ms)^```csharp\r?\n(.*?)^```')
    if ($blocks.Count -ne 3) { throw "Expected three complete rendered snippets: $id" }
    foreach ($variant in 'net10.0','net11.0') {
      $certificate = $id -ceq 'certificates.encode-registered-identifiers'
      $name = if ($certificate) { 'RegisteredIdentifiers' } else { 'TypedDns' }
      $directory = Join-Path $session "$variant/$name"
      New-Item -ItemType Directory -Path $directory -Force | Out-Null
      Copy-Item (Join-Path $root "samples/$variant/$name/global.json") $directory
      $indices = if ($certificate) { if ($variant -eq 'net10.0') { @(0,2) } else { @(1,2) } }
        else { if ($variant -eq 'net10.0') { @(0,1) } else { @(0,2) } }
      foreach ($index in $indices) {
        # Only the installed response supplies snippet code; add no imports to it.
        [IO.File]::WriteAllText((Join-Path $directory "Snippet$index.cs"), $blocks[$index].Groups[1].Value)
      }
      $language = if ($variant -eq 'net10.0') { '14.0' } else { '15.0' }
      $extra = ''
      if (-not $certificate) {
        $fixture = [Security.SecurityElement]::Escape((Join-Path $root 'samples/TypedDns/AuthoritativeFixture.cs'))
        $extra = '<ItemGroup><Compile Include="' + $fixture + '" Link="AuthoritativeFixture.cs" /></ItemGroup>'
        if ($variant -eq 'net10.0') {
          Copy-Item (Join-Path $root 'samples/net10.0/TypedDns/Directory.Packages.props') $directory
          Copy-Item (Join-Path $root 'samples/net10.0/TypedDns/packages.lock.json') $directory
          $extra += '<ItemGroup><PackageReference Include="DnsClient" /></ItemGroup>'
        }
      }
      $project = '<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>{0}</TargetFramework><LangVersion>{1}</LangVersion><Nullable>enable</Nullable><ImplicitUsings>enable</ImplicitUsings><TreatWarningsAsErrors>true</TreatWarningsAsErrors></PropertyGroup>{2}</Project>' -f $variant,$language,$extra
      [IO.File]::WriteAllText((Join-Path $directory 'Probe.csproj'), $project)
      $program = if ($certificate) {
        $builder = if ($variant -eq 'net10.0') { 'ManualSan' } else { 'PermitSan' }
        @"
using Playbook.Samples;
var extension = $builder.Create("permit.reedbank.test", ["1.3.6.1.4.1.32473.41"]);
var result = CertificateRoundTrip.Read(extension);
if (result.DnsName != "permit.reedbank.test" || !result.Identifiers.SequenceEqual(new[] { "1.3.6.1.4.1.32473.41" }))
    throw new InvalidOperationException("Copied certificate snippets changed values.");
Console.WriteLine("PASS copied certificate snippets " + System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription);
"@
      }
      else {
        @'
using System.Net;
using Playbook.Samples;
using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(10));
await using var server = new AuthoritativeFixture(IPAddress.Loopback, "west", deadline.Token);
using var inspector = new ZoneInspector(server.Endpoint);
var result = await inspector.QueryAsync("txt." + Guid.NewGuid().ToString("N") + ".reedbank.test", RecordKind.Txt, deadline.Token);
if (result.Status != "NoError" || result.Records.Count != 1 || result.Records[0].Ttl != TimeSpan.FromSeconds(106)
    || !result.Records[0].Values.SequenceEqual(new[] { "station=west", "batch=41" }))
    throw new InvalidOperationException("Copied DNS snippets changed fields.");
Console.WriteLine("PASS copied DNS snippets " + System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription);
'@
      }
      [IO.File]::WriteAllText((Join-Path $directory 'Program.cs'), $program)
      $dotnet = if ($variant -eq 'net10.0') { $Net10Dotnet } else { $Net11Dotnet }
      Push-Location $directory
      try {
        & $dotnet build Probe.csproj -c Release -p:RestoreLockedMode=true
        & $dotnet "bin/Release/$variant/Probe.dll"
      }
      finally { Pop-Location }
    }
  }
  Write-Host 'PASS: actual installed certificate/DNS snippets compile and execute as separate copied files on both targets.'
}
finally {
  if (Test-Path -LiteralPath $session) {
    $resolved = [IO.Path]::GetFullPath($session)
    $temporary = [IO.Path]::GetFullPath([IO.Path]::GetTempPath()).TrimEnd('\','/') + [IO.Path]::DirectorySeparatorChar
    if (-not $resolved.StartsWith($temporary, [StringComparison]::OrdinalIgnoreCase) -or [IO.Path]::GetFileName($resolved) -notlike 'playbook-id-snippets-*') { throw 'Unexpected cleanup path.' }
    Remove-Item -LiteralPath $resolved -Recurse -Force
  }
  foreach ($key in $previous.Keys) { [Environment]::SetEnvironmentVariable($key,$previous[$key]) }
  foreach ($scope in $paths.Keys) { if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." } }
}
