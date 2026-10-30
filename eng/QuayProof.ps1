# Authoring-only helpers. Child environments never change PATH or persistent settings.
function Write-QuayFile([string]$Path,[string]$Content) {
  [IO.File]::WriteAllText($Path,$Content.Replace("`r`n","`n"))
}
function Assert-QuayText([string]$Actual,[string]$Expected) {
  if (-not $Actual.Contains($Expected,[StringComparison]::Ordinal)) { throw "Missing '$Expected': $Actual" }
}
function Invoke-Quay([string]$File,[string[]]$Arguments,[string]$Directory=$root,[hashtable]$Environment=@{},[int]$Exit=0,[switch]$Failure) {
  $start = [Diagnostics.ProcessStartInfo]::new($File)
  foreach ($argument in $Arguments) { $start.ArgumentList.Add($argument) }
  $start.WorkingDirectory=$Directory; $start.UseShellExecute=$false; $start.CreateNoWindow=$true
  $start.RedirectStandardOutput=$true; $start.RedirectStandardError=$true; $start.RedirectStandardInput=$true
  $settings=@{
    DOTNET_ADD_GLOBAL_TOOLS_TO_PATH='0'; DOTNET_GENERATE_ASPNET_CERTIFICATE='false'; DOTNET_NOLOGO='1'
    DOTNET_CLI_HOME=(Join-Path $scratch 'home'); NUGET_PACKAGES=(Join-Path $scratch 'packages')
    NUGET_HTTP_CACHE_PATH=(Join-Path $scratch 'http'); DOTNET_CLI_USE_MSBUILD_SERVER='false'
    MSBUILDUSESERVER='0'; MSBUILDDISABLENODEREUSE='1'; DOTNET_CLI_TELEMETRY_OPTOUT='1'
    DOTNET_CLI_WORKLOAD_UPDATE_NOTIFY_DISABLE='true'
  }
  foreach($key in $Environment.Keys){$settings[$key]=$Environment[$key]}
  foreach($key in $settings.Keys){$start.Environment[$key]=$settings[$key]}
  $process=[Diagnostics.Process]::Start($start)
  try {
    $process.StandardInput.Close()
    $stdout=$process.StandardOutput.ReadToEndAsync(); $stderr=$process.StandardError.ReadToEndAsync()
    if(-not $process.WaitForExit(300000)){$process.Kill($true);throw 'Quay proof child timed out.'}
    $output=$stdout.GetAwaiter().GetResult()+$stderr.GetAwaiter().GetResult()
    $log.Add("cwd=$Directory`n$File $($Arguments -join ' ')`nenv=$($Environment|ConvertTo-Json -Compress);exit=$($process.ExitCode)`n$output")
    Write-QuayFile (Join-Path $scratch 'commands.log') ($log -join "`n")
    if(($Failure -and $process.ExitCode -eq 0) -or (-not $Failure -and $process.ExitCode -ne $Exit)){throw "Unexpected exit $($process.ExitCode): $output"}
    return $output
  }finally{$process.Dispose()}
}
function Save-QuayEvidence([string]$Sample,[string]$Sdk,[string]$Language,[string]$Info,[string[]]$Dependencies,[string]$Output,[string]$Scope,[string[]]$Native=@()) {
  $directory=Split-Path (Join-Path $root $Sample)
  $record=[ordered]@{
    kind='compile-and-run';sample=$Sample;target="net$($Sdk.Split('.')[0]).0";sdk=$Sdk;language=$Language
    command=$command;scope=$Scope;recordedAt=[DateTimeOffset]::UtcNow.ToString('O')
    sourceCommit=(& 'C:/Program Files/Git/cmd/git.exe' -C $root rev-parse HEAD).Trim();sourceDirty=[bool](& 'C:/Program Files/Git/cmd/git.exe' -C $root status --porcelain)
    inputs=(Get-EvidenceInputs $root $Sample);dependencies=@($Dependencies);workloads=@();nativeToolchains=@($Native)
    host=[Runtime.InteropServices.RuntimeInformation]::OSDescription+' / '+[Runtime.InteropServices.RuntimeInformation]::OSArchitecture
    sdkInfo=$Info;workloadInfo='No optional workload. See exact platform/tool identities in output.';output=$Output.Replace($root,'<repo>').Replace($scratch,'<session>')
  }
  Write-QuayFile (Join-Path $directory 'verification-evidence.json') (($record|ConvertTo-Json -Depth 40)+"`n")
}
