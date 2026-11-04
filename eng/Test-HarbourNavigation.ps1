#Requires -Version 7.3
[CmdletBinding()]
param([switch]$Record, [switch]$Windows)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
. (Join-Path $PSScriptRoot 'Evidence.ps1')
$client = [Net.Http.HttpClient]::new()
$manifest = Get-Content (Join-Path $root 'samples/HarbourNavigation/sources.json') -Raw | ConvertFrom-Json
try {
    foreach ($source in $manifest.sources) {
        $bytes = $client.GetByteArrayAsync($source.url).GetAwaiter().GetResult()
        $hash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant()
        if ($hash -cne $source.sha256) { throw "Official source changed: $($source.url)" }
    }
    $url = 'https://api.nuget.org/v3-flatcontainer/microsoft.maui.controls.core/11.0.0-rc.2.26505.5/microsoft.maui.controls.core.11.0.0-rc.2.26505.5.nupkg'
    $bytes = $client.GetByteArrayAsync($url).GetAwaiter().GetResult()
    if ([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant() -cne '92787cfc8a46b941b88adefc9966624ec2f183f73bf08c25102b04fff1833fcc') { throw 'Controls.Core package hash changed.' }
    $zip = [IO.Compression.ZipArchive]::new([IO.MemoryStream]::new($bytes))
    try {
        $reader = [IO.StreamReader]::new($zip.GetEntry('lib/net11.0/Microsoft.Maui.Controls.xml').Open())
        try { $xml = $reader.ReadToEnd() } finally { $reader.Dispose() }
        foreach ($member in 'TabbedPage.BadgeText','SwipeItem.IconColor','SwipeItem.TextColor','BackButtonBehavior.AccessibilityLabel') {
            if (-not $xml.Contains($member)) { throw "Missing package member: $member" }
        }
        if (-not @($zip.Entries | Where-Object FullName -like 'lib/net11.0-android37.0/*').Count) { throw 'Android37 package asset missing.' }
        $reference = [IO.MemoryStream]::new()
        $entryStream = $zip.GetEntry('lib/net11.0-android37.0/Microsoft.Maui.Controls.dll').Open()
        try { $entryStream.CopyTo($reference) } finally { $entryStream.Dispose() }
        $reference.Position = 0
        $pe = [Reflection.PortableExecutable.PEReader]::new($reference)
        try {
            $metadata = [Reflection.Metadata.PEReaderExtensions]::GetMetadataReader($pe)
            $types = foreach ($handle in $metadata.TypeDefinitions) {
                $type = $metadata.GetTypeDefinition($handle)
                if (($type.Attributes -band [Reflection.TypeAttributes]::VisibilityMask) -eq [Reflection.TypeAttributes]::Public) {
                    $metadata.GetString($type.Namespace) + '.' + $metadata.GetString($type.Name)
                }
            }
            foreach ($name in 'Microsoft.Maui.Controls.Handlers.ShellHandler','Microsoft.Maui.Controls.Handlers.Compatibility.ShellRenderer') {
                if ($name -cnotin $types) { throw "Missing public Android type: $name" }
            }
        } finally { $pe.Dispose(); $reference.Dispose() }
    } finally { $zip.Dispose() }
    if ($Record) {
        foreach ($sample in @('samples/net11.0/HarbourNavigation/HarbourNavigation.csproj','samples/net11.0/HarbourAndroid/HarbourAndroid.csproj')) {
            $directory = Split-Path (Join-Path $root $sample)
            $target = if ($sample -like '*HarbourAndroid*') { 'net11.0-android37.0' } else { 'net11.0-windows10.0.20348.0' }
            $receipt = [ordered]@{
                kind='official-review'; sample=$sample; target=$target; sdk='11.0.100-rc.2.26504.105'; language='15.0'
                command='eng/Test-HarbourNavigation.ps1 -Record'
                scope='Pinned official source and actual Controls.Core RC2 artifact review; not compilation or native execution.'
                recordedAt=[DateTimeOffset]::UtcNow.ToString('O'); sourceCommit=(& 'C:/Program Files/Git/cmd/git.exe' -C $root rev-parse HEAD).Trim(); sourceDirty=$true
                inputs=(Get-EvidenceInputs $root $sample); dependencies=@(); workloads=@(); nativeToolchains=@()
                host=[Runtime.InteropServices.RuntimeInformation]::OSDescription
                sdkInfo='Intended SDK11 RC2 pin; Windows build attempted and failed NETSDK1147 because Microsoft.Maui.Sdk is absent.'
                workloadInfo='SDK11 native MAUI SDK pack unavailable. No workload installed by this check.'
                output='Official source hashes and Controls.Core package hash/member/Android37 asset checks passed. Android/Windows native behavior not executed.'
            }
            [IO.File]::WriteAllText((Join-Path $directory 'verification-evidence.json'),(ConvertTo-Json $receipt -Depth 30).Replace("`r`n","`n")+"`n")
        }
    }
    if ($Windows) {
        $receiptPath = Join-Path ([IO.Path]::GetTempPath()) ('harbour-navigation-' + [guid]::NewGuid().ToString('N') + '.txt')
        $start = [Diagnostics.ProcessStartInfo]::new((Join-Path $root 'samples/net10.0/HarbourNavigation/bin/Release/net10.0-windows10.0.19041.0/win-x64/HarbourNavigation.exe'))
        $start.UseShellExecute = $false
        $start.Environment['HARBOUR_NAVIGATION_RECEIPT'] = $receiptPath
        $start.Environment['DOTNET_ROOT'] = 'C:/Program Files/dotnet'
        $start.Environment['DOTNET_ROOT_X64'] = 'C:/Program Files/dotnet'
        $start.Environment['DOTNET_ADD_GLOBAL_TOOLS_TO_PATH'] = '0'
        $process = [Diagnostics.Process]::Start($start)
        try {
            if (-not $process.WaitForExit(30000)) { throw 'Windows navigation probe timed out.' }
            $observation = Get-Content -LiteralPath $receiptPath -Raw
            if ($process.ExitCode -ne 0 -or $observation -cne 'PASS: actual Windows Shell forward/query delivery/back navigation.') { throw "Windows probe failed: $observation" }
            Write-Host $observation
        } finally {
            if (-not $process.HasExited) { $process.Kill($true); $process.WaitForExit() }
            $process.Dispose()
            if (Test-Path -LiteralPath $receiptPath) { Remove-Item -LiteralPath $receiptPath }
        }
    }
    Write-Host 'PASS: pinned navigation sources and exact Controls.Core contracts/assets; native behavior only when -Windows is selected.'
} finally { $client.Dispose() }
