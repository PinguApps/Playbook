#Requires -Version 7.4
[CmdletBinding()]
param([switch]$Record)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
. (Join-Path $PSScriptRoot 'Evidence.ps1')
$client = [Net.Http.HttpClient]::new()
try {
    foreach ($file in 'sources.json','artifacts.json') {
        foreach ($item in (Get-Content (Join-Path $root "samples/CoastalProbe/$file") -Raw | ConvertFrom-Json)) {
            $bytes = $client.GetByteArrayAsync($item.url).GetAwaiter().GetResult()
            $hash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant()
            if ($hash -cne $item.sha256) { throw "Changed Android input: $($item.url)" }
            if ($file -eq 'artifacts.json' -and $item.id -eq 'Microsoft.Android.Ref.37.2') {
                $zip = [IO.Compression.ZipArchive]::new([IO.MemoryStream]::new($bytes))
                try {
                    $entry = $zip.GetEntry('ref/net11.0/Mono.Android.dll')
                    if (-not $entry) { throw 'Missing actual Android reference assembly' }
                    $assembly = [IO.MemoryStream]::new()
                    $stream = $entry.Open()
                    try { $stream.CopyTo($assembly) } finally { $stream.Dispose() }
                    $assembly.Position = 0
                    $pe = [Reflection.PortableExecutable.PEReader]::new($assembly)
                    try {
                        $reader = [Reflection.Metadata.PEReaderExtensions]::GetMetadataReader($pe)
                        $obsolete = $false
                        foreach ($handle in $reader.TypeDefinitions) {
                            $type = $reader.GetTypeDefinition($handle)
                            if ($reader.GetString($type.Namespace) -cne 'Java.Lang' -or $reader.GetString($type.Name) -cne 'Object') { continue }
                            foreach ($methodHandle in $type.GetMethods()) {
                                $method = $reader.GetMethodDefinition($methodHandle)
                                if ($reader.GetString($method.Name) -cne 'JavaFinalize') { continue }
                                foreach ($attributeHandle in $method.GetCustomAttributes()) {
                                    $attribute = $reader.GetCustomAttribute($attributeHandle)
                                    if ($attribute.Constructor.Kind -ne 'MemberReference') { continue }
                                    $constructor = $reader.GetMemberReference([Reflection.Metadata.MemberReferenceHandle]$attribute.Constructor)
                                    if ($constructor.Parent.Kind -ne 'TypeReference') { continue }
                                    $attributeType = $reader.GetTypeReference([Reflection.Metadata.TypeReferenceHandle]$constructor.Parent)
                                    if ($reader.GetString($attributeType.Namespace) -cne 'System' -or $reader.GetString($attributeType.Name) -cne 'ObsoleteAttribute') { continue }
                                    $blob = $reader.GetBlobReader($attribute.Value)
                                    if ($blob.ReadUInt16() -ne 1) { throw 'Invalid attribute prolog' }
                                    $message = $blob.ReadSerializedString()
                                    if (-not $message.Contains('Dispose (bool disposing)')) { throw 'Changed JavaFinalize replacement' }
                                    $obsolete = $true
                                }
                            }
                        }
                        if (-not $obsolete) { throw 'Missing actual JavaFinalize obsolete contract' }
                    } finally { $pe.Dispose(); $assembly.Dispose() }
                } finally { $zip.Dispose() }
            }
        }
    }
    if ($Record) {
        $sample = 'samples/net11.0/CoastalProbe/CoastalProbe.csproj'
        $receipt = [ordered]@{
            kind='official-review'; sample=$sample; target='net11.0-android37.2'
            sdk='11.0.100-rc.3.26522.148'; language='15.0'
            command='eng/Test-CoastalProbe.ps1 -Record'
            scope='Exact pinned sources and Android RC2 manifest/reference artifact hashes; actual JavaFinalize obsolete metadata. Native compilation/execution not-tested under PIN-699.'
            recordedAt=[DateTimeOffset]::UtcNow.ToString('O')
            sourceCommit=(& 'C:/Program Files/Git/cmd/git.exe' -C $root rev-parse HEAD).Trim()
            sourceDirty=[bool](& 'C:/Program Files/Git/cmd/git.exe' -C $root status --porcelain -- samples/CoastalProbe samples/net11.0/CoastalProbe eng/Test-CoastalProbe.ps1)
            inputs=(Get-EvidenceInputs $root $sample); dependencies=@(); workloads=@(); nativeToolchains=@()
            host=[Runtime.InteropServices.RuntimeInformation]::OSDescription
            sdkInfo='RC3 SDK selected for actual attempted build. RC2 SDK installation lacks runtimeconfig; no SDK files were modified.'
            workloadInfo='RC3 native build failed NETSDK1147: android workload unavailable. Android NDK/device execution unavailable; optional native library not built.'
            output='PASS pinned sources, package hashes and actual reference metadata. No current native generation, JNI order, device callback or GA proof.'
        }
        [IO.File]::WriteAllText((Join-Path $root 'samples/net11.0/CoastalProbe/verification-evidence.json'),(ConvertTo-Json $receipt -Depth 30)+"`n")
    }
    Write-Host 'PASS: exact CoastalProbe sources, RC2 artifacts and JavaFinalize metadata; native execution excluded.'
} finally { $client.Dispose() }
