[CmdletBinding()]
param(
  [Parameter(Mandatory)][string]$ResponsesDirectory,
  [Parameter(Mandatory)][string]$OutputPath,
  [ValidateSet('both','redirect','session')][string]$Workflow = 'both',
  [string]$Net10Dotnet = 'C:\Program Files\dotnet\dotnet.exe',
  [string]$Net11Dotnet = 'C:\Users\pingu\.dotnet-playbook-11\dotnet.exe'
)

$ErrorActionPreference = 'Stop'
$work = Join-Path ([IO.Path]::GetTempPath()) "harbour-guidance-$([guid]::NewGuid().ToString('N'))"
New-Item -ItemType Directory -Path $work | Out-Null
# Independent modes never read the other recipe, or checkout samples/configuration.
$hasRedirect = $Workflow -ne 'session'
$hasSession = $Workflow -ne 'redirect'
$ids = @()
if ($hasRedirect) {
  $ids += 'blazor.show-once-after-redirect'
  $outcome = Get-Content (Join-Path $ResponsesDirectory 'blazor.show-once-after-redirect.md') -Raw
}
if ($hasSession) {
  $ids += 'blazor.retain-session-progress'
  $session = Get-Content (Join-Path $ResponsesDirectory 'blazor.retain-session-progress.md') -Raw
}
$setup = if ($hasRedirect) { $outcome } else { $session }
function Assert-True([bool]$Value, [string]$Message) { if (-not $Value) { throw $Message } }
function Get-Block([string]$Body, [string]$Label) {
  $match = [regex]::Match($Body, '(?ms)^' + [regex]::Escape($Label) + ':\n\n```[^\n]+\n(.*?)\n```')
  Assert-True $match.Success "Missing complete served block: $Label"
  return $match.Groups[1].Value + "`n"
}
function Save-Source([string]$Directory, [string]$Name, [string]$Text) {
  [IO.File]::WriteAllText((Join-Path $Directory $Name), $Text.Replace("`r`n", "`n"))
}
function New-Start([string]$Dotnet, [string]$Directory, [string[]]$Arguments) {
  $start = [Diagnostics.ProcessStartInfo]::new($Dotnet)
  $start.WorkingDirectory = $Directory
  $start.UseShellExecute = $false
  $start.CreateNoWindow = $true
  $start.RedirectStandardOutput = $true
  $start.RedirectStandardError = $true
  foreach ($argument in $Arguments) { $start.ArgumentList.Add($argument) }
  $start.Environment['DOTNET_ADD_GLOBAL_TOOLS_TO_PATH'] = '0'
  $start.Environment['DOTNET_CLI_HOME'] = Join-Path $work 'cli'
  $start.Environment['NUGET_PACKAGES'] = Join-Path $work 'packages'
  return $start
}
function Invoke-Dotnet([string]$Dotnet, [string]$Directory, [string[]]$Arguments) {
  $process = [Diagnostics.Process]::Start((New-Start $Dotnet $Directory $Arguments))
  try {
    $stdout = $process.StandardOutput.ReadToEndAsync()
    $stderr = $process.StandardError.ReadToEndAsync()
    $process.WaitForExit()
    $text = $stdout.GetAwaiter().GetResult() + $stderr.GetAwaiter().GetResult()
    Assert-True ($process.ExitCode -eq 0) $text
    return $text
  } finally { $process.Dispose() }
}
function New-Client([uri]$Uri) {
  $handler = [Net.Http.HttpClientHandler]::new()
  $handler.AllowAutoRedirect = $false
  $client = [Net.Http.HttpClient]::new($handler)
  $client.BaseAddress = $Uri
  $client.Timeout = [TimeSpan]::FromSeconds(15)
  return $client
}
function Get-Page($Client, [string]$Path) {
  $response = $Client.GetAsync($Path).GetAwaiter().GetResult()
  try {
    $null = $response.EnsureSuccessStatusCode()
    return [Net.WebUtility]::HtmlDecode($response.Content.ReadAsStringAsync().GetAwaiter().GetResult())
  } finally { $response.Dispose() }
}
function Submit($Client, [string]$Path, [string]$Handler, [string]$Berth) {
  $token = [regex]::Match((Get-Page $Client $Path), 'name="__RequestVerificationToken" value="([^"]+)"').Groups[1].Value
  Assert-True ($token.Length -gt 0) 'Missing served form antiforgery token'
  $fields = [Collections.Generic.Dictionary[string,string]]::new()
  $fields.Add('_handler', $Handler)
  $fields.Add('__RequestVerificationToken', $token)
  if ($Berth) { $fields.Add('Input.Berth', $Berth) }
  $content = [Net.Http.FormUrlEncodedContent]::new($fields)
  try {
    $response = $Client.PostAsync($Path, $content).GetAwaiter().GetResult()
    try {
      Assert-True ([int]$response.StatusCode -eq 302) "Expected actual redirect from $Path"
      return $response.Headers.Location.ToString()
    } finally { $response.Dispose() }
  } finally { $content.Dispose() }
}

