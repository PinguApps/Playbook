[CmdletBinding()]
param(
  [string]$MSBuild = 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\amd64\MSBuild.exe',
  [string]$FrameworkReferences = 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8',
  [string]$Dotnet = 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe',
  [switch]$Record
)
& (Join-Path $PSScriptRoot '../samples/net11.0/BuildAccessObserver/Test-Observer.ps1') @PSBoundParameters
