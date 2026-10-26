[CmdletBinding()]
param([string]$Dotnet = (Get-Command dotnet -CommandType Application | Select-Object -First 1).Source)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
$before = @{}
foreach ($scope in @('Process', 'User', 'Machine')) { $before[$scope] = [Environment]::GetEnvironmentVariable('PATH', $scope) }
$previous = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
$work = Join-Path $root ('artifacts/passkey-template-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $work | Out-Null
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  $sdk = '11.0.100-rc.2.26504.105'
  $runtime = '11.0.0-rc.2.26504.105'
  $inventory = & $Dotnet --list-sdks
  $line = @($inventory | Where-Object { $_ -match ('^' + [regex]::Escape($sdk) + ' \[') })
  if ($line.Count -ne 1 -or $line[0] -notmatch '\[(.+)\]') { throw 'Install the declared .NET 11 RC2 SDK before this check.' }
  $dotnetRoot = Split-Path $Matches[1] -Parent
  $archivePath = Join-Path $dotnetRoot "templates/$runtime/microsoft.dotnet.web.projecttemplates.11.0.$runtime.nupkg"
  $archive = [IO.Compression.ZipFile]::OpenRead($archivePath)
  try {
    $entry = $archive.GetEntry('content/BlazorWeb-CSharp/BlazorWebCSharp.1/Components/Account/PasskeyAuthenticators.cs')
    if (-not $entry) { throw 'Pinned template has no authenticator mapping.' }
    $reader = [IO.StreamReader]::new($entry.Open())
    try { $source = $reader.ReadToEnd() } finally { $reader.Dispose() }
  } finally { $archive.Dispose() }
  # Keep the vendor implementation and license intact. Add only the documented dictionary entry.
  $anchor = 'private static readonly Dictionary<Guid, string> KnownAuthenticators = new()'
  if (-not $source.Contains($anchor)) { throw 'Template customization anchor changed.' }
  $addition = '        [new("32aa82d8-9ea2-42bd-aabc-52e970752036")] = "Beacon loan key",'
  $source = $source.Replace($anchor + "`r`n    {", $anchor + "`r`n    {`r`n" + $addition)
  if (-not $source.Contains('Beacon loan key')) { throw 'Template entry was not inserted.' }
  [IO.File]::WriteAllText((Join-Path $work 'PasskeyAuthenticators.cs'), $source)
  Copy-Item -LiteralPath (Join-Path $root 'samples/net11.0/BeaconKeys/global.json') -Destination $work
  foreach ($file in @('Directory.Build.props', 'Directory.Build.targets', 'Directory.Packages.props')) { '<Project />' | Set-Content -LiteralPath (Join-Path $work $file) }
  @'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net11.0</TargetFramework><Nullable>enable</Nullable><ImplicitUsings>enable</ImplicitUsings><TreatWarningsAsErrors>true</TreatWarningsAsErrors></PropertyGroup>
  <ItemGroup><FrameworkReference Include="Microsoft.AspNetCore.App" /></ItemGroup>
</Project>
'@ | Set-Content -LiteralPath (Join-Path $work 'TemplateCheck.csproj')
  @'
using BlazorWebCSharp._1.Components.Account;
using Microsoft.AspNetCore.Identity;

UserPasskeyInfo passkey = new([1], [], DateTimeOffset.UtcNow, 0, null, true, false, false, [], []);
void Expect(string expected)
{
    string actual = PasskeyAuthenticators.GetDisplayName(passkey);
    if (actual != expected) { throw new InvalidOperationException($"Expected {expected}, received {actual}"); }
}
passkey.Aaguid = new Guid("08987058-cadc-4b81-b6e1-30de50dcbe96").ToByteArray(bigEndian: true);
Expect("Windows Hello");
passkey.Aaguid = new Guid("32aa82d8-9ea2-42bd-aabc-52e970752036").ToByteArray(bigEndian: true);
Expect("Beacon loan key");
passkey.Name = "Keeper's preferred name";
Expect("Keeper's preferred name");
passkey.Name = null;
foreach (byte[]? value in new byte[]?[] { null, [], [1, 2], new byte[16] })
{
    passkey.Aaguid = value;
    Expect("Unnamed passkey");
    if (PasskeyAuthenticators.TryGetDefaultDisplayName(passkey, out _)) { throw new InvalidOperationException("Unknown authenticator matched."); }
}
Console.WriteLine("PASS actual SDK template: built-in mapping, added fleet mapping, explicit name priority, unknown/malformed rename fallback signal. No physical authenticator test.");
'@ | Set-Content -LiteralPath (Join-Path $work 'Program.cs')
  Push-Location -LiteralPath $work
  try { & $Dotnet run -c Release } finally { Pop-Location }
  Write-Host "Template archive SHA256: $((Get-FileHash -LiteralPath $archivePath -Algorithm SHA256).Hash.ToLowerInvariant())"
} finally {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previous
  foreach ($scope in $before.Keys) {
    if ($before[$scope] -cne [Environment]::GetEnvironmentVariable('PATH', $scope)) { throw "Unexpected $scope PATH change; inspect shared settings." }
  }
}
