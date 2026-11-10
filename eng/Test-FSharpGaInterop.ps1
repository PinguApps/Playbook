[CmdletBinding()]
param([Parameter(Mandatory)][string]$Net11Dotnet)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$scratch = Join-Path ([IO.Path]::GetTempPath()) ('playbook-ga-interop-' + [guid]::NewGuid().ToString('N'))
$paths = @{}
foreach ($scope in 'Process','User','Machine') { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$previous = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
$native = $PSNativeCommandUseErrorActionPreference
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  $PSNativeCommandUseErrorActionPreference = $false
  New-Item $scratch -ItemType Directory | Out-Null
  Copy-Item (Join-Path $root 'samples/net11.0/FSharpRuntime/global.json') $scratch
  Copy-Item (Join-Path $root 'NuGet.Config') $scratch
  $runtime = Join-Path $root 'samples/net11.0/FSharpRuntime/FSharpRuntime.fsproj'
  @"
<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net11.0</TargetFramework><LangVersion>preview</LangVersion><OutputType>Exe</OutputType><DisableImplicitFSharpCoreReference>true</DisableImplicitFSharpCoreReference><DisableImplicitLibraryPacksFolder>true</DisableImplicitLibraryPacksFolder></PropertyGroup><ItemGroup><PackageReference Include="FSharp.Core" Version="[11.0.101]" /><ProjectReference Include="$runtime" /><Compile Include="Probe.fs" /></ItemGroup></Project>
"@ | Set-Content (Join-Path $scratch 'Probe.fsproj')
  @'
module Probe
open System.Threading.Tasks
open Playbook.Samples.RuntimeWorkflow
let pending = TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously)
let run () =
    surveyTask {
        do! (pending.Task :> Task)
        return 1
    }
try
    let work = run ()
    pending.SetResult 0
    work.GetAwaiter().GetResult() |> ignore
    failwith "Upcast limitation changed; reassess guidance"
with :? System.InvalidProgramException -> printfn "PASS: retained InvalidProgramException for pending generic Task upcast"
'@ | Set-Content (Join-Path $scratch 'Probe.fs')
  Push-Location $scratch
  try {
    & $Net11Dotnet run --project Probe.fsproj -c Release
    if ($LASTEXITCODE -ne 0) { throw 'Upcast regression probe failed' }
    $api = Join-Path $root 'samples/net11.0/FSharpApiPreview/FSharpApiPreview.fsproj'
    & $Net11Dotnet build $api -c Release -p:RestoreLockedMode=true
    if ($LASTEXITCODE -ne 0) { throw 'Extension witness library build failed' }
    $dll = Join-Path (Split-Path $api) 'bin/Release/net11.0/FSharpApiPreview.dll'
    @"
#r @"$dll"
open System
open Playbook.Samples.ApiPreview
open Playbook.Samples.ApiPreview.HarbourExtensions
let inline forwarded value = Manifest.width value;;
if forwarded (Uri("https://reed.example/dock")) <> 12 then failwith "First fragment";;
if forwarded (Uri("https://moss.example/dock")) <> 12 then failwith "Later fragment";;
printfn "PASS: extension witness survives inline calls and FSI fragments";;
"@ | Set-Content Witness.fsx
    & $Net11Dotnet fsi --langversion:preview --exec Witness.fsx
    if ($LASTEXITCODE -ne 0) { throw 'FSI witness probe failed' }
  }
  finally { Pop-Location }
}
finally {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previous
  $PSNativeCommandUseErrorActionPreference = $native
  $resolved = [IO.Path]::GetFullPath($scratch)
  if (-not $resolved.StartsWith([IO.Path]::GetFullPath([IO.Path]::GetTempPath()),[StringComparison]::OrdinalIgnoreCase)) { throw 'Unexpected scratch path' }
  if (Test-Path -LiteralPath $resolved) { Remove-Item -LiteralPath $resolved -Recurse -Force }
  foreach ($scope in $paths.Keys) { if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change" } }
}
