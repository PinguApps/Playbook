[CmdletBinding()]
param([string]$Dotnet = 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe')
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$before = @{}
foreach ($scope in 'Process','User','Machine') { $before[$scope] = [Environment]::GetEnvironmentVariable('PATH', $scope) }
$previous = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
try {
    $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
    foreach ($release in 10,11) {
        Push-Location -LiteralPath (Join-Path $PSScriptRoot "../samples/net$release.0/MarshAccess")
        try { & $Dotnet run --configuration Release -p:RestoreLockedMode=true }
        finally { Pop-Location }
    }
}
finally {
    $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previous
    foreach ($scope in 'Process','User','Machine') {
        if ($before[$scope] -cne [Environment]::GetEnvironmentVariable('PATH', $scope)) { throw "PATH changed at $scope scope; investigate without overwriting shared settings." }
    }
}
