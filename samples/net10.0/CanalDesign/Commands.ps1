# snippet:complete
param(
    [Parameter(Mandatory)][string]$Dotnet,
    [Parameter(Mandatory)][string]$EfTool,
    [Parameter(Mandatory)][string]$ProjectDirectory,
    [string]$Context = 'CanalContext'
)
# Matching local dotnet-ef10 DLL; paths are explicit from any working directory.
& $Dotnet $EfTool dbcontext info --project $ProjectDirectory --startup-project $ProjectDirectory --context $Context --configuration Release --no-build
# endsnippet:complete
