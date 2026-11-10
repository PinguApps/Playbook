#Requires -Version 7.4
[CmdletBinding()]
param([switch]$Record)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
. (Join-Path $PSScriptRoot 'Evidence.ps1')
$manifest = Get-Content (Join-Path $root 'samples/QuayIntegration/sources.json') -Raw | ConvertFrom-Json
$client = [Net.Http.HttpClient]::new()
try {
    foreach ($source in @($manifest.sources) + @($manifest.packages)) {
        $bytes = $client.GetByteArrayAsync($source.url).GetAwaiter().GetResult()
        $hash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant()
        if ($hash -cne $source.sha256) { throw "Changed official input: $($source.url)" }
    }
    if ($Record) {
        $sample = 'samples/net11.0/QuayCSharp/QuayCSharp.csproj'
        $receipt = [ordered]@{
            kind='official-review'; sample=$sample; target='net11.0-windows10.0.20348.0'; sdk='11.0.100'; language='15.0'
            command='eng/Test-QuayContracts.ps1 -Record'
            scope='Pinned official source and artifact review. Actual native build failed NETSDK1147 before compilation; not-tested under PIN-699.'
            recordedAt=[DateTimeOffset]::UtcNow.ToString('O'); sourceCommit=(& 'C:/Program Files/Git/cmd/git.exe' -C $root rev-parse HEAD).Trim()
            sourceDirty=[bool](& 'C:/Program Files/Git/cmd/git.exe' -C $root status --porcelain -- samples eng/Test-QuayContracts.ps1)
            inputs=(Get-EvidenceInputs $root $sample); dependencies=@(); workloads=@(); nativeToolchains=@()
            host=[Runtime.InteropServices.RuntimeInformation]::OSDescription
            sdkInfo='Intended SDK11 pin; actual SDK present, required native MAUI SDK pack absent.'
            workloadInfo='NETSDK1147 maui-tizen, actual net11 Windows project build attempted.'
            output='Source/artifact hashes passed. No native compilation, rendering or interaction claimed.'
        }
        [IO.File]::WriteAllText((Join-Path $root 'samples/net11.0/QuayCSharp/verification-evidence.json'),(ConvertTo-Json $receipt -Depth 30).Replace("`r`n","`n")+"`n")
    }
    Write-Host 'PASS: pinned Quay source and package identities. No native execution claimed.'
} finally { $client.Dispose() }
