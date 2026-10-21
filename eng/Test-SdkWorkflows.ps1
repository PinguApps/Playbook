[CmdletBinding()]
param(
  [string]$Net9Dotnet = 'dotnet',
  [string]$Net10Dotnet = 'dotnet',
  [string]$Net11Dotnet = 'dotnet',
  [switch]$Record
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
. (Join-Path $PSScriptRoot 'Evidence.ps1')
$root = Split-Path $PSScriptRoot -Parent
$Net9Dotnet = (Get-Command $Net9Dotnet -CommandType Application | Select-Object -First 1).Source
$Net10Dotnet = (Get-Command $Net10Dotnet -CommandType Application | Select-Object -First 1).Source
$Net11Dotnet = (Get-Command $Net11Dotnet -CommandType Application | Select-Object -First 1).Source
$scratch = Join-Path ([IO.Path]::GetTempPath()) ('playbook-sdk-' + [guid]::NewGuid().ToString('N'))
$null = New-Item -ItemType Directory -Path $scratch
$paths = @{}
foreach ($scope in @('Process', 'User', 'Machine')) { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH', $scope) }
$log = [Collections.Generic.List[string]]::new()
$recordInputs = @{}
if ($Record) {
  foreach ($release in @('10', '11')) {
    $recordInputs[$release] = Get-EvidenceInputs $root "samples/net$release.0/SdkWorkflows/SdkWorkflows.csproj"
  }
}

function Invoke-Sdk([string]$Dotnet, [string]$Directory, [string[]]$Arguments, [hashtable]$Environment = @{}, [int]$ExpectedExit = 0) {
  $start = [Diagnostics.ProcessStartInfo]::new($Dotnet)
  $start.WorkingDirectory = $Directory
  $start.UseShellExecute = $false
  $start.CreateNoWindow = $true
  $start.RedirectStandardOutput = $true
  $start.RedirectStandardError = $true
  foreach ($argument in $Arguments) { $start.ArgumentList.Add($argument) }
  $start.Environment['DOTNET_ADD_GLOBAL_TOOLS_TO_PATH'] = '0'
  $start.Environment['DOTNET_CLI_HOME'] = Join-Path $scratch ('home-' + [IO.Path]::GetFileName((Split-Path $Dotnet)))
  $start.Environment['DOTNET_NOLOGO'] = '1'
  $start.Environment['DOTNET_CLI_USE_MSBUILD_SERVER'] = 'false'
  $start.Environment['MSBUILDUSESERVER'] = '0'
  foreach ($key in $Environment.Keys) { $start.Environment[$key] = $Environment[$key] }
  $process = [Diagnostics.Process]::Start($start)
  $stdout = $process.StandardOutput.ReadToEndAsync()
  $stderr = $process.StandardError.ReadToEndAsync()
  if (-not $process.WaitForExit(90000)) { $process.Kill($true); throw "Timed out: $($Arguments -join ' ')" }
  if (-not [Threading.Tasks.Task]::WaitAll([Threading.Tasks.Task[]]@($stdout, $stderr), 15000)) {
    throw "Output streams did not close: $($Arguments -join ' ')"
  }
  $output = $stdout.GetAwaiter().GetResult() + $stderr.GetAwaiter().GetResult()
  $log.Add("$Dotnet $($Arguments -join ' ')`n$output")
  if (($ExpectedExit -eq 0 -and $process.ExitCode -ne 0) -or ($ExpectedExit -ne 0 -and $process.ExitCode -eq 0)) {
    throw "Unexpected exit $($process.ExitCode): $($Arguments -join ' ')`n$output"
  }
  return $output
}

function Assert-Contains([string]$Actual, [string]$Expected) {
  if (-not $Actual.Contains($Expected, [StringComparison]::Ordinal)) { throw "Missing '$Expected': $Actual" }
}

function Write-Input([string]$Directory, [string]$Name, [string]$Text) {
  $path = Join-Path $Directory $Name
  $null = New-Item -ItemType Directory -Path (Split-Path $path) -Force
  [IO.File]::WriteAllText($path, $Text.Replace("`r`n", "`n"))
}

function New-TemplatePackage([string]$Feed, [string]$Version, [string]$Target, [string]$Label) {
  $source = Join-Path $scratch "template-$Target-$Version"
  Write-Input $source 'Estuary.Depot.Templates.nuspec' "<package><metadata><id>Estuary.Depot.Templates</id><version>$Version</version><authors>Playbook</authors><description>Original local-only verification template.</description><packageTypes><packageType name=`"Template`" /></packageTypes></metadata></package>"
  Copy-Item -LiteralPath (Join-Path $root "samples/net$Target.0/SdkWorkflows/Template") -Destination (Join-Path $source 'content') -Recurse
  $program = Join-Path $source 'content/Program.cs'
  [IO.File]::WriteAllText($program, [IO.File]::ReadAllText($program).Replace('__CHANNEL__', $Label))
  $null = New-Item -ItemType Directory -Path $Feed -Force
  [IO.Compression.ZipFile]::CreateFromDirectory($source, (Join-Path $Feed "Estuary.Depot.Templates.$Version.nupkg"))
}

try {
  # Run each actual older/newer utility, including rejection rather than only the default output.
  foreach ($release in @('9', '10', '11')) {
    $dotnet = switch ($release) { '9' { $Net9Dotnet } '10' { $Net10Dotnet } '11' { $Net11Dotnet } }
    $directory = Join-Path $root "samples/net$release.0/RepositoryUtility"
    $inputArgs = if ($release -eq '9') { @('--project', 'RepositoryUtility.csproj') } else { @('--file', 'slots.cs') }
    Assert-Contains (Invoke-Sdk $dotnet $directory (@('run', '-c', 'Release') + $inputArgs + @('--', 'north:7', 'south:4'))) 'slots=11'
    Assert-Contains (Invoke-Sdk $dotnet $directory (@('run', '-c', 'Release') + $inputArgs + @('--', 'north:-1')) @{} 1) 'Expected a named rack'
  }

  foreach ($release in @('10', '11')) {
    $dotnet = if ($release -eq '10') { $Net10Dotnet } else { $Net11Dotnet }
    $directory = Join-Path $root "samples/net$release.0/SdkWorkflows"
    $workspace = Join-Path $scratch "net$release"
    $null = New-Item -ItemType Directory -Path $workspace
    Copy-Item -LiteralPath (Join-Path $directory 'global.json') -Destination $workspace
    Write-Input $workspace 'NuGet.Config' '<configuration><packageSources><clear/><add key="nuget" value="https://api.nuget.org/v3/index.json"/></packageSources></configuration>'
    Write-Input $workspace 'Directory.Build.props' '<Project><PropertyGroup><Nullable>enable</Nullable><ImplicitUsings>enable</ImplicitUsings><TreatWarningsAsErrors>true</TreatWarningsAsErrors></PropertyGroup></Project>'
    Write-Input $workspace 'Selected.csproj' "<Project Sdk=`"Microsoft.NET.Sdk`"><PropertyGroup><TargetFramework>net$release.0</TargetFramework><EnableDefaultCompileItems>false</EnableDefaultCompileItems></PropertyGroup></Project>"
    Write-Input $workspace 'Unselected.csproj' "<Project Sdk=`"Microsoft.NET.Sdk`"><PropertyGroup><TargetFramework>net$release.0</TargetFramework><EnableDefaultCompileItems>false</EnableDefaultCompileItems></PropertyGroup><Target Name=`"MustNotBuild`" BeforeTargets=`"Build`"><Error Text=`"UNSELECTED PROJECT BUILT`" /></Target></Project>"
    Write-Input $workspace 'Depot.slnx' '<Solution><Project Path="Selected.csproj"/><Project Path="Unselected.csproj"/></Solution>'
    if ($release -eq '10') {
      # .NET 10 can build a hand-authored filter; no CLI editor is required.
      Write-Input $workspace 'Depot.slnf' '{"solution":{"path":"Depot.slnx","projects":["Selected.csproj"]}}'
    }
    else {
      # snippet:filter
      $null = Invoke-Sdk $dotnet $workspace @('new', 'slnf', '--name', 'Depot', '--parent-solution', 'Depot.slnx')
      $null = Invoke-Sdk $dotnet $workspace @('sln', 'Depot.slnf', 'add', 'Selected.csproj')
      Assert-Contains (Invoke-Sdk $dotnet $workspace @('sln', 'Depot.slnf', 'list')) 'Selected.csproj'
      # endsnippet:filter
    }
    $null = Invoke-Sdk $dotnet $workspace @('build', 'Depot.slnf', '-c', 'Release')
    Assert-Contains (Invoke-Sdk $dotnet $workspace @('build', 'Depot.slnx', '-c', 'Release') @{} 1) 'UNSELECTED PROJECT BUILT'
    if ($release -eq '11') {
      $null = Invoke-Sdk $dotnet $workspace @('sln', 'Depot.slnf', 'remove', 'Selected.csproj')
      if ((Invoke-Sdk $dotnet $workspace @('sln', 'Depot.slnf', 'list')).Contains('Selected.csproj')) { throw 'Filter remove failed.' }
    }

    # Formatting changes the chosen source, preserves its sibling, and respects the ancestor configuration.
    Write-Input $workspace '.editorconfig' "root = true`n[*.cs]`nindent_style = space`nindent_size = 2`n"
    $unformatted = "class Rack{public int Count{get;set;}}`n"
    Write-Input $workspace 'Chosen.cs' $unformatted
    Write-Input $workspace 'Untouched.cs' $unformatted
    $null = Invoke-Sdk $dotnet $workspace @('format', 'whitespace', '.', '--folder', '--include', 'Chosen.cs')
    if ((Get-Content -LiteralPath (Join-Path $workspace 'Chosen.cs') -Raw) -eq $unformatted) { throw 'Selected file was not formatted.' }
    if ((Get-Content -LiteralPath (Join-Path $workspace 'Untouched.cs') -Raw) -cne $unformatted) { throw 'Unselected source changed.' }
    $null = Invoke-Sdk $dotnet $workspace @('format', 'whitespace', '.', '--folder', '--include', 'Chosen.cs', '--verify-no-changes')
    if ($release -eq '11') {
      Write-Input $workspace 'Single.cs' "#:property PublishAot=false`nSystem.Console.WriteLine(  17 );`n"
      $null = Invoke-Sdk $dotnet $workspace @('format', 'Single.cs')
      $null = Invoke-Sdk $dotnet $workspace @('format', 'Single.cs', '--verify-no-changes')
      Write-Input $workspace 'Multi.csproj' '<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFrameworks>net10.0;net11.0</TargetFrameworks><EnableDefaultCompileItems>false</EnableDefaultCompileItems></PropertyGroup><ItemGroup><Compile Include="Chosen.cs" /></ItemGroup></Project>'
      Write-Input $workspace 'NuGet.Config' '<configuration><packageSources><clear/><add key="nuget" value="https://api.nuget.org/v3/index.json"/></packageSources></configuration>'
      $null = Invoke-Sdk $dotnet $workspace @('format', 'Multi.csproj', '--framework', 'net11.0', '--verify-no-changes')
    }

    # App arguments, evaluated MSBuild properties and process environment remain distinct.
    $output = Invoke-Sdk $dotnet $directory @('run', '--project', 'SdkWorkflows.csproj', '-c', 'Release', '--launch-profile', 'Depot')
    Assert-Contains $output '"Arguments":["--shelf","DepotProbe"]'
    Assert-Contains $output '"Environment":"profile"'
    if ($release -eq '10') {
      $output = Invoke-Sdk $dotnet $directory @('run', '--project', 'SdkWorkflows.csproj', '-c', 'Release', '--no-launch-profile', '--', '-e', 'DEPOT_ZONE=literal') @{ DEPOT_ZONE = 'inherited' }
      Assert-Contains $output '"Environment":"inherited"'
      Assert-Contains $output '"Arguments":["-e","DEPOT_ZONE=literal"]'
    }
    else {
      # snippet:launch
      $output = Invoke-Sdk $dotnet $directory @('run', '--project', 'SdkWorkflows.csproj', '-c', 'Release', '-e', 'DEPOT_ZONE=cli', '-p:AssemblyName=NightDepot', '--launch-profile', 'Depot')
      Assert-Contains $output '"Environment":"cli"'
      Assert-Contains $output '"Arguments":["--shelf","NightDepot"]'
      # endsnippet:launch
    }
    $output = Invoke-Sdk $dotnet $directory @('run', '--project', 'SdkWorkflows.csproj', '--no-launch-profile') @{ Configuration = 'Release' }
    Assert-Contains $output '"Configuration":"Release"'
    $output = Invoke-Sdk $dotnet $directory @('run', '--project', 'SdkWorkflows.csproj', '-c', 'Debug', '--no-launch-profile') @{ Configuration = 'Release' }
    Assert-Contains $output '"Configuration":"Debug"'
    $published = Join-Path $workspace 'published'
    $null = Invoke-Sdk $dotnet $directory @('publish', 'SdkWorkflows.csproj', '-o', $published) @{ Configuration = 'Debug' }
    $output = Invoke-Sdk $dotnet $directory @((Join-Path $published 'DepotProbe.dll'))
    Assert-Contains $output $(if ($release -eq '10') { '"Configuration":"Release"' } else { '"Configuration":"Debug"' })
    $output = Invoke-Sdk $dotnet $directory @('run', '--project', 'SdkWorkflows.csproj', '-c', 'Debug', '--no-launch-profile') @{ Configuration = '  ' }
    Assert-Contains $output '"Configuration":"Debug"'

    # Actual include/package routes, with separate .NET 10 servicing and .NET 11 metadata checks.
    $composed = Join-Path $root "samples/net$release.0/ComposedUtility"
    $output = Invoke-Sdk $dotnet $composed @('run', '--file', 'slots.cs', '-c', 'Release', '--', '--reserve', '4')
    Assert-Contains $output 'free=8'
    if ($release -eq '11') { Assert-Contains $output 'company=Estuary Depot Tools' }
    Assert-Contains (Invoke-Sdk $dotnet $composed @('run', '--file', 'slots.cs', '-c', 'Release', '--', '--reserve', '13') @{} 1) 'ArgumentOutOfRangeException'
    $null = Invoke-Sdk $dotnet $composed @('build', 'RackLibrary/RackLibrary.csproj', '-c', 'Release')
    $referenceFile = if ($release -eq '10') { 'project-route.cs' } else { 'dll-route.cs' }
    Assert-Contains (Invoke-Sdk $dotnet $composed @('run', '--file', $referenceFile, '-c', 'Release')) 'free=8'
    if ($release -eq '11') {
      $referenceCopy = Join-Path $workspace 'reference-managed.cs'
      Copy-Item -LiteralPath (Join-Path $composed 'dll-route.cs') -Destination $referenceCopy
      $source = [IO.File]::ReadAllText($referenceCopy)
      [IO.File]::WriteAllText($referenceCopy, [regex]::Replace($source, '(?m)^#:include .*\r?\n', ''))
      $library = Join-Path $composed 'RackLibrary/RackLibrary.csproj'
      $null = Invoke-Sdk $dotnet $workspace @('reference', 'add', '--file', $referenceCopy, $library)
      Assert-Contains (Invoke-Sdk $dotnet $workspace @('reference', 'list', '--file', $referenceCopy)) 'RackLibrary.csproj'
      Assert-Contains (Invoke-Sdk $dotnet $workspace @('run', '--file', $referenceCopy, '-c', 'Release')) 'free=8'
      $null = Invoke-Sdk $dotnet $workspace @('reference', 'remove', '--file', $referenceCopy, $library)
      if ([IO.File]::ReadAllText($referenceCopy).Contains('#:project ')) { throw 'Reference remove did not update the file.' }
    }
    $central = Join-Path $workspace 'central'
    $null = New-Item -ItemType Directory -Path $central
    $source = [IO.File]::ReadAllText((Join-Path $composed 'slots.cs'))
    $source = $source.Replace('#:property ManagePackageVersionsCentrally=false', '').Replace('System.CommandLine@2.0.1', 'System.CommandLine')
    Write-Input $central 'slots.cs' $source
    Copy-Item -LiteralPath (Join-Path $composed 'Rack.cs') -Destination $central
    Write-Input $central 'Directory.Packages.props' '<Project><PropertyGroup><ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally><CentralPackageVersionOverrideEnabled>false</CentralPackageVersionOverrideEnabled></PropertyGroup><ItemGroup><PackageVersion Include="System.CommandLine" Version="2.0.1" /></ItemGroup></Project>'
    Assert-Contains (Invoke-Sdk $dotnet $central @('run', '--file', 'slots.cs', '-c', 'Release', '--', '--reserve', '4')) 'free=8'
    if ($release -eq '11') {
      $helper = Get-Content -LiteralPath (Join-Path $central 'Rack.cs') -Raw
      Write-Input $central 'Rack.cs' ("#:property Company=`"Estuary Depot Tools`"`n" + $helper)
      Assert-Contains (Invoke-Sdk $dotnet $central @('run', '--file', 'slots.cs', '-c', 'Release')) 'company=Estuary Depot Tools'
      Write-Input $central 'Rack.cs' ("#:property Company=`"Different Company`"`n" + $helper)
      $null = Invoke-Sdk $dotnet $central @('build', 'slots.cs', '-c', 'Release') @{} 1
      Write-Input $central 'Rack.cs' $helper
      Write-Input $central 'slots.cs' ("#:property Depot:Zone=one`n" + $source)
      $null = Invoke-Sdk $dotnet $central @('build', 'slots.cs', '-c', 'Release') @{} 1
    }

    # A local-only feed makes stable/preview selection observable without publishing anything.
    $feed = Join-Path $workspace 'feed'
    New-TemplatePackage $feed '1.0.0' $release 'stable'
    New-TemplatePackage $feed '1.1.0-preview.1' $release 'preview'
    Write-Input $workspace 'NuGet.Config' ('<configuration><packageSources><clear/><add key="fixture" value="' + [Security.SecurityElement]::Escape($feed) + '"/></packageSources></configuration>')
    $null = Invoke-Sdk $dotnet $workspace @('new', 'install', 'Estuary.Depot.Templates', '--nuget-source', $feed)
    $null = Invoke-Sdk $dotnet $workspace @('new', 'depot-audit', '--name', 'DepotAudit', '-o', 'stable-app')
    Assert-Contains (Invoke-Sdk $dotnet $workspace @('run', '--project', 'stable-app/DepotAudit.csproj', '-c', 'Release')) 'template=stable'
    $install = if ($release -eq '10') {
      @('new', 'install', 'Estuary.Depot.Templates@1.1.0-preview.1', '--nuget-source', $feed)
    } else { @('new', 'install', 'Estuary.Depot.Templates', '--prerelease', '--nuget-source', $feed) }
    $null = Invoke-Sdk $dotnet $workspace $install
    $null = Invoke-Sdk $dotnet $workspace @('new', 'depot-audit', '--name', 'DepotAudit', '-o', 'preview-app')
    Assert-Contains (Invoke-Sdk $dotnet $workspace @('run', '--project', 'preview-app/DepotAudit.csproj', '-c', 'Release')) 'template=preview'
    if ($release -eq '11') {
      $null = Invoke-Sdk $dotnet $workspace @('new', 'install', 'Estuary.Depot.Templates@1.0.0', '--prerelease', '--nuget-source', $feed, '--force')
      $null = Invoke-Sdk $dotnet $workspace @('new', 'depot-audit', '--name', 'DepotAudit', '-o', 'pinned-app')
      Assert-Contains (Invoke-Sdk $dotnet $workspace @('run', '--project', 'pinned-app/DepotAudit.csproj', '-c', 'Release')) 'template=stable'
    }
    $log.Add("PASS: SDK $release selected-project build, targeted formatting, launch boundaries, explicit configuration, composed utility and local preview template selection.")
    Write-Host $log[$log.Count - 1]
  }
  $downlevel = Join-Path $root 'samples/net10.0/ComposedUtilitySdk11'
  $null = Invoke-Sdk $Net11Dotnet $downlevel @('build', 'slots.cs', '-c', 'Release', '-p:RestoreLockedMode=true')
  $properties = Invoke-Sdk $Net11Dotnet $downlevel @('build', 'slots.cs', '-c', 'Release', '-getProperty:TargetPath')
  Assert-Contains (Invoke-Sdk $Net10Dotnet $downlevel @($properties.Trim(), '--reserve', '4')) 'free=8;company=Estuary Depot Tools'
  $log.Add('PASS: SDK 11 builds the explicit net10.0/C# 14 file, which executes on the .NET 10 host.')
  Write-Host $log[$log.Count - 1]
  if ($Record) {
    foreach ($release in @('10', '11')) {
      $sample = "samples/net$release.0/SdkWorkflows/SdkWorkflows.csproj"
      $directory = Join-Path $root "samples/net$release.0/SdkWorkflows"
      $baseline = Get-Content -LiteralPath (Join-Path $directory 'verification-evidence.json') -Raw | ConvertFrom-Json -AsHashtable
      $baseline.kind = 'execution'
      $baseline.command = 'eng/Test-SdkWorkflows.ps1 -Record (executable paths recorded below)'
      $baseline.scope = 'Real CLI execution on Windows; native/device and Aspire execution are not established by these portable checks.'
      $baseline.recordedAt = [DateTimeOffset]::UtcNow.ToString('O')
      $baseline.inputs = Get-EvidenceInputs $root $sample
      if ((ConvertTo-Json $recordInputs[$release] -Compress) -cne (ConvertTo-Json $baseline.inputs -Compress)) {
        throw 'SDK command inputs changed during execution; stabilize the inputs and rerun before recording proof.'
      }
      $baseline.output = $log -join "`n"
      [IO.File]::WriteAllText((Join-Path $directory 'commands.evidence.json'), (ConvertTo-Json $baseline -Depth 30).Replace("`r`n", "`n") + "`n")
    }
  }
}
finally {
  foreach ($scope in $paths.Keys) {
    if ([Environment]::GetEnvironmentVariable('PATH', $scope) -cne $paths[$scope]) { throw "Unexpected $scope PATH change; shared setting was not restored." }
  }
  # Leave this unique scratch directory and logs available when a command fails.
  Write-Host "SDK command scratch: $scratch"
}
