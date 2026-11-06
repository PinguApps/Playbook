#Requires -Version 7.4
[CmdletBinding()]
param(
    [switch]$Record,
    [switch]$CompileReference,
    [string]$Dotnet = (Get-Command dotnet -CommandType Application | Select-Object -First 1).Source,
    [string]$Git = (Get-Command git -CommandType Application | Select-Object -First 1).Source
)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
. (Join-Path $PSScriptRoot 'Evidence.ps1')
$client = [Net.Http.HttpClient]::new()
try {
    foreach ($source in (Get-Content "$root/samples/AppleRoutes/sources.json" -Raw | ConvertFrom-Json)) {
        $bytes = $client.GetByteArrayAsync($source.url).GetAwaiter().GetResult()
        if ([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant() -cne $source.sha256) { throw "Changed official source: $($source.url)" }
        $text = if ($source.checks) { [Text.Encoding]::UTF8.GetString($bytes) } else { '' }
        foreach ($check in $source.checks) {
            if (-not $text.Contains($check)) { throw "Missing source contract '$check': $($source.url)" }
        }
    }
    if ($CompileReference) {
        $artifact = Get-Content "$root/samples/AppleRoutes/artifact.json" -Raw | ConvertFrom-Json
        $bytes = $client.GetByteArrayAsync($artifact.url).GetAwaiter().GetResult()
        if ([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant() -cne $artifact.sha256) { throw 'Changed iOS reference artifact' }
        $scratch = Join-Path $root 'artifacts/apple-reference'
        $null = New-Item -ItemType Directory $scratch -Force
        [IO.File]::WriteAllBytes("$scratch/reference.zip", $bytes)
        Expand-Archive "$scratch/reference.zip" "$scratch/reference" -Force
        $dotnetRoot = Split-Path (Get-Item -LiteralPath $Dotnet).FullName -Parent
        $compiler = "$dotnetRoot/sdk/11.0.100-rc.3.26522.148/Roslyn/bincore/csc.dll"
        $referenceDirectory = "$dotnetRoot/packs/Microsoft.NETCore.App.Ref/11.0.0-rc.3.26522.148/ref/net11.0"
        if (-not (Test-Path $compiler) -or -not (Test-Path $referenceDirectory)) {
            throw 'Reference compilation requires the pinned RC3 SDK and reference pack; pass -Dotnet with that installation executable.'
        }
        $references = @(Get-ChildItem "$dotnetRoot/packs/Microsoft.NETCore.App.Ref/11.0.0-rc.3.26522.148/ref/net11.0/*.dll") + @(Get-Item "$scratch/reference/ref/net11.0/Microsoft.iOS.dll")
        [IO.File]::WriteAllText("$scratch/GlobalUsings.cs", 'global using System;')
        $arguments = @('/noconfig','/nostdlib+','/target:library','/nullable:enable','/langversion:15.0',"/out:$scratch/TernProxy.dll") + @($references | ForEach-Object { '/reference:' + $_.FullName }) + @("$root/samples/net11.0/TernProxy/Route.cs", "$scratch/GlobalUsings.cs")
        & $Dotnet $compiler @arguments
        if ($LASTEXITCODE -ne 0) { throw 'Canonical proxy reference compilation failed' }
        Write-Host 'PASS canonical proxy C# reference compilation; no Apple native build or execution.'
        $catalyst = Get-Content "$root/samples/AppleRoutes/sources.json" -Raw | ConvertFrom-Json | Where-Object asset
        $bytes = $client.GetByteArrayAsync($catalyst.url).GetAwaiter().GetResult()
        if ([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant() -cne $catalyst.sha256) { throw 'Changed Catalyst reference artifact' }
        [IO.File]::WriteAllBytes("$scratch/catalyst.zip", $bytes)
        Expand-Archive "$scratch/catalyst.zip" "$scratch/catalyst" -Force
        $references = @(Get-ChildItem "$referenceDirectory/*.dll") + @(Get-Item "$scratch/catalyst/$($catalyst.asset)")
        $arguments = @('/noconfig','/nostdlib+','/target:library','/nullable:enable','/langversion:15.0',"/out:$scratch/TernPublish.dll") + @($references | ForEach-Object { '/reference:' + $_.FullName }) + @(Get-ChildItem "$root/samples/net11.0/TernPublish/*.cs" | ForEach-Object FullName) + @("$scratch/GlobalUsings.cs")
        & $Dotnet $compiler @arguments
        if ($LASTEXITCODE -ne 0) { throw 'Canonical Catalyst reference compilation failed' }
        Write-Host 'PASS canonical Catalyst C# reference compilation; no native publish or execution.'
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
                sourceCommit=(& $Git -C $root rev-parse HEAD).Trim()
                sourceDirty=[bool](& $Git -C $root status --porcelain -- samples/AppleRoutes "samples/net11.0/$name" eng/Test-AppleRoutes.ps1)
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
