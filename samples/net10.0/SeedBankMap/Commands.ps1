# snippet:baseline
param([Parameter(Mandatory)][string]$Dotnet,
      [Parameter(Mandatory)][string]$Repository)
# Supply an absolute SDK executable and an existing, configured seed-bank solution.
# This is the full-test baseline. Do not add affected-test switches to this lane.
$start = [Diagnostics.ProcessStartInfo]::new($Dotnet)
$start.UseShellExecute = $false
$start.WorkingDirectory = $Repository
$start.ArgumentList.Add('test')
$start.Environment['DOTNET_ADD_GLOBAL_TOOLS_TO_PATH'] = '0'
$start.Environment.Remove('DOTNET_CLI_ENABLE_AFFECTED_TESTS') | Out-Null
$start.Environment.Remove('DOTNET_CLI_TEST_AFFECTED_TESTS_MODE') | Out-Null
$child = [Diagnostics.Process]::Start($start)
try { $child.WaitForExit(); exit $child.ExitCode } finally { $child.Dispose() }
# endsnippet:baseline
