[CmdletBinding()]
param(
  [string]$Net10Dotnet = 'C:\Program Files\dotnet\dotnet.exe',
  [string]$Net11Dotnet = 'C:\Users\pingu\.dotnet-playbook-11\dotnet.exe',
  [switch]$Record
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
if (-not $IsWindows) { throw 'This authoring check uses Windows x64 host layouts; no other platform execution is claimed.' }
. (Join-Path $PSScriptRoot 'Evidence.ps1')
$root = Split-Path $PSScriptRoot -Parent
$scratch = Join-Path ([IO.Path]::GetTempPath()) ('playbook-runtime-overrides-' + [guid]::NewGuid().ToString('N'))
$null = New-Item -ItemType Directory -Path $scratch
$paths = @{}
foreach ($scope in 'Process','User','Machine') { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$log = [Collections.Generic.List[string]]::new()

function Invoke-Check([string]$Exe, [string]$Directory, [string[]]$Arguments, [hashtable]$Environment = @{}) {
  $start = [Diagnostics.ProcessStartInfo]::new($Exe)
  $start.WorkingDirectory = $Directory
  $start.UseShellExecute = $false
  $start.CreateNoWindow = $true
  $start.RedirectStandardOutput = $true
  $start.RedirectStandardError = $true
  foreach ($argument in $Arguments) { $start.ArgumentList.Add($argument) }
  $start.Environment['DOTNET_ADD_GLOBAL_TOOLS_TO_PATH'] = '0'
  $start.Environment['DOTNET_CLI_HOME'] = Join-Path $scratch 'home'
  $start.Environment['NUGET_PACKAGES'] = Join-Path $scratch 'packages'
  $start.Environment['DOTNET_NOLOGO'] = '1'
  $start.Environment['DOTNET_CLI_USE_MSBUILD_SERVER'] = 'false'
  $start.Environment['MSBUILDUSESERVER'] = '0'
  foreach ($key in $Environment.Keys) { $start.Environment[$key] = $Environment[$key] }
  $process = [Diagnostics.Process]::Start($start)
  $stdout = $process.StandardOutput.ReadToEndAsync()
  $stderr = $process.StandardError.ReadToEndAsync()
  if (-not $process.WaitForExit(120000)) { $process.Kill($true); throw 'Host check timed out.' }
  $output = $stdout.GetAwaiter().GetResult() + $stderr.GetAwaiter().GetResult()
  $log.Add("cwd=$Directory`n$Exe $($Arguments -join ' ')`nexit=$($process.ExitCode)`n$output")
  [IO.File]::WriteAllText((Join-Path $scratch 'commands.log'),($log -join "`n"))
  if ($process.ExitCode -ne 0) { throw $output }
  $output
}

function Assert-Text([string]$Actual, [string]$Expected) {
  if (-not $Actual.Contains($Expected,[StringComparison]::Ordinal)) { throw "Missing '$Expected': $Actual" }
}

try {
  foreach ($release in '10','11') {
    $dotnet = if ($release -eq '10') { $Net10Dotnet } else { $Net11Dotnet }
    $dotnet = (Get-Command $dotnet -CommandType Application).Source
    $runtime = if ($release -eq '10') { '10.0.13' } else { '11.0.0-rc.2.26504.105' }
    $sdk = if ($release -eq '10') { '10.0.402' } else { '11.0.100-rc.2.26504.105' }
    $sample = "samples/net$release.0/TidalDiagnostics/TidalDiagnostics.csproj"
    $source = Split-Path (Join-Path $root $sample)
    $inputs = Get-EvidenceInputs $root $sample
    $firstLog = $log.Count
    # Only this hostfxr and runtime are visible to the muxer. A net10 app on
    # hostfxr 11 would test the new parser, even if it selected CoreCLR 10.
    $hostRoot = Join-Path $scratch "host-$release"
    $null = New-Item -ItemType Directory -Path "$hostRoot/host/fxr","$hostRoot/shared/Microsoft.NETCore.App"
    Copy-Item -LiteralPath $dotnet -Destination "$hostRoot/dotnet.exe"
    Copy-Item -LiteralPath (Join-Path (Split-Path $dotnet) "host/fxr/$runtime") -Destination "$hostRoot/host/fxr" -Recurse
    $null = New-Item -ItemType Junction -Path "$hostRoot/shared/Microsoft.NETCore.App/$runtime" -Target (Join-Path (Split-Path $dotnet) "shared/Microsoft.NETCore.App/$runtime")
    $hostExe = Join-Path $hostRoot 'dotnet.exe'
    $log.Add("hostfxr=$runtime;sha256=$((Get-FileHash -LiteralPath "$hostRoot/host/fxr/$runtime/hostfxr.dll").Hash.ToLowerInvariant())")

    $work = Join-Path $scratch "source-$release"
    $null = New-Item -ItemType Directory -Path $work
    Get-ChildItem -LiteralPath $source -File | Where-Object { $_.Extension -in '.cs','.csproj','.json' -and $_.Name -notlike '*evidence.json' } | Copy-Item -Destination $work
    # Preserve the effective sample build/package boundary in the detached consumer.
    Copy-Item -LiteralPath (Join-Path $root 'samples/Directory.Build.props'),(Join-Path $root 'samples/Directory.Packages.props'),(Join-Path $root 'samples/Directory.Build.targets') -Destination $work
    Assert-Text (Invoke-Check $dotnet $work @('--version')) $sdk
    $null = Invoke-Check $dotnet $work @('build','-c','Release')
    $build = Join-Path $work "bin/Release/net$release.0"
    $dll = Join-Path $build 'TidalDiagnostics.dll'
    $config = Join-Path $build 'TidalDiagnostics.runtimeconfig.json'
    $dev = Join-Path $build 'TidalDiagnostics.runtimeconfig.dev.json'
    if (Test-Path -LiteralPath $dev) { throw 'Default build unexpectedly generated a dev sidecar; reconcile the guidance.' }
    $production = Get-Content -LiteralPath $config -Raw | ConvertFrom-Json
    if ($production.runtimeOptions.configProperties.'TidalStation.DiagnosticMode' -cne 'production') { throw 'Production source input lost.' }

    function Run-Tidal([string]$Directory,[string]$Mode,[string]$Operator) {
      $trace = Join-Path $scratch "trace-$release.txt"
      $output = Invoke-Check $hostExe $scratch @((Join-Path $Directory 'TidalDiagnostics.dll')) @{ DOTNET_HOST_TRACE='1'; DOTNET_HOST_TRACEFILE=$trace; DOTNET_ROOT=$hostRoot; DOTNET_ROOT_X64=$hostRoot; DOTNET_ROLL_FORWARD='Disable' }
      Assert-Text $output "mode=$Mode;operator=$Operator;runtime=.NET $runtime"
      Assert-Text $output "$runtime\System.Private.CoreLib.dll"
      $traceText = Get-Content -LiteralPath $trace -Raw
      Assert-Text $traceText "Loaded library from $hostRoot\host\fxr\$runtime\hostfxr.dll"
      $log.Add("observed hostfxr/runtime=$runtime; $($output.Trim())")
    }
    Run-Tidal $build 'production' 'none'
    Copy-Item -LiteralPath (Join-Path $work 'development.json') -Destination $dev
    Run-Tidal $build $(if ($release -eq '10') { 'production' } else { 'development' }) 'night-watch'
    $sidecar = Get-Content -LiteralPath $dev -Raw | ConvertFrom-Json -AsHashtable
    $sidecar.runtimeOptions.configProperties.Remove('TidalStation.DiagnosticMode')
    $sidecar | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $dev
    Run-Tidal $build 'production' 'night-watch'
    Remove-Item -LiteralPath $dev
    Run-Tidal $build 'production' 'none'

    # A durable input change must survive regeneration of deliberately corrupted output.
    $project = Join-Path $work 'TidalDiagnostics.csproj'
    $projectText = Get-Content -LiteralPath $project -Raw
    $projectText.Replace('Value="production"','Value="survey"') | Set-Content -LiteralPath $project
    '{}' | Set-Content -LiteralPath $config
    $null = Invoke-Check $dotnet $work @('build','-c','Release','-t:Rebuild')
    Run-Tidal $build 'survey' 'none'
    $projectText | Set-Content -LiteralPath $project
    $null = Invoke-Check $dotnet $work @('build','-c','Release','-t:Rebuild')
    Run-Tidal $build 'production' 'none'

    $evaluated = Invoke-Check $dotnet $work @('msbuild','-p:Configuration=Release','-getProperty:GenerateRuntimeConfigDevFile')
    if ($release -eq '10') { Assert-Text $evaluated 'false' }
    elseif ($evaluated.Trim()) { throw "Unexpected RC2 Release dev-file generation: $evaluated" }
    $null = Invoke-Check $dotnet $work @('build','-c','Release','-p:GenerateRuntimeConfigDevFile=true')
    if (-not (Test-Path -LiteralPath $dev)) { throw 'Explicit dev generation produced no file.' }
    $generated = Get-Content -LiteralPath $dev -Raw | ConvertFrom-Json
    if (-not $generated.runtimeOptions.PSObject.Properties['additionalProbingPaths']) { throw 'Reconcile SDK-generated probing paths.' }
    Remove-Item -LiteralPath $dev

    $publish = Join-Path $scratch "publish-$release"
    $null = Invoke-Check $dotnet $work @('publish','-c','Release','-o',$publish)
    if (Test-Path -LiteralPath (Join-Path $publish 'TidalDiagnostics.runtimeconfig.dev.json')) { throw 'Default publish sidecar changed.' }
    # Demonstrate that explicit copy metadata can ship a sidecar: no universal exclusion.
    $copyItem = '<ItemGroup><None Update="development.json" CopyToOutputDirectory="PreserveNewest" CopyToPublishDirectory="Always" TargetPath="TidalDiagnostics.runtimeconfig.dev.json" /></ItemGroup>'
    $projectText.Replace('</Project>',"$copyItem</Project>") | Set-Content -LiteralPath $project
    $null = Invoke-Check $dotnet $work @('publish','-c','Release','-o',$publish)
    if (-not (Test-Path -LiteralPath (Join-Path $build 'TidalDiagnostics.runtimeconfig.dev.json'))) { throw 'Explicit build copy missing.' }
    Run-Tidal $publish $(if ($release -eq '10') { 'production' } else { 'development' }) 'night-watch'
    # The deployed set is an allowlist in a fresh directory, not an assertion about SDK exclusion.
    $productionDirectory = Join-Path $scratch "production-$release"
    $null = New-Item -ItemType Directory -Path $productionDirectory
    foreach ($name in 'TidalDiagnostics.dll','TidalDiagnostics.deps.json','TidalDiagnostics.runtimeconfig.json') {
      Copy-Item -LiteralPath (Join-Path $publish $name) -Destination $productionDirectory
    }
    if (@(Get-ChildItem -LiteralPath $productionDirectory -Filter '*.runtimeconfig.dev.json').Count) { throw 'Production allowlist contains a dev sidecar.' }
    Run-Tidal $productionDirectory 'production' 'none'
    $log.Add("build files=$((Get-ChildItem -LiteralPath $build -File).Name -join ','); publish files=$((Get-ChildItem -LiteralPath $publish -File).Name -join ','); production files=$((Get-ChildItem -LiteralPath $productionDirectory -File).Name -join ',')")
    $log.Add("PASS: $sdk / hostfxr+CoreCLR $runtime duplicate, missing duplicate, absent file, merge, regeneration, generation control, explicit copy and safe production artifact set.")
    Write-Host $log[$log.Count-1]
    if ($Record) {
      $evidence = Get-Content -LiteralPath (Join-Path $source 'verification-evidence.json') -Raw | ConvertFrom-Json -AsHashtable
      $evidence.kind = 'execution'
      $evidence.command = "eng/Test-RuntimeOverrides.ps1 -Net10Dotnet '$Net10Dotnet' -Net11Dotnet '$Net11Dotnet' -Record"
      $evidence.recordedAt = [DateTimeOffset]::UtcNow.ToString('O')
      $evidence.inputs = Get-EvidenceInputs $root $sample
      if (($inputs | ConvertTo-Json -Compress) -cne ($evidence.inputs | ConvertTo-Json -Compress)) { throw 'Inputs changed during execution.' }
      $evidence.output = ($log | Select-Object -Skip $firstLog) -join "`n"
      [IO.File]::WriteAllText((Join-Path $source 'commands.evidence.json'),($evidence | ConvertTo-Json -Depth 30).Replace("`r`n","`n") + "`n")
    }
  }
}
finally {
  foreach ($scope in $paths.Keys) {
    if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." }
  }
  Write-Host "Runtime override scratch: $scratch"
}
