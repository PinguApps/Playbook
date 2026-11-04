# snippet:complete
param([Parameter(Mandatory)][string]$Dotnet,[Parameter(Mandatory)][string]$EfTool)
# RC2 wildcard list has a confirmed fall-through bug: use explicit names until fixed.
foreach ($context in 'BerthContext','BeaconContext') {
    & $Dotnet $EfTool migrations list --context $context --configuration Release --no-build
    if ($LASTEXITCODE) { throw "List failed for $context" }
}
& $Dotnet $EfTool migrations script --context '*' --configuration Release --no-build --output fleet-combined.sql
if ($LASTEXITCODE) { throw 'Wildcard script failed.' }
# Combined output contains both models, not one deployable file per database.
# Review targets before using database update --context '*' or database drop --context '*' --force.
# endsnippet:complete
