[CmdletBinding()]
param(
  [ValidateSet(10, 11)][int]$Version,
  [Parameter(Mandatory)][uri]$HostUri,
  [Parameter(Mandatory)][uri]$LegacyUri,
  [string]$OutputPath
)

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$sample = Join-Path $root "samples/net$Version.0/ParcelStartup"
$assets = Get-Content (Join-Path $sample 'Host/obj/project.assets.json') -Raw | ConvertFrom-Json -AsHashtable
$packageRoot = @($assets.packageFolders.Keys)[0]
$assetVersion = if ($Version -eq 10) { '10.0.13' } else { '11.0.0-rc.2.26504.105' }
$client = [Net.Http.HttpClient]::new()
function Hash-Bytes([byte[]]$Bytes) { [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($Bytes)).ToLowerInvariant() }
try {
  $responses = foreach ($entry in @(
    @{ base = $HostUri; path = '_framework/blazor.server.js' },
    @{ base = $HostUri; path = '_framework/blazor.web.js' },
    @{ base = $LegacyUri; path = '_framework/blazor.webassembly.js' }
  )) {
    $url = [uri]::new($entry.base, $entry.path)
    $bytes = $client.GetByteArrayAsync($url).GetAwaiter().GetResult()
    $expectedPath = Join-Path $packageRoot "microsoft.aspnetcore.app.internal.assets/$assetVersion/$($entry.path)"
    if ($Version -eq 11) { $expectedPath = Join-Path $packageRoot "microsoft.aspnetcore.app.internal.assets/$assetVersion/staticwebassets/$($entry.path)" }
    if ($Version -eq 11 -and $entry.base -eq $LegacyUri) { $expectedPath = Join-Path $packageRoot "microsoft.aspnetcore.components.webassembly/$assetVersion/staticwebassets/$($entry.path)" }
    $expected = (Get-FileHash $expectedPath -Algorithm SHA256).Hash.ToLowerInvariant()
    $actual = Hash-Bytes $bytes
    if ($actual -cne $expected) { throw "Served script does not match declared assets: $url" }
    @{ url = "$url"; asset = "$expectedPath"; sha256 = $actual; bytes = $bytes.Length }
  }
  foreach ($route in @('legacy-server', 'web-server', 'web-wasm')) {
    $html = $client.GetStringAsync([uri]::new($HostUri, $route)).GetAwaiter().GetResult()
    $script = if ($route -eq 'legacy-server') { 'blazor.server.js' } else { 'blazor.web.js' }
    if ($html -notmatch "(?s)$([regex]::Escape($script))\x22 autostart=\x22false\x22.*?<script src=\x22startup.js\x22") { throw "Wrong boot order/autostart: $route" }
  }
  foreach ($entry in @(@{base=$HostUri;part='Host'},@{base=$LegacyUri;part='LegacyClient'})) {
    $bytes = $client.GetByteArrayAsync([uri]::new($entry.base,'startup.js')).GetAwaiter().GetResult()
    $expected = (Get-FileHash (Join-Path $sample "$($entry.part)/wwwroot/startup.js")).Hash.ToLowerInvariant()
    if ((Hash-Bytes $bytes) -cne $expected) { throw 'Served startup JavaScript differs from canonical source.' }
  }
  $report = @{kind='real-loopback-http'; recordedAt=[DateTimeOffset]::UtcNow.ToString('O'); version=$Version; scripts=@($responses); documents='Legacy Server and both Web App pages: correct script, autostart=false, startup.js follows'; startup='Both served copies match canonical SHA-256'}
  if ($OutputPath) { [IO.File]::WriteAllText([IO.Path]::GetFullPath($OutputPath), ($report | ConvertTo-Json -Depth 12).Replace("`r`n","`n")+"`n") }
  Write-Host "PASS .NET ${Version}: actual matching script bytes, canonical startup and document boot order."
}
finally { $client.Dispose() }
