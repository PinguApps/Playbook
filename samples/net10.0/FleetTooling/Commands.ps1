# snippet:complete
param([Parameter(Mandatory)][string]$Dotnet,[Parameter(Mandatory)][string]$EfTool)
foreach ($context in 'BerthContext','BeaconContext') {
    & $Dotnet $EfTool migrations list --context $context --configuration Release --no-build
    if ($LASTEXITCODE) { throw "List failed for $context" }
    & $Dotnet $EfTool migrations script --context $context --configuration Release --no-build --output "$context.sql"
    if ($LASTEXITCODE) { throw "Script failed for $context" }
}
# endsnippet:complete
