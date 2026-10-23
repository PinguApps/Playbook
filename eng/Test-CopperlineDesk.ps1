[CmdletBinding()]
param(
  [Parameter(Mandatory)][ValidateSet(10,11)][int]$Version,
  [Parameter(Mandatory)][ValidateSet('Production','Review')][string]$Environment,
  [Parameter(Mandatory)][uri]$HostUri,
  [string]$OutputPath
)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$client=[Net.Http.HttpClient]::new()
function Hash([byte[]]$Bytes){[Convert]::ToHexStringLower([Security.Cryptography.SHA256]::HashData($Bytes))}
try {
  $url=[uri]::new($HostUri,'server')
  $bytes=$client.GetByteArrayAsync($url).GetAwaiter().GetResult()
  $html=[Text.Encoding]::UTF8.GetString($bytes)
  $actual=@([regex]::Matches($html,'data-chrome="([^"]+)"')|ForEach-Object{$_.Groups[1].Value})
  $expected=if($Environment -eq 'Review'){@('identity','review','rehearse','whitespace','empty-list')}else{@('identity','receipts','whitespace','empty-list')}
  if(($actual -join ',') -cne ($expected -join ',')){throw "Wrong server chrome: $actual"}
  if(-not $html.Contains("data-environment=`"$Environment`"") -or -not $html.Contains('Hidden chrome is not authorization.')){throw 'Wrong server environment or missing boundary'}
  # This response must contain actual rendered chrome, not a browser-only marker.
  if(-not $html.Contains('data-renderer="server"')){throw 'Not the server renderer'}
  $browserHtml=$client.GetStringAsync([uri]::new($HostUri,'client?browserEnvironment=Review')).GetAwaiter().GetResult()
  if($browserHtml.Contains('data-chrome=')){throw 'Client route unexpectedly prerendered chrome'}
  $artifact="samples/net$Version.0/CopperlineDesk/Host/bin/Release/net$Version.0/Host.dll"
  $record=@{kind='real-loopback-http-static-ssr';url="$url";environment=$Environment;actual=$actual;expected=$expected;response=@{sha256=(Hash $bytes);bytes=$bytes.Length;html=$html};hostArtifact=@{path=$artifact;sha256=(Get-FileHash (Join-Path $root $artifact)).Hash.ToLowerInvariant()};browserResponse='No prerendered chrome; actual browser execution is a separate record';recordedAt=[DateTimeOffset]::UtcNow.ToString('O');verifierSha256=(Get-FileHash $PSCommandPath).Hash.ToLowerInvariant()}
  if($OutputPath){[IO.File]::WriteAllText([IO.Path]::GetFullPath($OutputPath),($record|ConvertTo-Json -Depth 12).Replace("`r`n","`n")+"`n")}
  Write-Host "PASS real .NET $Version server ${Environment}: $($actual -join ', ')"
} finally {$client.Dispose()}
