#Requires -Version 7.4
[CmdletBinding()]
param([string]$Dotnet = (Get-Command dotnet -CommandType Application | Select-Object -First 1).Source)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
$work = Join-Path $root 'artifacts/quay-templates'
$before = @{}
foreach ($scope in 'Process','User','Machine') { $before[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$previous = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
New-Item -ItemType Directory -Path $work -Force | Out-Null
$session = Join-Path $work ([guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $session -Force | Out-Null
$client = [Net.Http.HttpClient]::new()
try {
    $olderBytes = $client.GetByteArrayAsync('https://api.nuget.org/v3-flatcontainer/microsoft.maui.templates.net10/10.0.110/microsoft.maui.templates.net10.10.0.110.nupkg').GetAwaiter().GetResult()
    if ([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($olderBytes)).ToLowerInvariant() -cne '7c1b8f504316c187ce4c6e113b674b9d3d7d31997d03d39c42ff45f37460aeac') { throw 'Older template identity changed.' }
    $zip = [IO.Compression.ZipArchive]::new([IO.MemoryStream]::new($olderBytes))
    try {
        $reader = [IO.StreamReader]::new($zip.GetEntry('content/templates/maui-mobile/.template.config/template.json').Open())
        try { $older = $reader.ReadToEnd() | ConvertFrom-Json } finally { $reader.Dispose() }
        if ($older.symbols.UI -or $older.symbols.WithAvalonia) { throw 'Reconcile older template equivalence before serving this comparison.' }
    } finally { $zip.Dispose() }
    $version = '11.0.0-rc.2.26505.5'
    $package = Join-Path $session "Microsoft.Maui.Templates.net11.$version.nupkg"
    [IO.File]::WriteAllBytes($package, $client.GetByteArrayAsync("https://api.nuget.org/v3-flatcontainer/microsoft.maui.templates.net11/$version/microsoft.maui.templates.net11.$version.nupkg").GetAwaiter().GetResult())
    if ((Get-FileHash $package -Algorithm SHA256).Hash.ToLowerInvariant() -cne 'f79b83f8c93756b9ba812043ef204c06b51640bedf5b807012f04db916b710b7') { throw 'Template package identity changed.' }
    Push-Location (Join-Path $root 'samples/net11.0/QuayBackend')
    try {
        $hive = Join-Path $session 'hive'
        & $Dotnet new install $package --debug:custom-hive $hive
        & $Dotnet new maui -n QuayCSharp -o (Join-Path $session 'csharp') --ui csharp --no-restore --debug:custom-hive $hive
        & $Dotnet new maui -n QuayDesktop -o (Join-Path $session 'avalonia') --with-avalonia --avaloniaControlsMauiVersion '11.0.0-rc.2.26287.618' --no-restore --debug:custom-hive $hive
        & $Dotnet new maui -n QuayRestricted -o (Join-Path $session 'restricted') --ui csharp --sample-content --no-restore --debug:custom-hive $hive
        & $Dotnet new maui -n QuayXaml -o (Join-Path $session 'xaml') --with-avalonia --sample-content --no-restore --debug:custom-hive $hive
        & $Dotnet new maui-multiproject -n QuaySeparate -o (Join-Path $session 'separate') --avalonia --avaloniaControlsMauiVersion '11.0.0-rc.2.26287.618' --debug:custom-hive $hive
    } finally { Pop-Location }
    # The template also emits floating logging and an SDK-selected MAUI version.
    # Freeze these generated inputs before a restore; never grant sample auto-updates.
    foreach ($projectFile in Get-ChildItem $session -Recurse -Filter '*.csproj') {
        $text = [IO.File]::ReadAllText($projectFile.FullName)
        $text = $text.Replace('Version="$(MauiVersion)"', 'Version="11.0.0-rc.2.26505.5"')
        $text = $text.Replace('Version="11.0.0-*"', 'Version="11.0.0-rc.2.26504.105"')
        [IO.File]::WriteAllText($projectFile.FullName, $text)
    }
    foreach ($file in 'MainPage.cs','App.cs','AppShell.cs','Resources/Styles/Styles.xaml','Resources/Styles/Colors.xaml') {
        if (-not (Test-Path (Join-Path $session "csharp/$file"))) { throw "C# template lost $file" }
    }
    if (Test-Path (Join-Path $session 'restricted/Pages/MainPage.xaml')) { throw 'C# option unexpectedly included XAML sample content.' }
    $project = [IO.File]::ReadAllText((Join-Path $session 'avalonia/QuayDesktop.csproj'))
    foreach ($token in 'Avalonia.Controls.Maui','Avalonia.Controls.Maui.Desktop','11.0.0-rc.2.26287.618','net11.0') {
        if (-not $project.Contains($token)) { throw "Missing generated Avalonia input $token" }
    }
    if ($project.Contains('preview.6.*')) { throw 'Generated dependency is floating.' }
    if ([IO.File]::ReadAllText((Join-Path $session 'xaml/QuayXaml.csproj')).Contains('Avalonia.Controls.Maui')) { throw 'XAML sample-content restriction ignored.' }
    if (-not (Test-Path (Join-Path $session 'separate/QuaySeparate.Desktop/QuaySeparate.Desktop.csproj'))) { throw 'Separate desktop head missing.' }
    Write-Host "PASS: real pinned template generation, retained resource XAML, sample-content restriction, exact Avalonia options. Output: $session"
    Write-Host "Template SHA256: $((Get-FileHash $package -Algorithm SHA256).Hash.ToLowerInvariant())"
} finally {
    $client.Dispose()
    $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previous
    foreach ($scope in 'Process','User','Machine') {
        if ([Environment]::GetEnvironmentVariable('PATH',$scope) -cne $before[$scope]) { throw "Unexpected $scope PATH change." }
    }
}
