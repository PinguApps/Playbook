[CmdletBinding()]
param(
  [string]$Net11Dotnet = 'C:\Users\pingu\.dotnet-playbook-11\dotnet.exe'
)

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$scratch = Join-Path ([IO.Path]::GetTempPath()) ('playbook-fsharp-lifetimes-' + [guid]::NewGuid().ToString('N'))
$paths = @{}
foreach ($scope in @('Process', 'User', 'Machine')) { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH', $scope) }
$previousIntegration = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
$previousNative = $PSNativeCommandUseErrorActionPreference
if (-not [IO.Path]::IsPathFullyQualified($Net11Dotnet) -or [IO.Path]::GetFileNameWithoutExtension($Net11Dotnet) -ne 'dotnet') { throw 'Supply an absolute dotnet host path.' }
New-Item -ItemType Directory $scratch | Out-Null
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  $PSNativeCommandUseErrorActionPreference = $false
  Copy-Item (Join-Path $root 'samples/net11.0/FSharpSequence/global.json') $scratch
  Copy-Item (Join-Path $root 'NuGet.Config') $scratch
  Push-Location $scratch
  try {
    $core = Join-Path $root 'artifacts/fsharp-packages/fsharp.core/11.0.101-rc2.26504.105/lib/net10.0/FSharp.Core.dll'
    $cases = @(
      @{ Name = 'sequence preview gate'; Target = 'net11.0'; Language = '11.0'; Pattern = 'FS3350'; Body = 'let values () = __runtimeAsyncSequence (fun () -> seq { yield 31 })' },
      @{ Name = 'sequence runtime reference gate'; Target = 'net9.0'; Language = 'preview'; Pattern = 'FS3351'; Body = 'let values () = __runtimeAsyncSequence (fun () -> seq { yield 31 })' },
      @{ Name = 'opaque sequence recipe'; Target = 'net11.0'; Language = 'preview'; Pattern = 'FS\d+.*([Ss]tatical|[Rr]untime.async|[Ss]equence)'; Body = "let opaque () = seq { yield 31 }`nlet values () = __runtimeAsyncSequence opaque" },
      @{ Name = 'optimized tail handoff'; Target = 'net11.0'; Language = 'preview'; Pattern = 'FS\d+.*([Tt]ail|[Rr]untime.async|[Ss]equence)'; Body = 'let values (source: seq<int>) = __runtimeAsyncSequence (fun () -> seq { yield! source })' }
    )
    foreach ($case in $cases) {
      @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup><TargetFramework>$($case.Target)</TargetFramework><LangVersion>$($case.Language)</LangVersion><DisableImplicitFSharpCoreReference>true</DisableImplicitFSharpCoreReference></PropertyGroup>
  <ItemGroup><Reference Include="FSharp.Core"><HintPath>$core</HintPath></Reference><Compile Include="Probe.fs" /></ItemGroup>
</Project>
"@ | Set-Content Probe.fsproj
      ("module Probe`nopen Microsoft.FSharp.Core.CompilerServices.StateMachineHelpers`n" + $case.Body) | Set-Content Probe.fs
      $output = & $Net11Dotnet build Probe.fsproj -c Release -t:Rebuild --disable-build-servers --nologo 2>&1 | Out-String
      if ($LASTEXITCODE -eq 0 -or $output -notmatch $case.Pattern) { throw "$($case.Name): $output" }
      $output -split '\r?\n' | Where-Object { $_ -match 'error FS' } | Select-Object -Unique | Write-Host
      Write-Host "PASS: $($case.Name)"
    }

    # Execute the original based checks on the actual newer net10.0 package asset under JIT.
    Copy-Item (Join-Path $root 'samples/net10.0/FSharpArrayAot/Grid.fs') Probe.fs
    @"
<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net11.0</TargetFramework><OutputType>Exe</OutputType><DisableImplicitFSharpCoreReference>true</DisableImplicitFSharpCoreReference></PropertyGroup><ItemGroup><Reference Include="FSharp.Core"><HintPath>$core</HintPath></Reference><Compile Include="Probe.fs" /></ItemGroup></Project>
"@ | Set-Content Probe.fsproj
    & $Net11Dotnet run --project Probe.fsproj -c Release --disable-build-servers
    if ($LASTEXITCODE -ne 0) { throw 'Newer JIT based-array comparison failed' }
    Write-Host 'PASS: based semantics and all helpers on the actual newer package asset (JIT only)'

    # All three annotated public calls, including literal zero bases, must fail native analysis.
    @'
module Probe
[<EntryPoint>]
let main _ =
    let a = Array2D.zeroCreateBased<int> 0 0 1 1
    let b = Array2D.createBased 0 0 1 1 7
    let c = Array2D.initBased 0 0 1 1 (fun row column -> row + column)
    System.Console.WriteLine(a[0,0] + b[0,0] + c[0,0])
    0
'@ | Set-Content Probe.fs
    $output = & $Net11Dotnet publish Probe.fsproj -c Release -r win-x64 -p:PublishAot=true -p:TreatWarningsAsErrors=true -p:IlcTreatWarningsAsErrors=true -p:IlcSingleWarn=false --disable-build-servers 2>&1 | Out-String
    if ($LASTEXITCODE -eq 0) { throw 'Annotated Based calls unexpectedly published' }
    foreach ($method in @('ZeroCreateBased', 'CreateBased', 'InitializeBased')) {
      if ($output -notmatch "IL3050:.*Array2DModule\.$method") { throw "Missing $method call-site diagnostic: $output" }
    }
    $output -split '\r?\n' | Where-Object { $_ -match 'IL3050:' } | Write-Host
    Write-Host 'PASS: IL3050 errors for all three Based calls with literal zero bases'
  }
  finally { Pop-Location }
}
finally {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previousIntegration
  $PSNativeCommandUseErrorActionPreference = $previousNative
  $resolved = [IO.Path]::GetFullPath($scratch)
  if (-not $resolved.StartsWith([IO.Path]::GetFullPath([IO.Path]::GetTempPath()), [StringComparison]::OrdinalIgnoreCase)) { throw 'Unexpected scratch path' }
  Remove-Item -LiteralPath $resolved -Recurse -Force
  foreach ($scope in $paths.Keys) {
    if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH', $scope)) { throw "Unexpected $scope PATH change" }
  }
}
