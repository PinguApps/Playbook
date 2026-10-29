[CmdletBinding()]
param(
  [string]$Net10Dotnet = 'C:/Program Files/dotnet/dotnet.exe',
  [string]$Net11Dotnet = 'C:/Users/pingu/.dotnet-playbook-11/dotnet.exe'
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
. (Join-Path $PSScriptRoot 'Evidence.ps1')
$root = Split-Path $PSScriptRoot -Parent
$paths = @{}
foreach ($scope in 'Process','User','Machine') { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$previous = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
# The shared capture helper's Git calls resolve to this absolute executable.
function git { & 'C:/Program Files/Git/cmd/git.exe' @args }
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  foreach ($major in 10,11) {
    $dotnet = if ($major -eq 10) { $Net10Dotnet } else { $Net11Dotnet }
    $sample = "samples/net$major.0/MapCompiler/MapCompiler.csproj"
    & (Join-Path $PSScriptRoot 'Record-Evidence.ps1') -Sample $sample -Dotnet $dotnet -Kind compile
    $directory = Join-Path $root "samples/net$major.0/MapCompiler"
    $record = Get-Content -LiteralPath (Join-Path $directory 'verification-evidence.json') -Raw | ConvertFrom-Json -AsHashtable
    $check = Join-Path $root "artifacts/map-compiler/check-$major"
    New-Item -ItemType Directory -Path $check -Force | Out-Null
    foreach ($file in 'global.json','Directory.Build.props','Directory.Build.targets','Directory.Packages.props') { Copy-Item -LiteralPath (Join-Path $directory $file) -Destination $check }
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'MapCompilerChecks.cs.txt') -Destination (Join-Path $check 'Program.cs')
    $project = (Get-Content -LiteralPath (Join-Path $directory 'MapCompiler.csproj') -Raw).Replace('<OutputType>Library</OutputType>', '<OutputType>Exe</OutputType>').Replace("<AssemblyName>Map$major</AssemblyName>", '<AssemblyName>MapChecks</AssemblyName>')
    $defines = if ($major -eq 10) { '<DefineConstants>PREVIOUS</DefineConstants>' } else { '' }
    $project = $project.Replace('</PropertyGroup>', "$defines</PropertyGroup>").Replace('</Project>', "<ItemGroup><ProjectReference Include=`"$directory/MapCompiler.csproj`" /></ItemGroup></Project>")
    [IO.File]::WriteAllText((Join-Path $check 'MapChecks.csproj'), $project)
    Push-Location -LiteralPath $check
    try { $output = & $dotnet run --project MapChecks.csproj -c Release | Out-String }
    finally { Pop-Location }
    Write-Host $output.Trim()
    $installation = Split-Path $dotnet
    $files = @("sdk/$($record.sdk)/Sdks/Microsoft.NET.Sdk.Razor/source-generators/Microsoft.CodeAnalysis.Razor.Compiler.dll", "sdk/$($record.sdk)/Sdks/Microsoft.NET.Sdk.Razor/source-generators/Microsoft.AspNetCore.Razor.Utilities.Shared.dll", "sdk/$($record.sdk)/Roslyn/bincore/Microsoft.CodeAnalysis.dll", "sdk/$($record.sdk)/Roslyn/bincore/Microsoft.CodeAnalysis.CSharp.dll")
    $tooling = @($files | ForEach-Object { $file = Join-Path $installation $_; @{ file=$file; version=[Diagnostics.FileVersionInfo]::GetVersionInfo($file).ProductVersion; sha256=(Get-FileHash -LiteralPath $file).Hash.ToLowerInvariant() } })
    if (((Get-EvidenceInputs $root $sample) | ConvertTo-Json -Compress) -cne ($record.inputs | ConvertTo-Json -Compress)) { throw 'Map compiler inputs changed during verification.' }
    $record.kind = 'execution'
    $record.command = "& '$PSCommandPath' -Net10Dotnet '$Net10Dotnet' -Net11Dotnet '$Net11Dotnet'"
    $record.scope = 'Real old/current Razor pipeline, original-node preservation, isolated markup/token/container mutation, shared immutable annotation/diagnostic values, common state and unsupported-node refusal; actual generated C# compiled and executed for baseline/transformed maps. Current protected shell hook and pinned internal recursion/missing-hook failure are verification-only observations. No public generic clone, arbitrary tree/IDE, native, MVC/browser or GA claim.'
    $record.recordedAt = [DateTimeOffset]::UtcNow.ToString('O')
    $record.output = $output.Trim() + "`n" + ($tooling | ConvertTo-Json -Depth 10)
    [IO.File]::WriteAllText((Join-Path $directory 'behavior.evidence.json'), ($record | ConvertTo-Json -Depth 40).Replace("`r`n","`n") + "`n")
  }
  Write-Host 'PASS: matching old/current Razor map consumers, copy boundaries, real generated execution and failure controls; PATH unchanged.'
} finally {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previous
  foreach ($scope in $paths.Keys) { if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." } }
}
