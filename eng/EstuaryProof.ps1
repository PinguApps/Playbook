# Shared process and evidence helpers for the bounded Estuary authoring checks.
. (Join-Path $PSScriptRoot 'Evidence.ps1')
function Write-EstuaryFile([string]$File,[string]$Text) {
  [IO.File]::WriteAllText($File,$Text.Replace("`r`n","`n"))
}
function Invoke-Estuary([string]$Executable,[string]$Directory,[string[]]$Arguments,[int]$Exit=0,[hashtable]$Environment=@{}) {
  $start=[Diagnostics.ProcessStartInfo]::new($Executable)
  $start.WorkingDirectory=$Directory
  $start.UseShellExecute=$false; $start.CreateNoWindow=$true
  $start.RedirectStandardOutput=$true; $start.RedirectStandardError=$true
  foreach($argument in $Arguments){$start.ArgumentList.Add($argument)}
  $settings=@{
    DOTNET_ADD_GLOBAL_TOOLS_TO_PATH='0'; DOTNET_GENERATE_ASPNET_CERTIFICATE='false'; DOTNET_NOLOGO='1'
    DOTNET_CLI_HOME=(Join-Path $scratch 'home'); DOTNET_CLI_TELEMETRY_OPTOUT='1'
    DOTNET_CLI_USE_MSBUILD_SERVER='false'; MSBUILDUSESERVER='0'; DOTNET_CLI_WORKLOAD_UPDATE_NOTIFY_DISABLE='true'
  }
  foreach($key in $settings.Keys){$start.Environment[$key]=$settings[$key]}
  foreach($key in $Environment.Keys){$start.Environment[$key]=$Environment[$key]}
  $process=[Diagnostics.Process]::Start($start)
  try {
    $stdout=$process.StandardOutput.ReadToEndAsync();$stderr=$process.StandardError.ReadToEndAsync()
    if(-not $process.WaitForExit(240000)){$process.Kill($true);throw 'Estuary child timed out'}
    $output=$stdout.GetAwaiter().GetResult()+$stderr.GetAwaiter().GetResult()
    $log.Add("cwd=$Directory`n$Executable $($Arguments -join ' ')`nexit=$($process.ExitCode)`n$output")
    Write-EstuaryFile (Join-Path $scratch 'commands.log') ($log -join "`n")
    if($process.ExitCode -ne $Exit){throw "Expected exit $Exit, got $($process.ExitCode): $output"}
    return $output
  } finally {$process.Dispose()}
}
function Assert-Estuary([string]$Text,[string]$Expected) {
  if(-not $Text.Contains($Expected,[StringComparison]::Ordinal)){throw "Missing '$Expected': $Text"}
}
function Initialize-EstuaryInputs([string]$Sample,[string[]]$Native=@()) {
  $directory=Split-Path (Join-Path $root $Sample)
  $manifest=Get-Content (Join-Path $directory 'verification.json') -Raw|ConvertFrom-Json -AsHashtable
  $dependencies=@((Get-Content (Join-Path $directory 'obj/project.assets.json') -Raw|ConvertFrom-Json -AsHashtable).libraries.Keys|Sort-Object -CaseSensitive)
  if($Record){
    $manifest.dependencies=$dependencies;$manifest.nativeToolchains=@($Native)
    Write-EstuaryFile (Join-Path $directory 'verification.json') (($manifest|ConvertTo-Json -Depth 10)+"`n")
    foreach($lock in Get-ChildItem -LiteralPath $directory -Filter packages.lock.json){Write-EstuaryFile $lock.FullName (Get-Content $lock.FullName -Raw)}
  }
  return Get-EvidenceInputs $root $Sample
}
function Assert-EstuaryInputs($Before,$After) {
  if(($Before|ConvertTo-Json -Compress) -cne ($After|ConvertTo-Json -Compress)){throw 'Sample inputs changed during execution; recapture required'}
}
function Save-EstuaryEvidence([string]$Sample,[string]$Sdk,[string]$Language,[string]$Target,[string]$Scope,[string]$Info,[string]$Command,[string[]]$Native=@(),$ExpectedInputs) {
  $directory=Split-Path (Join-Path $root $Sample)
  $dependencies=@((Get-Content (Join-Path $directory 'obj/project.assets.json') -Raw|ConvertFrom-Json -AsHashtable).libraries.Keys|Sort-Object -CaseSensitive)
  $inputs=Get-EvidenceInputs $root $Sample
  Assert-EstuaryInputs $ExpectedInputs $inputs
  if($Record){
    $recordValue=[ordered]@{
      kind='compile-and-run';sample=$Sample;target=$Target;sdk=$Sdk;language=$Language;command=$Command;scope=$Scope
      recordedAt=[DateTimeOffset]::UtcNow.ToString('O');sourceCommit=(& 'C:/Program Files/Git/cmd/git.exe' -C $root rev-parse HEAD).Trim();sourceDirty=$true
      inputs=$inputs;dependencies=$dependencies;workloads=@();nativeToolchains=@($Native)
      host=[Runtime.InteropServices.RuntimeInformation]::OSDescription+' / '+[Runtime.InteropServices.RuntimeInformation]::OSArchitecture
      sdkInfo=$Info;workloadInfo='No optional workloads';output=($log -join "`n")
    }
    Write-EstuaryFile (Join-Path $directory 'verification-evidence.json') (($recordValue|ConvertTo-Json -Depth 30)+"`n")
  }
}
