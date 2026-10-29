[CmdletBinding()]
param(
  [string]$ResponsePath = (Join-Path $PSScriptRoot '../artifacts/pin1176/inspection/web.order-static-asset-routes.json'),
  [string]$Dotnet = 'C:/Program Files/dotnet/dotnet.exe',
  [string]$Net11Dotnet = 'C:/Users/pingu/.dotnet-playbook-11/dotnet.exe',
  [string]$OutputDirectory = (Join-Path $PSScriptRoot '../artifacts/pin1176/consumer')
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$OutputDirectory = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($OutputDirectory)
$standalone = Join-Path ([IO.Path]::GetTempPath()) ('playbook-amber-consumer-' + [guid]::NewGuid().ToString('N'))
$before = @{}
foreach ($scope in 'Process','User','Machine') { $before[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$previous = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  $recipe = (Get-Content -LiteralPath $ResponsePath -Raw | ConvertFrom-Json).result.structuredContent.recipe
  if ($recipe.id -cne 'web.order-static-asset-routes' -or $recipe.edition -cne '11.0') { throw 'Wrong installed recipe.' }
  $blocks = [regex]::Matches($recipe.markdown, '(?ms)^### \.NET (10|11): `([^`]+)`\n\n```[^\n]*\n(.*?)^```$')
  if ($blocks.Count -ne 18) { throw 'Incomplete returned standalone files.' }
  $expected = @('global.json','AmberGallery.csproj','Directory.Build.props','Directory.Build.targets','Directory.Packages.props','ArtifactIdentity.cs','Program.cs','wwwroot/exhibits/amber.html','wwwroot/labels/amber.html')
  foreach ($major in 10,11) {
    $selected = @($blocks | Where-Object { $_.Groups[1].Value -ceq "$major" })
    $files = @($selected | ForEach-Object { $_.Groups[2].Value } | Sort-Object -CaseSensitive)
    if (($files | ConvertTo-Json -Compress) -cne (@($expected | Sort-Object -CaseSensitive) | ConvertTo-Json -Compress)) { throw "Wrong .NET$major file set." }
    $directory = Join-Path $standalone "net$major.0/AmberGallery"
    foreach ($block in $selected) {
      $file = Join-Path $directory $block.Groups[2].Value
      New-Item -ItemType Directory -Path (Split-Path $file -Parent) -Force | Out-Null
      [IO.File]::WriteAllText($file,$block.Groups[3].Value)
    }
  }
  # All claimed dependencies are installed framework/SDK assets. No remote feed is available.
  [IO.File]::WriteAllText((Join-Path $standalone 'NuGet.Config'),'<configuration><packageSources><clear/></packageSources></configuration>')
  New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
  $results = @()
  foreach ($major in 10,11) {
    $executable = if ($major -eq 10) { $Dotnet } else { $Net11Dotnet }
    $directory = Join-Path $standalone "net$major.0/AmberGallery"
    Push-Location -LiteralPath $directory
    try {
      $sdk = (& $executable --version | Out-String).Trim()
      $evidence = @($recipe.evidence | Where-Object sample -CEQ "samples/net$major.0/AmberGallery/AmberGallery.csproj")
      if ($evidence.Count -ne 1 -or $sdk -cne $evidence[0].sdk -or $evidence[0].freshness -cne 'current') { throw 'Wrong SDK or stale installed evidence.' }
      & $executable build AmberGallery.csproj -c Release *> (Join-Path $OutputDirectory "net$major-build.log")
      $assets = Get-Content -LiteralPath 'obj/project.assets.json' -Raw | ConvertFrom-Json -AsHashtable
      $dependencies = @($assets.libraries.Keys | Sort-Object -CaseSensitive)
      if ($dependencies.Count -ne 0 -or @($evidence[0].dependencies).Count -ne 0) { throw 'Expected empty package closure.' }
      $output = (& $executable "./bin/Release/net$major.0/AmberGallery.dll" | Out-String).Trim()
      if ($output -notmatch '(?m)^PASS AmberGallery:') { throw "Consumer .NET$major did not execute." }
      [IO.File]::WriteAllText((Join-Path $OutputDirectory "net$major-run.log"),$output)
      $results += @{ target = "net$major.0"; sdk = $sdk; dependencies = $dependencies; result = 'PASS'; directory = $directory }
    }
    finally { Pop-Location }
  }
  @{ responseSha256 = (Get-FileHash -LiteralPath $ResponsePath).Hash.ToLowerInvariant(); consumers = $results } |
    ConvertTo-Json -Depth 10 | Set-Content -LiteralPath (Join-Path $OutputDirectory 'consumer.json')
  Write-Host 'PASS: both complete consumers reconstructed only from installed Markdown, built with no remote feeds and executed real static manifest/HTTP/failure checks in an unrelated directory.'
}
finally {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previous
  foreach ($scope in $before.Keys) { if ($before[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." } }
}
