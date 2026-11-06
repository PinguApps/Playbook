#Requires -Version 7.4
[CmdletBinding()]
param([switch]$Record)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
. (Join-Path $PSScriptRoot 'Evidence.ps1')
$client = [Net.Http.HttpClient]::new()
try {
    foreach ($source in (Get-Content (Join-Path $root 'samples/HeronGuidance/sources.json') -Raw | ConvertFrom-Json)) {
        $bytes = $client.GetByteArrayAsync($source.url).GetAwaiter().GetResult()
        if ([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant() -cne $source.sha256) { throw "Changed Heron source: $($source.url)" }
    }
    foreach ($artifact in (Get-Content (Join-Path $root 'samples/HeronGuidance/artifacts.json') -Raw | ConvertFrom-Json)) {
        $bytes = $client.GetByteArrayAsync($artifact.url).GetAwaiter().GetResult()
        if ([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant() -cne $artifact.sha256) { throw "Changed Heron artifact: $($artifact.url)" }
        $zip = [IO.Compression.ZipArchive]::new([IO.MemoryStream]::new($bytes))
        try {
            $nativeAsset = $artifact.assets[0].path.Replace('.xml','.dll')
            $assembly = [IO.MemoryStream]::new()
            $inputStream = $zip.GetEntry($nativeAsset).Open()
            try { $inputStream.CopyTo($assembly) } finally { $inputStream.Dispose() }
            $assembly.Position = 0
            $pe = [Reflection.PortableExecutable.PEReader]::new($assembly)
            try {
                $metadata = [Reflection.Metadata.PEReaderExtensions]::GetMetadataReader($pe)
                $experimental = $false
                foreach ($handle in $metadata.GetAssemblyDefinition().GetCustomAttributes()) {
                    $attribute = $metadata.GetCustomAttribute($handle)
                    if ($attribute.Constructor.Kind -ne 'MemberReference') { continue }
                    $constructor = $metadata.GetMemberReference([Reflection.Metadata.MemberReferenceHandle]$attribute.Constructor)
                    if ($constructor.Parent.Kind -ne 'TypeReference') { continue }
                    $type = $metadata.GetTypeReference([Reflection.Metadata.TypeReferenceHandle]$constructor.Parent)
                    if ($metadata.GetString($type.Namespace) -cne 'System.Diagnostics.CodeAnalysis' -or $metadata.GetString($type.Name) -cne 'ExperimentalAttribute') { continue }
                    $blob = $metadata.GetBlobReader($attribute.Value)
                    if ($blob.ReadUInt16() -ne 1) { throw 'Invalid experimental attribute prolog' }
                    if ($blob.ReadSerializedString() -ceq 'MAUIAI0001') { $experimental = $true }
                }
                if (-not $experimental) { throw "Missing actual MAUIAI0001 attribute: $nativeAsset" }
            } finally { $pe.Dispose(); $assembly.Dispose() }
            foreach ($asset in $artifact.assets) {
                $entry = $zip.GetEntry($asset.path)
                if (-not $entry) { throw "Missing actual asset: $($asset.path)" }
                $reader = [IO.StreamReader]::new($entry.Open())
                try { $body = $reader.ReadToEnd() } finally { $reader.Dispose() }
                foreach ($token in $asset.tokens) { if (-not $body.Contains($token)) { throw "Missing asset contract: $token" } }
            }
        } finally { $zip.Dispose() }
    }
    if ($Record) {
        foreach ($edition in 'net10.0','net11.0') {
            $sample = "samples/$edition/HeronNotes/HeronNotes.csproj"
            $directory = Join-Path $root "samples/$edition/HeronNotes"
            $pin = Get-Content (Join-Path $directory 'global.json') -Raw | ConvertFrom-Json
            $receipt = [ordered]@{
                kind='official-review'; sample=$sample
                target=$(if ($edition -eq 'net10.0') {'net10.0-ios26.0'} else {'net11.0-ios26.5'})
                sdk=$pin.sdk.version; language=$(if ($edition -eq 'net10.0') {'14.0'} else {'15.0'})
                command='eng/Test-HeronGuidance.ps1 -Record'
                scope='Immutable source and actual Apple package contract review; native compilation and model execution not-tested under PIN-699.'
                recordedAt=[DateTimeOffset]::UtcNow.ToString('O')
                sourceCommit=(& 'C:/Program Files/Git/cmd/git.exe' -C $root rev-parse HEAD).Trim()
                sourceDirty=[bool](& 'C:/Program Files/Git/cmd/git.exe' -C $root status --porcelain -- samples/HeronGuidance "samples/$edition/HeronNotes" eng/Test-HeronGuidance.ps1)
                inputs=(Get-EvidenceInputs $root $sample); dependencies=@(); workloads=@(); nativeToolchains=@()
                host=[Runtime.InteropServices.RuntimeInformation]::OSDescription
                sdkInfo='Intended SDK pin; actual attempted native build selected this SDK but failed before compilation.'
                workloadInfo='Actual native build: NETSDK1147, ios workload unavailable. Mac/Xcode and eligible Apple hardware unavailable.'
                output='PASS exact source and package hashes, native XML constructor/response contracts and MAUIAI0001 documentation. No model generation or native compilation claim.'
            }
            [IO.File]::WriteAllText((Join-Path $directory 'verification-evidence.json'),(ConvertTo-Json $receipt -Depth 30).Replace("`r`n","`n")+"`n")
        }
    }
    Write-Host 'PASS: exact Heron official sources and actual native package contracts; no native ceremony or model execution claimed.'
} finally { $client.Dispose() }
