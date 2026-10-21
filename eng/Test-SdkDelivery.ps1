[CmdletBinding()]
param(
  [string]$Net10Dotnet = 'dotnet',
  [string]$Net11Dotnet = 'dotnet',
  [string]$NodeExe = 'node',
  [switch]$Record
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
. (Join-Path $PSScriptRoot 'Evidence.ps1')
$root = Split-Path $PSScriptRoot -Parent
$Net10Dotnet = (Get-Command $Net10Dotnet -CommandType Application | Select-Object -First 1).Source
$Net11Dotnet = (Get-Command $Net11Dotnet -CommandType Application | Select-Object -First 1).Source
$NodeExe = (Get-Command $NodeExe -CommandType Application | Select-Object -First 1).Source
$scratch = Join-Path ([IO.Path]::GetTempPath()) ('playbook-delivery-' + [guid]::NewGuid().ToString('N'))
$null = New-Item -ItemType Directory -Path $scratch
$paths = @{}
foreach ($scope in @('Process', 'User', 'Machine')) { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH', $scope) }
$log = [Collections.Generic.List[string]]::new()
$before = @{}
if ($Record) {
  foreach ($release in @('10', '11')) {
    foreach ($name in @('RunnerTests', 'NUnitTests', 'DeliveryTool', 'WebAssets', 'RclAssets')) {
      $sample = "samples/net$release.0/$name/$name.csproj"
      $before[$sample] = Get-EvidenceInputs $root $sample
    }
  }
}

function Invoke-Delivery([string]$Exe, [string]$Directory, [string[]]$Arguments, [hashtable]$Environment = @{}, [int]$ExpectedExit = 0) {
  $start = [Diagnostics.ProcessStartInfo]::new($Exe)
  $start.WorkingDirectory = $Directory
  $start.UseShellExecute = $false
  $start.CreateNoWindow = $true
  $start.RedirectStandardOutput = $true
  $start.RedirectStandardError = $true
  foreach ($argument in $Arguments) { $start.ArgumentList.Add($argument) }
  $start.Environment['DOTNET_ADD_GLOBAL_TOOLS_TO_PATH'] = '0'
  $start.Environment['DOTNET_CLI_HOME'] = Join-Path $scratch 'home'
  $start.Environment['DOTNET_NOLOGO'] = '1'
  $start.Environment['DOTNET_CLI_USE_MSBUILD_SERVER'] = 'false'
  $start.Environment['MSBUILDUSESERVER'] = '0'
  $start.Environment['DOTNET_ROOT'] = Split-Path $script:dotnet
  $start.Environment['DOTNET_ROOT_X64'] = Split-Path $script:dotnet
  foreach ($key in $Environment.Keys) { $start.Environment[$key] = $Environment[$key] }
  $process = [Diagnostics.Process]::Start($start)
  $stdout = $process.StandardOutput.ReadToEndAsync()
  $stderr = $process.StandardError.ReadToEndAsync()
  if (-not $process.WaitForExit(120000)) { $process.Kill($true); throw "Timed out: $($Arguments -join ' ')" }
  $output = $stdout.GetAwaiter().GetResult() + $stderr.GetAwaiter().GetResult()
  $log.Add("cwd=$Directory`n$Exe $($Arguments -join ' ')`nexit=$($process.ExitCode)`n$output")
  [IO.File]::WriteAllText((Join-Path $scratch 'commands.log'), ($log -join "`n"))
  if ($process.ExitCode -ne $ExpectedExit) { throw "Expected exit $ExpectedExit, received $($process.ExitCode): $output" }
  return $output
}

function Assert-Text([string]$Actual, [string]$Expected) {
  if (-not $Actual.Contains($Expected, [StringComparison]::Ordinal)) { throw "Missing '$Expected': $Actual" }
}

try {
  $script:dotnet = $Net10Dotnet
  Assert-Text (Invoke-Delivery $NodeExe $scratch @('--version')) 'v24.21.0'
  foreach ($release in @('10', '11')) {
    $script:dotnet = if ($release -eq '10') { $Net10Dotnet } else { $Net11Dotnet }
    $runner = Join-Path $root "samples/net$release.0/RunnerTests"
    $nunit = Join-Path $root "samples/net$release.0/NUnitTests"
    $null = Invoke-Delivery $dotnet $runner @('test', '--project', 'RunnerTests.csproj', '-c', 'Release')
    $nunitArgs = if ($release -eq '10') { @('test', 'NUnitTests.csproj', '-c', 'Release') } else { @('test', '--project', 'NUnitTests.csproj', '-c', 'Release') }
    $null = Invoke-Delivery $dotnet $nunit $nunitArgs

    if ($release -eq '11') {
      # Verify templates in an isolated directory with its own global.json: creation can edit that file.
      $templates = Join-Path $scratch 'templates'
      $null = New-Item -ItemType Directory -Path $templates
      Copy-Item -LiteralPath (Join-Path $runner 'global.json') -Destination $templates
      $null = Invoke-Delivery $dotnet $templates @('new', 'xunit', '--xunit-version', 'v3', '--no-restore', '-o', 'Xunit')
      $null = Invoke-Delivery $dotnet $templates @('new', 'nunit', '--test-runner', 'Microsoft.Testing.Platform', '--no-restore', '-o', 'NUnit')
      Assert-Text ([IO.File]::ReadAllText((Join-Path $templates 'Xunit/Xunit.csproj'))) 'xunit.v3.mtp-v2'
      Assert-Text ([IO.File]::ReadAllText((Join-Path $templates 'NUnit/NUnit.csproj'))) '<EnableNUnitRunner>true'

      $null = Invoke-Delivery $dotnet $runner @('test', '--project', 'RunnerTests.csproj', '--no-build', '-c', 'Release', '--maximum-failed-tests', '1') @{ PIER_FAIL = '1' } 13
      $null = Invoke-Delivery $dotnet $runner @('test', '--project', 'RunnerTests.csproj', '--no-build', '-c', 'Release', '--timeout', '1s') @{ PIER_DELAY = '1' } 3
      $reports = Join-Path $scratch 'reports'
      $modules = Invoke-Delivery $dotnet $runner @('test', '--root-directory', '..', '--test-modules', '**/bin/Release/net11.0/*Tests.dll; !**/NUnitTests.dll', '--results-directory', $reports, '--results-directory-layout', 'per-module', '--', '--report-xunit-trx', '--report-xunit-trx-filename', 'berths.trx')
      Assert-Text $modules 'total: 4'
      $report = @(Get-ChildItem -LiteralPath $reports -Filter 'berths.trx' -Recurse)
      if ($report.Count -ne 1 -or $report[0].FullName -notmatch 'RunnerTests') { throw 'Per-module xUnit report missing or excluded module ran.' }
      $null = Invoke-Delivery $dotnet $runner @('test', '--project', 'RunnerTests.csproj', '--no-dependencies', '-c', 'Release')
      $traversal = Invoke-Delivery $dotnet $runner @('test', 'Traversal/dirs.proj', '-c', 'Release', '--results-directory-layout', 'per-module')
      Assert-Text $traversal 'total: 6'
      $null = Invoke-Delivery $dotnet $runner @('test', '--project', 'RunnerTests.csproj', '--no-build', '-c', 'Release') @{ DOTNET_TEST_RUNNER = 'Microsoft.Testing.Platform' }
    }

    $tool = Join-Path $root "samples/net$release.0/DeliveryTool"
    $feed = Join-Path $scratch "feed-$release"
    $null = Invoke-Delivery $dotnet $tool @('pack', 'DeliveryTool.csproj', '-c', 'Release', '-o', $feed)
    $config = Join-Path $scratch "nuget-$release.config"
    [IO.File]::WriteAllText($config, ('<configuration><packageSources><clear/><add key="local" value="{0}"/></packageSources></configuration>' -f [Security.SecurityElement]::Escape($feed)))
    $install = Join-Path $scratch "tool-$release"
    $null = Invoke-Delivery $dotnet $scratch @('tool', 'install', 'Pier.Manifest.Tool', '--version', '1.0.0', '--tool-path', $install, '--configfile', $config, '--no-cache') @{ NUGET_PACKAGES = (Join-Path $scratch "cache-$release") }
    Assert-Text (Invoke-Delivery (Join-Path $install 'pier-manifest.exe') $scratch @()) 'manifest=PIER-42'
    # Invalid custom-nuspec packing is an actual SDK error on the newer SDK.
    if ($release -eq '11') {
      $invalid = Invoke-Delivery $dotnet $tool @('pack', 'DeliveryTool.csproj', '-p:NuspecFile=not-a-tool.nuspec') @{} 1
      Assert-Text $invalid 'NETSDK1235'
    }

    $publish = Join-Path $scratch "single-$release"
    $singleArgs = @('publish', 'DeliveryTool.csproj', '-c', 'Release', '-p:PublishProfile=SingleFile', '-o', $publish)
    $null = Invoke-Delivery $dotnet $tool $singleArgs
    $output = Invoke-Delivery (Join-Path $publish 'DeliveryTool.exe') $scratch @()
    Assert-Text $output 'manifest=PIER-42'
    if ($release -eq '11') {
      Assert-Text $output 'assembly=;base='
      $invalid = Invoke-Delivery $dotnet $tool ($singleArgs + '-p:IncludeAllContentForSelfExtract=true')
      Assert-Text $invalid 'NETSDK1244'
      $null = Invoke-Delivery $dotnet $tool ($singleArgs + '-p:IncludeNativeLibrariesForSelfExtract=true')
    }
    elseif ($output.Contains('assembly=;')) { throw 'Expected the older full-extraction route to expose an assembly path.' }

    $assets = Join-Path $root "samples/net$release.0/WebAssets"
    $publishedAssets = Join-Path $scratch "web-$release"
    $null = Invoke-Delivery $dotnet $assets @('clean', 'WebAssets.csproj', '-c', 'Release')
    $null = Invoke-Delivery $dotnet $assets @('publish', 'WebAssets.csproj', '-c', 'Release', "-p:NodePath=$NodeExe", '-o', $publishedAssets)
    Assert-Text ([IO.File]::ReadAllText((Join-Path $publishedAssets 'wwwroot/bin/berths.js'))) '18 - 5'
    Assert-Text ([IO.File]::ReadAllText((Join-Path $publishedAssets 'wwwroot/_content/Pier.Deck.Assets/bin/deck.js'))) '42'
    if (-not (Test-Path -LiteralPath (Join-Path $publishedAssets 'wwwroot/crew/index.html'))) { throw 'Crew asset missing.' }
    if ($release -eq '11') {
      $manifest = Get-Content -LiteralPath (Join-Path $assets 'obj/Release/net11.0/staticwebassets.publish.json') -Raw | ConvertFrom-Json
      if (-not @($manifest.Assets | Where-Object AssetGroups -eq 'Audience=crew').Count) { throw 'Asset group was not authored into the manifest.' }
      $endpoints = Get-Content -LiteralPath (Join-Path $publishedAssets 'WebAssets.staticwebassets.endpoints.json') -Raw | ConvertFrom-Json
      if (-not @($endpoints.Endpoints | Where-Object Route -eq 'crew').Count) { throw 'Captured-stem endpoint missing.' }
      $excluded = Join-Path $scratch 'web-excluded'
      $null = Invoke-Delivery $dotnet $assets @('publish', 'WebAssets.csproj', '-c', 'Release', "-p:NodePath=$NodeExe", '-p:PierAudience=visitor', '-o', $excluded)
      $excludedEndpoints = Get-Content -LiteralPath (Join-Path $excluded 'WebAssets.staticwebassets.endpoints.json') -Raw | ConvertFrom-Json
      if (@($excludedEndpoints.Endpoints | Where-Object Route -Match '^crew').Count) { throw 'Unselected group endpoint was published.' }
      if (-not (Test-Path -LiteralPath (Join-Path $excluded 'wwwroot/crew/index.html'))) { throw 'RC2 physical-content observation changed; reconcile the guidance.' }
      Assert-Text ([IO.File]::ReadAllText((Join-Path $excluded 'wwwroot/bin/berths.js'))) '18 - 5'
    }
    $log.Add("PASS: SDK $release actual runner, local tool installation/execution, single-file execution and TypeScript publish.")
    Write-Host $log[$log.Count - 1]
  }
  if ($Record) {
    foreach ($sample in $before.Keys) {
      $directory = Split-Path (Join-Path $root $sample)
      $evidenceRecord = Get-Content -LiteralPath (Join-Path $directory 'verification-evidence.json') -Raw | ConvertFrom-Json -AsHashtable
      $evidenceRecord.kind = 'execution'
      $evidenceRecord.command = 'eng/Test-SdkDelivery.ps1 -Record (absolute executables in output)'
      $evidenceRecord.recordedAt = [DateTimeOffset]::UtcNow.ToString('O')
      $evidenceRecord.inputs = Get-EvidenceInputs $root $sample
      if ((ConvertTo-Json $before[$sample] -Compress) -cne (ConvertTo-Json $evidenceRecord.inputs -Compress)) { throw "Inputs changed during execution: $sample" }
      $release = if ($sample.Contains('/net10.0/')) { '10' } else { '11' }
      $related = [Collections.Generic.List[string]]::new()
      $related.Add($directory)
      if ($sample.Contains('/DeliveryTool/')) {
        foreach ($name in @("feed-$release", "tool-$release", "single-$release")) { $related.Add((Join-Path $scratch $name)) }
      }
      if ($sample.Contains('/RclAssets/')) { $related.Add((Join-Path $root "samples/net$release.0/WebAssets")) }
      if ($sample.Contains('/RunnerTests/')) { $related.Add((Join-Path $scratch 'templates')) }
      $evidenceRecord.output = @($log | Where-Object {
        $entry = $_
        @($related | Where-Object { $entry.Contains($_, [StringComparison]::OrdinalIgnoreCase) }).Count -gt 0 -or
          $entry.Contains("$NodeExe --version", [StringComparison]::Ordinal)
      }) -join "`n"
      [IO.File]::WriteAllText((Join-Path $directory 'commands.evidence.json'), (ConvertTo-Json $evidenceRecord -Depth 30).Replace("`r`n", "`n") + "`n")
    }
  }
}
finally {
  foreach ($scope in $paths.Keys) {
    if ([Environment]::GetEnvironmentVariable('PATH', $scope) -cne $paths[$scope]) { throw "Unexpected $scope PATH change; shared settings were not restored." }
  }
  Write-Host "Delivery command scratch: $scratch"
}
