#Requires -Version 7.4
[CmdletBinding()]
param([switch]$Record, [switch]$CompileAndroidReference)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
. (Join-Path $PSScriptRoot 'Evidence.ps1')
$client = [Net.Http.HttpClient]::new()
try {
    foreach ($file in 'sources.json','artifacts.json') {
        foreach ($source in (Get-Content "$root/samples/LifecycleGuidance/$file" -Raw | ConvertFrom-Json)) {
            $bytes = $client.GetByteArrayAsync($source.url).GetAwaiter().GetResult()
            if ([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant() -cne $source.sha256) { throw "Changed source/artifact: $($source.url)" }
            if ($source.id -eq 'Microsoft.Android.Ref.37.2') {
                if ($CompileAndroidReference) {
                    $scratch = Join-Path $root 'artifacts/lifecycle-reference'
                    $null = New-Item -ItemType Directory $scratch -Force
                    [IO.File]::WriteAllBytes("$scratch/android.zip", $bytes)
                    Expand-Archive "$scratch/android.zip" "$scratch/android" -Force
                    $dotnetRoot = 'C:/Users/pingu/AppData/Local/dotnetup/dotnet'
                    $references = @(Get-ChildItem "$dotnetRoot/packs/Microsoft.NETCore.App.Ref/11.0.0-rc.3.26522.148/ref/net11.0/*.dll") + @(Get-ChildItem "$scratch/android/ref/net11.0/*.dll")
                    $arguments = @('/noconfig','/nostdlib+','/target:library','/nullable:enable','/langversion:15.0',"/out:$scratch/SkuaBack.dll") + @($references | ForEach-Object { '/reference:' + $_.FullName }) + @("$root/samples/net11.0/SkuaBack/MainActivity.cs")
                    & "$dotnetRoot/dotnet.exe" "$dotnetRoot/sdk/11.0.100-rc.3.26522.148/Roslyn/bincore/csc.dll" @arguments
                    if ($LASTEXITCODE -ne 0) { throw 'Canonical Android reference compilation failed' }
                    Write-Host 'PASS canonical .NET11 Android reference compilation; CS1701 unifies Java.Interop System.Runtime10 reference. No manifest generation or device execution.'
                }
                $zip = [IO.Compression.ZipArchive]::new([IO.MemoryStream]::new($bytes))
                try {
                    $entry = $zip.GetEntry('ref/net11.0/Mono.Android.dll')
                    $stream = [IO.MemoryStream]::new()
                    $inputStream = $entry.Open()
                    try { $inputStream.CopyTo($stream) } finally { $inputStream.Dispose() }
                    $stream.Position = 0
                    $pe = [Reflection.PortableExecutable.PEReader]::new($stream)
                    try {
                        $reader = [Reflection.Metadata.PEReaderExtensions]::GetMetadataReader($pe)
                        $found = $false
                        foreach ($handle in $reader.TypeDefinitions) {
                            $type = $reader.GetTypeDefinition($handle)
                            if ($reader.GetString($type.Namespace) -cne 'Android.App' -or $reader.GetString($type.Name) -cne 'ApplicationAttribute') { continue }
                            foreach ($propertyHandle in $type.GetProperties()) {
                                if ($reader.GetString($reader.GetPropertyDefinition($propertyHandle).Name) -ceq 'EnableOnBackInvokedCallback') { $found = $true }
                            }
                        }
                        if (-not $found) { throw 'Actual Android reference lacks predictive-back property' }
                    } finally { $pe.Dispose(); $stream.Dispose() }
                } finally { $zip.Dispose() }
            }
        }
    }
    if ($Record) {
        foreach ($name in 'SkuaBack','PuffinScenes','PuffinMaui') {
            $sample = "samples/net11.0/$name/$name.csproj"
            $receipt = [ordered]@{
                kind='official-review'; sample=$sample
                target=if($name -eq 'SkuaBack'){'net11.0-android37.2'}else{'net11.0-ios27.0'}
                sdk='11.0.100-rc.3.26522.148'; language='15.0'
                command='eng/Test-LifecycleGuidance.ps1 -Record'
                scope='Pinned official scene implementation/template and Android reference metadata review. Native build NETSDK1147; current native compilation/execution not-tested under PIN-699.'
                recordedAt=[DateTimeOffset]::UtcNow.ToString('O')
                sourceCommit=(& 'C:/Program Files/Git/cmd/git.exe' -C $root rev-parse HEAD).Trim()
                sourceDirty=[bool](& 'C:/Program Files/Git/cmd/git.exe' -C $root status --porcelain -- samples/LifecycleGuidance "samples/net11.0/$name" eng/Test-LifecycleGuidance.ps1)
                inputs=(Get-EvidenceInputs $root $sample); dependencies=@(); workloads=@(); nativeToolchains=@()
                host=[Runtime.InteropServices.RuntimeInformation]::OSDescription
                sdkInfo='Intended RC3 SDK; actual build attempts fail before compilation.'
                workloadInfo='NETSDK1147 android, ios or maui-ios; no Xcode/device execution.'
                output='PASS exact sources/artifacts and actual ApplicationAttribute property. Source review is not compilation or execution.'
            }
            [IO.File]::WriteAllText("$root/samples/net11.0/$name/verification-evidence.json",(ConvertTo-Json $receipt -Depth 30)+"`n")
        }
    }
    Write-Host 'PASS: pinned lifecycle sources/artifacts and Android property metadata; native behaviour not-tested.'
} finally { $client.Dispose() }
