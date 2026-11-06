#Requires -Version 7.4
[CmdletBinding()]
param([switch]$Record)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
. (Join-Path $PSScriptRoot 'Evidence.ps1')
$manifest = Get-Content (Join-Path $root 'samples/QuayMaps/sources.json') -Raw | ConvertFrom-Json
$client = [Net.Http.HttpClient]::new()
try {
    foreach ($source in $manifest.sources) {
        $bytes = $client.GetByteArrayAsync($source.url).GetAwaiter().GetResult()
        if ([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant() -cne $source.sha256) { throw "Changed map source: $($source.url)" }
        $body = [Text.Encoding]::UTF8.GetString($bytes)
        foreach ($token in $source.tokens) { if (-not $body.Contains($token)) { throw "Missing map contract: $token" } }
        if ($source.PSObject.Properties['absentTokens']) {
            foreach ($token in $source.absentTokens) { if ($body.Contains($token)) { throw "Unexpected older map contract: $token" } }
        }
    }
    foreach ($artifact in $manifest.artifacts) {
        $bytes = $client.GetByteArrayAsync($artifact.url).GetAwaiter().GetResult()
        if ([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant() -cne $artifact.sha256) { throw "Changed map artifact: $($artifact.url)" }
        $zip = [IO.Compression.ZipArchive]::new([IO.MemoryStream]::new($bytes))
        try {
            foreach ($asset in $artifact.assets) { if (-not $zip.GetEntry($asset)) { throw "Missing map asset: $asset" } }
            $reader = [IO.StreamReader]::new(($zip.Entries | Where-Object FullName -Like '*.nuspec' | Select-Object -First 1).Open())
            try { [xml]$nuspec = $reader.ReadToEnd() } finally { $reader.Dispose() }
            if ($nuspec.package.metadata.repository.commit -cne $artifact.repositoryCommit) { throw "Unexpected map artifact source: $($artifact.url)" }
            foreach ($check in $artifact.apiChecks) {
                $stream = [IO.MemoryStream]::new()
                $inputStream = $zip.GetEntry($check.asset).Open()
                try { $inputStream.CopyTo($stream) } finally { $inputStream.Dispose() }
                $stream.Position = 0
                $pe = [Reflection.PortableExecutable.PEReader]::new($stream)
                try {
                    $metadata = [Reflection.Metadata.PEReaderExtensions]::GetMetadataReader($pe)
                    $methods = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
                    foreach ($handle in $metadata.TypeDefinitions) {
                        $type = $metadata.GetTypeDefinition($handle)
                        $name = $metadata.GetString($type.Namespace) + '.' + $metadata.GetString($type.Name)
                        foreach ($methodHandle in $type.GetMethods()) {
                            $null = $methods.Add($name + '::' + $metadata.GetString($metadata.GetMethodDefinition($methodHandle).Name))
                        }
                    }
                    foreach ($method in $check.presentMethods) { if (-not $methods.Contains($method)) { throw "Missing actual map API: $method" } }
                    foreach ($method in $check.absentMethods) { if ($methods.Contains($method)) { throw "Unexpected older map API: $method" } }
                } finally { $pe.Dispose(); $stream.Dispose() }
            }
        }
        finally { $zip.Dispose() }
    }
    if ($Record) {
        foreach ($edition in 'net10.0','net11.0') {
            foreach ($name in 'QuayAtlas','QuayWindows') {
                if ($edition -eq 'net10.0' -and $name -eq 'QuayWindows') { continue }
                $sample = "samples/$edition/$name/$name.csproj"
                $directory = Join-Path $root "samples/$edition/$name"
                $pin = Get-Content (Join-Path $directory 'global.json') -Raw | ConvertFrom-Json
                $target = ([xml](Get-Content (Join-Path $root $sample) -Raw)).Project.PropertyGroup.TargetFramework
                $receipt = [ordered]@{
                    kind='official-review'; sample=$sample; target=$target; sdk=$pin.sdk.version; language=$(if ($edition -eq 'net10.0') {'14.0'} else {'15.0'})
                    command='eng/Test-QuayMaps.ps1 -Record'; scope='Pinned source/artifact review; native compilation, rendering and service execution not tested.'
                    recordedAt=[DateTimeOffset]::UtcNow.ToString('O'); sourceCommit=(& 'C:/Program Files/Git/cmd/git.exe' -C $root rev-parse HEAD).Trim()
                    sourceDirty=[bool](& 'C:/Program Files/Git/cmd/git.exe' -C $root status --porcelain -- samples/QuayMaps "samples/$edition/$name" eng/Test-QuayMaps.ps1)
                    inputs=(Get-EvidenceInputs $root $sample); dependencies=@(); workloads=@(); nativeToolchains=@()
                    host=[Runtime.InteropServices.RuntimeInformation]::OSDescription
                    sdkInfo='Intended canonical SDK; separate QuayContracts compilation checks the linked source against real Windows assets.'
                    workloadInfo='QuayAtlas: Mac/Xcode unavailable. QuayWindows11: actual RC3 Windows MAUI build fails NETSDK1147 (maui-tizen).'
                    output='Official source hashes, contract tokens and exact package assets passed. Native actions excluded under PIN-699; PIN-935 service proof has not run.'
                }
                [IO.File]::WriteAllText((Join-Path $directory 'verification-evidence.json'),(ConvertTo-Json $receipt -Depth 30).Replace("`r`n","`n")+"`n")
            }
        }
    }
    Write-Host 'PASS: exact Quay map source and package assets; no map rendering or service execution claimed.'
} finally { $client.Dispose() }
