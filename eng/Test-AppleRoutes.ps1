#Requires -Version 7.4
[CmdletBinding()]
param([switch]$Record, [switch]$CompileReference)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
. (Join-Path $PSScriptRoot 'Evidence.ps1')
$client = [Net.Http.HttpClient]::new()
try {
    foreach ($source in (Get-Content "$root/samples/AppleRoutes/sources.json" -Raw | ConvertFrom-Json)) {
        $bytes = $client.GetByteArrayAsync($source.url).GetAwaiter().GetResult()
        if ([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant() -cne $source.sha256) { throw "Changed official source: $($source.url)" }
    }
    if ($CompileReference) {
        $artifact = Get-Content "$root/samples/AppleRoutes/artifact.json" -Raw | ConvertFrom-Json
        $bytes = $client.GetByteArrayAsync($artifact.url).GetAwaiter().GetResult()
        if ([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant() -cne $artifact.sha256) { throw 'Changed iOS reference artifact' }
        $scratch = Join-Path $root 'artifacts/apple-reference'
        $null = New-Item -ItemType Directory $scratch -Force
        [IO.File]::WriteAllBytes("$scratch/reference.zip", $bytes)
        Expand-Archive "$scratch/reference.zip" "$scratch/reference" -Force
        $dotnetRoot = 'C:/Users/pingu/AppData/Local/dotnetup/dotnet'
        $references = @(Get-ChildItem "$dotnetRoot/packs/Microsoft.NETCore.App.Ref/11.0.0-rc.3.26522.148/ref/net11.0/*.dll") + @(Get-Item "$scratch/reference/ref/net11.0/Microsoft.iOS.dll")
        [IO.File]::WriteAllText("$scratch/GlobalUsings.cs", 'global using System;')
        $arguments = @('/noconfig','/nostdlib+','/target:library','/nullable:enable','/langversion:15.0',"/out:$scratch/TernProxy.dll") + @($references | ForEach-Object { '/reference:' + $_.FullName }) + @("$root/samples/net11.0/TernProxy/Route.cs", "$scratch/GlobalUsings.cs")
        & "$dotnetRoot/dotnet.exe" "$dotnetRoot/sdk/11.0.100-rc.3.26522.148/Roslyn/bincore/csc.dll" @arguments
        if ($LASTEXITCODE -ne 0) { throw 'Canonical proxy reference compilation failed' }
        Write-Host 'PASS canonical proxy C# reference compilation; no Apple native build or execution.'
    }
    if ($Record) {
        foreach ($name in 'TernPublish','TernProxy') {
            $sample = "samples/net11.0/$name/$name.csproj"
            $receipt = [ordered]@{
                kind='official-review'; sample=$sample
                target=if($name -eq 'TernPublish'){'net11.0-maccatalyst27.0'}else{'net11.0-ios27.0'}
                sdk='11.0.100-rc.3.26522.148'; language='15.0'
                command='eng/Test-AppleRoutes.ps1 -Record'
                scope='Exact RC2 Apple handler/build source review. Native compilation and execution not-tested under PIN-699; no native publish output or endpoint observation.'
                recordedAt=[DateTimeOffset]::UtcNow.ToString('O')
                sourceCommit=(& 'C:/Program Files/Git/cmd/git.exe' -C $root rev-parse HEAD).Trim()
                sourceDirty=[bool](& 'C:/Program Files/Git/cmd/git.exe' -C $root status --porcelain -- samples/AppleRoutes "samples/net11.0/$name" eng/Test-AppleRoutes.ps1)
                inputs=(Get-EvidenceInputs $root $sample); dependencies=@(); workloads=@(); nativeToolchains=@()
                host=[Runtime.InteropServices.RuntimeInformation]::OSDescription
                sdkInfo='Intended RC3 SDK pin; no compatible current Apple workload installed.'
                workloadInfo='Apple workload/Xcode execution unavailable.'
                output='PASS exact official-source hashes. Source review is not compilation or execution.'
            }
            [IO.File]::WriteAllText("$root/samples/net11.0/$name/verification-evidence.json",(ConvertTo-Json $receipt -Depth 30)+"`n")
        }
    }
    Write-Host 'PASS exact Apple RC2 handler/build sources; native behaviour not-tested.'
} finally { $client.Dispose() }
