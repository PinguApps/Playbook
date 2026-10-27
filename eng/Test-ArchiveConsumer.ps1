[CmdletBinding()]
param(
  [string]$Dotnet = (Get-Command dotnet -CommandType Application | Select-Object -First 1).Source,
  [string]$Net11Dotnet = 'C:\Users\pingu\.dotnet-playbook-11\dotnet.exe',
  [switch]$RecordEvidence
)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
. (Join-Path $PSScriptRoot 'Evidence.ps1')
$git = (Get-Command git -CommandType Application | Select-Object -First 1).Source
$sample = 'samples/net11.0/ArchiveContracts/ArchiveContracts.csproj'
$before = Get-EvidenceInputs $root $sample
$paths = @{}
foreach ($scope in @('Process', 'User', 'Machine')) { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH', $scope) }
$previous = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
$temporary = Join-Path ([IO.Path]::GetTempPath()) ('archive-consumer-' + [guid]::NewGuid().ToString('N'))
$server = $null
Push-Location (Join-Path $root 'samples/net11.0/ArchiveContracts')
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  New-Item -ItemType Directory -Path $temporary | Out-Null
  & $Net11Dotnet build ArchiveContracts.csproj -c Release -p:RestoreLockedMode=true | Out-Host
  $start = [Diagnostics.ProcessStartInfo]::new($Net11Dotnet)
  $start.ArgumentList.Add((Join-Path $PWD 'bin/Release/net11.0/ArchiveContracts.dll'))
  $start.UseShellExecute = $false
  $start.CreateNoWindow = $true
  $start.RedirectStandardOutput = $true
  $start.RedirectStandardError = $true
  $start.Environment['DOTNET_ADD_GLOBAL_TOOLS_TO_PATH'] = '0'
  $start.Environment['ARCHIVE_CONTRACT_SERVE'] = '1'
  $start.Environment['ARCHIVE_CONTRACT_OUTPUT'] = $temporary
  $server = [Diagnostics.Process]::Start($start)
  $errors = $server.StandardError.ReadToEndAsync()
  $line = $server.StandardOutput.ReadLineAsync().WaitAsync([TimeSpan]::FromSeconds(45)).GetAwaiter().GetResult()
  if ($line -notmatch '^ARCHIVE_ADDRESS=(http://127\.0\.0\.1:\d+)$') { throw "Server failed to start: $line" }
  $address = $Matches[1]
  $toolDirectory = Join-Path $root 'artifacts/archive-client/tools'
  $tool = Join-Path $toolDirectory $(if ($IsWindows) { 'nswag.exe' } else { 'nswag' })
  Push-Location $root
  try {
    if (-not (Test-Path -LiteralPath $tool)) {
      & $Dotnet tool install NSwag.ConsoleCore --version 14.7.1 --tool-path $toolDirectory | Out-Host
    }
  } finally { Pop-Location }
  $version = & $tool version | Out-String
  if ($version -notmatch '14\.7\.1') { throw 'Expected NSwag 14.7.1.' }
  & $tool openapi2csclient "/Input:$temporary/openapi.json" "/Output:$temporary/Client.cs" /ClassName:ArchiveGenerated /Namespace:ArchiveConsumer /JsonLibrary:SystemTextJson /UseBaseUrl:false /OperationGenerationMode:SingleClientFromOperationId | Out-Host
  # Independent .NET 10 consumer, outside repository/shared project inheritance.
  Copy-Item -LiteralPath (Join-Path $root 'samples/net11.0/ArchiveContracts/GeneratedConsumer.cs.txt') -Destination (Join-Path $temporary 'Program.cs')
  [IO.File]::WriteAllText((Join-Path $temporary 'Consumer.csproj'), '<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings><Nullable>disable</Nullable><TreatWarningsAsErrors>true</TreatWarningsAsErrors></PropertyGroup></Project>')
  [IO.File]::WriteAllText((Join-Path $temporary 'global.json'), '{"sdk":{"version":"10.0.402","rollForward":"disable"}}')
  Push-Location $temporary
  try { $result = & $Dotnet run -c Release -- $address | Out-String; Write-Host $result }
  finally { Pop-Location }
  $after = Get-EvidenceInputs $root $sample
  if ((ConvertTo-Json $before -Compress) -cne (ConvertTo-Json $after -Compress)) { throw 'Consumer inputs changed during execution.' }
  if ($RecordEvidence) {
    $manifest = Get-Content -LiteralPath verification.json -Raw | ConvertFrom-Json
    $record = [ordered]@{
      kind = 'execution'; sample = $sample; target = 'net11.0'; sdk = '11.0.100-rc.2.26504.105'; language = '15.0'
      command = "eng/Test-ArchiveConsumer.ps1 -Dotnet '$Dotnet' -Net11Dotnet '$Net11Dotnet' -RecordEvidence"
      scope = 'Actual server OpenAPI 3.0 export, NSwag 14.7.1 generated C#, independent .NET 10 compilation and real loopback consumer; JSON extension data and expected text limitation.'
      recordedAt = [DateTimeOffset]::UtcNow.ToString('O'); sourceCommit = (& $git -C $root rev-parse HEAD).Trim(); sourceDirty = [bool](& $git -C $root status --porcelain -- samples eng/Test-ArchiveConsumer.ps1)
      inputs = $after; dependencies = @($manifest.dependencies); workloads = @(); nativeToolchains = @()
      host = [Runtime.InteropServices.RuntimeInformation]::OSDescription; sdkInfo = (& $Net11Dotnet --info | Out-String).Trim(); workloadInfo = (& $Net11Dotnet workload list | Out-String).Trim()
      output = $result.Trim() + "`nOpenAPI SHA256=" + (Get-FileHash -LiteralPath (Join-Path $temporary 'openapi.json')).Hash.ToLowerInvariant() + "`nGenerated C# SHA256=" + (Get-FileHash -LiteralPath (Join-Path $temporary 'Client.cs')).Hash.ToLowerInvariant()
    }
    [IO.File]::WriteAllText((Join-Path $PWD 'generated-client.evidence.json'), (ConvertTo-Json $record -Depth 30).Replace("`r`n", "`n") + "`n")
  }
}
finally {
  if ($server) {
    if (-not $server.HasExited) { $server.Kill($true); $server.WaitForExit() }
    $diagnostics = $errors.GetAwaiter().GetResult()
    if ($diagnostics) { Write-Host $diagnostics }
    $server.Dispose()
  }
  Pop-Location
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previous
  # The random temporary directory is intentionally outside the repository.
  $resolvedTemporary = [IO.Path]::GetFullPath($temporary)
  $expectedParent = [IO.Path]::GetFullPath([IO.Path]::GetTempPath()).TrimEnd([IO.Path]::DirectorySeparatorChar)
  if ((Split-Path $resolvedTemporary -Parent) -cne $expectedParent -or (Split-Path $resolvedTemporary -Leaf) -notmatch '^archive-consumer-[a-f0-9]{32}$') { throw "Refusing cleanup outside the expected temporary directory: $resolvedTemporary" }
  Remove-Item -LiteralPath $temporary -Recurse -Force
  foreach ($scope in $paths.Keys) {
    if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH', $scope)) { throw "Unexpected $scope PATH change; inspect shared settings." }
  }
}
