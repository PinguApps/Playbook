[CmdletBinding()]
param(
  [string]$Net10Dotnet = 'C:/Program Files/dotnet/dotnet.exe',
  [string]$Net11Dotnet = 'C:/Users/pingu/.dotnet-playbook-11/dotnet.exe',
  [switch]$VerifySources
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
if (-not $IsWindows) { throw 'This measured apphost/symlink route requires Windows x64 and symlink permission.' }
$root = Split-Path $PSScriptRoot -Parent
. (Join-Path $PSScriptRoot 'Evidence.ps1')
$git = (Get-Command git -CommandType Application | Select-Object -First 1).Source
$paths = @{}
foreach ($scope in 'Process','User','Machine') { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$persistent = @{}
foreach ($scope in 'User','Machine') { $persistent[$scope] = [Environment]::GetEnvironmentVariables($scope) | ConvertTo-Json -Compress }
$previous = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
function Invoke-Quarry([string]$Executable, [string[]]$Arguments, [string]$Directory, [string]$RuntimeRoot) {
  $start = [Diagnostics.ProcessStartInfo]::new($Executable)
  foreach ($argument in $Arguments) { $start.ArgumentList.Add($argument) }
  $start.WorkingDirectory = $Directory
  $start.UseShellExecute = $false
  $start.CreateNoWindow = $true
  $start.RedirectStandardOutput = $true
  $start.RedirectStandardError = $true
  $start.Environment['DOTNET_ROOT'] = $RuntimeRoot
  $start.Environment['DOTNET_ROOT_X64'] = $RuntimeRoot
  $start.Environment['DOTNET_ADD_GLOBAL_TOOLS_TO_PATH'] = '0'
  $child = [Diagnostics.Process]::Start($start)
  try {
    $stdout = $child.StandardOutput.ReadToEndAsync()
    $stderr = $child.StandardError.ReadToEndAsync()
    if (-not $child.WaitForExit(30000)) { $child.Kill($true); throw 'Launch timed out.' }
    $output = $stdout.GetAwaiter().GetResult().Trim()
    $errors = $stderr.GetAwaiter().GetResult().Trim()
    if ($child.ExitCode -ne 0 -or $errors) { throw "Launch failed: $errors" }
    return $output | ConvertFrom-Json
  }
  finally { $child.Dispose() }
}
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  if ($VerifySources) {
    $manifest = Get-Content -LiteralPath (Join-Path $root 'docs/planning/audits/archive-path-handle-clock.sources.json') -Raw | ConvertFrom-Json
    foreach ($source in ($manifest.sources | Where-Object id -In @('zip-eager','zip-initialization','apphost-name','system-runtime-10','system-runtime-11','system-io-compression-10','system-io-compression-11'))) {
      $bytes = (Invoke-WebRequest -Uri $source.url).RawContentStream.ToArray()
      $hash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant()
      if ($hash -cne $source.sha256) { throw "Source changed: $($source.id)" }
      Write-Host "SOURCE $($source.id) $hash $($source.url)"
    }
  }
  foreach ($baseline in @(
    @{ target='net10.0'; dotnet=$Net10Dotnet; sdk='10.0.402'; runtime='10.0.13' },
    @{ target='net11.0'; dotnet=$Net11Dotnet; sdk='11.0.100-rc.2.26504.105'; runtime='11.0.0-rc.2.26504.105' }
  )) {
    foreach ($name in 'WeatherImport','QuarryMaintenance') {
      $sample = "samples/$($baseline.target)/$name/$name.csproj"
      & (Join-Path $PSScriptRoot 'Record-Evidence.ps1') -Sample $sample -Dotnet $baseline.dotnet
      $directory = Split-Path (Join-Path $root $sample)
      $recordPath = Join-Path $directory 'verification-evidence.json'
      $record = Get-Content -LiteralPath $recordPath -Raw | ConvertFrom-Json -AsHashtable
      if (-not $record.output.Contains(".NET $($baseline.runtime)")) { throw "Wrong runtime: $sample" }
      Push-Location $directory
      try { $resolved = & $baseline.dotnet msbuild "$name.csproj" -t:ResolveFrameworkReferences -p:Configuration=Release -getItem:ResolvedTargetingPack | ConvertFrom-Json }
      finally { Pop-Location }
      $pack = @($resolved.Items.ResolvedTargetingPack | Where-Object Identity -CEQ 'Microsoft.NETCore.App')
      if ($pack.Count -ne 1 -or $pack[0].NuGetPackageVersion -cne $baseline.runtime) { throw "Wrong reference pack: $sample" }
      $record.command += "; $($baseline.dotnet) msbuild $sample -t:ResolveFrameworkReferences -p:Configuration=Release -getItem:ResolvedTargetingPack"
      $artifacts = @(
        (Join-Path (Split-Path $baseline.dotnet) "sdk/$($baseline.sdk)/Roslyn/bincore/Microsoft.CodeAnalysis.CSharp.dll"),
        (Join-Path $pack[0].Path "ref/$($baseline.target)/System.Runtime.dll"),
        (Join-Path $pack[0].Path "ref/$($baseline.target)/System.IO.Compression.dll"),
        (Join-Path (Split-Path $baseline.dotnet) "shared/Microsoft.NETCore.App/$($baseline.runtime)/System.Private.CoreLib.dll")
      )
      foreach ($artifact in $artifacts) {
        $version = [Diagnostics.FileVersionInfo]::GetVersionInfo($artifact).ProductVersion
        $record.output += "`nBUILD ARTIFACT $artifact version=$version sha256=$((Get-FileHash -LiteralPath $artifact).Hash.ToLowerInvariant())"
      }
      [IO.File]::WriteAllText($recordPath, ($record | ConvertTo-Json -Depth 40).Replace("`r`n","`n") + "`n")
      if ($name -ne 'QuarryMaintenance') { continue }
      $before = Get-EvidenceInputs $root $sample
      $commands = @()
      $outputs = @('Real Windows x64 apphosts: absolute launch through ProcessStartInfo, relative/symlink launches through cmd.exe /d /c from the publication directory. Real NTFS symbolic links, no simulated alias.')
      foreach ($mode in 'ordinary','single-file') {
        $outputDirectory = Join-Path $root "artifacts/weather-maintenance/$($baseline.target)/$mode/$([guid]::NewGuid().ToString('N'))"
        $arguments = @('publish', "$name.csproj", '-c','Release','-r','win-x64','--self-contained','false','-o',$outputDirectory, "-p:PublishSingleFile=$($mode -eq 'single-file')", "-p:RuntimeFrameworkVersion=$($baseline.runtime)")
        Push-Location $directory
        try { $published = (& $baseline.dotnet @arguments | Out-String).Trim() }
        finally { Pop-Location }
        $commands += "$($baseline.dotnet) $($arguments -join ' ') [cwd=$directory]"
        $outputs += $published
        Push-Location $directory
        try {
          $hostPack = & $baseline.dotnet msbuild "$name.csproj" -t:ResolveFrameworkReferences -p:RuntimeIdentifier=win-x64 "-p:RuntimeFrameworkVersion=$($baseline.runtime)" -getItem:ResolvedAppHostPack | ConvertFrom-Json
        }
        finally { Pop-Location }
        $apphost = @($hostPack.Items.ResolvedAppHostPack)
        if ($apphost.Count -ne 1 -or (Split-Path $apphost[0].PackageDirectory -Leaf) -cne $baseline.runtime) { throw 'Unexpected selected native apphost pack.' }
        $commands += "$($baseline.dotnet) msbuild $sample -t:ResolveFrameworkReferences -p:RuntimeIdentifier=win-x64 -p:RuntimeFrameworkVersion=$($baseline.runtime) -getItem:ResolvedAppHostPack"
        $outputs += "NATIVE APPHOST $($apphost[0].Path); version=$([Diagnostics.FileVersionInfo]::GetVersionInfo($apphost[0].Path).ProductVersion); sha256=$((Get-FileHash -LiteralPath $apphost[0].Path).Hash.ToLowerInvariant())"
        $exe = Join-Path $outputDirectory 'QuarryMaintenance.exe'
        $alias = Join-Path $outputDirectory 'quarry-audit.exe'
        $null = [IO.File]::CreateSymbolicLink($alias, $exe)
        $outputs += "SYMLINK $alias -> $((Get-Item -LiteralPath $alias).LinkTarget); exe-sha256=$((Get-FileHash -LiteralPath $exe).Hash.ToLowerInvariant())"
        $launches = @(
          @{ kind='absolute-apphost'; executable=$exe; arguments=@(); invocation=$exe },
          @{ kind='relative-apphost'; executable=(Join-Path $env:SystemRoot 'System32/cmd.exe'); arguments=@('/d','/c','.\QuarryMaintenance.exe'); invocation='.\QuarryMaintenance.exe' },
          @{ kind='symlink-apphost'; executable=(Join-Path $env:SystemRoot 'System32/cmd.exe'); arguments=@('/d','/c','.\quarry-audit.exe'); invocation='.\quarry-audit.exe' }
        )
        if ($mode -eq 'ordinary') { $launches += @{ kind='dotnet-dll'; executable=$baseline.dotnet; arguments=@((Join-Path $outputDirectory 'QuarryMaintenance.dll')) } }
        foreach ($launch in $launches) {
          $actual = Invoke-Quarry $launch.executable $launch.arguments $outputDirectory (Split-Path $baseline.dotnet)
          $commands += "ProcessStartInfo FileName=$($launch.executable) ArgumentList=$($launch.arguments -join ',') WorkingDirectory=$outputDirectory DOTNET_ROOT_X64=$(Split-Path $baseline.dotnet)"
          if ($actual.Runtime -cne ".NET $($baseline.runtime)" -or -not [IO.File]::Exists($actual.Executable)) { throw 'Wrong runtime/executable location.' }
          if ($mode -eq 'single-file') {
            if ($actual.RawLocation -cne '' -or $null -ne $actual.ManagedFile) { throw 'Bundled Location must be empty and normalized to unavailable.' }
          } elseif (-not [IO.File]::Exists($actual.ManagedFile)) { throw 'Ordinary assembly location must exist.' }
          if ($launch.kind -eq 'dotnet-dll') {
            if ($actual.Invocation -cne $actual.ManagedFile -or $actual.Executable -cne [IO.Path]::GetFullPath($baseline.dotnet)) { throw 'dotnet DLL launch contract changed.' }
          } elseif ($baseline.target -eq 'net11.0') {
            if ($actual.Invocation -cne $launch.invocation) { throw "RC2 invocation spelling mismatch: $($actual.Invocation)" }
            if ($launch.kind -eq 'symlink-apphost' -and $actual.HelpName -cne 'quarry-audit') { throw 'Real alias dispatch failed.' }
          } else {
            $expected = if ($mode -eq 'ordinary') { $actual.ManagedFile } else { $actual.Executable }
            if ($actual.Invocation -cne $expected) { throw 'Expected older managed/resolved-executable invocation identity.' }
          }
          $outputs += "$mode/$($launch.kind): $($actual | ConvertTo-Json -Compress)"
          Write-Host $outputs[-1]
        }
        $dependencies = (Get-Content -LiteralPath (Join-Path $directory 'obj/project.assets.json') -Raw | ConvertFrom-Json -AsHashtable).libraries.Keys | Sort-Object -CaseSensitive
        $outputs += "Resolved publication dependencies: $($dependencies -join ', ')"
      }
      $after = Get-EvidenceInputs $root $sample
      if ((ConvertTo-Json $before -Compress) -cne (ConvertTo-Json $after -Compress)) { throw 'Deployment inputs changed.' }
      $record.kind = 'execution'
      $record.command = $commands -join '; '
      $record.output = $outputs -join "`n"
      $record.recordedAt = [DateTimeOffset]::UtcNow.ToString('O')
      $record.inputs = $after
      $record.sourceCommit = (& $git -C $root rev-parse HEAD).Trim()
      $record.scope = 'Real Windows x64 ordinary absolute/relative/symlink apphosts and dotnet DLL; framework-dependent single-file absolute/relative/symlink apphosts; exact launch spelling, process/assembly locations, bounded alias choice. No Unix, other launcher, AOT or GA proof.'
      [IO.File]::WriteAllText((Join-Path $directory 'deployment.evidence.json'), ($record | ConvertTo-Json -Depth 40).Replace("`r`n","`n") + "`n")
    }
  }
  Write-Host 'PASS: paired real ZIP execution and fourteen ordinary/single-file launch cases; exact SDK/compiler/ref/runtime artifacts.'
}
finally {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previous
  foreach ($scope in $paths.Keys) {
    if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." }
  }
  foreach ($scope in $persistent.Keys) {
    if ($persistent[$scope] -cne ([Environment]::GetEnvironmentVariables($scope) | ConvertTo-Json -Compress)) { throw "Unexpected persistent $scope environment change; shared settings preserved." }
  }
}
