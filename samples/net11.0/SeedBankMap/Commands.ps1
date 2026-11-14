# snippet:affected
param([Parameter(Mandatory)][string]$Dotnet,
      [Parameter(Mandatory)][string]$Repository,
      [Parameter(Mandatory)][ValidateSet('collect','select')][string]$Mode)
# Use the complete pinned Consumer fixture; it registers the extension automatically
# and uses default local storage. GA SDK11 collection fails its handshake check.
$start = [Diagnostics.ProcessStartInfo]::new($Dotnet)
$start.UseShellExecute = $false
$start.WorkingDirectory = $Repository
$start.ArgumentList.Add('test')
$start.ArgumentList.Add('-c')
$start.ArgumentList.Add('Release')
$start.ArgumentList.Add($(if ($Mode -eq 'collect') { '--collect-test-map' } else { '--affected-tests' }))
$start.Environment['DOTNET_ADD_GLOBAL_TOOLS_TO_PATH'] = '0'
$start.Environment['DOTNET_GENERATE_ASPNET_CERTIFICATE'] = 'false'
$start.Environment['DOTNET_CLI_ENABLE_AFFECTED_TESTS'] = '1'
# This marker belongs to the SDK; applications must never manufacture it.
$start.Environment.Remove('DOTNET_CLI_TEST_AFFECTED_TESTS_MODE') | Out-Null
$child = [Diagnostics.Process]::Start($start)
try { $child.WaitForExit(); exit $child.ExitCode } finally { $child.Dispose() }
# endsnippet:affected
