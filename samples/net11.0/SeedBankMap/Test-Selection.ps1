# snippet:complete
[CmdletBinding()]
param(
  [string]$Net11Dotnet = 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe',
  [string]$Sdk12Dotnet = (Join-Path $PSScriptRoot '../../../artifacts/seed-selection-sdk/dotnet.exe'),
  [switch]$Record
)
$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../..'))
$source = Join-Path $root 'samples/net11.0/SeedBankMap'
$scratch = Join-Path $root ('artifacts/seed-selection/' + [guid]::NewGuid().ToString('N'))
$consumer = Join-Path $scratch 'consumer'
$markers = Join-Path $scratch 'markers'
$null = New-Item -ItemType Directory -Path $consumer,$markers -Force
$git = 'C:\Program Files\Git\cmd\git.exe'
$savedPaths = @{}
foreach ($scope in 'User','Machine') { $savedPaths[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$log = [Collections.Generic.List[string]]::new()
$environment = @{SEED_MARKERS=$markers}
function Invoke-Seed($Exe, [string[]]$Arguments, $Expected = 0) {
  $start = [Diagnostics.ProcessStartInfo]::new($Exe)
  $start.UseShellExecute = $false
  $start.RedirectStandardOutput = $true
  $start.RedirectStandardError = $true
  $start.WorkingDirectory = $consumer
  foreach ($argument in $Arguments) { $start.ArgumentList.Add($argument) }
  foreach ($name in 'DOTNET_ADD_GLOBAL_TOOLS_TO_PATH','DOTNET_CLI_TELEMETRY_OPTOUT','DOTNET_SKIP_FIRST_TIME_EXPERIENCE') { $start.Environment[$name] = if ($name -eq 'DOTNET_ADD_GLOBAL_TOOLS_TO_PATH') { '0' } else { '1' } }
  $start.Environment['DOTNET_GENERATE_ASPNET_CERTIFICATE'] = 'false'
  $start.Environment['DOTNET_CLI_WORKLOAD_UPDATE_NOTIFY_DISABLE'] = 'true'
  $start.Environment['DOTNET_CLI_HOME'] = "$scratch/home"
  $start.Environment['NUGET_PACKAGES'] = "$scratch/packages"
  $start.Environment['DOTNET_CLI_ENABLE_AFFECTED_TESTS'] = '1'
  $start.Environment.Remove('DOTNET_CLI_TEST_AFFECTED_TESTS_MODE') | Out-Null
  $start.Environment['DOTNET_ROOT'] = Split-Path $Sdk12Dotnet
  foreach ($name in 'SEED_FAIL','SEED_WAIT','SEED_STARTUP_FAIL','SEED_MARKERS') { $start.Environment.Remove($name) | Out-Null }
  foreach ($name in $environment.Keys) { $start.Environment[$name] = $environment[$name] }
  $process = [Diagnostics.Process]::Start($start)
  try {
    $stdout = $process.StandardOutput.ReadToEndAsync()
    $stderr = $process.StandardError.ReadToEndAsync()
    if (-not $process.WaitForExit(180000)) { $process.Kill($true); throw 'Seed-bank consumer timed out.' }
    $output = $stdout.GetAwaiter().GetResult() + $stderr.GetAwaiter().GetResult()
    $log.Add("cwd=$consumer`n$Exe $($Arguments -join ' ')`nexit=$($process.ExitCode)`n$output")
    $log | Set-Content "$scratch/commands.log"
    if ($null -ne $Expected -and $process.ExitCode -ne $Expected) { throw "Expected exit $Expected, got $($process.ExitCode): $output" }
    return @{Output=$output;Exit=$process.ExitCode}
  } finally { $process.Dispose() }
}
function Test-Run($Mode, [int]$Count, $Expected = 0) {
  Get-ChildItem $markers -File | Remove-Item
  $result = Invoke-Seed $Sdk12Dotnet (@('test','-c','Release','--no-ansi') + $Mode) $Expected
  $executed = @(Get-ChildItem $markers -File | Select-Object -ExpandProperty Name | Sort-Object)
  if ($executed.Count -ne $Count) { throw "Expected $Count actual test markers, got $($executed -join ','): $($result.Output)" }
  return $result
}
foreach ($file in Get-ChildItem "$source/Consumer" -File -Recurse) {
  $relative = [IO.Path]::GetRelativePath("$source/Consumer",$file.FullName)
  if ($relative -match '(^|[\\/])(bin|obj)([\\/]|$)' -or $file.Name -match 'verification|evidence') { continue }
  $destination = Join-Path $consumer $relative
  $null = New-Item -ItemType Directory -Path (Split-Path $destination) -Force
  Copy-Item -LiteralPath $file.FullName -Destination $destination
}
"**/bin/`n**/obj/`n**/TestResults/`n.cts/" | Set-Content "$consumer/.gitignore"
$null = Invoke-Seed $git @('init','--initial-branch=main')
$sdk12Global = Get-Content "$consumer/global.json" -Raw
$sdk12Props = Get-Content "$consumer/Directory.Build.props" -Raw
$ga = $sdk12Global | ConvertFrom-Json
$ga.sdk.version = '11.0.100'
$ga.sdk.allowPrerelease = $false
$globalPaths = @('global.json','Germination/global.json','ColdStore/global.json','Germination.Tests/global.json','ColdStore.Tests/global.json')
foreach ($path in $globalPaths) { $ga | ConvertTo-Json -Depth 10 | Set-Content "$consumer/$path" }
$sdk12Props.Replace('<RuntimeFrameworkVersion>12.0.0-alpha.1.26509.101</RuntimeFrameworkVersion>','') | Set-Content "$consumer/Directory.Build.props"
$null = Invoke-Seed $Net11Dotnet @('test','-c','Release','--no-ansi')
if (@(Get-ChildItem $markers -File).Count -ne 4) { throw 'GA11 full baseline did not execute four tests.' }
$null = Invoke-Seed $git @('add','.')
$null = Invoke-Seed $git @('commit','-m','Original GA11 seed-bank baseline')
$gaResult = Invoke-Seed $Net11Dotnet @('test','-c','Release','--no-ansi','--collect-test-map') 1
if (-not $gaResult.Output.Contains('Handshake failures:')) { throw 'GA11 failure changed; reassess compatibility instead of carrying forward the old limitation.' }
foreach ($path in $globalPaths) { [IO.File]::WriteAllText("$consumer/$path",$sdk12Global) }
[IO.File]::WriteAllText("$consumer/Directory.Build.props",$sdk12Props)
$null = Invoke-Seed $Sdk12Dotnet @('test','-c','Release','--no-ansi')
$null = Invoke-Seed $git @('add','.')
$null = Invoke-Seed $git @('commit','-m','Explicit SDK12 net11 conditional baseline')
$mapsPath = "$consumer/.cts/mappings"
Get-ChildItem $mapsPath -Filter '*.json' -ErrorAction SilentlyContinue | Remove-Item
$null = Test-Run @('--collect-test-map') 4
$maps = @{}
foreach ($file in Get-ChildItem $mapsPath -Filter '*.json') {
  $map = Get-Content $file.FullName -Raw | ConvertFrom-Json
  if ($map.allTestUids.Count -ne 2 -or @($map.sourceToTestUids.PSObject.Properties).Count -lt 2) { throw 'Incomplete test/source map.' }
  $product = if ($map.moduleIdentity.StartsWith('Germination.Tests/')) { 'germination/germination.cs' } else { 'coldstore/coldstore.cs' }
  if ($map.sourceToTestUids.$product.Count -ne 2) { throw "Product associations missing: $product" }
  $maps[$file.Name] = [IO.File]::ReadAllBytes($file.FullName)
}
if ($maps.Count -ne 2) { throw 'Expected two module maps.' }
function Restore-Maps {
  Get-ChildItem $mapsPath -Filter '*.json' | Remove-Item
  foreach ($name in $maps.Keys) { [IO.File]::WriteAllBytes("$mapsPath/$name",$maps[$name]) }
}
$null = Test-Run @('--affected-tests') 0
$productPath = "$consumer/Germination/Germination.cs"
$originalProduct = Get-Content $productPath -Raw
$originalProduct.Replace('80L','(40L + 40L)') | Set-Content $productPath
$selection = Test-Run @('--affected-tests') 4
if (-not $selection.Output.Contains("changed path 'Germination/Germination.cs' has no source-to-test association")) { throw 'Conservative module fallback changed; reassess the supported behavior.' }
$environment['SEED_FAIL'] = 'Germination.first'
$null = Test-Run @('--affected-tests') 4 2
$environment.Remove('SEED_FAIL')
$environment['SEED_STARTUP_FAIL'] = '1'
$startup = Test-Run @('--affected-tests') 0 $null
if ($startup.Exit -eq 0) { throw 'Startup failure became success.' }
$environment.Remove('SEED_STARTUP_FAIL')
$csc = 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\Roslyn\csc.exe'
$null = Invoke-Seed $csc @('/nologo','/target:winexe',"/out:$scratch/CancelSelection.exe","$source/Host/CancelSelection.cs")
$environment['SEED_WAIT'] = 'Germination.first'
Get-ChildItem $markers -File | Remove-Item
$null = Invoke-Seed "$scratch/CancelSelection.exe" @($Sdk12Dotnet,$consumer,"$markers/Germination.first","$scratch/cancellation.log",'--affected-tests')
$cancelExit = [int](Get-Content "$scratch/cancellation.log.exit")
if ($cancelExit -eq 0) { throw 'Cancelled SDK reported success.' }
$environment.Remove('SEED_WAIT')
[IO.File]::WriteAllText($productPath,$originalProduct)
$null = New-Item -ItemType Directory "$consumer/docs" -Force
'Unrelated original documentation' | Set-Content "$consumer/docs/notes.md"
$null = Test-Run @('--affected-tests') 0
foreach ($condition in 'missing','stale','incompatible') {
  Restore-Maps
  foreach ($file in Get-ChildItem $mapsPath -Filter '*.json') {
    if ($condition -eq 'missing') { Remove-Item -LiteralPath $file.FullName; continue }
    $map = Get-Content $file.FullName -Raw | ConvertFrom-Json
    if ($condition -eq 'stale') { $map.baselineCommit = '0000000000000000000000000000000000000000' }
    if ($condition -eq 'incompatible') { $map.schemaVersion = 999 }
    $map | ConvertTo-Json -Depth 15 | Set-Content $file.FullName
  }
  $null = Test-Run @('--affected-tests') 4
}
Restore-Maps
Remove-Item -LiteralPath "$consumer/docs/notes.md"
Get-ChildItem $mapsPath -Filter '*.json' | Remove-Item
$environment['SEED_WAIT'] = 'Germination.first'
Get-ChildItem $markers -File | Remove-Item
$null = Invoke-Seed "$scratch/CancelSelection.exe" @($Sdk12Dotnet,$consumer,"$markers/Germination.first","$scratch/collection-cancellation.log",'--collect-test-map')
foreach ($file in Get-ChildItem $mapsPath -Filter '*.json') {
  $map = Get-Content $file.FullName -Raw | ConvertFrom-Json
  if ($map.moduleIdentity.StartsWith('Germination.Tests/')) { throw 'Cancelled module published a reusable map.' }
}
$environment.Remove('SEED_WAIT')
if ($Record) {
  foreach ($module in 'Germination','ColdStore','Germination.Tests','ColdStore.Tests') {
    $lock = (Get-Content "$consumer/$module/packages.lock.json" -Raw) -replace '(?m)^// (?:end)?snippet:complete\r?\n?', ''
    $lockText = ("// snippet:complete`n" + $lock.Trim() + "`n// endsnippet:complete`n").Replace("`r`n", "`n")
    if ((Get-Content "$source/Consumer/$module/packages.lock.json" -Raw) -cne $lockText) {
      [IO.File]::WriteAllText("$source/Consumer/$module/packages.lock.json", $lockText)
    }
    $assets = Get-Content "$consumer/$module/obj/project.assets.json" -Raw | ConvertFrom-Json -AsHashtable
    $manifestPath = "$source/Consumer/$module/verification.json"
    $manifest = Get-Content $manifestPath -Raw | ConvertFrom-Json -AsHashtable
    $dependencies = @($assets.libraries.Keys | Sort-Object -CaseSensitive)
    if (($manifest.dependencies -join ';') -cne ($dependencies -join ';')) {
      $manifest.dependencies = $dependencies
      [IO.File]::WriteAllText($manifestPath, ($manifest | ConvertTo-Json -Depth 8).Replace("`r`n", "`n") + "`n")
    }
  }
  $pwsh = (Get-Process -Id $PID).Path
  foreach ($module in 'Germination','ColdStore','Germination.Tests','ColdStore.Tests') {
    $null = Invoke-Seed $pwsh @('-NoProfile','-File',"$root/eng/Record-Evidence.ps1",'-Sample',"samples/net11.0/SeedBankMap/Consumer/$module/$module.csproj",'-Dotnet',$Sdk12Dotnet,'-Kind','compile')
  }
}
foreach ($scope in 'User','Machine') { if ($savedPaths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "$scope PATH changed" } }
$identityPaths = @("$(Split-Path $Sdk12Dotnet)/sdk/12.0.100-alpha.1.26509.101/Roslyn/bincore/Microsoft.CodeAnalysis.CSharp.dll","$(Split-Path $Sdk12Dotnet)/packs/Microsoft.NETCore.App.Ref/12.0.0-alpha.1.26509.101/ref/net11.0/System.Runtime.dll")
$testOutput = "$consumer/Germination.Tests/bin/Release/net11.0"
foreach ($name in 'Microsoft.Testing.Extensions.AffectedTests','Microsoft.Testing.Extensions.CodeCoverage','Microsoft.Testing.Platform') {
  $dll = "$testOutput/$name.dll"
  if (-not (Test-Path $dll)) { throw "Missing executed dependency $name" }
  $identityPaths += $dll
}
$identityPaths += Get-ChildItem $testOutput -Recurse -File | Where-Object Name -Match '^(ctsrun64\.dll|Cts_x64\.config|msdia140\.dll|MicrosoftInstrumentationEngine_x64\.dll)$' | Select-Object -ExpandProperty FullName
$assets = @($identityPaths | ForEach-Object { @{path=$_;sha256=(Get-FileHash $_).Hash;version=(Get-Item $_).VersionInfo.FileVersion} })
@{sdk12=$Sdk12Dotnet;ga11='collection handshake failure';target='net11.0';referencePack='12.0.0-alpha.1.26509.101/ref/net11.0';systemRuntimeAssembly='11.0.0.0';runtime='12.0.0-alpha.1.26509.101';onlyAffectedModule=$false;checks=@('baseline-four','real-source-maps','changed-module-two-plus-unmapped-module-fallback-two','failure-exit2','empty-selection','startup-failure','missing-stale-incompatible','ignored-doc-change','active-selection-cancellation','active-collection-cancellation');cancellationExit=$cancelExit;assets=$assets;receipt=$scratch} | ConvertTo-Json -Depth 8 | Set-Content "$scratch/summary.json"
if ($Record) {
  . (Join-Path $root 'eng/Evidence.ps1')
  $sample = 'samples/net11.0/SeedBankMap/Consumer/Germination.Tests/Germination.Tests.csproj'
  $proofRecord = Get-Content "$source/Consumer/Germination.Tests/verification-evidence.json" -Raw | ConvertFrom-Json -AsHashtable
  $proofRecord.kind = 'execution'
  $proofRecord.inputs = Get-EvidenceInputs $root $sample
  $proofRecord.recordedAt = [DateTimeOffset]::UtcNow.ToString('O')
  $proofRecord.sourceCommit = (& $git -C $root rev-parse HEAD).Trim()
  $proofRecord.sourceDirty = $true
  $proofRecord.command = "eng/Test-SeedBankSelection.ps1 -Net11Dotnet '$Net11Dotnet' -Sdk12Dotnet '$Sdk12Dotnet' -Record"
  $proofRecord.scope = 'Both actual independent net11 consumers on the explicit preview SDK/reference/runtime route: local maps, conservative unmapped-module fallback, empty selection, stale/missing/schema fallback, real failures and selection/collection cancellation. GA11 handshake failure retained; no cross-module elimination or performance claim.'
  $proofRecord.output = Get-Content "$scratch/summary.json" -Raw
  $proofRecord | ConvertTo-Json -Depth 40 | Set-Content "$source/Consumer/Germination.Tests/selection.evidence.json"
}
Write-Host "PASS: real seed-bank maps and conservative fallback, failures and cancellation. Cross-module test elimination is not claimed. Receipts: $scratch"
# endsnippet:complete
