[CmdletBinding()]
param(
  [string]$Net10Dotnet = 'C:/Program Files/dotnet/dotnet.exe',
  [string]$Net11Dotnet = 'C:/Users/pingu/.dotnet-playbook-11/dotnet.exe',
  [string]$VisualCppRoot = 'C:/Program Files (x86)/Microsoft Visual Studio/18/BuildTools/VC/Tools/MSVC/14.51.36231',
  [string]$WindowsSdkLibRoot = 'C:/Program Files (x86)/Windows Kits/10/Lib/10.0.26100.0'
)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
if (-not $IsWindows) { throw 'This authoring evidence requires the measured Windows x64 native route.' }
. (Join-Path $PSScriptRoot 'Evidence.ps1')
$root = Split-Path $PSScriptRoot -Parent
$paths = @{}
foreach ($scope in @('Process', 'User', 'Machine')) { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH', $scope) }
$previous = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  $linker = (Resolve-Path -LiteralPath (Join-Path $VisualCppRoot 'bin/Hostx64/x64/link.exe')).Path
  $libraries = @((Join-Path $VisualCppRoot 'lib/x64'), (Join-Path $WindowsSdkLibRoot 'um/x64'), (Join-Path $WindowsSdkLibRoot 'ucrt/x64'))
  foreach ($library in $libraries) { $null = Resolve-Path -LiteralPath $library }
  $nativeVersions = @("MSVC/$((Get-Item -LiteralPath $linker).VersionInfo.FileVersion)", "WindowsSDK/$(Split-Path $WindowsSdkLibRoot -Leaf)")
  foreach ($baseline in @(@{ Target = 'net10.0'; Dotnet = $Net10Dotnet }, @{ Target = 'net11.0'; Dotnet = $Net11Dotnet })) {
    $sample = "samples/$($baseline.Target)/DeploymentInspection/DeploymentInspection.csproj"
    $directory = Split-Path (Join-Path $root $sample)
    Push-Location $directory
    try {
      $before = Get-EvidenceInputs $root $sample
      $record = Get-Content -LiteralPath (Join-Path $directory 'verification-evidence.json') -Raw | ConvertFrom-Json -AsHashtable
      $commands = @()
      $outputs = @("Native toolchains: $($nativeVersions -join ', '); native library directories: $($libraries -join '; ')")
      foreach ($mode in @('single-file', 'native-enabled', 'native-disabled', 'core-disabled')) {
        $output = Join-Path $root "artifacts/assembly-deployments/$($baseline.Target)/$mode"
        $arguments = @('publish', 'DeploymentInspection.csproj', '-c', 'Release', '-r', 'win-x64', '--self-contained', 'true', '-o', $output, '--disable-build-servers')
        if ($mode -like 'native-*') {
          # Explicit tools/libraries avoid vcvarsall and its PATH mutation.
          $arguments += @('-p:PublishAot=true', '-p:IlcUseEnvironmentalTools=true', "-p:CppLinker=$linker", "-p:PlaybookCppLibraries=$($libraries[0])", "-p:PlaybookWindowsLibraries=$($libraries[1])", "-p:PlaybookUcrtLibraries=$($libraries[2])")
        }
        if ($mode -eq 'single-file') { $arguments += '-p:PublishSingleFile=true' }
        $arguments += "-p:StackTraceSupport=$($mode -notlike '*disabled')"
        $commands += "$($baseline.Dotnet) $($arguments -join ' ')"
        $PSNativeCommandUseErrorActionPreference = $false
        $build = (& $baseline.Dotnet @arguments 2>&1 | Out-String).Trim()
        $buildExit = $LASTEXITCODE
        $PSNativeCommandUseErrorActionPreference = $true
        if ($buildExit -ne 0) { throw "Publication failed ($mode):`n$build" }
        $executable = Join-Path $output 'DeploymentInspection.exe'
        $locationArgument = if ($mode -eq 'core-disabled') { '--physical-location' } else { '--no-location' }
        $commands += "$executable $locationArgument"
        $actual = (& $executable $locationArgument | Out-String).Trim()
        if ($actual -notmatch '^PASS:') { throw "Missing behavior result: $mode" }
        $dependencies = @((Get-Content -LiteralPath (Join-Path $directory 'obj/project.assets.json') -Raw | ConvertFrom-Json -AsHashtable).libraries.Keys | Sort-Object -CaseSensitive)
        $outputs += "$mode`n$build`nResolved publication dependencies: $($dependencies -join ', ')`n$actual"
        Write-Host $actual
      }
      $after = Get-EvidenceInputs $root $sample
      if ((ConvertTo-Json $before -Compress) -cne (ConvertTo-Json $after -Compress)) { throw 'Deployment evidence inputs changed.' }
      $record.command = $commands -join '; '
      $record.output = $outputs -join "`n"
      $record.recordedAt = [DateTimeOffset]::UtcNow.ToString('O')
      $record.inputs = $before
      $record.sourceCommit = (& 'C:/Program Files/Git/cmd/git.exe' -C $root rev-parse HEAD).Trim()
      $record.sourceDirty = [bool](& 'C:/Program Files/Git/cmd/git.exe' -C $root status --porcelain -- samples eng/Test-AssemblyDeployments.ps1)
      $record.scope = 'Real Windows x64 single-file/CoreCLR and Native AOT publication/execution; enabled/disabled stack-trace caller outcomes and unavailable assembly locations. No cross-compilation or mocks.'
      # Preserve ordinary sample compilation prerequisites. The supplemental native
      # publication tools, dependency closures and commands are labelled in output.
      [IO.File]::WriteAllText((Join-Path $directory 'deployment.evidence.json'), (ConvertTo-Json $record -Depth 30).Replace("`r`n", "`n") + "`n")
    }
    finally { Pop-Location }
  }
}
finally {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previous
  foreach ($scope in $paths.Keys) {
    if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH', $scope)) { throw "Unexpected $scope PATH change; inspect shared settings." }
  }
}
