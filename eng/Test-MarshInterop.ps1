#Requires -Version 7.4
[CmdletBinding()]
param([switch]$Record)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
. (Join-Path $PSScriptRoot 'Evidence.ps1')
$manifest = Get-Content (Join-Path $root 'samples/MarshInterop/sources.json') -Raw | ConvertFrom-Json
$client = [Net.Http.HttpClient]::new()
try {
    $package = $client.GetByteArrayAsync('https://api.nuget.org/v3-flatcontainer/microsoft.maui.core/11.0.0-rc.2.26505.5/microsoft.maui.core.11.0.0-rc.2.26505.5.nupkg').GetAwaiter().GetResult()
    if ([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($package)).ToLowerInvariant() -cne '47915931c89820f3623d287c46763308b8eb730f74c731c872eb666c62b300bc') { throw 'MAUI Core artifact changed.' }
    $zip = [IO.Compression.ZipArchive]::new([IO.MemoryStream]::new($package))
    try {
        $stream = [IO.MemoryStream]::new()
        $inputStream = $zip.GetEntry('lib/net11.0-android37.0/Microsoft.Maui.dll').Open()
        try { $inputStream.CopyTo($stream) } finally { $inputStream.Dispose() }
        $stream.Position = 0
        $pe = [Reflection.PortableExecutable.PEReader]::new($stream)
        try {
            $metadata = [Reflection.Metadata.PEReaderExtensions]::GetMetadataReader($pe)
            $names = foreach ($handle in $metadata.TypeDefinitions) {
                $type = $metadata.GetTypeDefinition($handle)
                if (($type.Attributes -band [Reflection.TypeAttributes]::VisibilityMask) -eq [Reflection.TypeAttributes]::Public) { $metadata.GetString($type.Namespace) + '.' + $metadata.GetString($type.Name) }
            }
            foreach ($name in 'Microsoft.Maui.Handlers.EntryHandler2','Microsoft.Maui.Platform.MauiMaterialEditText','Microsoft.Maui.Platform.MauiMaterialTextInputLayout') {
                if ($name -cnotin $names) { throw "Missing public Android API: $name" }
            }
        } finally { $pe.Dispose(); $stream.Dispose() }
    } finally { $zip.Dispose() }
    foreach ($source in $manifest.sources) {
        $bytes = $client.GetByteArrayAsync($source.url).GetAwaiter().GetResult()
        if ([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant() -cne $source.sha256) { throw "Changed official source: $($source.url)" }
        $text = [Text.Encoding]::UTF8.GetString($bytes)
        foreach ($token in $source.tokens) { if (-not $text.Contains($token)) { throw "Missing official contract: $token" } }
    }
    if ($Record) {
        foreach ($edition in 'net10.0','net11.0') {
            foreach ($name in 'MarshEntry','MarshBridge') {
                if ($edition -eq 'net10.0' -and $name -eq 'MarshEntry') { continue }
                $sample = "samples/$edition/$name/$name.csproj"
                $target = ([xml]([IO.File]::ReadAllText((Join-Path $root $sample)))).Project.PropertyGroup.TargetFramework
                $pin = Get-Content (Join-Path $root "samples/$edition/$name/global.json") -Raw | ConvertFrom-Json
                $receipt = [ordered]@{
                    kind='official-review'; sample=$sample; target=$target; sdk=$pin.sdk.version; language=$(if ($edition -eq 'net10.0') {'14.0'} else {'15.0'})
                    command='eng/Test-MarshInterop.ps1 -Record'; scope='Pinned official source and platform API review only; no native compilation or execution.'
                    recordedAt=[DateTimeOffset]::UtcNow.ToString('O'); sourceCommit=(& 'C:/Program Files/Git/cmd/git.exe' -C $root rev-parse HEAD).Trim()
                    sourceDirty=[bool](& 'C:/Program Files/Git/cmd/git.exe' -C $root status --porcelain -- samples/MarshInterop "samples/$edition/$name" eng/Test-MarshInterop.ps1)
                    inputs=(Get-EvidenceInputs $root $sample); dependencies=@(); workloads=@(); nativeToolchains=@()
                    host=[Runtime.InteropServices.RuntimeInformation]::OSDescription
                    sdkInfo='Intended SDK pin; no native dependency closure resolved.'
                    workloadInfo='Apple requires unavailable Mac/Xcode. Isolated SDK11 reports NETSDK1147: maui-android unavailable; Apple Mac/Xcode unavailable.'
                    output='Pinned official source hashes and required contract tokens passed. Compilation/execution excluded under PIN-699 for these samples.'
                }
                [IO.File]::WriteAllText((Join-Path $root "samples/$edition/$name/verification-evidence.json"),(ConvertTo-Json $receipt -Depth 30).Replace("`r`n","`n")+"`n")
            }
        }
    }
    Write-Host 'PASS: pinned Marsh interop source contracts; no native execution claimed.'
} finally { $client.Dispose() }
