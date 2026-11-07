[CmdletBinding()]
param([string]$Dotnet = (Get-Command dotnet -CommandType Application | Select-Object -First 1).Source)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
if (![IO.Path]::IsPathFullyQualified($Dotnet) -or !(Test-Path -LiteralPath $Dotnet -PathType Leaf)) { throw 'Supply an existing absolute dotnet executable path.' }
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
