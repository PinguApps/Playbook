#Requires -Version 7.4
[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$client = [Net.Http.HttpClient]::new()
try {
    foreach ($source in (Get-Content (Join-Path $root 'samples/KittiwakeExchange/sources.json') -Raw | ConvertFrom-Json)) {
        $bytes = $client.GetByteArrayAsync($source.url).GetAwaiter().GetResult()
        if ([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant() -cne $source.sha256) {
            throw "Changed Kittiwake source: $($source.url)"
        }
    }
    Write-Host 'PASS: immutable GA share/drag source identities; native interactions not-tested.'
} finally { $client.Dispose() }