$builds = @()
foreach ($target in 'net10.0','net11.0') {
  $directory = Join-Path $work $target
  New-Item -ItemType Directory -Path $directory | Out-Null
  $dotnet = if ($target -eq 'net10.0') { $Net10Dotnet } else { $Net11Dotnet }
  Save-Source $directory 'HarbourPermit.csproj' (Get-Block $setup "$target/HarbourPermit.csproj")
  $pin = [regex]::Match($setup, '(?m)^' + [regex]::Escape("$target/global.json:") + '\n\n(\{[^\n]+\})$')
  Assert-True $pin.Success "Missing served SDK pin for $target"
  Save-Source $directory 'global.json' $pin.Groups[1].Value
  $imports = [regex]::Match($setup, '(?m)^One directive per line: (.+)$')
  Assert-True $imports.Success 'Missing complete served imports'
  $directives = @([regex]::Matches($imports.Groups[1].Value, '`([^`]+)`') | ForEach-Object { $_.Groups[1].Value })
  Assert-True ($directives.Count -eq 6) 'Expected all six served import/namespace directives'
  Save-Source $directory '_Imports.razor' (($directives -join "`n") + "`n")
  Save-Source $directory 'App.razor' (Get-Block $setup 'shared/App.razor')
  if ($hasSession) { Save-Source $directory 'PermitInput.cs' (Get-Block $session 'shared/PermitInput.cs') }
  $pages = @()
  if ($hasRedirect) { $pages += 'Apply','Outcome' }
  if ($hasSession) { $pages += 'Progress','Review' }
  foreach ($page in $pages) {
    $body = if ($page -in 'Apply','Outcome') { $outcome } else { $session }
    $route = $page.ToLowerInvariant()
    $header = '@page "/' + $route + '"'
    Assert-True ($body.Contains($header)) "Page directive must be present in served guidance: $header"
    if ($page -in 'Apply','Progress') { $header += "`n@inject NavigationManager Navigation" }
    if ($target -eq 'net10.0' -and $page -in 'Apply','Outcome') { $header += "`n@inject Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataDictionaryFactory Factory" }
    Save-Source $directory "$page.razor" ($header + "`n`n" + (Get-Block $body "$target/$page.razor body"))
  }
  # Assemble the Program.cs imports/Build/Run explicitly specified in the prose.
  $program = "using Playbook.Samples;`n"
  if ($target -eq 'net11.0') { $program += "using Microsoft.AspNetCore.Components.Endpoints;`n" }
  $program += (Get-Block $setup "$target/Program.cs registrations") + "var app = builder.Build();`n" + (Get-Block $setup "$target/Program.cs pipeline") + "app.Run();`n"
  Save-Source $directory 'Program.cs' $program
  $pinVersion = ($pin.Groups[1].Value | ConvertFrom-Json).sdk.version
  Assert-True ((Invoke-Dotnet $dotnet $directory @('--version')).Trim() -eq $pinVersion) 'Actual SDK must match returned pin'
  $build = Invoke-Dotnet $dotnet $directory @('build','HarbourPermit.csproj','-c','Release','-p:UseSharedCompilation=false','-nodeReuse:false')
  Save-Source $directory 'build.log' $build
  $builds += @{ target=$target; sdk=$pinVersion; output=$build }
}

