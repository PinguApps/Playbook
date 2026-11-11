[CmdletBinding()]
param([Parameter(Mandatory)][string]$Dotnet, [switch]$Record)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
. (Join-Path $PSScriptRoot 'Evidence.ps1')
$sample = 'samples/net11.0/MarshDispatch/MarshDispatch.csproj'
$directory = Join-Path $root 'samples/net11.0/MarshDispatch'
$output = Join-Path $root 'artifacts/marsh-dispatch-aot'
$previous = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
$paths = @{}
foreach ($scope in 'User','Machine') { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH', $scope) }
Push-Location $directory
try {
    $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
    $sdk = (& $Dotnet --version | Out-String).Trim()
    if ($sdk -cne '11.0.100') { throw "Unexpected SDK: $sdk" }
    & $Dotnet publish MarshDispatch.csproj -c Release -r win-x64 -p:PublishAot=true -o $output
    $result = (& (Join-Path $output 'MarshDispatch.exe') | Out-String).Trim()
    if ($result -cne '{"Station":"Reedbank","Birds":5}') { throw "Incorrect generated dispatch: $result" }
    if ($Record) {
        $receipt = [ordered]@{
            kind='compile-and-run'; sample=$sample; target='net11.0'; sdk=$sdk; language='15.0'
            command='eng/Test-MarshDispatch.ps1 -Dotnet <SDK11 executable> -Record'
            scope='Actual Windows x64 NativeAOT publication and execution of generated HybridWebView invocation and request/return JSON. Neutral MAUI asset; no native WebView or iOS execution.'
            recordedAt=[DateTimeOffset]::UtcNow.ToString('O')
            sourceCommit=(& 'C:/Program Files/Git/cmd/git.exe' -C $root rev-parse HEAD).Trim()
            sourceDirty=[bool](& 'C:/Program Files/Git/cmd/git.exe' -C $root status --porcelain -- samples/net11.0/MarshDispatch eng/Test-MarshDispatch.ps1)
            inputs=(Get-EvidenceInputs $root $sample)
            dependencies=@((Get-Content verification.json -Raw | ConvertFrom-Json).dependencies)
            workloads=@(); nativeToolchains=@()
            host=[Runtime.InteropServices.RuntimeInformation]::OSDescription
            sdkInfo=(& $Dotnet --info | Out-String).Trim()
            workloadInfo='Workload-free neutral asset test; Windows native compiler used by actual successful publish.'
            output=$result
        }
        $receipt | ConvertTo-Json -Depth 40 | Set-Content verification-evidence.json
    }
    Write-Host "PASS: actual generated invocation and JSON in Windows NativeAOT: $result"
} finally {
    Pop-Location
    $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previous
    foreach ($scope in 'User','Machine') { if ([Environment]::GetEnvironmentVariable('PATH',$scope) -cne $paths[$scope]) { throw "Unexpected $scope PATH change." } }
}
