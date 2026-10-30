# snippet:reader
[CmdletBinding()]
param(
  [Parameter(Mandatory)][string]$Dotnet,
  [Parameter(Mandatory)][string]$ProjectDirectory,
  [ValidateSet('default','false','true')][string]$ForceUtf8 = 'true',
  [int]$ReaderCodePage = 65001,
  [switch]$ProbeSourceOptOut,
  [switch]$Reject
)
$ErrorActionPreference = 'Stop'
[Text.Encoding]::RegisterProvider([Text.CodePagesEncodingProvider]::Instance)
$decoder = [Text.Encoding]::GetEncoding($ReaderCodePage,
  [Text.EncoderFallback]::ExceptionFallback, [Text.DecoderFallback]::ExceptionFallback)
$start = [Diagnostics.ProcessStartInfo]::new((Resolve-Path -LiteralPath $Dotnet).Path)
$start.WorkingDirectory = (Resolve-Path -LiteralPath $ProjectDirectory).Path
$start.UseShellExecute = $false
# Inherit the caller's console if present. The authoring fixture owns a private one.
$start.CreateNoWindow = $false
$start.RedirectStandardOutput = $true
$start.RedirectStandardError = $true
foreach ($argument in @('build','-c','Release','--no-restore','--disable-build-servers','-v:minimal',
    '-p:TreatWarningsAsErrors=false',"-p:EstuaryReject=$($Reject.IsPresent.ToString().ToLowerInvariant())")) {
  $start.ArgumentList.Add($argument)
}
$start.Environment['DOTNET_ADD_GLOBAL_TOOLS_TO_PATH'] = '0'
$start.Environment['DOTNET_CLI_UI_LANGUAGE'] = 'en-US'
$start.Environment['DOTNET_CLI_USE_MSBUILD_SERVER'] = 'false'
$start.Environment['DOTNET_CLI_TELEMETRY_OPTOUT'] = '1'
# A diagnostic control only: this source-level switch did not change the tested pipe.
$null = $start.Environment.Remove('DOTNET_CLI_CONSOLE_USE_DEFAULT_ENCODING')
if ($ProbeSourceOptOut) { $start.Environment['DOTNET_CLI_CONSOLE_USE_DEFAULT_ENCODING'] = '1' }
if ($ForceUtf8 -eq 'default') { $null = $start.Environment.Remove('DOTNET_CLI_FORCE_UTF8_ENCODING') }
else { $start.Environment['DOTNET_CLI_FORCE_UTF8_ENCODING'] = $ForceUtf8 }
$process = [Diagnostics.Process]::Start($start)
$stdout = [IO.MemoryStream]::new()
$stderr = [IO.MemoryStream]::new()
try {
  # Capture bytes before decoding. Drain both pipes concurrently to avoid deadlock.
  $outTask = $process.StandardOutput.BaseStream.CopyToAsync($stdout)
  $errTask = $process.StandardError.BaseStream.CopyToAsync($stderr)
  if (-not $process.WaitForExit(120000)) { $process.Kill($true); throw 'Estuary build timed out' }
  $outTask.GetAwaiter().GetResult()
  $errTask.GetAwaiter().GetResult()
  [pscustomobject]@{
    ExitCode = $process.ExitCode
    StdoutBytes = $stdout.ToArray()
    StderrBytes = $stderr.ToArray()
    Stdout = $decoder.GetString($stdout.ToArray())
    Stderr = $decoder.GetString($stderr.ToArray())
    ReaderCodePage = $ReaderCodePage
    ForceUtf8 = $ForceUtf8
    ProbeSourceOptOut = $ProbeSourceOptOut.IsPresent
  }
}
finally { $process.Dispose(); $stdout.Dispose(); $stderr.Dispose() }
# endsnippet:reader
