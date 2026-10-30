#Requires -Version 7.0
[CmdletBinding()]
param(
  [string]$Net10Dotnet = 'C:/Program Files/dotnet/dotnet.exe',
  [string]$Net11Dotnet = 'C:/Users/pingu/.dotnet-playbook-11/dotnet.exe',
  [switch]$Record
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
. (Join-Path $PSScriptRoot 'Evidence.ps1')
$root = Split-Path $PSScriptRoot -Parent
$output = Join-Path $root 'artifacts/compiler-inspection'
New-Item -ItemType Directory -Force $output | Out-Null
$before = @{}
foreach ($scope in 'Process','User','Machine') { $before[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$previous = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
$commands = [Collections.Generic.List[string]]::new()
$observations = [Collections.Generic.List[string]]::new()
function Invoke-Compiler([string]$Exe, [string[]]$Arguments, [int]$Expected = 0, [string]$Diagnostic) {
  $commands.Add("$Exe $($Arguments -join ' ')")
  $PSNativeCommandUseErrorActionPreference = $false
  $result = (& $Exe @Arguments 2>&1 | Out-String).Trim()
  $exit = $LASTEXITCODE
  if ($exit -ne $Expected -or ($Diagnostic -and $result -notmatch [regex]::Escape($Diagnostic))) { throw "Unexpected compiler result ($exit): $result" }
  $observations.Add($result)
  Write-Host $result
}
Push-Location -LiteralPath $root
try {
  foreach ($release in 10,11) {
    $exe = if ($release -eq 10) { $Net10Dotnet } else { $Net11Dotnet }
    foreach ($name in 'ReadingConversions','ReadingConversionsVB','PacketLayout','SeedFoldLibrary','SeedFold') {
      $extension = if ($name -like 'SeedFold*') { 'fsproj' } elseif ($name -like '*VB') { 'vbproj' } else { 'csproj' }
      $sample = "samples/net$release.0/$name/$name.$extension"
      if ($Record) {
        $kind = if ($name -eq 'SeedFoldLibrary') { 'compile' } else { 'compile-and-run' }
        & (Join-Path $PSScriptRoot 'Record-Evidence.ps1') -Sample $sample -Dotnet $exe -Kind $kind
      } else {
        Push-Location (Split-Path $sample)
        try {
          Invoke-Compiler $exe @('build', '-c', 'Release', '-p:RestoreLockedMode=true')
          if ($name -ne 'SeedFoldLibrary') { Invoke-Compiler $exe @("bin/Release/net$release.0/$name.dll") }
        } finally { Pop-Location }
      }
    }
  }
  if (-not ('SeedIl' -as [type])) { Add-Type -Path (Join-Path $root 'samples/CompilerInspections/ReadIl.cs') }
  $observations.AddRange([SeedIl]::Inspect((Join-Path $root 'samples/net11.0/SeedFold/bin/Release/net11.0/SeedFold.dll')))
  $observations | Select-Object -Last 3 | Write-Host

  # Direct compiler probes use actual Core and reference assemblies, not generated shims.
  $sdkRoot = Join-Path (Split-Path $Net11Dotnet) 'sdk/11.0.100-rc.2.26504.105'
  $refs = @(Get-ChildItem (Join-Path (Split-Path $Net11Dotnet) 'packs/Microsoft.NETCore.App.Ref/11.0.0-rc.2.26504.105/ref/net11.0') -Filter '*.dll' | ForEach-Object { '-r:' + $_.FullName })
  $core = Join-Path $root 'samples/net11.0/SeedFold/bin/Release/net11.0/FSharp.Core.dll'
  $baseArgs = @((Join-Path $sdkRoot 'FSharp/fsc.dll'), '--target:library', '--targetprofile:netcore', '--noframework', '--simpleresolution', '--optimize+', "-r:$core") + $refs
  $valid = 'let inline batch ([<InlineIfLambda; OptimizeClosureIfNotInlined>] step: int -> int -> int) state reading = step state reading'
  $source = Join-Path $output 'Rejected.fs'
  [IO.File]::WriteAllText($source, "module Rejected`n$valid`n")
  Invoke-Compiler $Net11Dotnet ($baseArgs + @('--langversion:10.0', "-o:$output/Rejected.dll", $source)) 1 'FS3350'
  $invalid = @(
    'let inline batch ([<InlineIfLambda; OptimizeClosureIfNotInlined>] step: int -> int) reading = step reading',
    'let inline batch ([<InlineIfLambda; OptimizeClosureIfNotInlined>] step: int -> int -> int -> int -> int -> int -> int) a b c d e f = step a b c d e f',
    'let inline batch ([<InlineIfLambda; OptimizeClosureIfNotInlined>] step: int) reading = step + reading',
    'let inline batch ([<InlineIfLambda; OptimizeClosureIfNotInlined>] step: (int * int) -> int) pair = step pair',
    'let batch ([<InlineIfLambda; OptimizeClosureIfNotInlined>] step: int -> int -> int) state reading = step state reading',
    'let inline batch ([<OptimizeClosureIfNotInlined>] step: int -> int -> int) state reading = step state reading',
    "type Tray =`n    static member inline Batch([<InlineIfLambda; OptimizeClosureIfNotInlined>] step: int -> int -> int, reading: int) = step 0 reading"
  )
  foreach ($declaration in $invalid) {
    [IO.File]::WriteAllText($source, "module Rejected`n$declaration`n")
    Invoke-Compiler $Net11Dotnet ($baseArgs + @('--langversion:11.0', "-o:$output/Rejected.dll", $source)) 1 'FS3916'
  }
  # The two new Roslyn surfaces are unavailable in the actual older package set.
  $oldRoot = Split-Path $Net10Dotnet
  $oldRefs = @(Get-ChildItem (Join-Path $oldRoot 'packs/Microsoft.NETCore.App.Ref/10.0.13/ref/net10.0') -Filter '*.dll' | ForEach-Object { '-r:' + $_.FullName })
  $oldCompiler = Join-Path $oldRoot 'sdk/10.0.402/Roslyn/bincore/csc.dll'
  $oldAssets = Join-Path $root 'samples/net10.0/ReadingConversions/bin/Release/net10.0'
  $roslynRefs = @('Microsoft.CodeAnalysis','Microsoft.CodeAnalysis.CSharp','Microsoft.CodeAnalysis.VisualBasic') | ForEach-Object { '-r:' + (Join-Path $oldAssets "$_.dll") }
  $missing = Join-Path $output 'Unavailable.cs'
  foreach ($call in @(
    'Microsoft.CodeAnalysis.CSharp.CSharpExtensions.GetValueConversion(value)',
    'Microsoft.CodeAnalysis.VisualBasic.VisualBasicExtensions.GetValueConversion(value)'
  )) {
    [IO.File]::WriteAllText($missing, "class Unavailable { static object Read(Microsoft.CodeAnalysis.Operations.ICoalesceOperation value) => $call; }")
    Invoke-Compiler $Net10Dotnet (@($oldCompiler,'-nologo','-target:library',"-out:$output/Unavailable.dll",$missing) + $oldRefs + $roslynRefs) 1 'CS0117'
  }
  [IO.File]::WriteAllText($missing, 'class Unavailable { static object Read(Microsoft.CodeAnalysis.INamedTypeSymbol value) => value.TypeLayout; }')
  Invoke-Compiler $Net10Dotnet (@($oldCompiler,'-nologo','-target:library',"-out:$output/Unavailable.dll",$missing) + $oldRefs + $roslynRefs) 1 'CS1061'

  foreach ($release in 10,11) {
    $exe = if ($release -eq 10) { $Net10Dotnet } else { $Net11Dotnet }
    $version = if ($release -eq 10) { '10.0.402' } else { '11.0.100-rc.2.26504.105' }
    $runtime = if ($release -eq 10) { '10.0.13' } else { '11.0.0-rc.2.26504.105' }
    $hostRoot = Split-Path $exe
    foreach ($file in @(
      (Join-Path $hostRoot "sdk/$version/Roslyn/bincore/csc.dll"),
      (Join-Path $hostRoot "sdk/$version/FSharp/fsc.dll"),
      (Join-Path $hostRoot "packs/Microsoft.NETCore.App.Ref/$runtime/ref/net$release.0/System.Runtime.dll"),
      (Join-Path $hostRoot "shared/Microsoft.NETCore.App/$runtime/System.Private.CoreLib.dll")
    ) + @(Get-ChildItem (Join-Path $root "samples/net$release.0/ReadingConversions/bin/Release/net$release.0") -Filter 'Microsoft.CodeAnalysis*.dll' | ForEach-Object FullName) + @((Join-Path $root "samples/net$release.0/SeedFold/bin/Release/net$release.0/FSharp.Core.dll"))) {
      $observations.Add("SHA256 $file $((Get-FileHash -LiteralPath $file -Algorithm SHA256).Hash.ToLowerInvariant())")
    }
  }
  # Consume the already-built attributed library with the NEW compiler in F#8 mode.
  $consumer = Join-Path $root 'samples/net11.0/SeedFold/SeedFold.fs'
  $library = Join-Path $root 'samples/net11.0/SeedFoldLibrary/bin/Release/net11.0/SeedFoldLibrary.dll'
  $import = Join-Path $output 'Imported.dll'
  Invoke-Compiler $Net11Dotnet ($baseArgs + @('--target:exe','--langversion:8.0',"-r:$library","-o:$import",$consumer))
  Copy-Item (Join-Path $root 'samples/net11.0/SeedFold/bin/Release/net11.0/SeedFold.runtimeconfig.json') ([IO.Path]::ChangeExtension($import,'.runtimeconfig.json'))
  Copy-Item -LiteralPath $core,$library -Destination $output
  Invoke-Compiler $Net11Dotnet @($import)

  if ($Record) {
    $sample = 'samples/net11.0/SeedFold/SeedFold.fsproj'
    $record = Get-Content 'samples/net11.0/SeedFold/verification-evidence.json' -Raw | ConvertFrom-Json -AsHashtable
    $record.kind = 'execution'
    $record.command = $commands -join "`n"
    $record.output = $observations -join "`n"
    $record.scope = 'Actual F#11 optimized separate-consumer IL; F#10 declaration and seven invalid shapes; newer compiler F#8-mode imported library execution. No older-compiler compatibility or performance percentage claim.'
    [IO.File]::WriteAllText((Join-Path $root 'samples/net11.0/SeedFold/contracts.evidence.json'), (ConvertTo-Json $record -Depth 50).Replace("`r`n","`n")+"`n")
  }
  Write-Host 'PASS: paired compiler consumers, hoisted adaptation, diagnostics and imported F#8-mode execution.'
} finally {
  Pop-Location
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previous
  foreach ($scope in 'Process','User','Machine') {
    if ($before[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "PATH changed: $scope" }
  }
}
