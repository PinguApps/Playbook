#Requires -Version 7.0
[CmdletBinding()]
param(
  [string]$Net10Dotnet = (Get-Command dotnet -CommandType Application | Select-Object -First 1).Source,
  [string]$Net11Dotnet = (Get-Command dotnet -CommandType Application | Select-Object -First 1).Source
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
. (Join-Path $PSScriptRoot 'Evidence.ps1')
$paths = @{}
foreach ($scope in 'Process','User','Machine') { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$previous = @{}
foreach ($key in 'DOTNET_ADD_GLOBAL_TOOLS_TO_PATH','DOTNET_GENERATE_ASPNET_CERTIFICATE') { $previous[$key] = [Environment]::GetEnvironmentVariable($key) }
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  $env:DOTNET_GENERATE_ASPNET_CERTIFICATE = 'false'
  foreach ($version in 10,11) {
    $dotnet = if ($version -eq 10) { $Net10Dotnet } else { $Net11Dotnet }
    foreach ($name in 'MuseumCaptions','ObservatoryLog','HarbourTls') {
      $sample = "samples/net$version.0/$name/$name.csproj"
      $directory = Join-Path $root "samples/net$version.0/$name"
      Push-Location -LiteralPath $directory
      try {
        & $dotnet restore "$name.csproj" --force-evaluate
        # Keep the exact lock canonical and embeddable; NuGet accepts JSON comments.
        $lockPath = Join-Path $directory 'packages.lock.json'
        $lock = [IO.File]::ReadAllText($lockPath).Replace("`r`n","`n").Trim()
        if (-not $lock.StartsWith('// snippet:complete', [StringComparison]::Ordinal)) {
          [IO.File]::WriteAllText($lockPath, "// snippet:complete`n$lock`n// endsnippet:complete`n")
        }
        $assets = Get-Content obj/project.assets.json -Raw | ConvertFrom-Json -AsHashtable
        $manifestPath = Join-Path $directory 'verification.json'
        $manifest = Get-Content $manifestPath -Raw | ConvertFrom-Json -AsHashtable
        $manifest.dependencies = @($assets.libraries.Keys | Sort-Object -CaseSensitive)
        [IO.File]::WriteAllText($manifestPath, ($manifest | ConvertTo-Json -Depth 20).Replace("`r`n","`n") + "`n")
        & (Join-Path $PSScriptRoot 'Record-Evidence.ps1') -Sample $sample -Dotnet $dotnet -Kind compile
      }
      finally { Pop-Location }
    }
  }
}
finally {
  foreach ($key in $previous.Keys) { [Environment]::SetEnvironmentVariable($key,$previous[$key]) }
  foreach ($scope in $paths.Keys) { if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved" } }
}
