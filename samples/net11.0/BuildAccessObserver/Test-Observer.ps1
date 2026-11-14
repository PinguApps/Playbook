# snippet:complete
[CmdletBinding()]
param(
  [string]$MSBuild = 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\amd64\MSBuild.exe',
  [string]$FrameworkReferences = 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8',
  [string]$Dotnet = 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe',
  [switch]$Record
)
$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../..'))
$source = Join-Path $root 'samples/net11.0/BuildAccessObserver'
$scratch = Join-Path $root ('artifacts/build-access/' + [guid]::NewGuid().ToString('N'))
$null = New-Item -ItemType Directory -Path "$scratch/inputs" -Force
$bin = Split-Path (Split-Path $MSBuild)
$csc = Join-Path $bin 'Roslyn/csc.exe'
$savedPaths = @{}
foreach ($scope in 'User','Machine') { $savedPaths[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$log = [Collections.Generic.List[string]]::new()
function Invoke-Check($Exe, [string[]]$Arguments, [int]$Expected = 0, [string]$Receipt = '') {
  $start = [Diagnostics.ProcessStartInfo]::new($Exe)
  $start.UseShellExecute = $false
  $start.RedirectStandardOutput = $true
  $start.RedirectStandardError = $true
  $start.WorkingDirectory = $scratch
  $start.Environment['SEED_OBSERVER_ROOT'] = "$scratch\inputs"
  $start.Environment['SEED_OBSERVER_RECEIPT'] = $Receipt
  $start.Environment['DOTNET_ADD_GLOBAL_TOOLS_TO_PATH'] = '0'
  $start.Environment['DOTNET_GENERATE_ASPNET_CERTIFICATE'] = 'false'
  $start.Environment['DOTNET_CLI_TELEMETRY_OPTOUT'] = '1'
  foreach ($argument in $Arguments) { $start.ArgumentList.Add($argument) }
  $process = [Diagnostics.Process]::Start($start)
  try {
    $stdout = $process.StandardOutput.ReadToEndAsync()
    $stderr = $process.StandardError.ReadToEndAsync()
    if (-not $process.WaitForExit(90000)) { $process.Kill($true); throw 'Observer check timed out.' }
    $output = $stdout.GetAwaiter().GetResult() + $stderr.GetAwaiter().GetResult()
    $log.Add("$Exe $($Arguments -join ' ')`nexit=$($process.ExitCode)`n$output")
    $log | Set-Content "$scratch/commands.log"
    if ($process.ExitCode -ne $Expected) { throw "Expected exit $Expected, got $($process.ExitCode): $output" }
    return $output
  } finally { $process.Dispose() }
}
function Compile($Name, $Sources, $ExtraReferences = @(), $Target = 'library') {
  $arguments = @('/nologo',"/target:$Target",'/langversion:preview','/nullable:enable','/nostdlib+',"/out:$scratch/$Name")
  foreach ($name in 'mscorlib','System','System.Core') { $arguments += "/reference:$FrameworkReferences/$name.dll" }
  foreach ($path in @("$bin/Microsoft.Build.dll","$bin/Microsoft.Build.Framework.dll") + $ExtraReferences) { $arguments += "/reference:$path" }
  $null = Invoke-Check $csc ($arguments + $Sources)
}
'namespace System.Runtime.CompilerServices { internal static class IsExternalInit {} }' | Set-Content "$scratch/IsExternalInit.cs"
$null = Invoke-Check $Dotnet @('build',"$source/Host/BuildAccessObserverHost.csproj",'-c','Release',"-p:VisualStudioMSBuildBin=$(Split-Path $MSBuild)")
Copy-Item "$source/Host/bin/Release/net48/SeedBuildObserver.dll" "$scratch/SeedBuildObserver.dll"
$previous = (Get-Content (Join-Path $root 'samples/net10.0/BuildAccessObserver/AccessDecoder.cs') -Raw).Replace('namespace CwmSeedBank.Build;', 'namespace CwmSeedBank.Previous;')
$previous | Set-Content "$scratch/PreviousDecoder.cs"
Compile 'ObserverProbe.dll' @("$source/Host/ObserverProbe.cs","$scratch/PreviousDecoder.cs","$scratch/IsExternalInit.cs") @("$scratch/SeedBuildObserver.dll")
Compile 'SeedIo.exe' @("$source/Host/SeedIo.cs") @() 'exe'
Compile 'CancelBuild.exe' @("$source/Host/CancelBuild.cs") @() 'exe'
[xml]$config = Get-Content ($MSBuild + '.config') -Raw
foreach ($node in $config.SelectNodes('//*[@href]')) { $node.href = [IO.Path]::GetFullPath((Join-Path (Split-Path $MSBuild) $node.href)) }
$config.Save("$scratch/CancelBuild.exe.config")
$fixture = [Security.SecurityElement]::Escape("$source/Host/SeedBuild.proj")
foreach ($batch in 'first','second','cancel','failure') {
  "Original seed input: $batch" | Set-Content "$scratch/inputs/$batch.seed"
}
'Unmatched input' | Set-Content "$scratch/inputs/unrelated.txt"
foreach ($batch in 'first','second') {
  "<Project><PropertyGroup><Batch>$batch</Batch></PropertyGroup><Import Project=`"$fixture`" /></Project>" | Set-Content "$scratch/$batch.proj"
}
'<Project DefaultTargets="Build"><ItemGroup><ProjectReference Include="first.proj" /><ProjectReference Include="second.proj" /><ProjectReferenceTargets Include="Build" Targets="Build" /></ItemGroup><Target Name="Build" /></Project>' | Set-Content "$scratch/Root.proj"
$properties = @("/p:ObserverAssembly=$scratch/ObserverProbe.dll","/p:SeedIo=$scratch/SeedIo.exe","/p:SeedRoot=$scratch/inputs")
$common = @('/nr:false','/nologo','/v:minimal') + $properties
$null = Invoke-Check $MSBuild (@("$source/Host/SeedBuild.proj",'/reportFileAccesses','/p:Batch=first') + $common) 0 "$scratch/report-only.tsv"
if (Test-Path "$scratch/report-only.tsv") { throw 'Reporting-only activation changed; reconcile this composition.' }
$null = Invoke-Check $MSBuild (@("$source/Host/SeedBuild.proj",'/graph','/p:Batch=first') + $common) 0 "$scratch/disabled.tsv"
if ((Get-Content "$scratch/disabled.tsv" -Raw) -match '(?m)^access\t') { throw 'Capture-disabled control emitted file accesses.' }
$null = Invoke-Check $MSBuild (@("$scratch/Root.proj",'/graph','/reportFileAccesses') + $common) 0 "$scratch/enabled.tsv"
$rows = @(Get-Content "$scratch/enabled.tsv" | Where-Object { $_.StartsWith("access`t") } | ForEach-Object { ,$_.Split("`t") })
if (@($rows | ForEach-Object { $_[1] } | Sort-Object -Unique).Count -ne 2) { throw 'Expected exactly two file-access contexts.' }
foreach ($batch in 'first','second') {
  $read = @($rows | Where-Object { $_[2].EndsWith("$batch.seed") -and $_[3] -eq 'CreateFile' -and $_[5] -eq '0' })
  $write = @($rows | Where-Object { $_[2].EndsWith("$batch.report") -and $_[4] -eq 'Write' -and $_[5] -eq '0' })
  $failed = @($rows | Where-Object { $_[2].EndsWith("$batch.missing") -and $_[5] -eq '2' })
  if ($read.Count -ne 1 -or $write.Count -ne 1 -or $failed.Count -ne 1) { throw "Missing real IO for $batch" }
  if ($read[0][1] -cne $write[0][1] -or $read[0][1] -cne $failed[0][1]) { throw 'IO lost build-context association.' }
}
if (-not @($rows | Where-Object { $_[4] -eq 'Write' -and $_[5] -eq '5' -and $_[8] -match 'DIRECTORY' }).Count) { throw 'Missing failed write.' }
if (-not @($rows | Where-Object { $_[4] -eq 'Enumerate' -and $_[7] -eq '*.seed' -and $_[8] -match 'DIRECTORY' }).Count) { throw 'Missing enumeration pattern/opened attributes.' }
if (-not @($rows | Where-Object { $_[7] -eq '<absent>' -and $_[6] -cne $_[8] }).Count) { throw 'Requested/opened or absent-pattern distinction lost.' }
$null = Invoke-Check $MSBuild (@("$source/Host/SeedBuild.proj",'/graph','/reportFileAccesses','/p:Batch=failure','/p:SeedMode=fail') + $common) 1 "$scratch/failed.tsv"
if ((Get-Content "$scratch/failed.tsv" -Raw) -notmatch 'finished\t[^\t]+\tfailed') { throw 'Failed context did not finalize as failed.' }
$null = Invoke-Check "$scratch/CancelBuild.exe" @((Split-Path $MSBuild),"$source/Host/SeedBuild.proj","$scratch/ObserverProbe.dll","$scratch/inputs","$scratch/SeedIo.exe") 0 "$scratch/cancelled.tsv"
$cancelled = Get-Content "$scratch/cancelled.tsv" -Raw
if (-not (Test-Path "$scratch/inputs/cancel.started") -or $cancelled -notmatch 'unfinished\t[^\t]+\tabandoned' -or $cancelled -match '\tsuccess') { throw 'Cancellation did not invalidate partial context.' }
$identities = @((Join-Path $env:SystemRoot 'Microsoft.NET/Framework64/v4.0.30319/clr.dll'),$MSBuild,"$MSBuild.config",$csc,"$bin/Microsoft.Build.dll","$bin/Microsoft.Build.Framework.dll")
$identities += Get-ChildItem $bin -Filter 'BuildXL*.dll' | Select-Object -ExpandProperty FullName
$identities += Get-ChildItem "$bin/x64" -File | Select-Object -ExpandProperty FullName
$assets = @($identities | ForEach-Object { $item = Get-Item $_; @{path=$item.FullName;sha256=(Get-FileHash $_).Hash;version=$item.VersionInfo.FileVersion} })
foreach ($scope in 'User','Machine') { if ($savedPaths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "$scope PATH changed" } }
@{host=$MSBuild;assets=$assets;checks=@('real-read-write-errors-enumeration','two-contexts','report-only','capture-disabled','failed-build','active-cancellation','previous-current-live-decoding','constructor-defaults');receipt=$scratch} | ConvertTo-Json -Depth 8 | Set-Content "$scratch/summary.json"
if ($Record) {
  . (Join-Path $root 'eng/Evidence.ps1')
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  $env:DOTNET_GENERATE_ASPNET_CERTIFICATE = 'false'
  & (Join-Path $root 'eng/Record-Evidence.ps1') -Sample 'samples/net11.0/BuildAccessObserver/BuildAccessObserver.csproj' -Dotnet $Dotnet -Kind compile
  & (Join-Path $root 'eng/Record-Evidence.ps1') -Sample 'samples/net11.0/BuildAccessObserver/Host/BuildAccessObserverHost.csproj' -Dotnet $Dotnet -Kind compile
  $sample = 'samples/net11.0/BuildAccessObserver/Host/BuildAccessObserverHost.csproj'
  $proofRecord = Get-Content "$source/Host/verification-evidence.json" -Raw | ConvertFrom-Json -AsHashtable
  $proofRecord.kind = 'execution'
  $proofRecord.inputs = Get-EvidenceInputs $root $sample
  $proofRecord.recordedAt = [DateTimeOffset]::UtcNow.ToString('O')
  $proofRecord.sourceCommit = (& 'C:\Program Files\Git\cmd\git.exe' -C $root rev-parse HEAD).Trim()
  $proofRecord.sourceDirty = $true
  $proofRecord.command = "eng/Test-BuildAccessObserver.ps1 -MSBuild '$MSBuild' -Dotnet '$Dotnet' -Record"
  $proofRecord.scope = 'Actual net48 canonical observer assembly compiled by SDK11 and loaded by matching Visual Studio x64 MSBuild; real IO, previous/current shared-field decoding, contexts, controls, failure and active cancellation. No older native-host or reusable cache claim.'
  $proofRecord.output = Get-Content "$scratch/summary.json" -Raw
  $proofRecord | ConvertTo-Json -Depth 40 | Set-Content "$source/Host/native.evidence.json"
}
Write-Host "PASS: native observer IO, contexts, controls and active cancellation. Receipts: $scratch"
# endsnippet:complete
