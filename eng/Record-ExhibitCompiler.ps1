[CmdletBinding()]
param(
  [string]$Net10Dotnet = 'C:/Program Files/dotnet/dotnet.exe',
  [string]$Net11Dotnet = 'C:/Users/pingu/.dotnet-playbook-11/dotnet.exe'
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
. (Join-Path $PSScriptRoot 'Evidence.ps1')
$root = Split-Path $PSScriptRoot -Parent
$paths = @{}
foreach ($scope in 'Process','User','Machine') { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$previous = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
function git { & 'C:/Program Files/Git/cmd/git.exe' @args }
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  foreach ($major in 10,11) {
    $dotnet = if ($major -eq 10) { $Net10Dotnet } else { $Net11Dotnet }
    $sample = "samples/net$major.0/ExhibitCompiler/ExhibitCompiler.csproj"
    $directory = Join-Path $root (Split-Path $sample)
    $artifacts = Join-Path $root "artifacts/exhibit-compiler/net$major"
    New-Item -ItemType Directory -Path $artifacts -Force | Out-Null
    & (Join-Path $PSScriptRoot 'Record-Evidence.ps1') -Sample $sample -Dotnet $dotnet
    $record = Get-Content -LiteralPath (Join-Path $directory 'verification-evidence.json') -Raw | ConvertFrom-Json -AsHashtable
    $observations = [Collections.Generic.List[object]]::new()
    function Invoke-Check([string]$Name, [string]$Project, [string[]]$Arguments, [string]$Failure = '') {
      Push-Location -LiteralPath (Split-Path $Project)
      try {
        $log = Join-Path $artifacts "$Name.log"
        $binlog = Join-Path $artifacts "$Name.binlog"
        # Expected failure controls retain the actual exit code and diagnostic.
        $PSNativeCommandUseErrorActionPreference = $false
        $output = (& $dotnet build $Project -c Release "-bl:$binlog" @Arguments 2>&1 | Out-String).Trim()
        $code = $LASTEXITCODE
        [IO.File]::WriteAllText($log, $output)
        if ($Failure) {
          if ($code -eq 0 -or $output -notmatch $Failure) { throw "Expected $Name failure /$Failure/, exit $code. See $log" }
        } elseif ($code -ne 0) { throw "Build $Name failed. See $log" }
        $run = ''
        if (-not $Failure -and '-t:CompileExhibit' -notin $Arguments) {
          $run = (& $dotnet "bin/Release/net$major.0/ExhibitCompiler.dll" 2>&1 | Out-String).Trim()
          if ($LASTEXITCODE -ne 0 -or $run -notmatch 'PASS generated exhibit') { throw "Execution $Name failed: $run" }
        }
        $generated = Join-Path (Split-Path $Project) "obj/Release/net$major.0/Exhibit.g.cs"
        $manifest = Join-Path (Split-Path $Project) "obj/Release/net$major.0/exhibit.taghelpers.json"
        $observation = [ordered]@{ name=$Name; command="& '$dotnet' build '$Project' -c Release '-bl:$binlog' $($Arguments -join ' ')"; exitCode=$code; expectedDiagnostic=$Failure; observedDiagnostic=(@($output -split "`r?`n" | Where-Object { $Failure -and $_ -match $Failure }) -join "`n"); projectSha256=(Get-FileHash -LiteralPath $Project).Hash.ToLowerInvariant(); sourceSha256=(Get-FileHash -LiteralPath (Join-Path (Split-Path $Project) 'Exhibit.razor')).Hash.ToLowerInvariant(); runCommand=$(if ($run) { "& '$dotnet' bin/Release/net$major.0/ExhibitCompiler.dll" } else { '' }); runtimeOutput=$run; binlogSha256=(Get-FileHash -LiteralPath $binlog).Hash.ToLowerInvariant() }
        if (Test-Path -LiteralPath $generated) {
          Copy-Item -LiteralPath $generated -Destination (Join-Path $artifacts "$Name.g.cs")
          $text = Get-Content -LiteralPath $generated -Raw
          $observation.generatedSha256 = (Get-FileHash -LiteralPath $generated).Hash.ToLowerInvariant()
          $observation.discoveredBadge = $text.Contains('OpenComponent<global::ExhibitParts.ExhibitBadge>')
        }
        if (Test-Path -LiteralPath $manifest) {
          Copy-Item -LiteralPath $manifest -Destination (Join-Path $artifacts "$Name.taghelpers.json")
          $observation.manifestSha256 = (Get-FileHash -LiteralPath $manifest).Hash.ToLowerInvariant()
          $observation.manifestHasBadge = (Get-Content -LiteralPath $manifest -Raw).Contains('ExhibitParts.ExhibitBadge')
        }
        $observations.Add($observation)
        Write-Host "PASS $major/$Name (exit $code) $run"
        return $observation
      } finally { Pop-Location }
    }
    function New-Control([string]$Name, [string]$Mode) {
      $destination = Join-Path $artifacts ("$Name-" + [guid]::NewGuid().ToString('N'))
      foreach ($file in Get-ChildItem -LiteralPath $directory -Recurse -File | Where-Object { $_.FullName -notmatch '[/\\](bin|obj)[/\\]' -and $_.Name -notlike '*evidence.json' }) {
        $target = Join-Path $destination ([IO.Path]::GetRelativePath($directory,$file.FullName))
        New-Item -ItemType Directory -Path (Split-Path $target) -Force | Out-Null
        Copy-Item -LiteralPath $file.FullName -Destination $target
      }
      $project = Join-Path $destination 'ExhibitCompiler.csproj'
      $text = Get-Content -LiteralPath $project -Raw
      if ($Mode -eq 'default') { $text = $text.Replace(' UseSourceGenerator="true"','') }
      if ($Mode -eq 'engine') { $text = $text.Replace(' UseSourceGenerator="true"',' UseSourceGenerator="false"') }
      [IO.File]::WriteAllText($project,$text)
      return $project
    }
    $mode = if ($major -eq 10) { 'default' } else { 'hosted' }
    $baseline = Invoke-Check $mode (Join-Path $root $sample) @()
    if (-not $baseline.discoveredBadge -or -not $baseline.manifestHasBadge) { throw 'Actual dependency was not discovered.' }
    if ($major -eq 11) {
      foreach ($choice in 'default','engine') {
        $project = New-Control $choice $choice
        $result = Invoke-Check $choice $project @()
        if (-not $result.discoveredBadge -or -not $result.manifestHasBadge) { throw 'Legacy engine discovery failed.' }
      }
      foreach ($choice in 'hosted','engine') {
        $project = New-Control "empty-manifest-$choice" $choice
        $text = (Get-Content -LiteralPath $project -Raw).Replace('    <SdkRazorGenerate', '    <WriteLinesToFile File="$(ExhibitManifest)" Lines="[]" Overwrite="true" />' + "`n    <SdkRazorGenerate")
        [IO.File]::WriteAllText($project,$text)
        $arguments = if ($choice -eq 'hosted') { @() } else { @('-t:CompileExhibit') }
        $result = Invoke-Check "empty-manifest-$choice" $project $arguments
        if ($result.manifestHasBadge -or $result.discoveredBadge -ne ($choice -eq 'hosted')) { throw 'Manifest/reference selection control failed.' }
      }
    }
    foreach ($choice in @('default') + $(if ($major -eq 11) { @('hosted') } else { @() })) {
      $project = New-Control "missing-$choice" $choice
      $text = (Get-Content -LiteralPath $project -Raw).Replace('<ExhibitReference Include="@(ReferencePath)" />', '<ExhibitReference Include="@(ReferencePath)" Condition="''%(ReferencePath.Filename)'' != ''ExhibitParts''" />')
      [IO.File]::WriteAllText($project,$text)
      $result = Invoke-Check "missing-$choice" $project @('-t:CompileExhibit')
      if ($result.discoveredBadge -or $result.manifestHasBadge) { throw 'Missing dependency control still discovered the badge.' }
      $project = New-Control "invalid-$choice" $choice
      Add-Content -LiteralPath (Join-Path (Split-Path $project) 'Exhibit.razor') -Value '@if (true) {'
      $null = Invoke-Check "invalid-$choice" $project @() 'RZ1006'
    }
    $project = New-Control 'relative-discovery' $mode
    $text = (Get-Content -LiteralPath $project -Raw).Replace('Assemblies="@(ExhibitReference)"','Assemblies="relative.dll"')
    [IO.File]::WriteAllText($project,$text)
    $null = Invoke-Check 'relative-discovery' $project @('-t:CompileExhibit') 'Assembly paths must be rooted'
    $project = New-Control 'missing-metadata' $mode
    $text = (Get-Content -LiteralPath $project -Raw).Replace('<GeneratedOutput>$(ExhibitGenerated)</GeneratedOutput>','')
    [IO.File]::WriteAllText($project,$text)
    $null = Invoke-Check 'missing-metadata' $project @('-t:CompileExhibit') "Missing required metadata 'GeneratedOutput'"
    if ($major -eq 10) {
      $project = New-Control 'unavailable-flag' $mode
      $text = (Get-Content -LiteralPath $project -Raw).Replace('TagHelperManifest="$(ExhibitManifest)"','TagHelperManifest="$(ExhibitManifest)" UseSourceGenerator="true"')
      [IO.File]::WriteAllText($project,$text)
      $null = Invoke-Check 'unavailable-flag' $project @('-t:CompileExhibit') 'MSB4064'
    }
    $sdkRoot = Join-Path (Split-Path $dotnet) "sdk/$($record.sdk)"
    $toolingRoot = Join-Path $sdkRoot 'Sdks/Microsoft.NET.Sdk.Razor'
    $toolFiles = @((Get-ChildItem -LiteralPath "$toolingRoot/tools" -File).FullName) + @((Get-ChildItem -LiteralPath "$toolingRoot/tasks/net$major.0" -File).FullName) + @("$toolingRoot/targets/Microsoft.NET.Sdk.Razor.CodeGeneration.targets", "$sdkRoot/MSBuild.dll", "$sdkRoot/Microsoft.Build.dll", "$sdkRoot/Microsoft.Build.Framework.dll", "$sdkRoot/Microsoft.Build.Utilities.Core.dll", "$sdkRoot/Roslyn/bincore/csc.dll", "$sdkRoot/Roslyn/bincore/Microsoft.CodeAnalysis.dll", "$sdkRoot/Roslyn/bincore/Microsoft.CodeAnalysis.CSharp.dll")
    $audit = (Get-Content -LiteralPath (Join-Path $root 'docs/planning/audits/blazor.assets.json') -Raw | ConvertFrom-Json).baselines | Where-Object sdk -EQ $record.sdk
    $tooling = @($toolFiles | ForEach-Object {
      $relative = [IO.Path]::GetRelativePath($sdkRoot,$_).Replace('\','/')
      $hash = (Get-FileHash -LiteralPath $_).Hash.ToLowerInvariant()
      $known = @($audit.sdkSurfaces + $audit.razorTooling | Where-Object path -CEQ $relative)
      if ($known.Count -and @($known | Where-Object sha256 -CNE $hash).Count) { throw "Artifact differs from source audit: $relative" }
      @{ path=$relative; version=[Diagnostics.FileVersionInfo]::GetVersionInfo($_).ProductVersion; sha256=$hash; matchesAudit=($known.Count -gt 0) }
    })
    if (((Get-EvidenceInputs $root $sample) | ConvertTo-Json -Compress) -cne ($record.inputs | ConvertTo-Json -Compress)) { throw 'Exhibit inputs changed during verification.' }
    $record.kind = 'execution'
    $record.command = "& '$PSCommandPath' -Net10Dotnet '$Net10Dotnet' -Net11Dotnet '$Net11Dotnet'"
    $record.scope = 'Real SdkRazorTagHelper/SdkRazorGenerate tasks, matching rzc/compiler/MSBuild closure, compiled generated C# and HtmlRenderer execution; old/current default, explicit engine/hosted, missing discovery dependency, invalid Razor, relative assembly path, required metadata and unavailable older flag controls. Windows x64 managed build host only; no browser, general IDE, old-target hosted route or actual GA claim.'
    $record.recordedAt = [DateTimeOffset]::UtcNow.ToString('O')
    $record.output = (@{ observations=@($observations.ToArray()); tooling=$tooling; generated=(Get-Content -LiteralPath (Join-Path $artifacts "$mode.g.cs") -Raw) } | ConvertTo-Json -Depth 20)
    [IO.File]::WriteAllText((Join-Path $directory 'behavior.evidence.json'), ($record | ConvertTo-Json -Depth 40).Replace("`r`n","`n") + "`n")
  }
  Write-Host 'PASS: actual old/current task generation and failure controls; generated artifacts/binlogs retained under artifacts/exhibit-compiler; PATH unchanged.'
} finally {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previous
  foreach ($scope in $paths.Keys) { if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." } }
}
