# snippet:complete
param([Parameter(Mandatory)][string]$Dotnet,[Parameter(Mandatory)][string]$EfTool)
# Build first. Preserve SQL stdout and diagnostics stderr as separate artifacts.
& $Dotnet $EfTool migrations script --context BerthContext --configuration Release --no-build --verbose 1> berth-deployment.sql 2> berth-diagnostics.log
if ($LASTEXITCODE) { throw 'Script generation failed; do not deploy the output.' }
# endsnippet:complete
