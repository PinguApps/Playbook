#Requires -Version 7.4
[CmdletBinding()]
param([switch]$Record)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
. (Join-Path $PSScriptRoot 'Evidence.ps1')
$manifest = Get-Content (Join-Path $root 'samples/JettyHandlers/sources.json') -Raw | ConvertFrom-Json
$client = [Net.Http.HttpClient]::new()
try {
    foreach ($source in $manifest.sources) {
        $bytes = $client.GetByteArrayAsync($source.url).GetAwaiter().GetResult()
        if ([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant() -cne $source.sha256) { throw "Changed official source: $($source.url)" }
        $text = [Text.Encoding]::UTF8.GetString($bytes)
        foreach ($token in $source.tokens) { if (-not $text.Contains($token)) { throw "Missing official contract: $token" } }
    }
    $packageUrl = 'https://api.nuget.org/v3-flatcontainer/microsoft.maui.controls.core/11.0.0-rc.2.26505.5/microsoft.maui.controls.core.11.0.0-rc.2.26505.5.nupkg'
    $package = $client.GetByteArrayAsync($packageUrl).GetAwaiter().GetResult()
    if ([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($package)).ToLowerInvariant() -cne '92787cfc8a46b941b88adefc9966624ec2f183f73bf08c25102b04fff1833fcc') { throw 'Controls.Core RC2 artifact changed.' }
    $zip = [IO.Compression.ZipArchive]::new([IO.MemoryStream]::new($package))
    try {
        foreach ($asset in 'net11.0-ios27.0','net11.0-windows10.0.20348') {
            $entry = $zip.GetEntry("lib/$asset/Microsoft.Maui.Controls.dll")
            if (-not $entry) { throw "Missing platform artifact: $asset" }
            $stream = [IO.MemoryStream]::new()
            $inputStream = $entry.Open()
            try { $inputStream.CopyTo($stream) } finally { $inputStream.Dispose() }
            $stream.Position = 0
            $pe = [Reflection.PortableExecutable.PEReader]::new($stream)
            try {
                $metadata = [Reflection.Metadata.PEReaderExtensions]::GetMetadataReader($pe)
                $names = foreach ($handle in $metadata.TypeDefinitions) {
                    $type = $metadata.GetTypeDefinition($handle)
                    if (($type.Attributes -band [Reflection.TypeAttributes]::VisibilityMask) -eq [Reflection.TypeAttributes]::Public) { $metadata.GetString($type.Namespace) + '.' + $metadata.GetString($type.Name) }
                }
                $required = @('Microsoft.Maui.Controls.Handlers.Items2.CollectionViewHandler2','Microsoft.Maui.Controls.Handlers.Items.CollectionViewHandler')
                if ($asset -like '*ios*') { $required += @('Microsoft.Maui.Controls.Handlers.Items2.CarouselViewHandler2','Microsoft.Maui.Controls.Handlers.Compatibility.NavigationRenderer','Microsoft.Maui.Controls.Handlers.Compatibility.TabbedRenderer','Microsoft.Maui.Controls.Handlers.Compatibility.PhoneFlyoutPageRenderer') }
                foreach ($name in $required) { if ($name -cnotin $names) { throw "Missing public platform contract: $name ($asset)" } }
            } finally { $pe.Dispose(); $stream.Dispose() }
        }
    } finally { $zip.Dispose() }
    if ($Record) {
        foreach ($edition in 'net10.0','net11.0') {
            foreach ($name in 'JettyApple','JettyItems','JettyWindows') {
                if ($edition -eq 'net10.0' -and $name -eq 'JettyWindows') { continue }
                $sample = "samples/$edition/$name/$name.csproj"
                $target = ([xml]([IO.File]::ReadAllText((Join-Path $root $sample)))).Project.PropertyGroup.TargetFramework
                $pin = Get-Content (Join-Path $root "samples/$edition/$name/global.json") -Raw | ConvertFrom-Json
                $receipt = [ordered]@{
                    kind='official-review'; sample=$sample; target=$target; sdk=$pin.sdk.version; language=$(if ($edition -eq 'net10.0') {'14.0'} else {'15.0'})
                    command='eng/Test-JettyHandlers.ps1 -Record'; scope='Pinned official source and platform API review only; no native compilation or execution.'
                    recordedAt=[DateTimeOffset]::UtcNow.ToString('O'); sourceCommit=(& 'C:/Program Files/Git/cmd/git.exe' -C $root rev-parse HEAD).Trim(); sourceDirty=$true
                    inputs=(Get-EvidenceInputs $root $sample); dependencies=@(); workloads=@(); nativeToolchains=@()
                    host=[Runtime.InteropServices.RuntimeInformation]::OSDescription
                    sdkInfo='Intended SDK pin; no native dependency closure resolved.'
                    workloadInfo='Apple requires unavailable Mac/Xcode. SDK11 Windows build fails NETSDK1147; Microsoft.Maui.Sdk unavailable in isolated SDK11 root.'
                    output='Pinned official source hashes and required contract tokens passed. Compilation/execution excluded under PIN-699 for these samples.'
                }
                [IO.File]::WriteAllText((Join-Path $root "samples/$edition/$name/verification-evidence.json"),(ConvertTo-Json $receipt -Depth 30).Replace("`r`n","`n")+"`n")
            }
        }
    }
    Write-Host 'PASS: pinned Jetty handler source contracts; no native execution claimed.'
} finally { $client.Dispose() }
