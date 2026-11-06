#Requires -Version 7.4
[CmdletBinding()]
param([switch]$Record)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
. (Join-Path $PSScriptRoot 'Evidence.ps1')
$client = [Net.Http.HttpClient]::new()
try {
    foreach ($source in (Get-Content "$root/samples/Breakwater/sources.json" -Raw | ConvertFrom-Json)) {
        $bytes = $client.GetByteArrayAsync($source.url).GetAwaiter().GetResult()
        if ([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant() -cne $source.sha256) { throw "Changed source: $($source.url)" }
        $body = [Text.Encoding]::UTF8.GetString($bytes)
        foreach ($text in $source.assertions) {
            if (-not $body.Contains($text)) { throw "Missing official contract: $text" }
        }
    }
    $scratch = Join-Path $root 'artifacts/breakwater'
    $java = 'C:/Program Files (x86)/Android/openjdk/jdk-21.0.8/bin'
    $r8 = 'C:/Program Files/dotnet/packs/Microsoft.Android.Sdk.Windows/36.1.118/tools/r8.jar'
    $null = New-Item -ItemType Directory "$scratch/classes" -Force
    & "$java/javac.exe" -d "$scratch/classes" "$root/samples/net10.0/BreakwaterNames/Java/Beacon.java"
    & "$java/jar.exe" --create --file "$scratch/input.jar" -C "$scratch/classes" .
    & "$java/java.exe" -cp $r8 com.android.tools.r8.R8 --release --classfile --lib (Split-Path $java) --pg-conf "$root/samples/net10.0/BreakwaterNames/beacon.pro" --output "$scratch/kept.jar" "$scratch/input.jar"
    $inspection = (& "$java/javap.exe" -private -classpath "$scratch/kept.jar" org.breakwater.names.Beacon | Out-String)
    if ($inspection -notmatch 'private java.lang.String signal\(\)' -or $inspection -match 'unused\(') { throw 'R8 failed to preserve the named entry point or remove unused code.' }
    Write-Host $inspection
    if ($Record) {
        foreach ($name in 'BreakwaterRequests','BreakwaterNames') {
            $sample = "samples/net11.0/$name/$name.csproj"
            $receipt = [ordered]@{
                kind='official-review'; sample=$sample; target='net11.0-android37.2'
                sdk='11.0.100-rc.3.26522.148'; language='15.0'
                command='eng/Test-BreakwaterGuidance.ps1 -Record'
                scope='Pinned Android RC2 handler implementation and R8 build policy. Intended SDK/workload composition; native compilation and device execution not-tested under PIN-699.'
                recordedAt=[DateTimeOffset]::UtcNow.ToString('O')
                sourceCommit=(& 'C:/Program Files/Git/cmd/git.exe' -C $root rev-parse HEAD).Trim()
                sourceDirty=[bool](& 'C:/Program Files/Git/cmd/git.exe' -C $root status --porcelain -- "samples/net11.0/$name" samples/Breakwater)
                inputs=(Get-EvidenceInputs $root $sample); dependencies=@(); workloads=@(); nativeToolchains=@()
                host=[Runtime.InteropServices.RuntimeInformation]::OSDescription
                sdkInfo='Intended RC3 SDK; build attempted separately.'
                workloadInfo='Actual build: NETSDK1147 android workload unavailable.'
                output='PASS exact source hashes and contract assertions. No Android runtime or GA proof.'
            }
            [IO.File]::WriteAllText("$root/samples/net11.0/$name/verification-evidence.json",(ConvertTo-Json $receipt -Depth 30)+"`n")
        }
        $sample = 'samples/net10.0/BreakwaterNames/BreakwaterNames.csproj'
        $receipt.kind='execution'; $receipt.sample=$sample
        $receipt.target='net10.0-android36.0'; $receipt.sdk='10.0.402'; $receipt.language='14.0'
        $receipt.inputs=Get-EvidenceInputs $root $sample
        $manifest=Get-Content "$root/samples/net10.0/BreakwaterNames/verification.json" -Raw | ConvertFrom-Json
        $receipt.dependencies=@($manifest.dependencies); $receipt.workloads=@($manifest.workloads); $receipt.nativeToolchains=@($manifest.nativeToolchains)
        $receipt.scope='JDK21 classfile compilation and real R8 optimization of canonical Beacon.java; javap confirms signal retained and unused removed. Not Android DEX/JNI execution or .NET11 workload proof.'
        $receipt.sdkInfo=(& "$java/java.exe" -version 2>&1 | Out-String)
        $receipt.workloadInfo="R8 SHA256=$((Get-FileHash $r8).Hash.ToLowerInvariant()); Java input SHA256=$((Get-FileHash "$scratch/input.jar").Hash.ToLowerInvariant()); optimized output SHA256=$((Get-FileHash "$scratch/kept.jar").Hash.ToLowerInvariant())"
        $receipt.output=$inspection
        [IO.File]::WriteAllText("$root/samples/net10.0/BreakwaterNames/r8.evidence.json",(ConvertTo-Json $receipt -Depth 30)+"`n")
    }
    Write-Host 'PASS: pinned HTTP and R8 source contracts; device behavior remains not-tested.'
} finally { $client.Dispose() }
