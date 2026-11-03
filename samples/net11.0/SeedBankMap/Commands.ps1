# snippet:affected
param([Parameter(Mandatory)][string]$Dotnet,
      [Parameter(Mandatory)][string]$Repository,
      [Parameter(Mandatory)][ValidateSet('collect','select')][string]$Mode)
# Documentation-based command shape, NOT a complete extension/storage setup.
# Run only after the selected MTP hosts register a compatible AffectedTests extension.
$start = [Diagnostics.ProcessStartInfo]::new($Dotnet)
$start.UseShellExecute = $false
$start.WorkingDirectory = $Repository
$start.ArgumentList.Add('test')
$start.ArgumentList.Add($(if ($Mode -eq 'collect') { '--collect-test-map' } else { '--affected-tests' }))
$start.Environment['DOTNET_ADD_GLOBAL_TOOLS_TO_PATH'] = '0'
$start.Environment['DOTNET_CLI_ENABLE_AFFECTED_TESTS'] = '1'
# This marker belongs to the SDK; applications must never manufacture it.
$start.Environment.Remove('DOTNET_CLI_TEST_AFFECTED_TESTS_MODE') | Out-Null
$child = [Diagnostics.Process]::Start($start)
try { $child.WaitForExit(); exit $child.ExitCode } finally { $child.Dispose() }
# endsnippet:affected
