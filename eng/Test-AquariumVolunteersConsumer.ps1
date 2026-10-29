[CmdletBinding()]
param(
  [string]$ResponsePath = (Join-Path $PSScriptRoot '../artifacts/pin1177/inspection/identity.confirm-before-registration-signin.json'),
  [string]$Dotnet = 'C:/Program Files/dotnet/dotnet.exe',
  [string]$Net11Dotnet = 'C:/Users/pingu/.dotnet-playbook-11/dotnet.exe',
  [string]$OutputDirectory = (Join-Path $PSScriptRoot '../artifacts/pin1177/consumer')
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$OutputDirectory = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($OutputDirectory)
$standalone = Join-Path ([IO.Path]::GetTempPath()) ('playbook-aquarium-consumer-' + [guid]::NewGuid().ToString('N'))
$beforePath = @{}
foreach ($scope in 'Process','User','Machine') { $beforePath[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$previousIntegration = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  $recipe = (Get-Content -LiteralPath $ResponsePath -Raw | ConvertFrom-Json).result.structuredContent.recipe
  if ($recipe.id -cne 'identity.confirm-before-registration-signin' -or $recipe.edition -cne '11.0') { throw 'Wrong installed recipe.' }
  $blocks = [regex]::Matches($recipe.markdown, '(?ms)^#### `net(10|11)\.0/AquariumVolunteers/([^`]+)`\n\n(?:First line: `@page`\. Then append this body:\n\n)?```[^\n]*\n(.*?)^```$')
  $expected = @('AquariumVolunteers.csproj','ArtifactIdentity.cs','Directory.Build.props','Directory.Build.targets','Directory.Packages.props','global.json','LocalProvider.cs','packages.lock.json','Pages/_ViewImports.cshtml','Pages/Shared/_LoginPartial.cshtml','Pages/Volunteer/Register.cshtml','Pages/Volunteer/Register.cshtml.cs','Pages/Volunteer/ExternalLogin.cshtml','Pages/Volunteer/ExternalLogin.cshtml.cs','Program.cs','RegistrationChecks.cs')
  if ($blocks.Count -ne 2 * $expected.Count) { throw 'Incomplete returned standalone files.' }
  foreach ($major in 10,11) {
    $selected = @($blocks | Where-Object { $_.Groups[1].Value -ceq "$major" })
    $files = @($selected | ForEach-Object { $_.Groups[2].Value } | Sort-Object -CaseSensitive)
    if (($files | ConvertTo-Json -Compress) -cne (@($expected | Sort-Object -CaseSensitive) | ConvertTo-Json -Compress)) { throw "Wrong .NET$major file set." }
    $directory = Join-Path $standalone "net$major.0/AquariumVolunteers"
    foreach ($block in $selected) {
      $name = $block.Groups[2].Value
      $file = Join-Path $directory $name
      New-Item -ItemType Directory -Path (Split-Path $file -Parent) -Force | Out-Null
      $contents = $block.Groups[3].Value
      if ($name -cin @('Pages/Volunteer/Register.cshtml','Pages/Volunteer/ExternalLogin.cshtml')) { $contents = "@page`n" + $contents }
      [IO.File]::WriteAllText($file,$contents)
    }
  }
  # Restored locked packages may be used, but no remote feed is available for this reconstruction.
  [IO.File]::WriteAllText((Join-Path $standalone 'NuGet.Config'),'<configuration><packageSources><clear/></packageSources></configuration>')
  New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
  $results = @()
  foreach ($major in 10,11) {
    $executable = if ($major -eq 10) { $Dotnet } else { $Net11Dotnet }
    $directory = Join-Path $standalone "net$major.0/AquariumVolunteers"
    Push-Location -LiteralPath $directory
    try {
      $sdk = (& $executable --version | Out-String).Trim()
      $evidence = @($recipe.evidence | Where-Object { $_.sample -ceq "samples/net$major.0/AquariumVolunteers/AquariumVolunteers.csproj" -and $_.kind -ceq 'compile-and-run' })
      if ($evidence.Count -ne 1 -or $sdk -cne $evidence[0].sdk -or $evidence[0].freshness -cne 'current') { throw 'Wrong SDK or stale installed evidence.' }
      & $executable build AquariumVolunteers.csproj -c Release -p:RestoreLockedMode=true *> (Join-Path $OutputDirectory "net$major-build.log")
      $assets = Get-Content -LiteralPath 'obj/project.assets.json' -Raw | ConvertFrom-Json -AsHashtable
      $dependencies = @($assets.libraries.Keys | Sort-Object -CaseSensitive)
      if (($dependencies | ConvertTo-Json -Compress) -cne (@($evidence[0].dependencies) | ConvertTo-Json -Compress)) { throw 'Reconstructed dependency closure drifted.' }
      $output = (& $executable "./bin/Release/net$major.0/AquariumVolunteers.dll" | Out-String).Trim()
      if ($output -notmatch '(?m)^PASS: default and original UI;' -or @($output -split '\r?\n' | Where-Object { $_ -match '^CHECK ' }).Count -ne 16) { throw "Consumer .NET$major did not execute the full registration matrix." }
      [IO.File]::WriteAllText((Join-Path $OutputDirectory "net$major-run.log"),$output)
      $results += @{ target = "net$major.0"; sdk = $sdk; dependencies = $dependencies; result = 'PASS'; registrations = 16 }
    }
    finally { Pop-Location }
  }
  @{ responseSha256 = (Get-FileHash -LiteralPath $ResponsePath).Hash.ToLowerInvariant(); consumers = $results } |
    ConvertTo-Json -Depth 10 | Set-Content -LiteralPath (Join-Path $OutputDirectory 'consumer.json')
  Write-Host 'PASS: both complete consumers reconstructed only from installed Markdown, built with locked dependencies and no remote feeds, and executed 32 actual UI/SQLite/registration checks plus confirmation/session/failure controls.'
}
finally {
  if (Test-Path -LiteralPath $standalone) {
    $resolved = [IO.Path]::GetFullPath($standalone)
    if (-not $resolved.StartsWith([IO.Path]::GetFullPath([IO.Path]::GetTempPath()),[StringComparison]::OrdinalIgnoreCase) -or (Split-Path $resolved -Leaf) -notlike 'playbook-aquarium-consumer-*') { throw 'Unexpected consumer cleanup path.' }
    Remove-Item -LiteralPath $resolved -Recurse -Force
  }
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previousIntegration
  foreach ($scope in $beforePath.Keys) { if ($beforePath[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." } }
}
