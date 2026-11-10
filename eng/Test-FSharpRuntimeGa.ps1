[CmdletBinding()]
param(
  [string]$Net11Dotnet = (Get-Command dotnet -CommandType Application | Select-Object -First 1).Source
)

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$scratch = Join-Path ([IO.Path]::GetTempPath()) ('playbook-runtime-async-' + [guid]::NewGuid().ToString('N'))
$paths = @{}
foreach ($scope in @('Process', 'User', 'Machine')) { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH', $scope) }
$previousIntegration = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
$previousNative = $PSNativeCommandUseErrorActionPreference
New-Item -ItemType Directory $scratch | Out-Null
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  $PSNativeCommandUseErrorActionPreference = $false
  Copy-Item (Join-Path $root 'samples/net11.0/FSharpRuntime/global.json') $scratch
  Copy-Item (Join-Path $root 'NuGet.Config') $scratch
  Push-Location $scratch
  try {
    $cases = @(
      @{ Name = 'preview gate'; Target = 'net11.0'; Language = '11.0'; Pattern = 'FS3350'; Body = 'let value () = __runtimeAsyncReturn 17' },
      @{ Name = 'unsupported target asset gate'; Target = 'net9.0'; Language = 'preview'; Pattern = 'FS0039|FS3351'; Body = 'let value (pending: Task<int>) = __runtimeAsyncReturn (AsyncHelpers.Await pending)' },
      @{ Name = 'reference capability gate with explicit net10 Core asset'; Target = 'net9.0'; Language = 'preview'; Pattern = 'FS3351'; Body = 'let value () = __runtimeAsyncReturn 17'; ForceCore = $true },
      @{ Name = 'byref across suspension'; Target = 'net11.0'; Language = 'preview'; Pattern = 'FS3919|FS0406'; Body = @'
let value (pending: Task<int>) = __runtimeAsyncReturn (
    let mutable depth = 17
    let address = &depth
    let correction = AsyncHelpers.Await pending
    address + correction)
'@ },
      @{ Name = 'byref-like across suspension'; Target = 'net11.0'; Language = 'preview'; Pattern = 'FS3919'; Body = @'
let value (pending: Task<int>) = __runtimeAsyncReturn (
    let depths = Span<int>(Array.zeroCreate 2)
    let correction = AsyncHelpers.Await pending
    depths[0] + correction)
'@ },
      @{ Name = 'pinned local across suspension'; Target = 'net11.0'; Language = 'preview'; Pattern = 'FS3919'; Body = @'
let value (pending: Task<int>) (depths: int array) = __runtimeAsyncReturn (
    use address = fixed depths
    let correction = AsyncHelpers.Await pending
    NativePtr.get address 0 + correction)
'@ },
      @{ Name = 'suspension outside marked method'; Target = 'net11.0'; Language = 'preview'; Pattern = 'FS\d+.*[Rr]untime.async'; Body = 'let value (pending: Task<int>) = AsyncHelpers.Await pending' }
    )
    foreach ($case in $cases) {
      $compileAsset = if ($case.ForceCore) {
        '<Reference Include="FSharp.Core"><HintPath>{0}</HintPath></Reference>' -f (Join-Path $root 'artifacts/fsharp-packages/fsharp.core/11.0.101/lib/net10.0/FSharp.Core.dll')
      } else { '<PackageReference Include="FSharp.Core" Version="[11.0.101]" />' }
      @"
<Project Sdk="Microsoft.NET.Sdk">
 <PropertyGroup><TargetFramework>$($case.Target)</TargetFramework><LangVersion>$($case.Language)</LangVersion>
 <DisableImplicitFSharpCoreReference>true</DisableImplicitFSharpCoreReference><DisableImplicitLibraryPacksFolder>true</DisableImplicitLibraryPacksFolder></PropertyGroup>
 <ItemGroup>$compileAsset<Compile Include="Probe.fs" /></ItemGroup>
</Project>
"@ | Set-Content Probe.fsproj
      ("module Probe`n#nowarn `"9`"`nopen System`nopen System.Threading.Tasks`nopen System.Runtime.CompilerServices`nopen Microsoft.FSharp.Core.CompilerServices.StateMachineHelpers`nopen Microsoft.FSharp.NativeInterop`n" + $case.Body) | Set-Content Probe.fs
      $output = & $Net11Dotnet build Probe.fsproj -c Release -t:Rebuild --nologo 2>&1 | Out-String
      if ($LASTEXITCODE -eq 0 -or $output -notmatch $case.Pattern) { throw "$($case.Name): $output" }
      $output -split '\r?\n' | Where-Object { $_ -match 'error FS' } | Select-Object -Unique | Write-Host
      Write-Host "PASS: $($case.Name)"
    }
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
