# snippet:complete
param([Parameter(Mandatory)][string]$Dotnet,[Parameter(Mandatory)][string]$EfTool)
# EF10 diagnostics share stdout. The supported output option avoids brittle log filtering.
& $Dotnet $EfTool migrations script --context BerthContext --configuration Release --no-build --output berth-deployment.sql
if ($LASTEXITCODE) { throw 'Script generation failed; do not deploy the output.' }
# endsnippet:complete