$checks = @()
foreach ($case in @(
  @{ Name='previous'; Target='net10.0'; Dotnet=$Net10Dotnet; Port=5394; Session=$false },
  @{ Name='cookie'; Target='net11.0'; Dotnet=$Net11Dotnet; Port=5395; Session=$false },
  @{ Name='session'; Target='net11.0'; Dotnet=$Net11Dotnet; Port=5396; Session=$true }
)) {
  if (-not $hasRedirect -and $case.Name -eq 'session') { continue }
  $directory = Join-Path $work $case.Target
  $dll = Join-Path $directory "bin/Release/$($case.Target)/HarbourPermit.dll"
  $process = [Diagnostics.Process]::Start((New-Start $case.Dotnet $directory @($dll,'--urls',"http://127.0.0.1:$($case.Port)",'--environment','Development','--SessionTempData',$case.Session.ToString())))
  $stdout = $process.StandardOutput.ReadToEndAsync()
  $stderr = $process.StandardError.ReadToEndAsync()
  $client = New-Client "http://127.0.0.1:$($case.Port)/"
  $other = New-Client $client.BaseAddress
  try {
    $deadline = [DateTime]::UtcNow.AddSeconds(20)
    do {
      try { $null = Get-Page $client $(if ($hasRedirect) { '/apply' } else { '/progress' }); break }
      catch { if ($process.HasExited -or [DateTime]::UtcNow -gt $deadline) { throw }; Start-Sleep -Milliseconds 100 }
    } while ($true)
    $check = @{ case=$case.Name; workflow=$Workflow; coreclr=@($process.Modules | Where-Object ModuleName -eq 'coreclr.dll' | ForEach-Object FileName) }
    if ($hasRedirect) {
      $location = Submit $client '/apply' 'permit' ''
      Assert-True ((Get-Page $client $location).Contains('Harbour permit request recorded')) 'Redirect must deliver confirmation'
      Assert-True ((Get-Page $client '/outcome').Contains('No new outcome')) 'Second read must consume confirmation'
      $check.redirect302=$true; $check.secondReadConsumed=$true
    }
    if ($hasSession) {
      $location = Submit $client '/progress' 'progress' 'North-12'
      foreach ($path in @($location,'/review')) { Assert-True ((Get-Page $client $path).Contains('Step 1; berth North-12')) 'Session must survive reads' }
      $location = Submit $client '/progress' 'progress' 'East-4'
      Assert-True ((Get-Page $client $location).Contains('Step 2; berth East-4')) 'Second edit must retain previous step'
      Assert-True ((Get-Page $other '/review').Contains('Step 0; berth unselected')) 'Separate cookie jar must be empty'
      $check.repeatedSessionReads=$true; $check.secondEdit=$true; $check.isolatedCookieJar=$true
    }
    $checks += $check
  } finally {
    $client.Dispose(); $other.Dispose()
    if (-not $process.HasExited) { $process.Kill($true) }
    $process.WaitForExit()
    Save-Source $directory "$($case.Name).log" ($stdout.GetAwaiter().GetResult() + $stderr.GetAwaiter().GetResult())
    $process.Dispose()
  }
}
$files = @(Get-ChildItem -LiteralPath $work -File -Recurse | Where-Object { $_.FullName -notmatch '[\\/](obj|packages|cli)[\\/]' } | ForEach-Object { @{ path=[IO.Path]::GetRelativePath($work,$_.FullName).Replace('\','/'); sha256=(Get-FileHash $_.FullName).Hash.ToLowerInvariant() } })
@{ kind='served-markdown-reconstruction-and-real-http'; recordedAt=[DateTimeOffset]::UtcNow.ToString('o'); workDirectory=$work; workflow=$Workflow; source='Only the selected returned Markdown; no other recipe or checkout sample/configuration inputs'; bodies=@($ids | ForEach-Object { @{ name="$_.md"; sha256=(Get-FileHash (Join-Path $ResponsesDirectory "$_.md")).Hash.ToLowerInvariant() } }); builds=$builds; checks=$checks; files=$files; verifierSha256=(Get-FileHash $PSCommandPath).Hash.ToLowerInvariant(); limits='Selected redirect/session reconstruction checks only; full streaming and absent-middleware proof is separate.' } | ConvertTo-Json -Depth 15 | Set-Content -LiteralPath $OutputPath -Encoding utf8
Write-Host "PASS: independent returned-body-only $Workflow .NET 10/11 reconstruction and real HTTP checks. $work"
