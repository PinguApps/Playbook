# snippet:verify
[CmdletBinding()]
param(
  [Parameter(Mandatory)][string]$Notation,
  [Parameter(Mandatory)][string]$Configuration,
  [Parameter(Mandatory)][string]$Image
)
$ErrorActionPreference = 'Stop'
if (-not [IO.Path]::IsPathFullyQualified($Notation) -or -not [IO.Path]::IsPathFullyQualified($Configuration)) {
  throw 'Use absolute executable and isolated configuration paths.'
}
if ($Image -cnotmatch '^mcr[.]microsoft[.]com/dotnet/runtime@sha256:[a-f0-9]{64}$') {
  throw 'Select the runtime repository and immutable digest explicitly.'
}
$start = [Diagnostics.ProcessStartInfo]::new($Notation)
$start.ArgumentList.Add('verify')
$start.ArgumentList.Add($Image)
$start.UseShellExecute = $false
$start.CreateNoWindow = $true
$start.RedirectStandardOutput = $true
$start.RedirectStandardError = $true
$start.Environment['NOTATION_CONFIG'] = $Configuration
$start.Environment['NOTATION_CACHE'] = Join-Path $Configuration 'cache'
$start.Environment['NOTATION_LIBEXEC'] = Join-Path $Configuration 'plugins'
# Preserve the actual verifier response even when the calling agent redirects output.
$process = [Diagnostics.Process]::Start($start)
try {
  $stdout = $process.StandardOutput.ReadToEndAsync()
  $stderr = $process.StandardError.ReadToEndAsync()
  if (-not $process.WaitForExit(120000)) { $process.Kill($true); throw 'Image verification timed out.' }
  Write-Output ($stdout.GetAwaiter().GetResult() + $stderr.GetAwaiter().GetResult())
  if ($process.ExitCode -ne 0) { throw "Base-image verification failed: $Image" }
}
finally { $process.Dispose() }
# endsnippet:verify
