[CmdletBinding()]
param([string]$Dotnet = 'C:\Users\pingu\.dotnet-playbook-11\dotnet.exe')

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$session = Join-Path ([IO.Path]::GetTempPath()) ('playbook-ef-adapter-' + [guid]::NewGuid().ToString('N'))
$previous = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
$paths = @{}
foreach ($scope in 'Process','User','Machine') { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
New-Item -ItemType Directory -Path $session | Out-Null
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  Get-ChildItem -LiteralPath (Join-Path $root 'samples/net11.0/FenWatch') -File |
    Where-Object Name -NotIn @('verification.json','verification-evidence.json') |
    Copy-Item -Destination $session
  Copy-Item -LiteralPath (Join-Path $root 'NuGet.Config') -Destination $session
  $sourcePath = Join-Path $session 'Program.cs'
  $source = [IO.File]::ReadAllText($sourcePath).Replace("#pragma warning disable EF1004`n",'').Replace("#pragma warning restore EF1004`n",'')
  [IO.File]::WriteAllText($sourcePath,$source)
  Push-Location -LiteralPath $session
  try {
    $output = & $Dotnet build FenWatch.csproj -c Release -p:RestoreLockedMode=true 2>&1 | Out-String
    if ($LASTEXITCODE -eq 0 -or $output -notmatch 'error EF1004:') { throw "Expected real EF1004 rejection: $output" }
    Write-Host 'PASS: actual EF11 analyzer rejects ToAsyncEnumerable on the EF IQueryable.'
    [IO.File]::WriteAllText($sourcePath,$source.Replace('query.ToAsyncEnumerable()','query.AsAsyncEnumerable()'))
    $output = & $Dotnet build FenWatch.csproj -c Release -p:RestoreLockedMode=true 2>&1 | Out-String
    if ($LASTEXITCODE -ne 0 -or $output -match 'error EF1004:') { throw "Replacement failed: $output" }
    Write-Host 'PASS: replacing the adapter with AsAsyncEnumerable compiles without the suppression.'
  }
  finally { Pop-Location }
}
finally {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previous
  foreach ($scope in $paths.Keys) {
    if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." }
  }
  # Keep the unique temporary reproduction for inspection.
}
