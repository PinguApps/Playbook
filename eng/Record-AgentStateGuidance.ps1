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
        if ($version -eq 11 -and $name -eq 'HarbourTls') {
          $before = Get-EvidenceInputs $root $sample
          $PSNativeCommandUseErrorActionPreference = $false
          $result = (& $dotnet build "$name.csproj" -c Release --no-restore 2>&1 | Out-String)
          $exit = $LASTEXITCODE
          $PSNativeCommandUseErrorActionPreference = $true
          if ($exit -eq 0 -or $result -notmatch "CS0246.*DirectTlsEndpoint") { throw "Expected unavailable documented endpoint API, received: $result" }
          if (($before | ConvertTo-Json -Compress) -cne ((Get-EvidenceInputs $root $sample) | ConvertTo-Json -Compress)) { throw 'Inputs changed during availability check' }
          $record = [ordered]@{
            kind='official-review'; sample=$sample; target='net11.0'; sdk='11.0.100'; language='15.0'
            command="$dotnet restore $name.csproj --force-evaluate; $dotnet build $name.csproj -c Release --no-restore (expected CS0246); review pinned DirectTls documentation and exact package metadata"
            scope='Documentation-based current example; compilation attempted and blocked by absent DirectTlsEndpoint in pinned RC1. Not a successful compilation or native Linux execution record.'
            recordedAt=[DateTimeOffset]::UtcNow.ToString('O'); sourceCommit=(& 'C:/Program Files/Git/cmd/git.exe' -C $root rev-parse HEAD).Trim(); sourceDirty=$true
            inputs=$before; dependencies=@($manifest.dependencies); workloads=@(); nativeToolchains=@()
            host=[Runtime.InteropServices.RuntimeInformation]::OSDescription; sdkInfo=(& $dotnet --info | Out-String).Trim()
            workloadInfo='No workloads or native OpenSSL runtime qualified'; output=$result.Trim()
          }
          [IO.File]::WriteAllText((Join-Path $directory 'source-review.evidence.json'), ($record | ConvertTo-Json -Depth 30).Replace("`r`n","`n") + "`n")
          $global:LASTEXITCODE = 0
          Write-Host 'PASS: documented DirectTlsEndpoint absence reproduced; source review retained, no successful compilation claimed.'
        }
        else {
          & (Join-Path $PSScriptRoot 'Record-Evidence.ps1') -Sample $sample -Dotnet $dotnet -Kind compile
        }
      }
      finally { Pop-Location }
    }
  }
}
finally {
  foreach ($key in $previous.Keys) { [Environment]::SetEnvironmentVariable($key,$previous[$key]) }
  foreach ($scope in $paths.Keys) { if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved" } }
}
