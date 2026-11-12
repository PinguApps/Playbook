#Requires -Version 7.4
[CmdletBinding()]
param([switch]$Record)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
. (Join-Path $PSScriptRoot 'Evidence.ps1')
$manifest = Get-Content (Join-Path $root 'samples/QuayBoard/sources.json') -Raw | ConvertFrom-Json
$client = [Net.Http.HttpClient]::new()
try {
    foreach ($source in @($manifest.sources) + @($manifest.artifacts)) {
        $bytes = $client.GetByteArrayAsync($source.url).GetAwaiter().GetResult()
        if ([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant() -cne $source.sha256) { throw "Changed XAML input: $($source.url)" }
    }
    $spec = $manifest.sources | Where-Object id -EQ 'expression-spec'
    $body = $client.GetStringAsync($spec.url).GetAwaiter().GetResult()
    if ($body.Contains('MAUIX2012') -or -not $body.Contains('MAUIX2013') -or -not $body.Contains('SourceGen only')) { throw 'Reconcile GA expression consent/limitations' }
    if ($Record) {
        $sample = 'samples/net11.0/QuayBoard/QuayBoard.csproj'
        $path = Join-Path $root 'samples/net11.0/QuayBoard/verification-evidence.json'
        $receipt = Get-Content $path -Raw | ConvertFrom-Json
        $receipt.inputs = Get-EvidenceInputs $root $sample
        $receipt.recordedAt = [DateTimeOffset]::UtcNow.ToString('O')
        $receipt.sourceCommit = (& 'C:/Program Files/Git/cmd/git.exe' -C $root rev-parse HEAD).Trim()
        $receipt.sourceDirty = $true
        $receipt.command = 'eng/Test-XamlExpressionContracts.ps1 -Record'
        $receipt.scope = 'Actual GA source/artifact hash review. No native compilation, generated-output or execution claim.'
        $receipt.output = 'PASS GA source and artifact hashes; GA expression specification retains SourceGen and MAUIX2013, removes MAUIX2012. Native checks remain not-tested under PIN-699.'
        [IO.File]::WriteAllText($path, ($receipt | ConvertTo-Json -Depth 40) + "`n")
    }
    Write-Host 'PASS: GA XAML source/artifact contracts; native checks not-tested.'
} finally { $client.Dispose() }
