[CmdletBinding()]
param(
  [string]$Net10Dotnet = 'C:/Program Files/dotnet/dotnet.exe',
  [string]$Net11Dotnet = 'C:/Users/pingu/.dotnet-playbook-11/dotnet.exe',
  [string]$VcRoot = 'C:/Program Files (x86)/Microsoft Visual Studio/18/BuildTools/VC/Tools/MSVC/14.51.36231',
  [string]$WindowsKit = 'C:/Program Files (x86)/Windows Kits/10',
  [string]$Packages = (Join-Path $HOME '.nuget/packages')
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
. (Join-Path $PSScriptRoot 'Evidence.ps1')
$root = Split-Path $PSScriptRoot -Parent
$output = Join-Path $root 'artifacts/quay-permits'
$Packages = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($Packages)
New-Item -ItemType Directory -Force -Path $output | Out-Null
$paths = @{}
foreach ($scope in 'Process','User','Machine') { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH', $scope) }
$persistent = @{}
foreach ($scope in 'User','Machine') { $persistent[$scope] = [Environment]::GetEnvironmentVariables($scope) | ConvertTo-Json -Compress }
$previous = @{}
foreach ($name in 'QUAY_MSBUILD_SDK','DOTNET_ADD_GLOBAL_TOOLS_TO_PATH','NUGET_PACKAGES') { $previous[$name] = [Environment]::GetEnvironmentVariable($name) }
$nativeTools = @('MSVC/14.51.36231','WindowsSDK/10.0.26100.0','ILCompiler/11.0.0-rc.2.26504.105')
$sdk10 = Join-Path (Split-Path $Net10Dotnet) 'sdk/10.0.402'
$sdk11 = Join-Path (Split-Path $Net11Dotnet) 'sdk/11.0.100-rc.2.26504.105'

function Invoke-Child([string]$Executable, [string[]]$Arguments, [string]$Directory, [hashtable]$Environment = @{}) {
  $start = [Diagnostics.ProcessStartInfo]::new($Executable)
  $start.WorkingDirectory = $Directory
  $start.UseShellExecute = $false
  $start.RedirectStandardOutput = $true
  $start.RedirectStandardError = $true
  $start.Environment['DOTNET_ADD_GLOBAL_TOOLS_TO_PATH'] = '0'
  $start.Environment['MSBUILDUSESERVER'] = '0'
  foreach ($key in $Environment.Keys) { $start.Environment[$key] = $Environment[$key] }
  foreach ($argument in $Arguments) { $start.ArgumentList.Add($argument) }
  Write-Host ('COMMAND ' + $Executable + ' ' + ($Arguments -join ' '))
  $child = [Diagnostics.Process]::Start($start)
  $stdout = $child.StandardOutput.ReadToEndAsync()
  $stderr = $child.StandardError.ReadToEndAsync()
  if (-not $child.WaitForExit(180000)) { $child.Kill($true); throw 'Own permit child timed out and was terminated.' }
  $text = $stdout.Result + $stderr.Result
  Write-Host $text.Trim()
  if ($child.ExitCode -ne 0) { throw "Permit child failed ($($child.ExitCode)): $text" }
  return "COMMAND $Executable $($Arguments -join ' ')`n$text"
}

try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  $env:NUGET_PACKAGES = $Packages
  # Populate precisely the locked closures before qualifying their files. This
  # also downloads the SDK-selected native runtime/compiler into a cold cache.
  $restores = @{}
  foreach ($major in 10,11) {
    $dotnet = if ($major -eq 10) { $Net10Dotnet } else { $Net11Dotnet }
    $directory = Join-Path $root "samples/net$major.0/NorthQuayBuildHost"
    $restores[$major] = Invoke-Child $dotnet @('restore','NorthQuayBuildHost.csproj','--locked-mode') $directory
  }
  $bases = @{ sdk10=$sdk10; sdk11=$sdk11; packages=$Packages; vc=$VcRoot; kit=$WindowsKit }
  $pins = Get-Content -LiteralPath (Join-Path $root 'samples/PermitHost/artifact-pins.json') -Raw | ConvertFrom-Json -AsHashtable
  $observations = foreach ($pin in $pins.artifacts) {
    $path = Join-Path $bases[$pin.root] $pin.path
    $hash = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant()
    if ($hash -cne $pin.sha256) { throw "Changed qualification artifact: $path" }
    "ARTIFACT $path sha256=$hash product=$([Diagnostics.FileVersionInfo]::GetVersionInfo($path).ProductVersion)"
  }
  foreach ($major in 10,11) {
    $dotnet = if ($major -eq 10) { $Net10Dotnet } else { $Net11Dotnet }
    $sdk = if ($major -eq 10) { $sdk10 } else { $sdk11 }
    $env:QUAY_MSBUILD_SDK = $sdk
    $sample = "samples/net$major.0/NorthQuayBuildHost/NorthQuayBuildHost.csproj"
    $tools = @()
    if ($major -eq 11) { $tools = $nativeTools }
    & (Join-Path $PSScriptRoot 'Record-Evidence.ps1') -Sample $sample -Dotnet $dotnet -NativeToolchains $tools
    $directory = Join-Path $root "samples/net$major.0/NorthQuayBuildHost"
    $recordPath = Join-Path $directory 'verification-evidence.json'
    $record = Get-Content -LiteralPath $recordPath -Raw | ConvertFrom-Json -AsHashtable
    $binary = Join-Path $directory "bin/Release/net$major.0/$(if ($major -eq 11) {'win-x64/'})NorthQuayBuildHost.dll"
    $extra = Invoke-Child $dotnet @($binary,'--missing-task') $output @{ QUAY_MSBUILD_SDK=$sdk }
    $record.output += "`n" + $restores[$major] + "`n" + ($observations -join "`n") + "`n" + $extra
    $record.command = "$dotnet restore NorthQuayBuildHost.csproj --locked-mode; " + $record.command + "; $dotnet $binary --missing-task; artifact hash qualification via eng/Record-NorthQuayBuildHost.ps1"
    [IO.File]::WriteAllText($recordPath, ($record | ConvertTo-Json -Depth 40).Replace("`r`n","`n") + "`n")
  }
  $directory = Join-Path $root 'samples/net11.0/NorthQuayBuildHost'
  $before = Get-EvidenceInputs $root 'samples/net11.0/NorthQuayBuildHost/NorthQuayBuildHost.csproj'
  $nativeEnvironment = @{
    QUAY_MSBUILD_SDK=$sdk11
    LIB="$VcRoot/lib/x64;$WindowsKit/Lib/10.0.26100.0/um/x64;$WindowsKit/Lib/10.0.26100.0/ucrt/x64"
    INCLUDE="$VcRoot/include;$WindowsKit/Include/10.0.26100.0/ucrt;$WindowsKit/Include/10.0.26100.0/shared;$WindowsKit/Include/10.0.26100.0/um"
  }
  # Force this one native compilation so a refresh observes diagnostics again.
  $object = Join-Path $directory 'obj/Release/net11.0/win-x64/native/NorthQuayBuildHost.obj'
  if (Test-Path -LiteralPath $object) { Remove-Item -LiteralPath $object }
  $publish = Invoke-Child $Net11Dotnet @('publish','NorthQuayBuildHost.csproj','-c','Release','--no-restore',
    '-p:IlcUseEnvironmentalTools=true',"-p:CppCompiler=$VcRoot/bin/Hostx64/x64/cl.exe", "-p:CppLinker=$VcRoot/bin/Hostx64/x64/link.exe",
    '-p:TrimmerSingleWarn=false') $directory $nativeEnvironment
  if ($publish -notmatch 'IL2026.*BuildManager.Build') { throw 'Expected retained public BuildManager IL2026 diagnostic missing.' }
  if ($publish -match '(?m)(?:warning|error) (?!IL2026\b)[A-Z]+\d+:' -or $publish -match 'will always throw') {
    throw 'Unexpected native diagnostic: inspect the complete output.'
  }
  $binary = Join-Path $directory 'bin/Release/net11.0/win-x64/publish/NorthQuayBuildHost.exe'
  $bytes = [IO.File]::ReadAllBytes($binary)
  $pe = [BitConverter]::ToInt32($bytes, 0x3c)
  if ([BitConverter]::ToUInt16($bytes,$pe + 4) -ne 0x8664 -or [BitConverter]::ToInt32($bytes,$pe + 24 + 112 + 14 * 8) -ne 0) {
    throw 'Expected x64 native PE without a CLR header.'
  }
  $native = "NATIVE PE x64 CLR-directory=0 sha256=$((Get-FileHash -LiteralPath $binary).Hash.ToLowerInvariant())`n$publish"
  $response = Get-Content -LiteralPath (Join-Path $directory 'obj/Release/net11.0/win-x64/native/NorthQuayBuildHost.ilc.rsp')
  $switches = @($response | Where-Object { $_ -match '^--feature:Microsoft.Build\.' })
  if ($switches.Count -ne 8 -or @($switches | Where-Object { $_ -notmatch '=(false|true)$' }).Count -ne 0) {
    throw 'Expected all eight packaged MSBuild trim-time feature settings in the real ILC response.'
  }
  $native += "`nILC package defaults:`n" + ($switches -join "`n")
  foreach ($arguments in @(@(), @('--evaluate-only'), @('--missing-task'), @('--missing-resolver'))) {
    $native += "`n" + (Invoke-Child $binary $arguments $output @{ QUAY_MSBUILD_SDK=$sdk11 })
  }
  $after = Get-EvidenceInputs $root 'samples/net11.0/NorthQuayBuildHost/NorthQuayBuildHost.csproj'
  if (($before | ConvertTo-Json -Compress) -cne ($after | ConvertTo-Json -Compress)) { throw 'Native proof inputs changed.' }
  $record = Get-Content -LiteralPath (Join-Path $directory 'verification-evidence.json') -Raw | ConvertFrom-Json -AsHashtable
  $record.kind = 'execution'
  $record.recordedAt = [DateTimeOffset]::UtcNow.ToString('O')
  $record.command = "& eng/Record-NorthQuayBuildHost.ps1 -Net10Dotnet '$Net10Dotnet' -Net11Dotnet '$Net11Dotnet' -VcRoot '$VcRoot' -WindowsKit '$WindowsKit' -Packages '$Packages'"
  $record.scope = 'Actual platform ILC/link publish and native PE execution; success, evaluation-only and missing task/resolver controls. IL2026 remains visible; not analyzer-clean or general MSBuild AOT support.'
  $record.output = $restores[10] + "`n" + $restores[11] + "`n" + ($observations -join "`n") + "`n" + $native
  [IO.File]::WriteAllText((Join-Path $directory 'native.evidence.json'), ($record | ConvertTo-Json -Depth 40).Replace("`r`n","`n") + "`n")
  Write-Host 'PASS: paired managed hosts, actual native publish/run, binding/output/logger, missing registrations and unchanged environment.'
}
finally {
  foreach ($name in $previous.Keys) { [Environment]::SetEnvironmentVariable($name, $previous[$name]) }
  foreach ($scope in $paths.Keys) {
    if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH', $scope)) { throw "Unexpected $scope PATH change; shared settings preserved." }
  }
  foreach ($scope in $persistent.Keys) {
    if ($persistent[$scope] -cne ([Environment]::GetEnvironmentVariables($scope) | ConvertTo-Json -Compress)) { throw "Unexpected persistent $scope environment change; shared settings preserved." }
  }
}
