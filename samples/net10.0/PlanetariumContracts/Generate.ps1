# snippet:complete
$dotnet = 'C:/Program Files/dotnet/dotnet.exe'
$previousIntegration = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
$previousCase = $env:PLANETARIUM_CASE
Push-Location -LiteralPath $PSScriptRoot
try {
    $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
    $env:PLANETARIUM_CASE = 'normal'
    & $dotnet restore ./PlanetariumContracts.csproj
    & $dotnet build ./PlanetariumContracts.csproj -c Release -t:Rebuild `
        -p:RestoreLockedMode=true -p:OpenApiGenerateDocuments=true `
        '-p:OpenApiGenerateDocumentsOptions=--openapi-version OpenApi3_0' `
        "-p:OpenApiDocumentsDirectory=$([IO.Path]::GetFullPath('./contracts-normal'))"
    & $dotnet ./bin/Release/net10.0/PlanetariumContracts.dll
    Get-Content ./contracts-normal/PlanetariumContracts_public.json
    Get-Content ./contracts-normal/PlanetariumContracts_staff.json
}
finally {
    Pop-Location
    $env:PLANETARIUM_CASE = $previousCase
    $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previousIntegration
}
# endsnippet:complete
