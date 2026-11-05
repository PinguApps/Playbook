[CmdletBinding()]
param(
  [string]$Net10Dotnet = (Get-Command dotnet -CommandType Application | Select-Object -First 1).Source,
  [string]$Net11Dotnet = (Get-Command dotnet -CommandType Application | Select-Object -First 1).Source,
  [string]$Compiler = 'C:/Program Files/Microsoft Visual Studio/18/Community/VC/Tools/MSVC/14.51.36231/bin/Hostx64/x64/cl.exe',
  [string]$Linker = 'C:/Program Files/Microsoft Visual Studio/18/Community/VC/Tools/MSVC/14.51.36231/bin/Hostx64/x64/link.exe',
  [string]$Node = 'C:/Program Files/nodejs/node.exe',
  [string]$Playwright = 'C:/Users/pingu/AppData/Local/npm-cache/_npx/e41f203b7505f1fb/node_modules/playwright-core',
  [string]$Chromium = 'C:/Users/pingu/AppData/Local/ms-playwright/chromium-1248/chrome-win64/chrome.exe',
  [switch]$Record,
  [switch]$VerifySources
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
$paths = @{}
$persistent = @{}
foreach ($scope in 'Process','User','Machine') { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
foreach ($scope in 'User','Machine') { $persistent[$scope] = [Environment]::GetEnvironmentVariables($scope) | ConvertTo-Json -Compress }
$previous = @{}
foreach ($key in 'DOTNET_ADD_GLOBAL_TOOLS_TO_PATH','DOTNET_GENERATE_ASPNET_CERTIFICATE','MSBUILDUSESERVER') { $previous[$key] = [Environment]::GetEnvironmentVariable($key) }
Push-Location -LiteralPath $root
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  $env:DOTNET_GENERATE_ASPNET_CERTIFICATE = 'false'
  $env:MSBUILDUSESERVER = '0'
  if ($VerifySources) {
    foreach ($source in (Get-Content docs/planning/audits/interop-soundings.sources.json -Raw | ConvertFrom-Json).sources) {
      $bytes = (Invoke-WebRequest $source.url).RawContentStream.ToArray()
      if ([Convert]::ToHexStringLower([Security.Cryptography.SHA256]::HashData($bytes)) -cne $source.sha256) { throw "Source changed: $($source.id)" }
      Write-Host "SOURCE $($source.id) $($source.sha256)"
    }
  }
  $native = Join-Path $root 'artifacts/native-soundings'
  New-Item -ItemType Directory -Force $native | Out-Null
  $compilerVersion = (Get-Item $Compiler).VersionInfo.FileVersion
  $linkerVersion = (Get-Item $Linker).VersionInfo.FileVersion
  $toolchains = @("MSVC/$compilerVersion", "MSVC-link/$linkerVersion")
  & $Compiler /nologo /c /GS- /TC "/Fo$native/soundings.obj" samples/net11.0/NativeSoundings/soundings.c
  & $Linker /NOLOGO /DLL /NOENTRY /NODEFAULTLIB /MACHINE:X64 "/OUT:$native/soundings.dll" "$native/soundings.obj"
  $audit = Get-Content docs/planning/audits/runtime-security.assets.json -Raw | ConvertFrom-Json
  foreach ($target in 'net10.0','net11.0') {
    $dotnet = if ($target -eq 'net10.0') { $Net10Dotnet } else { $Net11Dotnet }
    $baseline = @($audit.baselines | Where-Object target -CEQ $target)[0]
    foreach ($name in 'NativeSoundings','FloatSoundings') {
      $sample = "samples/$target/$name/$name.csproj"
      $directory = Split-Path (Join-Path $root $sample)
      Push-Location $directory
      try {
        & $dotnet build "$name.csproj" -c Release -p:RestoreLockedMode=true
        if ($name -eq 'NativeSoundings') {
          Copy-Item -LiteralPath "$native/soundings.dll" -Destination "bin/Release/$target/soundings.dll"
          if ($Record) { & "$PSScriptRoot/Record-Evidence.ps1" -Sample $sample -Dotnet $dotnet -NativeToolchains $toolchains }
          else { & $dotnet "bin/Release/$target/$name.dll" }
        } else {
          if ($Record) { & "$PSScriptRoot/Record-Evidence.ps1" -Sample $sample -Dotnet $dotnet -Kind compile }
          & $dotnet publish "$name.csproj" -c Release --no-restore -p:PublishTrimmed=false
        }
        $pack = (& $dotnet msbuild "$name.csproj" -t:ResolveFrameworkReferences -p:Configuration=Release -getItem:ResolvedTargetingPack | ConvertFrom-Json).Items.ResolvedTargetingPack | Where-Object Identity -CEQ 'Microsoft.NETCore.App'
        if ($pack.NuGetPackageVersion -cne $baseline.referencePack) { throw "Reference pack drift: $sample" }
        $assemblies = if ($name -eq 'NativeSoundings') { @('System.Runtime.dll','System.Runtime.InteropServices.dll') } else { @('System.Runtime.dll','System.Runtime.InteropServices.JavaScript.dll') }
        $artifacts = @($Compiler, $Linker)
        if ($name -eq 'NativeSoundings') { $artifacts += "$native/soundings.dll" }
        foreach ($assembly in $assemblies) {
          $path = Join-Path $pack.Path "ref/$target/$assembly"
          $expected = @($baseline.framework | Where-Object name -CEQ $assembly)[0].sha256
          if ((Get-FileHash $path).Hash.ToLowerInvariant() -cne $expected) { throw "Audit reference drift: $path" }
          $artifacts += $path
        }
        $sdk = (Get-Content global.json -Raw | ConvertFrom-Json).sdk.version
        $artifacts += Join-Path (Split-Path $dotnet) "sdk/$sdk/Roslyn/bincore/Microsoft.CodeAnalysis.CSharp.dll"
        if ($name -eq 'FloatSoundings') {
          $generated = @(Get-ChildItem obj -Recurse -Filter '*.g.cs' | Where-Object FullName -Match 'JavaScript')
          if ($generated.Count -lt 2) { throw 'Missing actual generated imports/exports.' }
          $generatedText = ($generated | ForEach-Object { Get-Content $_.FullName -Raw }) -join "`n"
          if ($generatedText -notmatch 'ToJS' -or $generatedText -notmatch 'ToManaged') { throw 'Missing generated conversion directions.' }
          $artifacts += @($generated.FullName)
          $artifacts += @(Get-ChildItem "bin/Release/$target/publish/wwwroot/_framework" -File | Where-Object Name -Match '^(dotnet\.(runtime.*\.js|native.*\.wasm)|System\.(Private.CoreLib|Runtime.InteropServices.JavaScript).*\.wasm)$' | Select-Object -ExpandProperty FullName)
          $artifacts += @(Get-ChildItem (Join-Path $pack.Path 'analyzers') -Recurse -Filter 'Microsoft.Interop.JavaScript.JSImportGenerator.dll' | Select-Object -ExpandProperty FullName)
        }
        $identities = @($artifacts | Sort-Object -Unique | ForEach-Object { "ARTIFACT $_ sha256=$((Get-FileHash -LiteralPath $_).Hash.ToLowerInvariant())" })
        if ($Record) {
          $receipt = Get-Content verification-evidence.json -Raw | ConvertFrom-Json -AsHashtable
          $receipt.output += "`n" + ($identities -join "`n")
          $receipt.command += '; ResolveFrameworkReferences; compare runtime-security.assets.json; SHA256 compiler/reference/native/generated/browser artifacts'
          if ($name -eq 'NativeSoundings') {
            $receipt.command = "$Compiler /nologo /c /GS- /TC /Fo$native/soundings.obj $root/samples/net11.0/NativeSoundings/soundings.c; $Linker /NOLOGO /DLL /NOENTRY /NODEFAULTLIB /MACHINE:X64 /OUT:$native/soundings.dll $native/soundings.obj; Copy-Item $native/soundings.dll $directory/bin/Release/$target/soundings.dll; " + $receipt.command
          } else {
            $receipt.command += "; $dotnet publish $directory/$name.csproj -c Release --no-restore -p:PublishTrimmed=false"
          }
          $receipt.scope = if ($name -eq 'NativeSoundings') { "Actual Windows x64 MSVC $compilerVersion/link $linkerVersion C aggregate metrics, pointer round trips and rejected packed negative control; no other ABI/GA proof." } else { 'Actual Mono browser build and generated import/export inspection; publish and real Chromium execution are recorded separately in browser.evidence.json. No AOT/native relink/CoreCLR-browser claim.' }
          [IO.File]::WriteAllText((Join-Path $directory 'verification-evidence.json'), ($receipt | ConvertTo-Json -Depth 40).Replace("`r`n","`n") + "`n")
        }
      } finally { Pop-Location }
    }
  }
  $browserOutput = Join-Path $root 'artifacts/float-soundings/browser.json'
  & $Node eng/Test-FloatSoundings.mjs $Playwright $Chromium $browserOutput
  if ($Record) {
    foreach ($target in 'net10.0','net11.0') {
      $directory = Join-Path $root "samples/$target/FloatSoundings"
      $receipt = Get-Content "$directory/verification-evidence.json" -Raw | ConvertFrom-Json -AsHashtable
      $receipt.kind = 'execution'
      $receipt.recordedAt = [DateTimeOffset]::UtcNow.ToString('O')
      $dotnet = if ($target -eq 'net10.0') { $Net10Dotnet } else { $Net11Dotnet }
      $receipt.command = "$dotnet publish $directory/FloatSoundings.csproj -c Release --no-restore -p:PublishTrimmed=false; $Node $root/eng/Test-FloatSoundings.mjs $Playwright $Chromium $browserOutput"
      $receipt.scope = 'Real published Mono browser consumers, Float64 older adapter versus Float32 current route; generated imports/exports, copy semantics, specials, segment offsets and surrounding sentinels, span expiry, retained segment mutation then disposal/rejected access. No performance, AOT, CoreCLR-browser, other-browser or GA claim.'
      $receipt.output += "`n" + [IO.File]::ReadAllText($browserOutput)
      $receipt.output += "`nCHROMIUM sha256=$((Get-FileHash $Chromium).Hash.ToLowerInvariant())`nPLAYWRIGHT " + [IO.File]::ReadAllText((Join-Path $Playwright 'package.json'))
      [IO.File]::WriteAllText("$directory/browser.evidence.json", ($receipt | ConvertTo-Json -Depth 40).Replace("`r`n","`n") + "`n")
    }
  }
  Write-Host 'PASS: paired native C ABI and published real browser soundings; audit-matching references and generated conversions.'
} finally {
  Pop-Location
  foreach ($key in $previous.Keys) { [Environment]::SetEnvironmentVariable($key,$previous[$key]) }
  foreach ($scope in $paths.Keys) { if ([Environment]::GetEnvironmentVariable('PATH',$scope) -cne $paths[$scope]) { throw "Unexpected $scope PATH change; shared settings preserved." } }
  foreach ($scope in $persistent.Keys) { if (([Environment]::GetEnvironmentVariables($scope) | ConvertTo-Json -Compress) -cne $persistent[$scope]) { throw "Unexpected persistent $scope environment change; shared settings preserved." } }
}
