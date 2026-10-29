[CmdletBinding()]
param([string]$Dotnet = (Get-Command dotnet -CommandType Application).Source)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
. (Join-Path $PSScriptRoot 'Evidence.ps1')
$root = Split-Path $PSScriptRoot -Parent
$paths = @{}
foreach ($scope in 'Process','User','Machine') { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$previous = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  foreach ($major in 10,11) {
    $sample = "samples/net$major.0/CatalogueCompiler/CatalogueCompiler.csproj"
    & (Join-Path $PSScriptRoot 'Record-Evidence.ps1') -Sample $sample -Dotnet $Dotnet -Kind compile
    $directory = Join-Path $root "samples/net$major.0/CatalogueCompiler"
    $record = Get-Content -LiteralPath (Join-Path $directory 'verification-evidence.json') -Raw | ConvertFrom-Json -AsHashtable
    $check = Join-Path $root "artifacts/identity-tooling/compiler-$major"
    New-Item -ItemType Directory -Path $check -Force | Out-Null
    foreach ($file in 'global.json','Directory.Build.props','Directory.Build.targets','Directory.Packages.props') { Copy-Item -LiteralPath (Join-Path $directory $file) -Destination $check }
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'CompilerChecks.cs.txt') -Destination (Join-Path $check 'Program.cs')
    $project = (Get-Content -LiteralPath (Join-Path $directory 'CatalogueCompiler.csproj') -Raw).Replace('<OutputType>Library</OutputType>', '<OutputType>Exe</OutputType>').Replace("<AssemblyName>Catalogue$major</AssemblyName>", '<AssemblyName>CompilerChecks</AssemblyName>')
    $defines = if ($major -eq 10) { '<DefineConstants>PREVIOUS</DefineConstants>' } else { '' }
    $project = $project.Replace('</PropertyGroup>', "$defines</PropertyGroup>").Replace('</Project>', "<ItemGroup><ProjectReference Include=`"$directory/CatalogueCompiler.csproj`" /></ItemGroup></Project>")
    [IO.File]::WriteAllText((Join-Path $check 'CompilerChecks.csproj'), $project)
    Push-Location -LiteralPath $check
    try {
      $arguments = if ($major -eq 11) { @('--', (Join-Path $root 'samples/net10.0/CatalogueCompiler/bin/Release/net10.0/Catalogue10.dll')) } else { @() }
      $output = & $Dotnet run --project CompilerChecks.csproj -c Release @arguments | Out-String
      Write-Host $output.Trim()
    } finally { Pop-Location }
    $installation = Split-Path $Dotnet
    $sdk = $record.sdk
    $files = @("sdk/$sdk/Sdks/Microsoft.NET.Sdk.Razor/source-generators/Microsoft.CodeAnalysis.Razor.Compiler.dll", "sdk/$sdk/Sdks/Microsoft.NET.Sdk.Razor/source-generators/Microsoft.AspNetCore.Razor.Utilities.Shared.dll", "sdk/$sdk/Roslyn/bincore/Microsoft.CodeAnalysis.dll", "sdk/$sdk/Roslyn/bincore/Microsoft.CodeAnalysis.CSharp.dll")
    $tooling = @($files | ForEach-Object { $file = Join-Path $installation $_; @{ file=$file; version=[Diagnostics.FileVersionInfo]::GetVersionInfo($file).ProductVersion; sha256=(Get-FileHash -LiteralPath $file).Hash.ToLowerInvariant() } })
    if (((Get-EvidenceInputs $root $sample) | ConvertTo-Json -Compress) -cne ($record.inputs | ConvertTo-Json -Compress)) { throw 'Compiler inputs changed during verification.' }
    $record.kind = 'execution'
    $record.command = "& '$PSCommandPath' -Dotnet '$Dotnet'"
    $record.scope = 'Real SDK Razor pipeline for valid/invalid input, copied generated text/diagnostics/source spans. Older internal output observed only in this verification harness; canonical old API consumes host-supplied output. Current diagnostic reporting and declaration/stub flags; previous compiled constructor consumer fails against new tooling while rebuilt source compiles. No supported older reader, arbitrary IDE host, rendering, ABI or GA promise.'
    $record.recordedAt = [DateTimeOffset]::UtcNow.ToString('O')
    $record.output = $output.Trim() + "`n" + ($tooling | ConvertTo-Json -Depth 10)
    [IO.File]::WriteAllText((Join-Path $directory 'behavior.evidence.json'), ($record | ConvertTo-Json -Depth 40).Replace("`r`n","`n") + "`n")
  }
  Write-Host 'PASS: actual old/current SDK compiler output, mapped text, diagnostics and source/binary constructor compatibility; PATH unchanged.'
} finally {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previous
  foreach ($scope in $paths.Keys) { if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." } }
}
