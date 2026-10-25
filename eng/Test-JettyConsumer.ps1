[CmdletBinding()]
param(
  [string]$Responses = (Join-Path (Split-Path $PSScriptRoot -Parent) 'artifacts/pin809-810/inspection'),
  [string]$Net10Dotnet = (Get-Command dotnet -CommandType Application | Select-Object -First 1).Source,
  [string]$Net11Dotnet = 'C:\Users\pingu\.dotnet-playbook-11\dotnet.exe'
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$Responses = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($Responses)
$directory = Join-Path ([IO.Path]::GetTempPath()) ('playbook-jetty-consumer-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $directory | Out-Null
$paths = @{}
foreach ($scope in @('Process','User','Machine')) { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$previous = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
$records = @()
try {
  foreach ($item in @(
    @{ id='authorization.share-policy-metadata'; app='JettyAccess'; count=10 },
    @{ id='identity.control-test-clock'; app='MooringClock'; count=8 }
  )) {
    $response = Join-Path $Responses "$($item.id).json"
    $recipe = Get-Content $response -Raw | ConvertFrom-Json
    $lines = $recipe.markdown -split "`n"
    $counts = @{ '10'=0; '11'=0 }
    for ($index = 0; $index -lt $lines.Count; $index++) {
      if ($lines[$index] -notmatch '^#### \.NET (10|11): \x60([^\x60]+)\x60\r?$') { continue }
      $major = $Matches[1]
      $file = $Matches[2]
      if ($file -notmatch '^[A-Za-z0-9._/-]+$' -or [IO.Path]::IsPathRooted($file) -or $file.Split('/') -contains '..') { throw 'Unsafe returned path.' }
      $target = Join-Path $directory "net$major.0/$($item.app)"
      while (++$index -lt $lines.Count -and $lines[$index] -notmatch '^\x60{3}') { }
      $code = [Collections.Generic.List[string]]::new()
      while (++$index -lt $lines.Count -and $lines[$index] -notmatch '^\x60{3}') { $code.Add($lines[$index].TrimEnd("`r")) }
      if ($index -ge $lines.Count -or -not $code.Count) { throw 'Missing returned source block.' }
      $text = ($code -join "`n") + "`n"
      $destination = Join-Path $target $file
      New-Item -ItemType Directory -Path (Split-Path $destination) -Force | Out-Null
      [IO.File]::WriteAllText($destination,$text)
      $counts[$major]++
    }
    foreach ($major in @('10','11')) {
      if ($counts[$major] -ne $item.count) { throw "Incomplete returned $($item.app)/$major source set." }
      Push-Location (Join-Path $directory "net$major.0/$($item.app)")
      try {
        $Dotnet = if ($major -eq '10') { $Net10Dotnet } else { $Net11Dotnet }
        $output = (& $Dotnet build "$($item.app).csproj" -c Release | Out-String)
        $output += (& $Dotnet "./bin/Release/net$major.0/$($item.app).dll" | Out-String)
        if (-not $output.Contains('PASS:')) { throw 'Returned consumer behavior failed.' }
        $sdk = (& $Dotnet --version | Out-String).Trim()
        $records += @{ id=$item.id; target="net$major.0"; sourceCount=$counts[$major]; sdk=$sdk; directory=(Get-Location).Path; output=$output; responseSha256=(Get-FileHash $response).Hash.ToLowerInvariant() }
      } finally { Pop-Location }
    }
  }
  @{ kind='compilation-and-execution-from-installed-Markdown-only'; directory=$directory; records=$records; scope='No canonical checkout files copied. Restores, compiles and executes returned consumers with actual loopback HTTP/hub/component and Identity decisions; no canonical checkout source copied.' } | ConvertTo-Json -Depth 10 | Set-Content (Join-Path $Responses 'consumer.evidence.json')
  Write-Host 'PASS: all four consumers compile and execute from complete installed Markdown alone.'
}
finally {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previous
  foreach ($scope in $paths.Keys) { if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." } }
}
