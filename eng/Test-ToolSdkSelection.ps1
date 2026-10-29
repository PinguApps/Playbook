[CmdletBinding()]
param(
  [string]$Net9Dotnet = 'C:\Users\pingu\.dotnet-playbook-9\dotnet.exe',
  [string]$Net10Dotnet = 'C:\Program Files\dotnet\dotnet.exe',
  [string]$Net11Dotnet = 'C:\Users\pingu\.dotnet-playbook-11\dotnet.exe',
  [switch]$Record
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
if (-not $IsWindows) { throw 'This check executes actual Windows SDK wrappers; Unix wrapper execution is not claimed.' }
. (Join-Path $PSScriptRoot 'Evidence.ps1')
$root = Split-Path $PSScriptRoot -Parent
$scratch = Join-Path ([IO.Path]::GetTempPath()) ('playbook-tool-sdk-' + [guid]::NewGuid().ToString('N'))
$null = New-Item -ItemType Directory -Path $scratch
$paths = @{}
foreach ($scope in 'Process','User','Machine') { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$log = [Collections.Generic.List[string]]::new()

function Invoke-Sdk([string]$Exe,[string]$Directory,[string[]]$Arguments,[string]$Trace = '',[int]$ExpectedExit = 0) {
  $start = [Diagnostics.ProcessStartInfo]::new($Exe)
  if ($Exe.EndsWith('.cmd',[StringComparison]::OrdinalIgnoreCase)) {
    $start.FileName = [Environment]::GetEnvironmentVariable('ComSpec')
    # Fixed local paths/arguments only. Reject cmd metacharacters before constructing its command line.
    foreach ($value in @($Exe) + $Arguments) { if ($value -match '["%&|<>^\r\n]') { throw 'Unsupported cmd argument.' } }
    $start.Arguments = '/d /s /c "' + ((@($Exe) + $Arguments | ForEach-Object { '"' + $_ + '"' }) -join ' ') + '"'
  }
  else { foreach ($argument in $Arguments) { $start.ArgumentList.Add($argument) } }
  $start.WorkingDirectory = $Directory
  $start.UseShellExecute = $false
  $start.CreateNoWindow = $true
  $start.RedirectStandardOutput = $true
  $start.RedirectStandardError = $true
  $start.RedirectStandardInput = $true
  $start.Environment['DOTNET_ADD_GLOBAL_TOOLS_TO_PATH'] = '0'
  $start.Environment['DOTNET_CLI_HOME'] = Join-Path $scratch 'home'
  $start.Environment['NUGET_PACKAGES'] = Join-Path $scratch 'packages'
  $start.Environment['DOTNET_NOLOGO'] = '1'
  $start.Environment['DOTNET_CLI_USE_MSBUILD_SERVER'] = 'false'
  $start.Environment['MSBUILDUSESERVER'] = '0'
  $start.Environment['DOTNET_ROOT'] = $layout
  $start.Environment['DOTNET_ROOT_X64'] = $layout
  if ($Trace) {
    $start.Environment['DOTNET_HOST_TRACE'] = '1'
    $start.Environment['DOTNET_HOST_TRACEFILE'] = $Trace
    $start.Environment['COREHOST_TRACE'] = '1'
    $start.Environment['COREHOST_TRACEFILE'] = $Trace
  }
  $process = [Diagnostics.Process]::Start($start)
  $process.StandardInput.Close()
  $stdout = $process.StandardOutput.ReadToEndAsync()
  $stderr = $process.StandardError.ReadToEndAsync()
  if (-not $process.WaitForExit(120000)) { $process.Kill($true); throw 'SDK selection check timed out.' }
  $output = $stdout.GetAwaiter().GetResult() + $stderr.GetAwaiter().GetResult()
  $log.Add("cwd=$Directory`n$Exe $($Arguments -join ' ')`nexit=$($process.ExitCode)`n$output")
  [IO.File]::WriteAllText((Join-Path $scratch 'commands.log'),($log -join "`n"))
  if ($process.ExitCode -ne $ExpectedExit) { throw "Expected exit $ExpectedExit, received $($process.ExitCode): $output" }
  $output
}

function Assert-Text([string]$Actual,[string]$Expected) {
  if (-not $Actual.Contains($Expected,[StringComparison]::OrdinalIgnoreCase)) { throw "Missing '$Expected': $Actual" }
}

function Assert-SdkTrace([string]$Trace,[string]$Sdk) {
  $assembly = Join-Path $layout "sdk/$Sdk/dotnet.dll"
  $selection = @(Get-Content -LiteralPath $Trace | Where-Object { $_.Contains("mgd_app='$assembly'",[StringComparison]::OrdinalIgnoreCase) })
  if ($selection.Count -eq 0) { throw "No actual managed SDK entry point for $Sdk in $Trace" }
  $log.Add($selection[0])
}

try {
  $layout = Join-Path $scratch 'dotnet with spaces'
  $null = New-Item -ItemType Directory -Path "$layout/sdk","$layout/host/fxr","$layout/shared/Microsoft.NETCore.App","$layout/shared/Microsoft.AspNetCore.App","$layout/packs"
  Copy-Item -LiteralPath $Net11Dotnet -Destination "$layout/dotnet.exe"
  $pins = @(
    @{ exe=$Net9Dotnet; sdk='9.0.319'; runtime='9.0.21' },
    @{ exe=$Net10Dotnet; sdk='10.0.402'; runtime='10.0.13' },
    @{ exe=$Net11Dotnet; sdk='11.0.100-rc.2.26504.105'; runtime='11.0.0-rc.2.26504.105' }
  )
  foreach ($pin in $pins) {
    $installation = Split-Path $pin.exe
    $null = New-Item -ItemType Junction -Path "$layout/sdk/$($pin.sdk)" -Target "$installation/sdk/$($pin.sdk)"
    foreach ($framework in 'Microsoft.NETCore.App','Microsoft.AspNetCore.App') {
      $null = New-Item -ItemType Junction -Path "$layout/shared/$framework/$($pin.runtime)" -Target "$installation/shared/$framework/$($pin.runtime)"
    }
    Copy-Item -LiteralPath "$installation/host/fxr/$($pin.runtime)" -Destination "$layout/host/fxr" -Recurse
  }
  $dotnet = Join-Path $layout 'dotnet.exe'
  $null = Invoke-Sdk $dotnet $scratch @('--list-sdks')
  $museum = Join-Path $scratch 'museum repository'
  $null = New-Item -ItemType Directory -Path $museum
  $cards = Join-Path $museum 'east wing cards.txt'
  "SHELL-004`nCOMPASS-019" | Set-Content -LiteralPath $cards
  $badCards = Join-Path $museum 'duplicate cards.txt'
  "SHELL-004`nSHELL-004" | Set-Content -LiteralPath $badCards

  foreach ($release in '10','11') {
    $sample = "samples/net$release.0/MuseumCatalogue/MuseumCatalogue.csproj"
    $source = Split-Path (Join-Path $root $sample)
    $inputs = Get-EvidenceInputs $root $sample
    $firstLog = $log.Count
    $sdk = if ($release -eq '10') { '10.0.402' } else { '11.0.100-rc.2.26504.105' }
    $runtime = if ($release -eq '10') { '10.0.13' } else { '11.0.0-rc.2.26504.105' }
    $buildExe = if ($release -eq '10') { $Net10Dotnet } else { $Net11Dotnet }
    $feed = Join-Path $scratch "local feed $release"
    $null = Invoke-Sdk $buildExe $source @('pack','MuseumCatalogue.csproj','-c','Release','-o',$feed)
    $package = Join-Path $feed 'Gullhaven.Museum.Catalogue.1.0.0.nupkg'
    $log.Add("local package=$package;sha256=$((Get-FileHash -LiteralPath $package).Hash.ToLowerInvariant())")
    $config = Join-Path $museum "NuGet-$release.Config"
    '<configuration><packageSources><clear/><add key="local" value="{0}"/></packageSources></configuration>' -f [Security.SecurityElement]::Escape($feed) | Set-Content -LiteralPath $config
    # Separate homes/caches per package asset: both original packages have the same
    # identity but intentionally different TFMs and never enter a shared cache.
    $scratchBefore = $scratch
    $scratch = Join-Path $scratchBefore "execution-$release"
    $null = New-Item -ItemType Directory -Path $scratch
    try {
      foreach ($sdkPin in '9.0.319','10.0.402','11.0.100-rc.2.26504.105') {
        @{sdk=@{version=$sdkPin;rollForward='disable';allowPrerelease=$true}} | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $museum 'global.json')
        Assert-Text (Invoke-Sdk $dotnet $museum @('--version')) $sdkPin
        $toolArgs = @('Gullhaven.Museum.Catalogue@1.0.0','--configfile',$config,'--no-http-cache','--',$cards,'--gallery','east wing')
        if ($sdkPin -eq '9.0.319') {
          $failure = Invoke-Sdk $dotnet $museum (@('dnx') + $toolArgs) '' 1
          Assert-Text $failure 'does not exist'
          $log.Add('PASS: explicit pre-dnx SDK pin failed before executing a package.')
        }
        elseif ($release -eq '11' -and $sdkPin -eq '10.0.402') {
          $failure = Invoke-Sdk $dotnet $museum (@('dnx') + $toolArgs) '' 1
          Assert-Text $failure "Settings file 'DotnetToolSettings.xml' was not found"
          $log.Add('PASS: SDK 10.0.402 cannot consume this net11-only tool package; installed runtime 11 alone does not establish tool asset compatibility.')
        }
        else {
          $trace = Join-Path $scratch "explicit-$sdkPin.txt"
          $output = Invoke-Sdk $dotnet $museum (@('dnx') + $toolArgs) $trace
          Assert-Text $output 'cards=2;unique=2'
          Assert-Text $output "runtime=.NET $runtime"
          $forwarded = [regex]::Match($output,'(?m)^arguments=(.+)$').Groups[1].Value | ConvertFrom-Json
          if ($forwarded.Count -ne 3 -or $forwarded[0] -cne $cards -or $forwarded[1] -cne '--gallery' -or $forwarded[2] -cne 'east wing') { throw 'Argument forwarding changed.' }
          Assert-SdkTrace $trace $sdkPin
          $log.Add("observed explicit SDK=$sdkPin; tool runtime=$runtime; forwarded quoted path and arguments exactly")
        }
        # Execute each actual shipped wrapper, unchanged, against the same layout.
        foreach ($wrapperSource in @((Join-Path (Split-Path $Net10Dotnet) 'dnx.cmd'),(Join-Path (Split-Path $Net11Dotnet) 'dnx.cmd'))) {
          Copy-Item -LiteralPath $wrapperSource -Destination "$layout/dnx.cmd" -Force
          $trace = Join-Path $scratch ([guid]::NewGuid().ToString('N') + '.txt')
          $output = Invoke-Sdk "$layout/dnx.cmd" $museum $toolArgs $trace
          Assert-Text $output 'cards=2;unique=2'
          Assert-Text $output "runtime=.NET $runtime"
          Assert-SdkTrace $trace '11.0.100-rc.2.26504.105'
          $forwarded = [regex]::Match($output,'(?m)^arguments=(.+)$').Groups[1].Value | ConvertFrom-Json
          if ($forwarded.Count -ne 3 -or $forwarded[0] -cne $cards -or $forwarded[2] -cne 'east wing') { throw 'Wrapper argument forwarding changed.' }
          $log.Add("wrapper=$wrapperSource;sha256=$((Get-FileHash -LiteralPath $wrapperSource).Hash.ToLowerInvariant());pin=$sdkPin;observed SDK=11.0.100-rc.2.26504.105;tool runtime=$runtime")
        }
      }
      $failure = Invoke-Sdk $dotnet $museum @('dnx','Gullhaven.Museum.Catalogue@1.0.0','--configfile',$config,'--',$badCards) '' 7
      Assert-Text $failure 'cards=2;unique=1'
      $log.Add("PASS: $sdk compiled local net$release.0 tool, 3 exact SDK pins, both shipped serviced wrappers, SDK/tool-runtime independence, quoted forwarding and tool failure exit 7.")
      Write-Host $log[$log.Count-1]
      if ($Record) {
        $evidence = Get-Content -LiteralPath (Join-Path $source 'verification-evidence.json') -Raw | ConvertFrom-Json -AsHashtable
        $evidence.kind = 'execution'
        $evidence.command = "eng/Test-ToolSdkSelection.ps1 -Net9Dotnet '$Net9Dotnet' -Net10Dotnet '$Net10Dotnet' -Net11Dotnet '$Net11Dotnet' -Record"
        $evidence.recordedAt = [DateTimeOffset]::UtcNow.ToString('O')
        $evidence.inputs = Get-EvidenceInputs $root $sample
        if (($inputs | ConvertTo-Json -Compress) -cne ($evidence.inputs | ConvertTo-Json -Compress)) { throw 'Inputs changed during execution.' }
        $evidence.output = ($log | Select-Object -Skip $firstLog) -join "`n"
        [IO.File]::WriteAllText((Join-Path $source 'commands.evidence.json'),($evidence | ConvertTo-Json -Depth 30).Replace("`r`n","`n") + "`n")
      }
    }
    finally { $scratch = $scratchBefore }
  }
}
finally {
  foreach ($scope in $paths.Keys) {
    if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." }
  }
  Write-Host "SDK selection scratch: $scratch"
}
