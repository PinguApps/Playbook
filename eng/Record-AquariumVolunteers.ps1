[CmdletBinding()]
param(
  [string]$Dotnet = 'C:/Program Files/dotnet/dotnet.exe',
  [string]$Net11Dotnet = 'C:/Users/pingu/.dotnet-playbook-11/dotnet.exe'
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
. (Join-Path $PSScriptRoot 'Evidence.ps1')
$beforePath = @{}
foreach ($scope in 'Process','User','Machine') { $beforePath[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$previousIntegration = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  $sources = Get-Content (Join-Path $root 'docs/planning/audits/web.sources.json') -Raw | ConvertFrom-Json
  $audit = Get-Content (Join-Path $root 'docs/planning/audits/web.assets.json') -Raw | ConvertFrom-Json
  $sourceProof = [Collections.Generic.List[string]]::new()
  $temporary = Join-Path ([IO.Path]::GetTempPath()) ('aquarium-sources-' + [guid]::NewGuid().ToString('N'))
  New-Item -ItemType Directory -Path $temporary | Out-Null
  try {
    $selected = @($sources.sources | Where-Object { $_.path -match '^src/Identity/UI/src/Areas/Identity/Pages/V5/Account/(Register|ExternalLogin)\.cshtml\.cs$' -or $_.path -eq 'release-notes/11.0/preview/rc1/aspnetcore.md' })
    if ($selected.Count -ne 5) { throw 'Expected four full registration implementations and the official confirmation change note.' }
    foreach ($source in $selected) {
      if ($source.commit -notmatch '^[0-9a-f]{40}$' -or $source.url -cne "https://raw.githubusercontent.com/$($source.repository)/$($source.commit)/$($source.path)") { throw 'Unpinned registration source.' }
      $file = Join-Path $temporary 'source'
      Invoke-WebRequest $source.url -OutFile $file
      $hash = (Get-FileHash -LiteralPath $file).Hash.ToLowerInvariant()
      if ($hash -cne $source.sha256) { throw "Source mismatch: $($source.path)" }
      $sourceProof.Add("SOURCE $($source.url) | $hash")
    }
  }
  finally {
    $resolved = [IO.Path]::GetFullPath($temporary)
    if (-not $resolved.StartsWith([IO.Path]::GetFullPath([IO.Path]::GetTempPath()),[StringComparison]::OrdinalIgnoreCase) -or (Split-Path $resolved -Leaf) -notlike 'aquarium-sources-*') { throw 'Unexpected source cleanup path.' }
    Remove-Item -LiteralPath $resolved -Recurse -Force
  }
  foreach ($release in '10','11') {
    $sample = "samples/net$release.0/AquariumVolunteers/AquariumVolunteers.csproj"
    $directory = Join-Path $root (Split-Path $sample)
    $hostDotnet = if ($release -eq '10') { $Dotnet } else { $Net11Dotnet }
    & (Join-Path $PSScriptRoot 'Record-Evidence.ps1') -Sample $sample -Dotnet $hostDotnet -RuntimeDotnet $hostDotnet
    $record = Get-Content (Join-Path $directory 'verification-evidence.json') -Raw | ConvertFrom-Json -AsHashtable
    $inputs = Get-EvidenceInputs $root $sample
    $output = [Collections.Generic.List[string]]::new()
    $output.AddRange($sourceProof)
    $assets = Get-Content (Join-Path $directory 'obj/project.assets.json') -Raw | ConvertFrom-Json -AsHashtable
    $output.Add('ACTUAL-RESOLVED-ASSETS ' + (ConvertTo-Json $assets.targets -Depth 40 -Compress))
    $packageRoot = @($assets.packageFolders.Keys)[0]
    foreach ($library in ($assets.libraries.Keys | Sort-Object -CaseSensitive)) {
      $package = Join-Path $packageRoot ($library.ToLowerInvariant() + '/' + $library.Replace('/','.').ToLowerInvariant() + '.nupkg')
      $hash = (Get-FileHash -LiteralPath $package).Hash.ToLowerInvariant()
      $baseline = $audit.baselines | Where-Object target -EQ "net$release.0"
      $audited = @($baseline.packages | Where-Object { "$($_.id)/$($_.version)" -ieq $library })
      if ($audited.Count -gt 0 -and $hash -cne $audited[0].sha256) { throw "Audited package mismatch: $library" }
      $output.Add("PACKAGE $library | SHA256=$hash | auditMatch=$($audited.Count -gt 0)")
    }
    $framework = if ($release -eq '10') { '10.0.13' } else { '11.0.0-rc.2.26504.105' }
    $baseline = $audit.baselines | Where-Object target -EQ "net$release.0"
    foreach ($name in 'Microsoft.AspNetCore.Identity.dll','Microsoft.Extensions.Identity.Core.dll','Microsoft.Extensions.Identity.Stores.dll') {
      $reference = Join-Path (Split-Path $hostDotnet -Parent) "packs/Microsoft.AspNetCore.App.Ref/$framework/ref/net$release.0/$name"
      $hash = (Get-FileHash -LiteralPath $reference).Hash.ToLowerInvariant()
      $expected = @($baseline.framework | Where-Object name -CEQ $name)
      if ($expected.Count -ne 1 -or $hash -cne $expected[0].sha256) { throw "Reference differs from source audit: $name" }
      $output.Add("REFERENCE $name | $framework | SHA256=$hash | auditMatch=true")
    }
    if ((ConvertTo-Json $inputs -Compress) -cne (ConvertTo-Json (Get-EvidenceInputs $root $sample) -Compress)) { throw 'Qualification inputs changed.' }
    $record.kind = 'execution'
    $record.command = "eng/Record-AquariumVolunteers.ps1 -Dotnet '$Dotnet' -Net11Dotnet '$Net11Dotnet'"
    $record.scope = 'Actual locked package ZIPs, selected compile/runtime/native assets and reference identities reconciled with PIN-709; four complete immutable registration implementations and official change note checked. Compilation/run record separately contains real UI/SQLite/request/cookie controls. No third-party provider or GA claim.'
    $record.output = $output -join "`n"
    [IO.File]::WriteAllText((Join-Path $directory 'artifacts.evidence.json'), (ConvertTo-Json $record -Depth 40).Replace("`r`n","`n") + "`n")
    Write-Host "PASS: net$release.0 original and packaged UI registration matrix plus actual artifact/source qualification."
  }
}
finally {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previousIntegration
  foreach ($scope in $beforePath.Keys) {
    if ($beforePath[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." }
  }
}
