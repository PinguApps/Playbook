# Source/artifact inspection only; never installs workloads or executes downloaded assemblies.
[CmdletBinding()]
param(
    [string]$OutputPath = (Join-Path $PSScriptRoot '../artifacts/ga-native.json'),
    [string]$CompareWith
)
$ErrorActionPreference = 'Stop'
if ($CompareWith -and [IO.Path]::GetFullPath($CompareWith) -eq [IO.Path]::GetFullPath($OutputPath)) { throw 'Do not overwrite the comparison baseline' }
$root = Split-Path $PSScriptRoot
$scratch = Join-Path $root "artifacts/pin966-$PID"
[IO.Directory]::CreateDirectory($scratch) | Out-Null
function Hash([byte[]]$bytes) { [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant() }
function Package([string]$id, [string]$version, [bool]$expectedMissing = $false) {
    $lower = $id.ToLowerInvariant()
    $url = "https://api.nuget.org/v3-flatcontainer/$lower/$version/$lower.$version.nupkg"
    $path = Join-Path $scratch "$lower.$version.zip"
    Write-Host "Inspecting $id/$version"
    $response = Invoke-WebRequest $url -OutFile $path -PassThru -SkipHttpErrorCheck
    if ($expectedMissing -and [int]$response.StatusCode -eq 404) {
        return [ordered]@{ id=$id; version=$version; url=$url; status=404; scope='Exact public NuGet URL unavailable; not proof of absence on every feed' }
    }
    if ([int]$response.StatusCode -ne 200 -or $expectedMissing) { throw "Unexpected availability: $url ($($response.StatusCode))" }
    $zip = [IO.Compression.ZipFile]::OpenRead($path)
    try {
        $entry = @($zip.Entries | Where-Object FullName -Like '*.nuspec')
        if ($entry.Count -ne 1) { throw "Ambiguous identity: $id" }
        $reader = [IO.StreamReader]::new($entry[0].Open())
        try { [xml]$spec = $reader.ReadToEnd() } finally { $reader.Dispose() }
        if ($spec.package.metadata.id -ine $id -or $spec.package.metadata.version -cne $version) { throw "Wrong package: $id" }
        $data = [ordered]@{}
        foreach ($name in @('data/microsoft.net.workloads.workloadset.json','data/WorkloadManifest.json')) {
            $entry = $zip.GetEntry($name)
            if ($entry) {
                $reader = [IO.StreamReader]::new($entry.Open())
                try { $data[$name] = $reader.ReadToEnd() | ConvertFrom-Json -AsHashtable } finally { $reader.Dispose() }
            }
        }
        $assets = @($zip.Entries | Where-Object { $_.FullName -match '\.(dll|props|targets)$' } | Sort-Object FullName | ForEach-Object {
            $stream = [IO.MemoryStream]::new()
            $inputStream = $_.Open()
            try { $inputStream.CopyTo($stream) } finally { $inputStream.Dispose() }
            try { [ordered]@{ path=$_.FullName; sha256=(Hash $stream.ToArray()) } } finally { $stream.Dispose() }
        })
        [ordered]@{ id=$id; version=$version; url=$url; sha256=(Get-FileHash $path).Hash.ToLowerInvariant(); repository=[string]$spec.package.metadata.repository.url; commit=[string]$spec.package.metadata.repository.commit; dependencies=$spec.package.metadata.dependencies.OuterXml; data=$data; assets=$assets }
    } finally { $zip.Dispose() }
}
$packages = [Collections.Generic.List[object]]::new()
$set = Package 'Microsoft.NET.Workloads.11.0.100' '11.100.0'
$packages.Add($set)
$packages.Add((Package 'Microsoft.NET.Workloads.11.0.100' '11.100.1'))
$map = $set.data['data/microsoft.net.workloads.workloadset.json']
if ($map.Count -ne 18) { throw 'Unexpected workload-set composition' }
foreach ($key in @('Android','iOS','tvOS','MacCatalyst','macOS','Maui')) {
    $selection = $map["Microsoft.NET.Sdk.$key"].Split('/')
    $packages.Add((Package "Microsoft.NET.Sdk.$key.Manifest-$($selection[1])" $selection[0]))
}
$packages.Add((Package 'Microsoft.NET.Sdk.Android.Manifest-11.0.100' '37.2.31'))
foreach ($id in @('Microsoft.Maui.Controls','Microsoft.Maui.Controls.Core','Microsoft.Maui.Controls.Xaml','Microsoft.Maui.Core','Microsoft.Maui.Essentials','Microsoft.Maui.Graphics','Microsoft.Maui.Maps','Microsoft.Maui.Controls.Maps','Microsoft.Maui.Resizetizer','Microsoft.AspNetCore.Components.WebView.Maui')) {
    $packages.Add((Package $id '11.0.0'))
}
$packages.Add((Package 'Microsoft.Maui.Essentials.AI' '11.0.0' $true))
$packages.Add((Package 'Microsoft.Maui.Essentials.AI' '11.0.0-preview.8.26555.2'))
foreach ($platform in @('iOS','tvOS','MacCatalyst','macOS')) {
    $packages.Add((Package "Microsoft.$platform.Ref.net11.0_27.2" '27.2.11039'))
}
$catalogue = @(Get-ChildItem (Join-Path $root 'content/recipes') -Recurse -Filter recipe.json | ForEach-Object {
    $recipe = Get-Content $_.FullName -Raw | ConvertFrom-Json
    if (@($recipe.areas | Where-Object { $_ -in @('maui','android','ios','macos','apple','winforms','wpf','windowsdesktop') }).Count) {
        [ordered]@{ id=$recipe.id; edition=$recipe.edition; stability=$recipe.stability; areas=$recipe.areas; metadataSha256=(Get-FileHash $_.FullName).Hash.ToLowerInvariant(); markdownSha256=(Get-FileHash (Join-Path $_.DirectoryName 'recipe.md')).Hash.ToLowerInvariant(); applicability=$recipe.applicability; evidence=$recipe.evidence; humanProof=$recipe.humanProof }
    }
} | Sort-Object { $_.id }, { $_.edition })
$sources = [Collections.Generic.List[object]]::new()
$baseline = Get-Content (Join-Path $root 'docs/planning/audits/mobile-api-gaps.sources.json') -Raw | ConvertFrom-Json
$tree = Invoke-RestMethod 'https://api.github.com/repos/dotnet/maui/git/trees/4e6582f7397a5736bc85984228c549d09a85a1de?recursive=1'
if ($tree.truncated) { throw 'Incomplete GA source tree' }
$gaApiPaths = @($tree.tree.path | Where-Object { $_ -match '(^|/)PublicAPI\.(Unshipped|Shipped)\.txt$' } | Sort-Object)
$retainedApiPaths = @($baseline.sources.id | Where-Object { $_ -match 'PublicAPI\.' } | Sort-Object)
if (@(Compare-Object $retainedApiPaths $gaApiPaths).Count) { throw 'PublicAPI file set changed; reconcile added/deleted files before auditing' }
$client = [Net.Http.HttpClient]::new()
try {
    foreach ($name in @('dotnetmaui','winforms','wpf')) {
        $url = "https://raw.githubusercontent.com/dotnet/core/33918e8affbb517bf654c15559ea6f813c08bbae/release-notes/11.0/11.0.0/$name.md"
        $bytes = $client.GetByteArrayAsync($url).GetAwaiter().GetResult()
        $sources.Add([ordered]@{ id="ga-$name-notes"; url=$url; sha256=(Hash $bytes) })
    }
    $platformSources = @(
        'https://raw.githubusercontent.com/dotnet/android/c6663f24d64147d204525b3ba0c2762c6176bfc9/src/Mono.Android/Android.Runtime/JNINativeWrapper.cs',
        'https://raw.githubusercontent.com/dotnet/android/c6663f24d64147d204525b3ba0c2762c6176bfc9/src/Xamarin.Android.Build.Tasks/Microsoft.Android.Sdk/targets/Microsoft.Android.Sdk.DefaultProperties.targets',
        'https://raw.githubusercontent.com/dotnet/macios/34b3b25d471c22f2a62c36ec0971b021d0a0e000/docs/nativeaot.md',
        'https://raw.githubusercontent.com/dotnet/macios/34b3b25d471c22f2a62c36ec0971b021d0a0e000/src/appkit.cs',
        'https://raw.githubusercontent.com/dotnet/macios/34b3b25d471c22f2a62c36ec0971b021d0a0e000/src/corenfc.cs',
        'https://raw.githubusercontent.com/dotnet/maui/4e6582f7397a5736bc85984228c549d09a85a1de/src/Controls/src/Build.Tasks/nuget/buildTransitive/netstandard2.0/Microsoft.Maui.Controls.Common.targets',
        'https://raw.githubusercontent.com/dotnet/maui/4e6582f7397a5736bc85984228c549d09a85a1de/src/Controls/src/SourceGen/Visitors/ExpandMarkupsVisitor.cs'
    )
    foreach ($url in $platformSources) {
        $bytes = $client.GetByteArrayAsync($url).GetAwaiter().GetResult()
        $sources.Add([ordered]@{ id=$url.Split('/')[-1]; url=$url; sha256=(Hash $bytes) })
    }
    foreach ($source in $baseline.sources | Where-Object { $_.url -match 'dotnet/maui/d32b79928709cf53b671336dabd0caef5a679fc2/' }) {
        $oldBytes = $client.GetByteArrayAsync($source.url).GetAwaiter().GetResult()
        if ((Hash $oldBytes) -cne $source.sha256) { throw "Retained source changed: $($source.id)" }
        $url = $source.url.Replace('d32b79928709cf53b671336dabd0caef5a679fc2','4e6582f7397a5736bc85984228c549d09a85a1de')
        $bytes = $client.GetByteArrayAsync($url).GetAwaiter().GetResult()
        $old = [Text.Encoding]::UTF8.GetString($oldBytes) -split '\r?\n'
        $new = [Text.Encoding]::UTF8.GetString($bytes) -split '\r?\n'
        $sources.Add([ordered]@{ id=$source.id; retainedUrl=$source.url; retainedSha256=$source.sha256; url=$url; sha256=(Hash $bytes); added=@($new | Where-Object { $_ -cnotin $old }); removed=@($old | Where-Object { $_ -cnotin $new }) })
    }
} finally { $client.Dispose() }
$desktopManifest = Get-Content (Join-Path $root 'docs/planning/audits/windowsdesktop.sources.json') -Raw | ConvertFrom-Json
$desktopManifest.packages = @($desktopManifest.packages | Where-Object id -In @('net10','rc2')) + @([pscustomobject]@{ id='ga'; version='11.0.0'; url='https://api.nuget.org/v3-flatcontainer/microsoft.windowsdesktop.app.ref/11.0.0/microsoft.windowsdesktop.app.ref.11.0.0.nupkg' })
$desktopInput = Join-Path $scratch 'desktop-inputs.json'
$desktopOutput = Join-Path $scratch 'desktop.json'
$desktopManifest | ConvertTo-Json -Depth 40 | Set-Content $desktopInput
& (Join-Path $PSScriptRoot 'Inspect-WindowsDesktopApis.ps1') -SourceManifest $desktopInput -OutputPath $desktopOutput
if (-not $?) { throw 'Desktop inspection failed' }
$desktop = Get-Content $desktopOutput -Raw | ConvertFrom-Json
$report = [ordered]@{ scope='PIN-966 GA source/artifact audit; no native execution or release approval'; workloadSet='11.0.100'; packages=$packages; gaPublicApiPaths=$gaApiPaths; sourceComparisons=$sources; desktop=$desktop; catalogue=$catalogue }
$json = $report | ConvertTo-Json -Depth 70
if ($CompareWith) {
    $expected = Get-Content $CompareWith -Raw | ConvertFrom-Json | ConvertTo-Json -Depth 70
    if ($expected -cne $json) { throw 'GA native evidence changed; review inputs and dispositions' }
}
$json | Set-Content -LiteralPath $OutputPath -Encoding utf8
"PASS: $(@($packages | Where-Object { $_.status -ne 404 }).Count) actual packages; one expected unavailable AI version; all 18 workload selections; $($catalogue.Count) prepared editions."
