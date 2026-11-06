[CmdletBinding()]
param([switch]$Record, [switch]$VerifySources,
  [string]$Dotnet='C:/Users/pingu/AppData/Local/dotnetup/dotnet/dotnet.exe')
$ErrorActionPreference='Stop'
$PSNativeCommandUseErrorActionPreference=$true
$root=Split-Path $PSScriptRoot -Parent
$before=@{}; foreach($scope in 'Process','User','Machine'){$before[$scope]=[Environment]::GetEnvironmentVariable('PATH',$scope)}
$saved=@{}; foreach($key in 'DOTNET_ADD_GLOBAL_TOOLS_TO_PATH','DOTNET_GENERATE_ASPNET_CERTIFICATE','DOTNET_CLI_TELEMETRY_OPTOUT','MSBUILDUSESERVER','UseSharedCompilation'){$saved[$key]=[Environment]::GetEnvironmentVariable($key)}
Push-Location $root
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH='0'; $env:DOTNET_GENERATE_ASPNET_CERTIFICATE='false'; $env:DOTNET_CLI_TELEMETRY_OPTOUT='1'; $env:MSBUILDUSESERVER='0'; $env:UseSharedCompilation='false'
  if($VerifySources){foreach($source in (Get-Content samples/GannetAuthentication/sources.json -Raw|ConvertFrom-Json).sources){
    $bytes=(Invoke-WebRequest $source.url).RawContentStream.ToArray()
    if([Convert]::ToHexStringLower([Security.Cryptography.SHA256]::HashData($bytes)) -cne $source.sha256){throw "Source drift: $($source.url)"}
  }}
  foreach($major in 10,11){
    $sample="samples/net$major.0/GannetGateway/GannetGateway.csproj"
    if($Record){& "$PSScriptRoot/Record-Evidence.ps1" -Sample $sample -Dotnet $Dotnet -Kind compile-and-run}
    else {Push-Location (Split-Path $sample);try{& $Dotnet run -c Release -p:RestoreLockedMode=true}finally{Pop-Location}}
  }
} finally {
  Pop-Location
  foreach($key in $saved.Keys){[Environment]::SetEnvironmentVariable($key,$saved[$key],'Process')}
  foreach($scope in $before.Keys){if($before[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)){throw "Unexpected PATH change: $scope"}}
}
