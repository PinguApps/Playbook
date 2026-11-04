param(
    [Parameter(Mandatory)][string] $Submission,
    [Parameter(Mandatory)][string] $Dotnet,
    [switch] $Reference
)
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path -LiteralPath $Submission).Path
$workspace = Join-Path ([IO.Path]::GetTempPath()) ('manifest-oracle-' + [guid]::NewGuid())
$paths = @{}
foreach ($scope in 'User','Machine') { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH', $scope) }
try {
    Get-ChildItem -LiteralPath $root -Recurse -File -Force | ForEach-Object {
        $relative = [IO.Path]::GetRelativePath($root, $_.FullName)
        if ($relative -match '(^|[\\/])(bin|obj|\.git)([\\/]|$)') { return }
        $destination = Join-Path $workspace $relative
        New-Item -ItemType Directory -Force (Split-Path $destination) | Out-Null
        Copy-Item -LiteralPath $_.FullName -Destination $destination
    }
    Copy-Item (Join-Path $PSScriptRoot 'Oracle.cs') (Join-Path $workspace 'Program.cs')
    if ($Reference) { Copy-Item (Join-Path $PSScriptRoot 'Reference.cs') (Join-Path $workspace 'Reconciliation.cs') }
    $start = [Diagnostics.ProcessStartInfo]::new($Dotnet)
    $start.WorkingDirectory = $workspace
    $start.UseShellExecute = $false
    $start.Environment['DOTNET_ADD_GLOBAL_TOOLS_TO_PATH'] = '0'
    foreach ($arg in @('run','--project','Task.csproj','-c','Release')) { $start.ArgumentList.Add($arg) }
    $process = [Diagnostics.Process]::Start($start)
    if (!$process.WaitForExit(120000)) { $process.Kill($true); throw 'Oracle timeout' }
    if ($process.ExitCode -ne 0) { throw 'Oracle failed; retain diagnostics, do not repair a submission' }
} finally {
    foreach ($scope in 'User','Machine') {
        if ([Environment]::GetEnvironmentVariable('PATH', $scope) -cne $paths[$scope]) { Write-Error "Unexpected $scope PATH change" }
    }
    Write-Host "Disposable source retained at $workspace"
}
